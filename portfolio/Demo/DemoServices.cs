using Microsoft.Extensions.Logging;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.Utilities;
using static Portfolio.Demo.DemoCatalog;

namespace Portfolio.Demo;

// In-memory implementations of every storefront service the client components inject.
// They mimic the shapes/behaviour described in the client code comments; none of this talks to
// a database or network.

// ─── Context ──────────────────────────────────────────────────────────────

public sealed class DemoWebsiteContext : IWebsiteContextProvider
{
    public int UkeyWebsite => DemoCatalog.UkeyWebsite;
}

public sealed class DemoSessionState : ISessionStateProvider
{
    public string SessionId { get; } = Guid.NewGuid().ToString("N");
    public string MachineId => "demo-machine";
}

public sealed class DemoLogService(ILogger<DemoLogService> logger) : ILogService
{
    public Task LogInformation(string message, string source)
    {
        logger.LogDebug("[{Source}] {Message}", source, message);
        return Task.CompletedTask;
    }

    public Task LogError(string message, string source, string? stackTrace = null)
    {
        logger.LogError("[{Source}] {Message}\n{StackTrace}", source, message, stackTrace);
        return Task.CompletedTask;
    }
}

public sealed class DemoAssetResolver : IClientAssetResolver
{
    // No server in WebAssembly: icons are generated client-side as SVG data URIs.
    public string Resolve(string path) => DemoImages.IconUrl(path);
}

// ─── Refine search / product search ───────────────────────────────────────

public sealed class DemoProductSearchService : IProductSearchService
{
    public async Task<IEnumerable<RefineSearchResult>> GetRefinedSearchAsync(RefineSearchRequest r)
    {
        await Task.Delay(80);
        return (r.ShowFitmentLevel ?? string.Empty).ToLowerInvariant() switch
        {
            "category" => CategoryTiles(r, accessory: false),
            "vehicleaccessorycategories" => CategoryTiles(r, accessory: true),
            "year" => YearRows(r),
            "model" => ModelRows(r, r.Year ?? Years[0]),
            "driveline" => DrivelineRows(r),
            "trimlevel" => TrimRows(r, r.UkeyDriveline),
            _ => LinkRows(r)
        };
    }

    public async Task<IEnumerable<ProductSearchResult>> GetSearchResultsAsync(ProductSearchRequest r)
    {
        await Task.Delay(SimulatedLatencyMs);

        IEnumerable<DemoProduct> q = Products;
        if (r.UkeyCategory > 0)
            q = q.Where(p => p.UkeyCategory == r.UkeyCategory);

        var words = (r.SearchString ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length > 0)
            q = q.Where(p => words.Any(w =>
                p.Description.Contains(w, StringComparison.OrdinalIgnoreCase)
                || p.StockCode.Contains(w, StringComparison.OrdinalIgnoreCase)
                || p.Category.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));

        q = r.SortOrder switch
        {
            "Description" => q.OrderBy(p => p.Description),
            "Part Number" => q.OrderBy(p => p.StockCode),
            "Price (Low to High)" => q.OrderBy(p => p.Price),
            "Price (High to Low)" => q.OrderByDescending(p => p.Price),
            _ => q
        };

        if (r.NumResults > 0) q = q.Take(r.NumResults);
        return q.Select(ToSearchResult).ToList();
    }

    public async Task<IEnumerable<SearchAssembly>> GetSearchAssembliesAsync(GetSearchAssembliesRequest request)
    {
        await Task.Delay(80);
        var brakes = CategoryByUkey(11)!;
        return brakes.Assemblies.Select(a => new SearchAssembly
        {
            AssemblyName = a,
            AssemblyDescription = a,
            AssemblyURL = BuildUrl(false, null, null, null, null, brakes.Ukey),
            ThumbnailURL = DemoImages.Url("diagram", a, brakes.Color, 320, 240)
        }).ToList();
    }

    public static ProductSearchResult ToSearchResult(DemoProduct p) => new()
    {
        Ukey = p.Ukey,
        ProductDescription = p.Description,
        StockCode = p.StockCode,
        Price = p.Price,
        ProductURL = p.Url,
        ThumbnailURL = p.Image(),
        // Legacy ships these with markup; the client strips it (HtmlHelper.StripHtml).
        CategoryList = $"<a href=\"{BuildUrl(p.Category.IsAccessory, null, null, null, null, p.UkeyCategory)}\">{p.Category.Name}</a>",
        AdditionalDescription = p.Blurb,
        AssemblyList = $"<span>{p.Category.Assemblies[0]}</span>"
    };

    private static IEnumerable<RefineSearchResult> CategoryTiles(RefineSearchRequest r, bool accessory) =>
        Categories.Where(c => c.IsAccessory == accessory).Select(c => new RefineSearchResult
        {
            Ukey = c.Ukey,
            SimpleString = c.Name,
            DisplayString = c.Name,
            ClassName = "category",
            ExtendedDescription = accessory ? null : c.Description,
            ImageURL = DemoImages.Url("tile", c.Name, c.Color, 480, 360),
            LinkURL = BuildUrl(false, r.Year, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel, c.Ukey),
            AccessoryLinkURL = BuildUrl(true, r.Year, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel, c.Ukey)
        }).ToList();

    private static IEnumerable<RefineSearchResult> YearRows(RefineSearchRequest r) =>
        Years.Select(y => new RefineSearchResult
        {
            Ukey = y,
            SimpleString = y.ToString(),
            DisplayString = y.ToString(),
            ClassName = "year",
            LinkURL = r.UkeyModel is > 0 ? BuildUrl(false, y, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel, r.UkeyCategory) : null,
            AccessoryLinkURL = r.UkeyModel is > 0 ? BuildUrl(true, y, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel, r.UkeyCategory) : null
        }).ToList();

    private static IEnumerable<RefineSearchResult> ModelRows(RefineSearchRequest r, int year) =>
        Models.Select(m => new RefineSearchResult
        {
            Ukey = m.Ukey,
            SimpleString = m.Name,
            DisplayString = m.Name,
            ClassName = "model",
            ImageURL = CarImage(m.Ukey, 240, 135),
            LinkURL = BuildUrl(false, year, m.Ukey, null, null, r.UkeyCategory),
            AccessoryLinkURL = BuildUrl(true, year, m.Ukey, null, null, r.UkeyCategory)
        }).ToList();

    private static IEnumerable<RefineSearchResult> DrivelineRows(RefineSearchRequest r)
    {
        var model = ModelByUkey(r.UkeyModel);
        if (model is null) return [];
        var year = r.Year ?? Years[0];
        return model.Drivelines.Select(d => DrivelineByUkey(d)!).Select(d => new RefineSearchResult
        {
            Ukey = d.Ukey,
            SimpleString = d.Name,
            DisplayString = d.Name,
            ClassName = "driveline",
            LinkURL = BuildUrl(false, year, model.Ukey, d.Ukey, null, r.UkeyCategory),
            AccessoryLinkURL = BuildUrl(true, year, model.Ukey, d.Ukey, null, r.UkeyCategory)
        }).ToList();
    }

    private static IEnumerable<RefineSearchResult> TrimRows(RefineSearchRequest r, int? driveline)
    {
        var model = ModelByUkey(r.UkeyModel);
        if (model is null) return [];
        var year = r.Year ?? Years[0];
        return Trims.Select(t => new RefineSearchResult
        {
            Ukey = t.Ukey,
            SimpleString = t.Name,
            DisplayString = t.Name,
            ClassName = "trimlevel",
            LinkURL = BuildUrl(false, year, model.Ukey, driveline, t.Ukey, r.UkeyCategory),
            AccessoryLinkURL = BuildUrl(true, year, model.Ukey, driveline, t.Ukey, r.UkeyCategory)
        }).ToList();
    }

    // Links mode (no explicit level): refine one step deeper than the current fitment —
    // no vehicle → models, vehicle without trim → trims, full vehicle → other years.
    private static IEnumerable<RefineSearchResult> LinkRows(RefineSearchRequest r)
    {
        if (r.UkeyModel is not > 0)
            return ModelRows(r, r.Year ?? Years[0]);
        if (r.UkeyTrimLevel is not > 0)
            return TrimRows(r, r.UkeyDriveline ?? ModelByUkey(r.UkeyModel)?.Drivelines.First());
        return YearRows(r);
    }
}

public sealed class DemoCategoryService : ICategoryService
{
    public Task<IEnumerable<Category>> GetCategoriesAsync(GetCategoriesRequest request) =>
        Task.FromResult<IEnumerable<Category>>(Categories
            .Select(c => new Category { Ukey = c.Ukey, CategoryName = c.Name })
            .ToList());

    public Task<string> GetCategoryNameAsync(GetCategoryNameRequest request) =>
        Task.FromResult(CategoryByUkey(request.UkeyCategory)?.Name ?? "Category");
}

// ─── Vehicle ──────────────────────────────────────────────────────────────

public sealed class DemoVehicleService : IVehicleService
{
    public Task<InterpretSearchResult> GetInterpretationAsync(InterpretSearchRequest r)
    {
        // The dealer's make always resolves, mirroring legacy.
        var result = new InterpretSearchResult
        {
            UkeyMake = MakeUkey,
            UkeyModel = r.UkeyModel,
            ModelYear = r.ModelYear ?? string.Empty,
            UkeyDriveLine = r.UkeyDriveLine,
            UkeyTrimLevel = r.UkeyTrimLevel,
            UkeyCategory = r.UkeyCategory,
            SearchTerm = string.IsNullOrWhiteSpace(r.SearchString) ? null : r.SearchString
        };

        if (!string.IsNullOrWhiteSpace(r.CategoryName))
            result.UkeyCategory = CategoryBySlug(r.CategoryName)?.Ukey ?? 0;

        // Compound model slug from the URL, e.g. "xc40-awd-plus".
        if (!string.IsNullOrWhiteSpace(r.ModelName))
        {
            var tokens = r.ModelName.ToLowerInvariant().Split('-', StringSplitOptions.RemoveEmptyEntries);
            var model = Models.FirstOrDefault(m => tokens.Contains(m.Slug));
            result.UkeyModel = model?.Ukey ?? 0;
            result.UkeyDriveLine = Drivelines.FirstOrDefault(d => tokens.Contains(d.Slug))?.Ukey ?? 0;
            result.UkeyTrimLevel = Trims.FirstOrDefault(t => tokens.Contains(t.Slug))?.Ukey ?? 0;
            result.ModelYear = int.TryParse(r.ModelYear, out var y) && Years.Contains(y) ? y.ToString() : string.Empty;
        }

        return Task.FromResult(result);
    }

    public Task<string> GetFitmentStringAsync(GetFitmentStringRequest r) =>
        Task.FromResult(FitmentString(
            int.TryParse(r.ModelYear, out var y) ? y : null,
            r.UkeyModel, r.UkeyDriveLine, r.UkeyTrimLevel));

    // Refine sprocs return a single "(x) remove" row when the level is locked in.
    private static string Locked(string name) => $"{name} <span style=\"color:blue\"> (x)</span>";

    public Task<IEnumerable<RefineLink>> GetModelRefineLinksAsync(GetRefineLinksRequest r) =>
        Links(ModelByUkey(r.UkeyModel) is { } m ? [Locked(m.Name)] : Models.Select(x => x.Name));

    public Task<IEnumerable<RefineLink>> GetYearRefineLinksAsync(GetRefineLinksRequest r) =>
        Links(!string.IsNullOrWhiteSpace(r.ModelYear) && r.ModelYear != "0" ? [Locked(r.ModelYear)] : Years.Select(y => y.ToString()));

    public Task<IEnumerable<RefineLink>> GetTrimLevelRefineLinksAsync(GetRefineLinksRequest r) =>
        Links(TrimByUkey(r.UkeyTrimLevel) is { } t ? [Locked(t.Name)] : r.UkeyModel > 0 ? Trims.Select(x => x.Name) : []);

    public Task<IEnumerable<RefineLink>> GetDrivelineRefineLinksAsync(GetRefineLinksRequest r) =>
        Links(DrivelineByUkey(r.UkeyDriveLine) is { } d ? [Locked(d.Name)] : r.UkeyModel > 0 ? Drivelines.Select(x => x.Name) : []);

    private static Task<IEnumerable<RefineLink>> Links(IEnumerable<string> names) =>
        Task.FromResult<IEnumerable<RefineLink>>(names.Select(n => new RefineLink { DisplayString = n }).ToList());
}

// ─── Placements / settings / banner / page heading ────────────────────────

public sealed class DemoPlacementService : IPlacementService
{
    public async Task<PlacementText?> GetAsync(GetPlacementRequest r)
    {
        await Task.Delay(60);

        var year = int.TryParse(r.ModelYear, out var y) && y > 0 ? y : (int?)null;
        var model = ModelByUkey(r.UkeyModel);
        var vehicle = model is not null && year is not null ? FitmentString(year, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel) : null;

        return r.Key switch
        {
            "HeroArea" => new PlacementText
            {
                Title = vehicle is null ? "Genuine Volvo Parts &amp; Accessories" : $"Genuine Parts for your {vehicle}",
                IntroTextSubTitle = "Shipped direct from your local dealer",
                Text = "<p>Every part is backed by the manufacturer's warranty and fits exactly like the original.</p>",
                ImageUrl = DemoImages.Url("hero", string.Empty, model?.Color ?? "113458", 1600, 480),
                TargetUrl = "/accessories",
                DisplayString = "Shop Accessories"
            },
            "AccessoriesHeroArea" => new PlacementText
            {
                Title = "Accessories",
                IntroTextSubTitle = "Personalise and protect your Volvo",
                ImageUrl = DemoImages.Url("hero", string.Empty, "34495e", 1600, 420)
            },
            "VehiclePickerHeading" or "AccessoriesVehiclePickerHeading" => vehicle is null
                ? new PlacementText { IntroTextTitle = "Choose your Vehicle to start shopping:" }
                : new PlacementText
                {
                    IntroTextTitle = $"Your {vehicle}",
                    Text = "<p>Showing parts and accessories that fit this vehicle. Categories, assemblies and links below are filtered to match.</p>",
                    ImageUrl = CarImage(r.UkeyModel, 480, 270)
                },
            "CategoryHeader" => new PlacementText { IntroTextTitle = vehicle is null ? "Shop Parts by Category" : $"Shop {model!.Name} Parts by Category" },
            "Accessories" => new PlacementText { IntroTextTitle = "Shop Accessories by Category" },
            "AccessoriesCategoryHeader" => new PlacementText { IntroTextTitle = "Browse Accessory Categories" },
            "RefineSearchLinksHeader" => new PlacementText { IntroTextTitle = "Shop by" },
            "FeaturedDefault" => new PlacementText
            {
                IntroTextTitle = "<a href=\"/accessories\">View All Accessories</a>",
                Text = "Everything in one place",
                ImageUrl = DemoImages.Url("tile", "All Accessories", "113458", 480, 360)
            },
            "HomeAccessories" => new PlacementText
            {
                IntroTextTitle = "The Lifestyle Collection",
                Text = "<p>Apparel, drinkware and travel gear designed in Sweden.</p>",
                ImageUrl = DemoImages.Url("promo", "Lifestyle", "8e3b46", 1200, 400),
                TargetUrl = "/lifestyle"
            },
            "ExtraDefaultContent" => new PlacementText
            {
                IntroTextTitle = "Why buy genuine?",
                Text = "<p>Genuine parts are engineered for your vehicle's safety systems and tested to the same standard as the parts it left the factory with.</p><p><a href=\"/testimonials\">Read what our customers say</a></p>",
                ImageUrl = DemoImages.Url("promo", "Genuine Parts", "2f6f5f", 900, 500)
            },
            "HomeFeaturedAccessories" => new PlacementText { IntroTextTitle = "Featured Parts and Accessories" },
            _ => null
        };
    }
}

public sealed class DemoWebsiteSettingsService : IWebsiteSettingsService
{
    // Fictional dealer. Phone uses the 555-01xx fictional range; email uses example.com.
    private static readonly WebsiteSettingsDto Settings = new()
    {
        BusinessName = "Demo Motors of Springfield",
        ManufacturerName = MakeName,
        LogoUrl = DemoImages.Url("logo", "VOLVO", "113458", 220, 48),
        FormattedAddress = "100 Example Way, Springfield, IL 62701",
        DealerStreet = "100 Example Way",
        DealerCity = "Springfield",
        DealerState = "IL",
        DealerZip = "62701",
        PhoneNumber = "555.010.0199",
        SalesEmail = "parts@example.com",
        WebsiteUrl = "demo-parts.example.com",
        PoliciesLink = "/policy",
        PriceBoxMessage = "Prices shown are <strong>dealer prices</strong> and may differ from MSRP.",
        ShowAssemblySearch = true,
        ShowPerformanceSuggestions = true,
        ShowAccessorySuggestions = true
    };

    public Task<WebsiteSettingsDto> GetSettingsAsync(int ukeyWebsite, string sessionId, string machineId) =>
        Task.FromResult(Settings);
}

public sealed class DemoBannerService : IBannerService
{
    public Task<Banner?> GetAsync(GetBannerRequest request) =>
        Task.FromResult<Banner?>(new Banner
        {
            BannerMode = "mainNav",
            BannerTitle = "<strong>Free shipping</strong> on accessory orders over $99",
            BannerText = "Demo banner — dismiss it and it stays hidden (localStorage).",
            BannerTarget = "/accessories",
            DismissalKey = "demo-banner-1"
        });
}

public sealed class DemoPageTrackingService : IPageTrackingService
{
    public Task<PageHeadingResponse> GetPageHeadingAsync(PageHeadingRequest request) =>
        Task.FromResult(new PageHeadingResponse { H1TagText = "Genuine Volvo Parts Online" });
}

public sealed class DemoLandingService : ILandingService
{
    public Task<LandingPageConfig?> GetLandingBySlugAsync(string slug) =>
        Task.FromResult<LandingPageConfig?>(slug?.ToLowerInvariant() switch
        {
            "brake-service" => new LandingPageConfig
            {
                Title = "Brake Service Essentials",
                Subtitle = "Pads, rotors and fluid for a complete brake job.",
                UkeyCategory = 11,
                UkeyMake = MakeUkey
            },
            "cargo-solutions" => new LandingPageConfig
            {
                Title = "Cargo Solutions",
                Subtitle = "Keep the load floor tidy and protected.",
                UkeyCategory = 51,
                UkeyMake = MakeUkey
            },
            _ => null
        });
}

// ─── Home: assemblies / featured / reviews ────────────────────────────────

public sealed class DemoHomeService : IHomeService
{
    public async Task<GetAssemblyResultsResponse?> GetAssemblyResultsAsync(GetAssemblyResultsRequest r)
    {
        await Task.Delay(SimulatedLatencyMs);
        var category = CategoryByUkey(r.UkeyCategory);
        return category is null ? null : Build(category);
    }

    public static GetAssemblyResultsResponse Build(DemoCategory category) => new()
    {
        CategoryName = category.Name,
        Assemblies = category.Assemblies.Select((name, i) => new AssemblyResult
        {
            AssemblyName = name,
            AssemblyDescription = $"Exploded diagram of the {name.ToLowerInvariant()} with every serviceable component called out. " +
                                  "Click through to see part numbers, quantities and prices for each numbered callout, and add parts straight to your cart.",
            ShowAssemblyURL = "#",
            HoverText = $"View {name} diagram",
            ImageURL = DemoImages.Url("diagram", name, Shade(category.Color, i), 400, 300),
            Keywords = $"{name} {category.Name}"
        }).ToList(),
        PartTypes = Products.Where(p => p.UkeyCategory == category.Ukey).Select(p => new CategoryPartType
        {
            TypeName = p.Description,
            CleanTypeName = p.Description,
            Keywords = $"{p.Description} {p.StockCode}",
            PageURL = p.Url,
            ImageURL = p.Image(),
            MinPrice = p.Price,
            MaxPrice = p.ListPrice,
            NumProducts = 1 + (int)(p.Ukey % 4)
        }).ToList()
    };
}

public sealed class DemoFeaturedProductsService : IFeaturedProductsService
{
    public async Task<IEnumerable<FeaturedProduct>> GetAsync(GetFeaturedProductsRequest r)
    {
        await Task.Delay(80);
        return Products.Where(p => p.Category.IsAccessory)
            .Take(r.ReturnNum > 0 ? r.ReturnNum : 6)
            .Select(p => new FeaturedProduct
            {
                UkeyProduct = p.Ukey,
                ProductDescription = p.Description,
                StockCode = p.StockCode,
                Price = p.Price,
                ProductURL = p.Url,
                FullsizeURL = p.Image(0, 600),
                ThumbnailURL = p.Image()
            }).ToList();
    }
}

public sealed class DemoReviewsService : IReviewsService
{
    private static readonly Review[] Testimonials =
    [
        new() { Rating = "5/5", DateString = "Sep 12, 2026", Testimonial = "Parts arrived two days after ordering and fit perfectly.", Name = "Sam K.", Location = "Springfield, IL" },
        new() { Rating = "4.5/5", DateString = "Aug 30, 2026", Testimonial = "Great prices compared to the counter, and the diagrams made it easy to find the right bolt.", Name = "Jordan P.", Location = "Decatur, IL" },
        new() { Rating = "5/5", DateString = "Aug 18, 2026", Testimonial = "<p>Ordered floor mats and a cargo organiser. Quality is excellent.</p>", Name = "Riley M.", Location = "Peoria, IL" },
        new() { Rating = "4/5", DateString = "Jul 29, 2026", Testimonial = "Shipping took a little longer than expected but support kept me updated.", Name = "Casey T." },
        new() { Rating = "5/5", DateString = "Jul 03, 2026", Testimonial = "Easy to use the vehicle picker — everything shown actually fit my car.", Name = "Morgan L.", Location = "Champaign, IL" },
        new() { Rating = "3.5/5", DateString = "Jun 15, 2026", Testimonial = "Good experience overall; would like more photos on some products.", Name = "Taylor B." },
    ];

    public async Task<IEnumerable<Review>> GetTestimonialsAsync(GetTestimonialsRequest r)
    {
        await Task.Delay(80);
        return r.ReturnAll ? Testimonials : Testimonials.Take(r.NumReviews > 0 ? r.NumReviews : 3);
    }

    public Task<AverageReviewsResult?> GetAverageReviewsAsync(int ukeyWebsite) =>
        Task.FromResult<AverageReviewsResult?>(new AverageReviewsResult { AverageRating = 4.7m, ReviewCount = 128 });
}

// ─── Product detail ───────────────────────────────────────────────────────

public sealed class DemoProductService : IProductService
{
    private static DemoProduct Resolve(long ukey, string? stock) =>
        ProductByUkey(ukey) ?? ProductByStockCode(stock) ?? Products[20];

    public async Task<ProductDetail?> GetDetailsAsync(GetProductDetailRequest r)
    {
        await Task.Delay(SimulatedLatencyMs);
        var p = ProductByStockCode(r.StockNumber);
        if (p is null) return null;

        var savings = p.ListPrice - p.Price;
        var pct = p.ListPrice > 0 ? (int)Math.Round(savings / p.ListPrice * 100) : 0;
        return new ProductDetail
        {
            UkeyProduct = p.Ukey,
            ProductHeading = p.Description,
            StockCode = p.StockCode,
            TimesViewed = 120 + (int)(p.Ukey % 400),
            Price = p.Price,
            ListPrice = p.ListPrice,
            SavingsStringWithPercent = $"<span class=\"savings\">{savings:C} ({pct}%)</span>"
        };
    }

    public Task<IEnumerable<ProductAssembly>> GetAssembliesAsync(GetProductAssembliesRequest r)
    {
        var p = Resolve(r.UkeyProduct, r.StockNumber);
        return Done<ProductAssembly>(p.Category.Assemblies.Take(2).Select((a, i) => new ProductAssembly
        {
            AssemblyName = a,
            AssemblyIndex = i + 1,
            AssemblyURL = "#",
            AssemblyImageURL = DemoImages.Url("diagram", a, p.Category.Color, 480, 320),
            AssemblyDescription = $"{p.Description} appears as callout {i + 3} in this diagram.",
            QuantityRequired = i + 1,
            ApplicationNotes = i == 0 ? "Replace in axle pairs." : null
        }));
    }

    public Task<IEnumerable<ProductReview>> GetReviewsAsync(GetProductReviewsRequest r) =>
        Done<ProductReview>(
        [
            new() { FirstName = "Alex", LastName = "R.", YMD = new DateTime(2026, 8, 14), ReviewScore = 5, ReviewText = "Exactly what I needed, fits perfectly." },
            new() { FirstName = "Jamie", LastName = "S.", YMD = new DateTime(2026, 7, 2), ReviewScore = 4, ReviewText = "Good quality, installation instructions could be clearer." },
            new() { FirstName = "Drew", LastName = "N.", YMD = new DateTime(2026, 5, 21), ReviewScore = 4, ReviewText = "Solid part and quick delivery." },
        ]);

    public Task<IEnumerable<ProductApplication>> GetApplicationsAsync(GetProductApplicationsRequest r) =>
        Done<ProductApplication>(Models.Select(m => new ProductApplication
        {
            Fitment = $"{MakeName} {m.Name}",
            ModelYears = $"{Years.Min()}-{Years.Max()}"
        }));

    public Task<IEnumerable<ProductType>> GetTypesAsync(GetProductTypesRequest r)
    {
        var p = Resolve(r.UkeyProduct, r.StockNumber);
        return Done<ProductType>(Products.Where(x => x.UkeyCategory == p.UkeyCategory && x.Ukey != p.Ukey).Take(3).Select(x => new ProductType
        {
            TypeName = x.Description,
            TypeDescription = x.Blurb,
            TargetURL = x.Url,
            ThumbnailURL = x.Image(0, 120)
        }));
    }

    public Task<IEnumerable<StorefrontProductService>> GetServicesAsync(GetProductServicesRequest r)
    {
        var p = Resolve(r.UkeyProduct, r.StockNumber);
        var kit = Products.Where(x => x.UkeyCategory == p.UkeyCategory).Take(2).ToList();
        return Done<StorefrontProductService>(
        [
            new()
            {
                AssemblyName = $"{p.Category.Name} Service",
                AssemblyDescription = $"Everything typically replaced during a {p.Category.Name.ToLowerInvariant()} service.",
                ServiceProducts = kit.Select(x => new ServiceProduct
                {
                    ProductDescription = x.Description,
                    StockCode = x.StockCode,
                    TargetURL = x.Url,
                    ThumbnailURL = x.Image(0, 120)
                }).ToList()
            }
        ]);
    }

    public Task<IEnumerable<ProductAttachment>> GetAttachmentsAsync(GetProductAttachmentsRequest r) =>
        Done<ProductAttachment>(
        [
            new() { AttachmentName = "Installation Instructions (PDF)", AttachmentUrl = "#", IsVideo = false },
            new() { AttachmentName = "Warranty Information (PDF)", AttachmentUrl = "#", IsVideo = false },
        ]);

    public Task<ProductReviewScore?> GetReviewScoreAsync(GetProductReviewScoreRequest r) =>
        Task.FromResult<ProductReviewScore?>(new ProductReviewScore { ReviewCount = 3, AverageRating = 4.3m });

    public Task<IEnumerable<ProductImage>> GetAdditionalImagesAsync(GetProductImagesRequest r)
    {
        var p = Resolve(r.UkeyProduct, r.StockNumber);
        return Done<ProductImage>(Enumerable.Range(0, 5).Select(i => new ProductImage
        {
            ImageName = $"{p.Description} view {i + 1}",
            ImageURL = p.Image(i, 800),
            RawThumbnailURL = p.Image(i, 96)
        }));
    }

    public Task<InventoryMessage?> GetInventoryMessageAsync(GetInventoryMessageRequest r) =>
        Task.FromResult<InventoryMessage?>(new InventoryMessage
        {
            Level = r.UkeyProduct % 3 == 0 ? InventoryLevel.LimitedStock : InventoryLevel.InStock,
            MessageText = r.UkeyProduct % 3 == 0 ? "Only 2 left in stock" : "In Stock",
            MessageTextShipping = "Ships in 1-2 business days",
            MessageTextLocalPickup = "Ready for pickup today"
        });

    public Task<IEnumerable<SuggestedProduct>> GetSuggestedProductsAsync(GetSuggestedProductsRequest r)
    {
        var p = Resolve(r.UkeyProduct, r.StockNumber);
        var pool = r.IsRelated
            ? Products.Where(x => x.Ukey != p.Ukey && x.Category.IsAccessory != p.Category.IsAccessory).Take(6)
            : Products.Where(x => x.Ukey != p.Ukey && x.UkeyCategory == p.UkeyCategory)
                .Concat(Products.Where(x => x.Ukey != p.Ukey && x.UkeyCategory != p.UkeyCategory))
                .Take(7);
        return Done<SuggestedProduct>(pool.Select(x => new SuggestedProduct
        {
            Ukey = x.Ukey,
            ProductDescription = x.Description,
            StockCode = x.StockCode,
            Price = x.Price,
            ListPrice = x.ListPrice,
            CategoryList = x.Category.Name,
            ImageURL = x.Image(),
            ProductURL = x.Url
        }));
    }

    private static Task<IEnumerable<T>> Done<T>(IEnumerable<T> items) => Task.FromResult<IEnumerable<T>>(items.ToList());
}

public sealed class DemoGuidedNavigationService : IGuidedNavigationService
{
    public async Task<(GuidedNavigationHeader? Header, GuidedNavigationDetailResponse? Detail)> GetBothAsync(GetGuidedNavigationRequest r)
    {
        await Task.Delay(80);
        var product = ProductByUkey(r.UkeyProduct);
        var hasVehicle = r.UkeyModel is > 0 && r.ModelYear is > 0;

        var header = hasVehicle
            ? new GuidedNavigationHeader
            {
                CssClass = "guided-navigation-fits",
                GuideHeader = $"This fits your {FitmentString(r.ModelYear, r.UkeyModel, r.UkeyDriveline, r.UkeyTrimLevel)}",
                GuideBody = "Fitment confirmed for your selected vehicle.",
                LinkUrl = "/",
                LinkAnchor = "Change vehicle",
                LinkHover = "Pick a different vehicle",
                ResultLabel1 = "Genuine Part",
                ResultLabel2 = "Direct Fit"
            }
            : new GuidedNavigationHeader
            {
                CssClass = "guided-navigation-unknown",
                GuideHeader = "Does this fit your vehicle?",
                GuideBody = "Pick one of the vehicles below to confirm fitment.",
                ResultLabel1 = "Genuine Part"
            };

        var detail = new GuidedNavigationDetailResponse
        {
            Details = product is null
                ? []
                : Models.Select(m => new GuidedNavigationDetail
                {
                    LinkAnchor = $"{Years[0]} {m.Name}",
                    LinkHover = $"Check fitment for the {Years[0]} {MakeName} {m.Name}",
                    LinkUrl = $"{product.Url}?makeName={MakeSlug}&modelYear={Years[0]}&modelName={m.Slug}"
                }).ToList()
        };

        return (header, detail);
    }
}

// ─── Fulfilment ───────────────────────────────────────────────────────────

public sealed class DemoFulfillmentService : IFulfillmentService
{
    private string _postalCode = "62701";

    public Task<string?> GetShippingPostalCodeAsync(GetShippingPostalCodeRequest request) =>
        Task.FromResult<string?>(_postalCode);

    public Task<decimal?> GetLocalPickupCostAsync(GetLocalPickupCostRequest request) =>
        Task.FromResult<decimal?>(0m);

    public async Task<ShippingEstimate?> GetEstimatedShippingCostAsync(GetEstimatedShippingCostRequest r)
    {
        await Task.Delay(600); // long enough to see the "Estimating…" state
        return new ShippingEstimate { ShippingDifference = 7.95m + 1.50m * Math.Max(0, r.Quantity - 1) };
    }

    public Task UpdateShippingPostalCodeAsync(UpdateShippingPostalCodeRequest request)
    {
        _postalCode = request.PostalCode;
        return Task.CompletedTask;
    }
}

// ─── Cart ─────────────────────────────────────────────────────────────────

/// <summary>Per-circuit cart contents shared by <see cref="DemoCartService"/> and CartState.</summary>
public sealed class DemoCartStore
{
    private readonly List<CartItem> _items = [];
    private long _nextLine = 1;

    public DemoCartStore()
    {
        Add(2001, 1, FitmentString(2024, 101, 201, 302));
        Add(1001, 2, FitmentString(2024, 101, 201, 302));
    }

    public IReadOnlyList<CartItem> Items => _items;
    public int ItemCount => _items.Sum(i => i.Quantity);

    public void Add(long productUkey, int quantity, string fitmentString)
    {
        var existing = _items.FirstOrDefault(i => i.UkeyProduct == productUkey);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            return;
        }

        var p = ProductByUkey(productUkey);
        _items.Add(new CartItem
        {
            Ukey = _nextLine++,
            UkeyProduct = productUkey,
            ProductDescription = p?.Description ?? $"Product {productUkey}",
            StockCode = p?.StockCode ?? productUkey.ToString(),
            ProductURL = p?.Url ?? "#",
            ThumbnailURL = p?.Image(0, 160) ?? string.Empty,
            FitmentString = string.IsNullOrWhiteSpace(fitmentString) ? "No vehicle selected" : fitmentString,
            Price = p?.Price ?? 0m,
            ListPrice = p?.ListPrice ?? 0m,
            Quantity = quantity
        });
    }

    public void Remove(long lineUkey) => _items.RemoveAll(i => i.Ukey == lineUkey);

    public void Clear() => _items.Clear();

    public void SetQuantity(long lineUkey, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Ukey == lineUkey);
        if (item is not null) item.Quantity = Math.Max(1, quantity);
    }

    public List<CartSummaryLine> BuildSummary()
    {
        if (_items.Count == 0) return [];
        var subtotal = _items.Sum(i => i.LineItemTotal);
        var msrp = _items.Sum(i => i.ListPrice * i.Quantity);
        return
        [
            // Mirrors the sproc's mixed shape: some rows have PlainName, some only HTML CartSummary.
            new() { CartSummary = "<span>MSRP</span>", DisplayAmount = $"<s>{msrp:C}</s>" },
            new() { PlainName = "Your Savings", DisplayAmount = $"-{msrp - subtotal:C}" },
            new() { PlainName = "Subtotal", DisplayAmount = subtotal.ToString("C") },
            new() { PlainName = "Shipping", DisplayAmount = "Calculated at checkout" },
            new() { PlainName = "Total", DisplayAmount = subtotal.ToString("C"), IsTotal = true },
        ];
    }
}

public sealed class DemoCartService(DemoCartStore store) : ICartService
{
    public Task AddToCartAsync(AddToCartRequest request)
    {
        store.Add(request.UkeyProduct, Math.Max(1, request.Quantity), string.Empty);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<RecommendedProduct>> GetCartRecommendationsAsync(GetCartRequest request)
    {
        await Task.Delay(80);
        return Products.Where(p => p.Category.IsAccessory).Skip(3).Take(4)
            .Select(p => new RecommendedProduct
            {
                Ukey = p.Ukey,
                ProductDescription = p.Description,
                StockCode = p.StockCode,
                Price = p.Price,
                ProductURL = p.Url,
                ImageURL = p.Image()
            })
            // One ghost row, like the real sproc returns — the panel filters it out.
            .Append(new RecommendedProduct { Ukey = 0, ProductDescription = "", Price = 0m })
            .ToList();
    }
}
