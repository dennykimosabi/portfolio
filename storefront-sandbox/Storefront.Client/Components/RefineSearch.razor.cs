using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public enum RefineSearchMode
{
    /// <summary>Tile grid of categories — "Shop Parts by Category" on the home page.</summary>
    Categories,

    /// <summary>Tile grid of accessory categories — "Shop Accessories by Category" on the home page.</summary>
    AccessoryCategories,

    /// <summary>Compact link list for vehicle-fitment refinement — "Shop by Year/Model/Make/etc."</summary>
    Links
}

public partial class RefineSearch : ComponentBase, IDisposable
{
    [Inject] public IProductSearchService ProductSearchService { get; set; } = default!;
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    [Parameter] public RefineSearchMode Mode { get; set; } = RefineSearchMode.Categories;

    /// <summary>
    /// Legacy: RefineSearch.ascx reads ControlModifications.PlacementTarget. When empty
    /// (the common case for Categories), it pulls the "CategoryHeader" placement as the
    /// heading override. Pass an explicit key here to override that default.
    /// </summary>
    [Parameter] public string? PlacementKey { get; set; }

    /// <summary>
    /// Optional placement key for a "featured" tile rendered before the regular tiles in
    /// the grid (e.g. "View All Accessories"). Mirrors the legacy SubCategoryFeaturedDefault
    /// sub-control that BrowseAccessoriesDefault.ascx embeds inside its tile container.
    /// AccessoryCategories defaults to "FeaturedDefault"; other modes default to none.
    /// </summary>
    [Parameter] public string? FeaturedPlacementKey { get; set; }

    /// <summary>
    /// Home's "Shop Accessories by Category" preview widget (AccessoryCategories mode) wants the
    /// featured "View All Accessories" tile; the Accessories page's own category grid uses
    /// AccessoryCategories mode too (for correct pr_refineSearch ShowFitmentLevel scoping) but must
    /// NOT show that tile — legacy's real accessories page renders only the returned categories.
    /// Defaults to true so Home's existing usage is unaffected; Accessories opts out explicitly.
    /// </summary>
    [Parameter] public bool ShowFeaturedTile { get; set; } = true;

    /// <summary>
    /// pr_refineSearch computes both linkURL (home) and accessoryLinkURL (/accessories) on every
    /// row — fn_returnStorefrontURL only prefixes '/accessories' when called with @cPageType =
    /// 'accessories'. Home wants linkURL; /accessories (and Home's own "Shop Accessories by
    /// Category" widget, whose tiles should stay on the accessories page) want accessoryLinkURL.
    /// Mirrors VehiclePicker's identical parameter/reasoning.
    /// </summary>
    [Parameter] public bool UseAccessoryLinks { get; set; }

    /// <summary>
    /// Legacy keys some pr_refineSearch scoping decisions off the literal calling page's URL
    /// (e.g. plain "category" mode only scopes to the accessory-root category when
    /// cPageURL = '/browseaccessories.aspx' — see pr_refineSearch's category-scoping block).
    /// This is a sentinel the sproc checks, not a real path our new URL scheme uses, so pass
    /// the legacy page path that matches the desired scoping rather than the actual route.
    /// </summary>
    [Parameter] public string PageUrl { get; set; } = "/default.aspx";

    /// <summary>
    /// Overrides the CSS class suffix (defaults to Mode's name) so the visual tile treatment
    /// can differ from the data-fetch mode — e.g. /accessories uses Categories mode for correct
    /// pr_refineSearch scoping but still wants the AccessoryCategories gradient-overlay tile style.
    /// </summary>
    [Parameter] public string? StyleVariant { get; set; }

    private string ResolvedStyleVariant =>
        (!string.IsNullOrWhiteSpace(StyleVariant) ? StyleVariant : Mode.ToString()).ToLowerInvariant();

    [PersistentState] public List<RefineSearchResult>? Results { get; set; }
    [PersistentState] public PlacementText? HeadingPlacement { get; set; }
    [PersistentState] public PlacementText? FeaturedPlacement { get; set; }

    private string ResolvedPlacementKey =>
        !string.IsNullOrWhiteSpace(PlacementKey) ? PlacementKey
        : Mode switch
        {
            RefineSearchMode.Categories => "CategoryHeader",
            RefineSearchMode.AccessoryCategories => "Accessories",
            RefineSearchMode.Links => "RefineSearchLinksHeader",
            _ => string.Empty
        };

    private string ResolvedFeaturedPlacementKey =>
        !ShowFeaturedTile ? string.Empty
        : !string.IsNullOrWhiteSpace(FeaturedPlacementKey) ? FeaturedPlacementKey
        : Mode switch
        {
            RefineSearchMode.AccessoryCategories => "FeaturedDefault",
            _ => string.Empty
        };

    private bool HasFeaturedTile =>
        FeaturedPlacement is not null
        && (!string.IsNullOrWhiteSpace(FeaturedPlacement.IntroTextTitle)
            || !string.IsNullOrWhiteSpace(FeaturedPlacement.Text));

    private string Heading
    {
        get
        {
            // Links mode is special: the placement's IntroTextTitle (if present) is just
            // a *prefix* (e.g. "Shop by") that legacy combines with the first result's
            // ClassName to produce "Shop by Year"/"Shop by Model"/etc.
            if (Mode == RefineSearchMode.Links)
            {
                var classDisplay = LinksClassDisplay();
                var prefix = HeadingPlacement?.IntroTextTitle;
                var resolvedPrefix = !string.IsNullOrWhiteSpace(prefix) ? prefix : "Shop by";
                return string.IsNullOrEmpty(classDisplay)
                    ? resolvedPrefix
                    : $"{resolvedPrefix} {classDisplay}";
            }

            if (HeadingPlacement is not null)
            {
                if (!string.IsNullOrWhiteSpace(HeadingPlacement.IntroTextTitle))
                    return HeadingPlacement.IntroTextTitle;
                if (!string.IsNullOrWhiteSpace(HeadingPlacement.Title))
                    return HeadingPlacement.Title;
            }
            return Mode switch
            {
                RefineSearchMode.Categories => "Shop Parts by Category",
                RefineSearchMode.AccessoryCategories => "Shop Accessories by Category",
                _ => string.Empty
            };
        }
    }

    // Maps the first result's ClassName ("make", "model", "year", "driveline",
    // "trimlevel", "modelrange") to a display string for the heading. Mirrors the
    // legacy CreateDefaultPlacementTitle helper.
    private string LinksClassDisplay()
    {
        var className = Results?.FirstOrDefault()?.ClassName?
            .ToLowerInvariant()
            .Replace("/", string.Empty);
        return className switch
        {
            "make" => "Make",
            "model" => "Model",
            "year" => "Year",
            "driveline" => "Driveline",
            "trimlevel" => "Trim Level",
            "modelrange" => "Model Range",
            _ => string.Empty
        };
    }

    protected override async Task OnInitializedAsync()
    {
        // Refetch tiles + heading when the active vehicle/category changes. RefineSearch is reused
        // across navigations (constant @key) so OnInitialized won't re-run on its own — without this
        // the category tiles keep their first-load LinkURLs (e.g. built before a vehicle was picked,
        // so they'd drop the fitment).
        FitmentState.OnChange += HandleFitmentChanged;

        // All three are PersistentState — restore from prerender if already populated.
        var resultsTask = Results is null ? LoadResults() : Task.CompletedTask;
        var headingTask = HeadingPlacement is null ? LoadHeading() : Task.CompletedTask;
        var featuredTask = FeaturedPlacement is null ? LoadFeatured() : Task.CompletedTask;
        await Task.WhenAll(resultsTask, headingTask, featuredTask);
    }

    // Action-based handler kept off the async-void path; RefreshAsync observes its own exceptions.
    private void HandleFitmentChanged() => _ = RefreshAsync();

    // Reload the tiles + heading for the new fitment/category. Featured stays put (root-level
    // "view all", fitment-independent). Best-effort — keep the last-good tiles rather than crash.
    private async Task RefreshAsync()
    {
        try { await Task.WhenAll(LoadResults(), LoadHeading()); }
        catch { /* leave the current tiles on a transient failure */ }
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => FitmentState.OnChange -= HandleFitmentChanged;

    private async Task LoadResults()
    {
        var rows = await ProductSearchService.GetRefinedSearchAsync(new RefineSearchRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            ShowFitmentLevel = FitmentLevelForMode(),
            UkeyMake = FitmentState.UkeyMake,
            UkeyModel = FitmentState.UkeyModel,
            Year = FitmentState.Year,
            UkeyDriveline = FitmentState.UkeyDriveline,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel,
            UkeyCategory = FitmentState.UkeyCategory,
            ShowCategory = true,
            PageURL = PageUrl
        });
        Results = rows.ToList();
    }

    private async Task LoadHeading()
    {
        if (string.IsNullOrWhiteSpace(ResolvedPlacementKey)) return;

        HeadingPlacement = await PlacementService.GetAsync(new GetPlacementRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            Key = ResolvedPlacementKey,
            UkeyModel = FitmentState.UkeyModel ?? 0,
            ModelYear = FitmentState.Year?.ToString() ?? string.Empty,
            UkeyDriveline = FitmentState.UkeyDriveline ?? 0,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel ?? 0,
            UkeyCategory = FitmentState.UkeyCategory ?? 0
        });
    }

    // Legacy SubCategoryFeaturedDefault.ascx forces UkeyCategory=0 — this is the root-level
    // "view all" tile prepended to the accessory grid, independent of any subcategory the
    // shopper might be browsing.
    private async Task LoadFeatured()
    {
        if (string.IsNullOrWhiteSpace(ResolvedFeaturedPlacementKey)) return;

        FeaturedPlacement = await PlacementService.GetAsync(new GetPlacementRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            Key = ResolvedFeaturedPlacementKey,
            UkeyModel = FitmentState.UkeyModel ?? 0,
            ModelYear = FitmentState.Year?.ToString() ?? string.Empty,
            UkeyDriveline = FitmentState.UkeyDriveline ?? 0,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel ?? 0,
            UkeyCategory = 0
        });
    }

    private string FitmentLevelForMode() => Mode switch
    {
        RefineSearchMode.Categories => "category",
        RefineSearchMode.AccessoryCategories => "VehicleAccessoryCategories",
        _ => string.Empty
    };

    private static string TileLabel(RefineSearchResult r) =>
        !string.IsNullOrWhiteSpace(r.SimpleString) ? r.SimpleString : r.DisplayString;

    private string? ResolvedLinkUrl(RefineSearchResult r) =>
        UseAccessoryLinks ? r.AccessoryLinkURL : r.LinkURL;
}
