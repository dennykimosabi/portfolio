using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.Models;
using Storefront.Client.State;
using Storefront.Client.Utilities;

namespace Storefront.Client.Pages.BlueprintPages;

/// <summary>
/// Product detail page.
///
/// Route: /product/{stockNumber}/{productSlug}[?makeName=...&modelYear=...&modelName=...&stockNumber=...]
/// - stockNumber: unique product identifier (from route or querystring)
/// - productSlug: product description slug (SEO), matched against URL for consistency
/// - makeName, modelYear, modelName: optional fitment context from querystring
///
/// Data flow:
/// 1. OnParametersSetAsync extracts stockNumber from route
/// 2. Parse querystring for fitment context (makeName, modelYear, modelName)
/// 3. If fitment provided, call GetInterpretationAsync to resolve ukeys
/// 4. Call ProductClientService.GetProductDetailsAsync with resolved fitment or FitmentState
/// 5. Remaining sprocs (images, tags, reviews, etc.) wired per build order in Storefront-PDP-Data-Flow.md
/// </summary>
public partial class BlueprintProduct : ComponentBase
{
    [Parameter]
    public string StockNumber { get; set; } = "VP-10421";

    [Parameter]
    public string ProductSlug { get; set; } = "front-brake-pad-set";

    [Inject]
    private IProductService ProductService { get; set; } = null!;

    [Inject]
    private IFulfillmentService FulfillmentService { get; set; } = null!;

    [Inject]
    private IVehicleService VehicleService { get; set; } = null!;

    [Inject]
    private VehicleFitmentState FitmentState { get; set; } = null!;

    [Inject]
    private ICartService CartService { get; set; } = null!;

    [Inject]
    private CartState CartState { get; set; } = null!;

    [Inject]
    private IWebsiteContextProvider WebsiteContext { get; set; } = null!;

    [Inject]
    private IWebsiteSettingsService WebsiteSettingsService { get; set; } = null!;

    [Inject]
    private ISessionStateProvider SessionState { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ILogService LogService { get; set; } = null!;

    // Persisted across the SSR→WASM hydration boundary so hydration doesn't re-fetch
    // everything from scratch (mirrors Search.razor.cs / GuidedNavigation.razor).
    [PersistentState] public ProductDetail? _productDetail { get; set; }
    [PersistentState] public WebsiteSettingsDto? _websiteSettings { get; set; }
    private bool _isLoading = true;
    private string? _errorMessage;
    private bool _firstParamSet = true;

    // ─── Fitment context (from querystring or FitmentState) ────────────────
    [PersistentState] public int? _ukeyMake { get; set; }
    [PersistentState] public int? _ukeyModel { get; set; }
    [PersistentState] public int? _modelYear { get; set; }
    [PersistentState] public int? _ukeyDriveline { get; set; }
    [PersistentState] public int? _ukeyTrimLevel { get; set; }
    private string _queryString = string.Empty;

    // ─── Image gallery ────────────────────────────────────────────────────
    // No initializers on [PersistentState] properties — one can race restoration and win
    // (BL0009). Stay nullable and null-check at each read site instead, same as
    // Search.razor.cs/Landing.razor (e.g. `_results is not null`, `_assemblies is { Count: > 0 }`).
    [PersistentState] public IEnumerable<ProductImage>? _images { get; set; }

    // ─── Accordion section data ─────────────────────────────────────────────
    [PersistentState] public IEnumerable<ProductAssembly>? _assemblies { get; set; }
    [PersistentState] public IEnumerable<ProductReview>? _reviews { get; set; }
    [PersistentState] public IEnumerable<ProductApplication>? _applications { get; set; }
    [PersistentState] public IEnumerable<ProductType>? _types { get; set; }
    [PersistentState] public IEnumerable<StorefrontProductService>? _services { get; set; }
    [PersistentState] public IEnumerable<ProductAttachment>? _attachments { get; set; }
    [PersistentState] public IEnumerable<ProductAttachment>? _videos { get; set; }
    [PersistentState] public ProductReviewScore? _reviewScore { get; set; }

    // ─── Price box (availability / delivery / pickup) ──────────────────────
    [PersistentState] public InventoryMessage? _inventoryMessage { get; set; }
    [PersistentState] public string? _shippingPostalCode { get; set; }
    [PersistentState] public decimal? _localPickupCost { get; set; }

    // Not persisted — an on-demand result from a user action, not spine data worth
    // surviving hydration; a fresh page load should start with no estimate shown.
    private ShippingEstimate? _shippingEstimate;
    private bool _isEstimatingShipping;

    protected override async Task OnInitializedAsync()
    {
        if (_websiteSettings is not null) return; // already loaded

        try
        {
            _websiteSettings = await WebsiteSettingsService.GetSettingsAsync(
                WebsiteContext.UkeyWebsite,
                SessionState.SessionId,
                SessionState.MachineId
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load website settings: {ex.Message}");
            _websiteSettings = new WebsiteSettingsDto();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // On first param set (hydration), if the spine datum was already restored from
        // prerender state, skip re-fetching everything — mirrors Search.razor.cs's
        // _firstParamSet guard (checked against _results there, _productDetail here).
        //
        // _isLoading is set BEFORE the first await in each branch below (not before this
        // check) — an await ahead of it would let Blazor render mid-yield with _isLoading
        // still at its default `true`, flashing a loading state even though nothing is
        // actually being (re)fetched. Learned the hard way via a diagnostic log that sat
        // above this guard.
        if (_firstParamSet)
        {
            _firstParamSet = false;
            if (_productDetail is not null)
            {
                _isLoading = false;

                // DIAGNOSTIC: confirm hydration restored from prerender instead of refetching.
                await LogService.LogInformation(
                    $"PDP: OnParametersSetAsync - restored from prerender state, skipping reload. " +
                    $"StockNumber={StockNumber}, UkeyWebsite={WebsiteContext.UkeyWebsite}, IsInteractive={RendererInfo.IsInteractive}",
                    "Product");
                return;
            }
        }

        _isLoading = true;
        _errorMessage = null;

        // DIAGNOSTIC: pin down the "Product Not Found" flash — is this the static prerender
        // pass, or a real interactive pass with WebsiteContext not yet resolved?
        await LogService.LogInformation(
            $"PDP: OnParametersSetAsync start (fetching). StockNumber={StockNumber}, " +
            $"UkeyWebsite={WebsiteContext.UkeyWebsite}, IsInteractive={RendererInfo.IsInteractive}",
            "Product");

        try
        {
            if (string.IsNullOrWhiteSpace(StockNumber))
            {
                _errorMessage = "Product not found.";
                return;
            }

            // ─── Parse fitment from querystring if provided ───────────────────────
            await ParseAndInterpretFitmentAsync();

            // ─── Extract full querystring for guided navigation ────────────────────
            await ExtractQueryStringAsync();

            // ─── Fetch product details (spine) — required
            // SessionId/MachineId/UkeyWebsite set here explicitly (not left for the controller
            // to inject) because prerendering calls ProductService in-process, bypassing the
            // controller entirely — see PDP "Product Not Found" flash investigation.
            var detailRequest = new GetProductDetailRequest
            {
                SessionId = SessionState.SessionId,
                MachineId = SessionState.MachineId,
                UkeyWebsite = WebsiteContext.UkeyWebsite,
                StockNumber = StockNumber,
                UkeyModel = _ukeyModel ?? FitmentState.UkeyModel,
                ModelYear = _modelYear ?? FitmentState.Year,
                UkeyDriveline = _ukeyDriveline ?? FitmentState.UkeyDriveline,
                UkeyTrimLevel = _ukeyTrimLevel ?? FitmentState.UkeyTrimLevel
            };

            _productDetail = await ProductService.GetDetailsAsync(detailRequest);

            if (_productDetail == null)
            {
                _errorMessage = "Product not found.";
                return;
            }

            // Now that we have ProductDetail with UkeyProduct (from the sproc), fetch all accordion section data in parallel
            // — failures degrade to empty, not error
            // Use resolved fitment from querystring, falling back to FitmentState
            var fitmentModel = _ukeyModel ?? FitmentState.UkeyModel;
            var fitmentYear = _modelYear ?? FitmentState.Year;
            var fitmentDriveline = _ukeyDriveline ?? FitmentState.UkeyDriveline;
            var fitmentTrim = _ukeyTrimLevel ?? FitmentState.UkeyTrimLevel;

            // SessionId/MachineId/UkeyWebsite set explicitly on every request below — same
            // reason as detailRequest above: prerendering calls ProductService in-process,
            // bypassing the controller's server-side injection entirely.
            var assemblyTask = ProductService.GetAssembliesAsync(
                new GetProductAssembliesRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct,
                    UkeyModel = fitmentModel,
                    ModelYear = fitmentYear,
                    UkeyDriveline = fitmentDriveline,
                    UkeyTrimLevel = fitmentTrim
                });

            var reviewTask = ProductService.GetReviewsAsync(
                new GetProductReviewsRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var applicationTask = ProductService.GetApplicationsAsync(
                new GetProductApplicationsRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var typeTask = ProductService.GetTypesAsync(
                new GetProductTypesRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct,
                    UkeyModel = fitmentModel,
                    ModelYear = fitmentYear,
                    UkeyDriveline = fitmentDriveline,
                    UkeyTrimLevel = fitmentTrim
                });

            var serviceTask = ProductService.GetServicesAsync(
                new GetProductServicesRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct,
                    UkeyModel = fitmentModel,
                    ModelYear = fitmentYear,
                    UkeyDriveline = fitmentDriveline,
                    UkeyTrimLevel = fitmentTrim
                });

            var attachmentTask = ProductService.GetAttachmentsAsync(
                new GetProductAttachmentsRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var reviewScoreTask = ProductService.GetReviewScoreAsync(
                new GetProductReviewScoreRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var imagesTask = ProductService.GetAdditionalImagesAsync(
                new GetProductImagesRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    StockNumber = StockNumber,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var inventoryMessageTask = ProductService.GetInventoryMessageAsync(
                new GetInventoryMessageRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    UkeyProduct = _productDetail.UkeyProduct
                });

            var shippingPostalCodeTask = FulfillmentService.GetShippingPostalCodeAsync(
                new GetShippingPostalCodeRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite
                    // IPAddress is filled server-side by FulfillmentService.
                });

            var localPickupCostTask = FulfillmentService.GetLocalPickupCostAsync(
                new GetLocalPickupCostRequest { UkeyWebsite = WebsiteContext.UkeyWebsite });

            var relatedProductsTask = ProductService.GetSuggestedProductsAsync(
                new GetSuggestedProductsRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    UkeyProduct = _productDetail.UkeyProduct,
                    IsRelated = false,  // Related Products
                    Performance = _websiteSettings?.ShowPerformanceSuggestions ?? false,
                    Accessory = _websiteSettings?.ShowAccessorySuggestions ?? false
                });

            var alsoBoughtTask = ProductService.GetSuggestedProductsAsync(
                new GetSuggestedProductsRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    UkeyProduct = _productDetail.UkeyProduct,
                    IsRelated = true  // People Also Bought
                });

            await Task.WhenAll(assemblyTask, reviewTask, applicationTask, typeTask, serviceTask, attachmentTask,
                reviewScoreTask, imagesTask, inventoryMessageTask, shippingPostalCodeTask, localPickupCostTask,
                relatedProductsTask, alsoBoughtTask);

            _assemblies = assemblyTask.Result;
            _reviews = reviewTask.Result;
            _applications = applicationTask.Result;
            _types = typeTask.Result;
            _services = serviceTask.Result;
            _images = imagesTask.Result;
            _inventoryMessage = inventoryMessageTask.Result;
            _shippingPostalCode = shippingPostalCodeTask.Result;
            _localPickupCost = localPickupCostTask.Result;
            _relatedSuggestions = relatedProductsTask.Result;
            _alsoBoughtSuggestions = alsoBoughtTask.Result;

            // Separate attachments and videos
            var allAttachments = attachmentTask.Result;
            _attachments = allAttachments.Where(a => !a.IsVideo).ToList();
            _videos = allAttachments.Where(a => a.IsVideo).ToList();

            _reviewScore = reviewScoreTask.Result;
        }
        catch (Exception ex)
        {
            _errorMessage = "An error occurred while loading the product.";
            System.Diagnostics.Debug.WriteLine($"Product detail load error: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Extract the full querystring from the current URL for passing to guided navigation sproc.
    /// </summary>
    private async Task ExtractQueryStringAsync()
    {
        try
        {
            var uri = new Uri(Navigation.Uri);
            _queryString = uri.Query.StartsWith("?") ? uri.Query.Substring(1) : uri.Query ?? string.Empty;

            await LogService.LogInformation(
                $"PDP: Extracted querystring: {_queryString}",
                "Product");
        }
        catch (Exception ex)
        {
            await LogService.LogError(
                $"PDP: Error extracting querystring: {ex.Message}",
                "Product",
                ex.StackTrace);
        }
    }

    /// <summary>
    /// Parse fitment from querystring and interpret to resolve ukeys.
    /// Querystring format: ?makeName=volvo&modelYear=2021&modelName=xc40-20l-4-cylinder-turbo
    /// </summary>
    private async Task ParseAndInterpretFitmentAsync()
    {
        try
        {
            // Extract querystring parameters (manual parsing for WASM compatibility)
            var uri = new Uri(Navigation.Uri);
            var query = uri.Query;

            if (string.IsNullOrEmpty(query))
            {
                // No querystring, use FitmentState
                await LogService.LogInformation(
                    $"PDP: No querystring, using FitmentState (Model={FitmentState.UkeyModel}, Year={FitmentState.Year})",
                    "Product");
                return;
            }

            // Parse querystring manually: query starts with '?'
            var parameters = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (query.StartsWith("?"))
            {
                query = query.Substring(1);
            }

            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = Uri.UnescapeDataString(parts[1]);
                    parameters[key] = value;
                }
            }

            var makeName = parameters.ContainsKey("makeName") ? parameters["makeName"] : null;
            var modelYearStr = parameters.ContainsKey("modelYear") ? parameters["modelYear"] : null;
            var modelName = parameters.ContainsKey("modelName") ? parameters["modelName"] : null;

            if (string.IsNullOrWhiteSpace(makeName) || string.IsNullOrWhiteSpace(modelName))
            {
                // No fitment in querystring, use FitmentState
                await LogService.LogInformation(
                    $"PDP: No fitment in querystring, using FitmentState (Model={FitmentState.UkeyModel}, Year={FitmentState.Year})",
                    "Product");
                return;
            }

            var modelYearStr2 = modelYearStr ?? string.Empty;
            if (!int.TryParse(modelYearStr2, out var modelYearInt))
            {
                modelYearInt = 0; // Allow 0 as valid for no-year fitments
            }

            await LogService.LogInformation(
                $"PDP: Fitment from querystring: makeName={makeName}, modelYear={modelYearInt}, modelName={modelName}",
                "Product");

            // Call interpret to resolve the ukeys
            var interpretRequest = new InterpretSearchRequest
            {
                SessionId = SessionState.SessionId,
                MachineId = SessionState.MachineId,
                UkeyWebsite = WebsiteContext.UkeyWebsite,
                MakeName = makeName,
                ModelYear = modelYearInt.ToString(),
                ModelName = modelName
            };

            var interpretResult = await VehicleService.GetInterpretationAsync(interpretRequest);

            if (interpretResult != null)
            {
                _ukeyMake = interpretResult.UkeyMake > 0 ? interpretResult.UkeyMake : null;
                _ukeyModel = interpretResult.UkeyModel > 0 ? interpretResult.UkeyModel : null;
                _modelYear = !string.IsNullOrEmpty(interpretResult.ModelYear) && int.TryParse(interpretResult.ModelYear, out var year) && year > 0 ? year : null;
                _ukeyDriveline = interpretResult.UkeyDriveLine > 0 ? interpretResult.UkeyDriveLine : null;
                _ukeyTrimLevel = interpretResult.UkeyTrimLevel > 0 ? interpretResult.UkeyTrimLevel : null;

                await LogService.LogInformation(
                    $"PDP: Interpreted fitment: Model={_ukeyModel}, Year={_modelYear}, Driveline={_ukeyDriveline}, Trim={_ukeyTrimLevel}",
                    "Product");
            }
            else
            {
                await LogService.LogInformation(
                    "PDP: Interpret returned null for querystring fitment",
                    "Product");
            }
        }
        catch (Exception ex)
        {
            await LogService.LogError(
                $"PDP: Error parsing/interpreting fitment from querystring: {ex.Message}",
                "Product",
                ex.StackTrace);
        }
    }

    // ─── Identity ──────────────────────────────────────────────────────────
    private string PartTitle => _productDetail?.ProductHeading ?? "Part Title";
    private string PartNumber => _productDetail?.StockCode ?? "T99C5-4RA0A";
    private string VehicleDescription => FitmentState.HasVehicle || _modelYear.HasValue ? $"{_modelYear ?? FitmentState.Year}" : "Year Make Model Driveline Trim";

    private int ViewCount => _productDetail?.TimesViewed ?? 303;
    private int VideoCount => 2; // TODO: wire from Pr_ReturnProductAttachments (IsVideo=true)
    private int ReviewCount => _reviewScore?.ReviewCount ?? 0;
    private decimal Rating => _reviewScore?.AverageRating ?? 0m;

    // ─── Pricing / buy box ─────────────────────────────────────────────────
    // Dealer site: one selling dealer, one price. "Your Price" is this dealer's, shown
    // against the manufacturer's MSRP.
    private decimal Price => _productDetail?.Price ?? 86.40m;
    private decimal Msrp => _productDetail?.ListPrice ?? 99.99m;

    // "All Discounts {savingsString_withPercent}" — mirrors legacy's PriceHeaderQuadrantComponent
    // (PriceBox.ascx.cs: SavingsString = $"{Resources.Global.AllDiscounts} {ProductDetail.SavingsString_withPercent}").
    // Legacy's separate DiscountName tag (a named-sale badge + icon, shown above this line in
    // legacy's DOM order) isn't wired here — this is the savings line only, placed under the
    // dealer rating per this app's own layout.
    private string? SavingsText =>
        _productDetail?.SavingsStringWithPercent is { Length: > 0 } savings
            ? $"All Discounts {HtmlHelper.StripHtml(savings)}"
            : null;

    // Level mirrors InventoryMessage.Level 1:1 by design (see that type's doc comment) —
    // a plain switch, no guessing involved here.
    private AvailabilityLevel Availability => _inventoryMessage?.Level switch
    {
        InventoryLevel.InStock => AvailabilityLevel.InStock,
        InventoryLevel.LimitedStock => AvailabilityLevel.LimitedStock,
        _ => AvailabilityLevel.AvailableForOrder
    };

    // Empty MessageText means "no pill" per InventoryMessage's contract, but the price box
    // always renders an availability line, so fall back to the orderable default.
    private string AvailabilityText =>
        _inventoryMessage?.MessageText is { Length: > 0 } text ? text : "Available for Order";

    private int Quantity { get; set; } = 1;

    // Deliver/Pick Up render side by side rather than as a toggle (unlike legacy's single
    // switched quadrant), so both variant messages show at once — MessageTextShipping here,
    // MessageTextLocalPickup in Pickup below.
    private DeliveryOption Delivery => new(
        ZipCode: _shippingPostalCode ?? string.Empty,
        Promise: _inventoryMessage?.MessageTextShipping is { Length: > 0 } shipMsg
            ? shipMsg
            : "Most orders ship in 2-4 business days");

    private PickupOption Pickup => new(
        Promise: _inventoryMessage?.MessageTextLocalPickup is { Length: > 0 } pickupMsg
            ? pickupMsg
            : "Most orders ready in 2-4 business days",
        CostText: _localPickupCost is > 0 ? _localPickupCost.Value.ToString("C") : "FREE");

    private string? ShippingEstimateText => _shippingEstimate switch
    {
        null => null,
        { IsTBD: true } => "Shipping cost: TBD",
        { ShippingDifference: 0 } e when !string.IsNullOrEmpty(e.Disclaimer) => e.Disclaimer,
        { } e => $"Estimated shipping: {e.ShippingDifference:C}"
    };

    // ─── Fulfillment box actions ────────────────────────────────────────────
    private async Task EstimateShippingAsync()
    {
        if (_productDetail is null || _isEstimatingShipping) return;

        _isEstimatingShipping = true;
        try
        {
            _shippingEstimate = await FulfillmentService.GetEstimatedShippingCostAsync(
                new GetEstimatedShippingCostRequest
                {
                    SessionId = SessionState.SessionId,
                    MachineId = SessionState.MachineId,
                    UkeyWebsite = WebsiteContext.UkeyWebsite,
                    UkeyProduct = _productDetail.UkeyProduct,
                    Quantity = Quantity
                });
        }
        finally
        {
            _isEstimatingShipping = false;
        }
    }

    private async Task UpdateZipCodeAsync(string newZip)
    {
        await FulfillmentService.UpdateShippingPostalCodeAsync(new UpdateShippingPostalCodeRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            PostalCode = newZip
        });

        _shippingPostalCode = newZip;
        _shippingEstimate = null; // a ZIP change invalidates any previous estimate
    }

    // ─── Selling dealer ────────────────────────────────────────────────────
    private DealerInfo Dealer =>
        _websiteSettings is not null
            ? new DealerInfo(
                Name: _websiteSettings.BusinessName,
                AddressLine1: _websiteSettings.DealerStreet,
                AddressLine2: $"{_websiteSettings.DealerCity}, {_websiteSettings.DealerState} {_websiteSettings.DealerZip}".Trim(),
                Phone: _websiteSettings.PhoneNumber,
                Email: _websiteSettings.SalesEmail,
                Rating: 3.5m) // TODO: wire to dealer rating from database when available
            : new DealerInfo(
                Name: "Dealer",
                AddressLine1: string.Empty,
                AddressLine2: string.Empty,
                Phone: string.Empty,
                Email: string.Empty,
                Rating: 0m);

    // Image gallery: main product images from pr_returnProductImages. If there are no
    // images, fall back to placeholders. Twelve entries so the strip shows nine thumbnails
    // plus the "3 More" tile, matching the mockup.
    private IReadOnlyList<string> GalleryImages =>
        _images is { } images && images.Any()
            ? images
                .Select(img => img.ImageURL)
                .Concat(Enumerable.Repeat(string.Empty, Math.Max(0, 12 - images.Count())))
                .Take(12)
                .ToList()
            : Enumerable.Repeat(string.Empty, 12).ToList();

    // ─── Description ───────────────────────────────────────────────────────
    // Tags come from pr_returnProductTags — the Prop 65 warning is a row, not a constant,
    // so the placeholder models it as one rather than as a special case.
    private static readonly IReadOnlyList<ProductTag> Tags =
    [
        new ProductTag
        {
            TagTitle = "California Specific Warning",
            Description = "WARNING: Cancer and Reproductive Harm - www.P65Warnings.ca.gov",
            TagType = "warning"
        }
    ];

    private static readonly IReadOnlyList<string> Features =
    [
        "Designed to help organize cargo",
        "Helps prevent cargo from sliding around with super-grippy underside",
        "Requires Carpeted Cargo Area Protector"
    ];

    private const string ApplicabilityNote = "All w/ Carpeted Cargo Area Mat/Protector";

    private static readonly IReadOnlyList<string> Supersessions = ["T99C54RA0A"];

    // ─── Cart operations ──────────────────────────────────────────────────
    // The price box's own Add to Cart — the quantity the customer set on the stepper,
    // rather than the fixed qty=1 the related-product tiles below always add.
    private async Task AddProductToCartAsync()
    {
        if (_productDetail is null) return;

        try
        {
            await CartService.AddToCartAsync(new AddToCartRequest
            {
                UkeyProduct = _productDetail.UkeyProduct,
                Quantity = Quantity
            });
            await CartState.RefreshCountAsync();
            // TODO: Show success message or toast notification
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Add to cart error: {ex.Message}");
            // TODO: Show error message
        }
    }

    private async Task HandleAddToCart(RelatedProduct product)
    {
        try
        {
            await CartService.AddToCartAsync(new AddToCartRequest
            {
                UkeyProduct = product.UkeyProduct,
                Quantity = 1  // Add 1 of the recommended product
            });
            // Refresh cart count to update CartWidget
            await CartState.RefreshCountAsync();
            // TODO: Show success message or toast notification
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Add to cart error: {ex.Message}");
            // TODO: Show error message
        }
    }

    // ─── Collapsible section stack ─────────────────────────────────────────
    private IReadOnlyList<ProductSection> DetailSections =>
    [
        new ProductSection("Diagrams & Kits", $"{_assemblies?.Count() ?? 0} Assemblies"),
        new ProductSection("Product Reviews", $"{ReviewCount} Reviews", ShowRating: true),
        new ProductSection("What This Fits", $"{_applications?.Count() ?? 0} Vehicles"),
        new ProductSection("Product Types", $"{_types?.Count() ?? 0} Product Types"),
        new ProductSection("Services", $"{_services?.Count() ?? 0} Services"),
        new ProductSection("Attachments", $"{_attachments?.Count() ?? 0} Attachments"),
        new ProductSection("Videos", $"{_videos?.Count() ?? 0} Videos"),
        new ProductSection("Share Product", "Facebook, Twitter, Instagram"),
        new ProductSection("Ask a Question", "Get an Answer")
    ];

    // ─── Cross-sell ────────────────────────────────────────────────────────
    [PersistentState] public IEnumerable<SuggestedProduct>? _relatedSuggestions { get; set; }
    [PersistentState] public IEnumerable<SuggestedProduct>? _alsoBoughtSuggestions { get; set; }

    private IReadOnlyList<RelatedProduct> RelatedItems =>
        (_relatedSuggestions ?? [])
            .Select(p => new RelatedProduct(
                Title: p.ProductDescription,
                StockCode: p.StockCode,
                Price: p.Price,
                UkeyProduct: p.Ukey,
                ListPrice: p.ListPrice > 0 ? p.ListPrice : null,
                SavingsText: p.ListPrice > 0 ? $"${(p.ListPrice - p.Price):F2}" : null,
                Category: p.CategoryList,
                ImageUrl: p.ImageURL,
                Url: p.ProductURL))
            .ToList();

    private IReadOnlyList<RelatedProduct> AlsoBoughtItems =>
        (_alsoBoughtSuggestions ?? [])
            .Select(p => new RelatedProduct(
                Title: p.ProductDescription,
                StockCode: p.StockCode,
                Price: p.Price,
                UkeyProduct: p.Ukey,
                ListPrice: p.ListPrice > 0 ? p.ListPrice : null,
                SavingsText: p.ListPrice > 0 ? $"${(p.ListPrice - p.Price):F2}" : null,
                Category: p.CategoryList,
                ImageUrl: p.ImageURL,
                Url: p.ProductURL))
            .ToList();
}
