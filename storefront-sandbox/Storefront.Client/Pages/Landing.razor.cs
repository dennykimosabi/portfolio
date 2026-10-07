using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Pages;

public partial class Landing : ComponentBase
{
    [Inject] public ILandingService LandingService { get; set; } = default!;
    [Inject] public IProductSearchService ProductSearchService { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public CartState CartState { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    // Grid is the default for landing pages. ?view=list flips to list view (same param as Search).
    [SupplyParameterFromQuery(Name = "view")]
    public string? View { get; set; }

    [SupplyParameterFromQuery(Name = "sortOrder")]
    public string? SortOrder { get; set; }

    private bool _isGridView => !string.Equals(View, "list", StringComparison.OrdinalIgnoreCase);

    [PersistentState] public LandingPageConfig? _config { get; set; }
    [PersistentState] public List<ProductSearchResult>? _results { get; set; }

    private bool _notFound;
    private string? _lastLoadedSlug;

    private int _filterCount => _config is null ? 0 :
        (string.IsNullOrWhiteSpace(_config.SearchString) ? 0 : 1)
        + (_config.UkeyCategory > 0 ? 1 : 0)
        + (_config.UkeyMake > 0 ? 1 : 0)
        + (_config.UkeyModel > 0 ? 1 : 0)
        + (_config.Year > 0 ? 1 : 0);

    private string? _lastLoadedSortOrder;

    protected override async Task OnParametersSetAsync()
    {
        // Re-fetch when slug or sortOrder changes; skip when only ?view= changes.
        var sortKey = SortOrder ?? string.Empty;
        if (_lastLoadedSlug == Slug && _lastLoadedSortOrder == sortKey && _config is not null)
            return;

        _lastLoadedSlug = Slug;
        _lastLoadedSortOrder = sortKey;
        _notFound = false;

        _config = await LandingService.GetLandingBySlugAsync(Slug);

        if (_config is null)
        {
            _notFound = true;
            _results = null;
            return;
        }

        var searchResults = await ProductSearchService.GetSearchResultsAsync(new ProductSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            SearchString = _config.SearchString,
            UkeyCategory = _config.UkeyCategory,
            UkeyMake = _config.UkeyMake,
            UkeyModel = _config.UkeyModel,
            ModelYear = _config.Year,
            SortOrder = MapSortOrderToSproc(SortOrder),
            QueryString = new Uri(NavigationManager.Uri).Query
        });

        _results = searchResults.ToList();
    }

    private void SetListView() => NavigateWithParam("view", "list");
    private void SetGridView() => NavigateWithParam("view", null);

    private void OnSortOrderChanged(string slug) =>
        NavigateWithParam("sortOrder", slug == "relevance" ? null : slug);

    private static string MapSortOrderToSproc(string? slug) => slug?.ToLowerInvariant() switch
    {
        "description" => "Description",
        "partnumber" => "Part Number",
        "priceasc" => "Price (Low to High)",
        "pricedesc" => "Price (High to Low)",
        _ => "Relevance"
    };

    // Landing has no user-selected fitment to pass — empty string is fine.
    private Task HandleAddToCart(ProductSearchResult product) =>
        CartState.AddToCartAsync(productUkey: product.Ukey);

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
