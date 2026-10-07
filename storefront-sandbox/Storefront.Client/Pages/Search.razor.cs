using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Pages;

public partial class Search : ComponentBase
{
    [Inject] public IVehicleService VehicleService { get; set; } = default!;
    [Inject] public IProductSearchService ProductSearchService { get; set; } = default!;
    [Inject] public IProductService ProductService { get; set; } = default!;
    [Inject] public ICategoryService CategoryService { get; set; } = default!;
    [Inject] public IWebsiteSettingsService WebsiteSettingsService { get; set; } = default!;
    [Inject] public CartState CartState { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "q")]
    public string? Q { get; set; }

    // Vehicle fitment carried from the header selector (legacy-compatible key names)
    [SupplyParameterFromQuery(Name = "ukey_make")]
    public int? UkeyMake { get; set; }

    [SupplyParameterFromQuery(Name = "ukey_model")]
    public int? UkeyModel { get; set; }

    [SupplyParameterFromQuery(Name = "modelYear")]
    public int? Year { get; set; }

    [SupplyParameterFromQuery(Name = "ukey_driveline")]
    public int? UkeyDriveline { get; set; }

    [SupplyParameterFromQuery(Name = "ukey_trimlevel")]
    public int? UkeyTrimLevel { get; set; }

    [SupplyParameterFromQuery(Name = "ukey_category")]
    public int? UkeyCategory { get; set; }

    [SupplyParameterFromQuery(Name = "view")]
    public string? View { get; set; }

    [SupplyParameterFromQuery(Name = "filters")]
    public string? Filters { get; set; }

    [SupplyParameterFromQuery(Name = "sortOrder")]
    public string? SortOrder { get; set; }

    private bool _isGridView => string.Equals(View, "grid", StringComparison.OrdinalIgnoreCase);

    // Default = shown when the param is missing or anything other than "hidden".
    private bool _showSidebar => !string.Equals(Filters, "hidden", StringComparison.OrdinalIgnoreCase);

    [PersistentState] public InterpretSearchResult? _interpretResult { get; set; }
    [PersistentState] public List<ProductSearchResult>? _results { get; set; }
    [PersistentState] public string? _fitmentString { get; set; }

    [PersistentState] public List<RefineLink>? _modelRefineLinks { get; set; }
    [PersistentState] public List<RefineLink>? _yearRefineLinks { get; set; }
    [PersistentState] public List<RefineLink>? _trimRefineLinks { get; set; }
    [PersistentState] public List<RefineLink>? _drivelineRefineLinks { get; set; }
    [PersistentState] public List<Category>? _categories { get; set; }
    [PersistentState] public List<SearchAssembly>? _assemblies { get; set; }
    [PersistentState] public WebsiteSettingsDto? _websiteSettings { get; set; }

    // Bound to the Category dropdown. Setter fires NavigateToCategory so the URL is the source of
    // truth (deep-linkable, survives back/forward). Blazor routing rebinds without a page reload.
    private int _selectedUkeyCategory;

    // Bound to the sidebar Refine keywords input. Seeded from ?q= so the box reflects the URL.
    private string _keywordInput = string.Empty;

    private Dictionary<long, List<ProductImage>> _additionalImages = new();
    private bool _additionalImagesLoaded;
    private bool _firstParamSet = true;

    // True while LoadResultsAsync is in flight. Set before the first await so the
    // lifecycle auto-render shows the spinner; cleared in a finally when the fetch
    // completes (or throws). Drives the .search-loading indicator in the markup.
    private bool _isSearching;

    // Effective values reflected in the chips — match the resolution order used by LoadResultsAsync.
    private int EffectiveYear =>
        Year ?? FitmentState.Year
        ?? (int.TryParse(_interpretResult?.ModelYear, out var yr) ? yr : 0);

    private int EffectiveUkeyMake => UkeyMake ?? _interpretResult?.UkeyMake ?? 0;
    private int EffectiveUkeyModel => UkeyModel ?? FitmentState.UkeyModel ?? _interpretResult?.UkeyModel ?? 0;
    private int EffectiveUkeyDriveLine => UkeyDriveline ?? FitmentState.UkeyDriveline ?? _interpretResult?.UkeyDriveLine ?? 0;
    private int EffectiveUkeyTrimLevel => UkeyTrimLevel ?? FitmentState.UkeyTrimLevel ?? _interpretResult?.UkeyTrimLevel ?? 0;
    private int EffectiveUkeyCategory => UkeyCategory ?? _interpretResult?.UkeyCategory ?? 0;

    private string CategoryName =>
        _categories?.FirstOrDefault(c => c.Ukey == EffectiveUkeyCategory)?.CategoryName ?? string.Empty;

    private bool HasAnyFitment =>
        EffectiveYear > 0 || EffectiveUkeyMake > 0 || EffectiveUkeyModel > 0
        || EffectiveUkeyDriveLine > 0 || EffectiveUkeyTrimLevel > 0;

    private int _filterCount =>
        (EffectiveYear > 0 ? 1 : 0)
        + (EffectiveUkeyMake > 0 ? 1 : 0)
        + (EffectiveUkeyModel > 0 ? 1 : 0)
        + (EffectiveUkeyDriveLine > 0 ? 1 : 0)
        + (EffectiveUkeyTrimLevel > 0 ? 1 : 0)
        + (EffectiveUkeyCategory > 0 ? 1 : 0);

    // "Your Vehicle" display helpers. Refine sprocs return Count == 1 when a level is locked in
    // (the "(x) remove" row); DisplayString embeds HTML like `XC40 <span style="color:blue"> (x)</span>`,
    // so strip everything from the first tag onward to get the plain name.
    private static string StripDisplayMarkup(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var idx = raw.IndexOf('<');
        return (idx < 0 ? raw : raw[..idx]).Trim();
    }

    private string ModelName =>
        _modelRefineLinks?.Count == 1 ? StripDisplayMarkup(_modelRefineLinks[0].DisplayString) : string.Empty;

    private string DrivelineName =>
        _drivelineRefineLinks?.Count == 1 ? StripDisplayMarkup(_drivelineRefineLinks[0].DisplayString) : string.Empty;

    private string TrimLevelName =>
        _trimRefineLinks?.Count == 1 ? StripDisplayMarkup(_trimRefineLinks[0].DisplayString) : string.Empty;

    // _fitmentString from pr_returnFitmentString is "{year} {make} {model} {driveline} {trim}".
    // Subtract off the trim and driveline suffixes (which we have names for) so the Current
    // Vehicle line shows just "{year} {make} {model}".
    private string CurrentVehicleLabel
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_fitmentString)) return string.Empty;
            var result = _fitmentString.Trim();
            result = StripSuffix(result, TrimLevelName);
            result = StripSuffix(result, DrivelineName);
            return result;
        }
    }

    // Make has no refine sproc — derive it from CurrentVehicleLabel by removing the year prefix
    // and the model suffix. Falls back to empty if anything's missing.
    private string MakeName
    {
        get
        {
            var yearMakeModel = CurrentVehicleLabel;
            if (string.IsNullOrWhiteSpace(yearMakeModel)) return string.Empty;
            var yearPrefix = EffectiveYear > 0 ? EffectiveYear.ToString() : string.Empty;
            var result = yearMakeModel;
            if (!string.IsNullOrWhiteSpace(yearPrefix) && result.StartsWith(yearPrefix, StringComparison.Ordinal))
                result = result[yearPrefix.Length..].TrimStart();
            result = StripSuffix(result, ModelName);
            return result.Trim();
        }
    }

    private static string StripSuffix(string s, string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix)) return s;
        var trimmed = suffix.Trim();
        if (s.EndsWith(trimmed, StringComparison.OrdinalIgnoreCase))
            return s[..^trimmed.Length].TrimEnd();
        return s;
    }

    protected override async Task OnInitializedAsync()
    {
        // Mirrors StorefrontHeader: skip the service call if PersistentState restored it from prerender.
        // 5-min server-side IMemoryCache keeps this cheap regardless.
        if (_websiteSettings is null)
        {
            _websiteSettings = await WebsiteSettingsService.GetSettingsAsync(
                WebsiteContext.UkeyWebsite,
                SessionState.SessionId,
                SessionState.MachineId);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_firstParamSet)
        {
            _firstParamSet = false;
            if (_results is not null) return; // restored from prerender state
        }
        await LoadResultsAsync();
    }

    private async Task LoadResultsAsync()
    {
        _isSearching = true;
        try
        {
            await LoadResultsCoreAsync();
        }
        finally
        {
            _isSearching = false;
        }
    }

    private async Task LoadResultsCoreAsync()
    {
        var uri = new Uri(NavigationManager.Uri);
        var searchString = Q?.Trim() ?? string.Empty;

        // Seed the interpret sproc with whatever vehicle context we already know
        // (URL params first, then FitmentState). The sproc derives missing values
        // (e.g. make from model, year from session) and echoes them back.
        var seedUkeyMake = UkeyMake ?? 0;
        var seedUkeyModel = UkeyModel ?? FitmentState.UkeyModel ?? 0;
        var seedYear = Year ?? FitmentState.Year ?? 0;
        var seedUkeyDriveLine = UkeyDriveline ?? FitmentState.UkeyDriveline ?? 0;
        var seedUkeyTrimLevel = UkeyTrimLevel ?? FitmentState.UkeyTrimLevel ?? 0;
        var seedUkeyCategory = UkeyCategory ?? 0;

        _interpretResult = await VehicleService.GetInterpretationAsync(new InterpretSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            SearchString = searchString,
            QueryString = uri.Query,
            UkeyMake = seedUkeyMake,
            UkeyModel = seedUkeyModel,
            ModelYear = seedYear > 0 ? seedYear.ToString() : string.Empty,
            UkeyDriveLine = seedUkeyDriveLine,
            UkeyTrimLevel = seedUkeyTrimLevel,
            UkeyCategory = seedUkeyCategory
        });

        // Priority: URL query params (deep-link) → FitmentState (direct interaction) → InterpretAsync
        var effectiveUkeyMake = UkeyMake ?? _interpretResult.UkeyMake;
        var effectiveUkeyModel = UkeyModel ?? FitmentState.UkeyModel ?? _interpretResult.UkeyModel;
        var effectiveYear = Year ?? FitmentState.Year ?? (int.TryParse(_interpretResult.ModelYear, out var yr) ? yr : 0);
        var effectiveUkeyDriveLine = UkeyDriveline ?? FitmentState.UkeyDriveline ?? _interpretResult.UkeyDriveLine;
        var effectiveUkeyTrimLevel = UkeyTrimLevel ?? FitmentState.UkeyTrimLevel ?? _interpretResult.UkeyTrimLevel;
        var effectiveUkeyCategory = UkeyCategory ?? _interpretResult.UkeyCategory;

        _fitmentString = await VehicleService.GetFitmentStringAsync(new GetFitmentStringRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyMake = effectiveUkeyMake,
            UkeyModel = effectiveUkeyModel,
            ModelYear = effectiveYear.ToString(),
            UkeyDriveLine = effectiveUkeyDriveLine,
            UkeyTrimLevel = effectiveUkeyTrimLevel
        });

        var searchResults = await ProductSearchService.GetSearchResultsAsync(new ProductSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            SearchString = searchString,
            UkeyMake = effectiveUkeyMake,
            UkeyModel = effectiveUkeyModel,
            ModelYear = effectiveYear,
            UkeyDriveLine = effectiveUkeyDriveLine,
            UkeyTrimLevel = effectiveUkeyTrimLevel,
            UkeyModelRange = _interpretResult.UkeyModelRange,
            UkeyCategory = effectiveUkeyCategory,
            SortOrder = MapSortOrderToSproc(SortOrder),
            QueryString = uri.Query
        });

        _results = searchResults.ToList();

        var refineRequest = new GetRefineLinksRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyMake = effectiveUkeyMake,
            UkeyModel = effectiveUkeyModel,
            ModelYear = effectiveYear > 0 ? effectiveYear.ToString() : string.Empty,
            UkeyCategory = effectiveUkeyCategory,
            SearchString = _interpretResult.SearchTerm ?? searchString,
            UkeyDriveLine = effectiveUkeyDriveLine,
            UkeyTrimLevel = effectiveUkeyTrimLevel,
            UkeyModelRange = _interpretResult.UkeyModelRange,
            QueryString = uri.Query
        };

        var modelTask = VehicleService.GetModelRefineLinksAsync(refineRequest);
        var yearTask = VehicleService.GetYearRefineLinksAsync(refineRequest);
        var trimTask = VehicleService.GetTrimLevelRefineLinksAsync(refineRequest);
        var drivelineTask = VehicleService.GetDrivelineRefineLinksAsync(refineRequest);
        var categoriesTask = CategoryService.GetCategoriesAsync(new GetCategoriesRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyMake = effectiveUkeyMake,
            UkeyModel = effectiveUkeyModel,
            UkeyParent = -1
        });
        var assembliesTask = ProductSearchService.GetSearchAssembliesAsync(new GetSearchAssembliesRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite
            // UkeyCachedSearch left at 0 — see GetSearchAssembliesRequest comment for context.
        });
        await Task.WhenAll(modelTask, yearTask, trimTask, drivelineTask, categoriesTask, assembliesTask);

        _modelRefineLinks = modelTask.Result.ToList();
        _yearRefineLinks = yearTask.Result.ToList();
        _trimRefineLinks = trimTask.Result.ToList();
        _drivelineRefineLinks = drivelineTask.Result.ToList();
        _categories = categoriesTask.Result.ToList();
        _selectedUkeyCategory = effectiveUkeyCategory;
        _keywordInput = Q ?? string.Empty;
        _assemblies = assembliesTask.Result.ToList();

        // Re-fetch additional images for the new result set.
        _additionalImages = new();
        _additionalImagesLoaded = false;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_additionalImagesLoaded || _results is null || _results.Count == 0)
            return;

        _additionalImagesLoaded = true;

        var tasks = _results.Select(async product =>
        {
            var images = await ProductService.GetAdditionalImagesAsync(new GetProductImagesRequest
            {
                UkeyProduct = product.Ukey
            });
            return (product.Ukey, Images: images.ToList());
        });

        var fetched = await Task.WhenAll(tasks);
        foreach (var (ukey, images) in fetched)
        {
            if (images.Count > 0)
                _additionalImages[ukey] = images;
        }

        await InvokeAsync(StateHasChanged);
    }

    // FitmentState is all-or-nothing (HasVehicle requires both Year and Model), so any fitment
    // chip removal clears the in-memory vehicle. The URL stays the source of truth: each handler
    // strips its own param plus anything downstream in the catalog hierarchy
    // (Make → Model → Driveline/Trim). NavigateTo re-fires OnParametersSetAsync → LoadResultsAsync.

    private void RemoveMake()
    {
        FitmentState.Clear();
        NavigateRemovingParams("ukey_make", "ukey_model", "ukey_driveline", "ukey_trimlevel");
    }

    private void RemoveModel()
    {
        FitmentState.Clear();
        NavigateRemovingParams("ukey_model", "ukey_driveline", "ukey_trimlevel");
    }

    private void RemoveYear()
    {
        FitmentState.Clear();
        NavigateRemovingParams("modelYear");
    }

    private void RemoveDriveline()
    {
        FitmentState.Clear();
        NavigateRemovingParams("ukey_driveline");
    }

    private void RemoveTrim()
    {
        FitmentState.Clear();
        NavigateRemovingParams("ukey_trimlevel");
    }

    private void RemoveCategory() => NavigateWithParam("ukey_category", null);

    private void OnCategoryChanged() =>
        NavigateWithParam("ukey_category", _selectedUkeyCategory > 0 ? _selectedUkeyCategory.ToString() : null);

    // Mirrors StorefrontHeader.HandleSearch — replaces the ?q= param and lets OnParametersSetAsync
    // re-run the pipeline. Other URL params (category, fitment) are preserved by NavigateWithParam.
    private void OnRefineKeywordsSubmit()
    {
        var q = _keywordInput.Trim();
        if (string.IsNullOrEmpty(q)) return;
        NavigateWithParam("q", q);
    }

    private void SetListView() => NavigateWithParam("view", null);
    private void SetGridView() => NavigateWithParam("view", "grid");

    // Pass the resolved fitment string so pr_addToCart records context (legacy behavior).
    private Task HandleAddToCart(ProductSearchResult product) =>
        CartState.AddToCartAsync(productUkey: product.Ukey, fitmentString: _fitmentString ?? string.Empty);

    // null strips the param (default = shown); "hidden" hides the sidebar.
    private void OnSidebarVisibilityChanged(bool show) =>
        NavigateWithParam("filters", show ? null : "hidden");

    // Default (relevance) strips the param to keep URLs clean; any other slug persists.
    private void OnSortOrderChanged(string slug) =>
        NavigateWithParam("sortOrder", slug == "relevance" ? null : slug);

    // URL slug → string the pr_productSearch sproc expects (matches legacy resource values).
    private static string MapSortOrderToSproc(string? slug) => slug?.ToLowerInvariant() switch
    {
        "description" => "Description",
        "partnumber" => "Part Number",
        "priceasc" => "Price (Low to High)",
        "pricedesc" => "Price (High to Low)",
        _ => "Relevance"
    };

    private void ClearAllFilters()
    {
        FitmentState.Clear();
        NavigateRemovingParams("ukey_make", "ukey_model", "modelYear", "ukey_driveline", "ukey_trimlevel");
    }

    private void NavigateRemovingParams(params string[] keysToRemove)
    {
        var uri = new Uri(NavigationManager.Uri);
        var path = uri.GetLeftPart(UriPartial.Path);
        var pairs = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(p =>
            {
                var eq = p.IndexOf('=');
                var key = eq < 0 ? p : p[..eq];
                return !keysToRemove.Contains(Uri.UnescapeDataString(key), StringComparer.OrdinalIgnoreCase);
            })
            .ToArray();
        var newUri = pairs.Length > 0 ? $"{path}?{string.Join("&", pairs)}" : path;
        NavigationManager.NavigateTo(newUri);
    }

    // Sets or replaces a single query param. Null/empty value removes it. Strips the existing key
    // (case-insensitive) before appending so we don't end up with duplicate keys.
    private void NavigateWithParam(string key, string? value)
    {
        var uri = new Uri(NavigationManager.Uri);
        var path = uri.GetLeftPart(UriPartial.Path);
        var pairs = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(p =>
            {
                var eq = p.IndexOf('=');
                var k = eq < 0 ? p : p[..eq];
                return !string.Equals(Uri.UnescapeDataString(k), key, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
        if (!string.IsNullOrEmpty(value))
            pairs.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        var newUri = pairs.Count > 0 ? $"{path}?{string.Join("&", pairs)}" : path;
        NavigationManager.NavigateTo(newUri);
    }
}
