using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

/// <summary>
/// Vehicle-specific accessory product grid for a category-scoped page (assemblies-equivalent
/// for /accessories). Ports legacy Controls/BrowseAccessory/ViewAllProducts.ascx's core case
/// (model + category resolved) — the sub-category grouping/recursion legacy also supported is
/// deliberately not ported (one flat level for now, per user direction).
/// </summary>
public partial class AccessoryResults : ComponentBase, IDisposable
{
    [Inject] public IProductSearchService ProductSearchService { get; set; } = default!;
    [Inject] public ICategoryService CategoryService { get; set; } = default!;
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;
    [Inject] public CartState CartState { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ILogService Log { get; set; } = default!;

    [PersistentState] public string? _categoryName { get; set; }
    [PersistentState] public List<ProductSearchResult>? _products { get; set; }

    private bool HasProducts => _products is { Count: > 0 };

    protected override async Task OnInitializedAsync()
    {
        // Reload when the active vehicle/category changes — reused across navigations so
        // OnInitialized won't re-run on its own (mirrors AssemblyResults).
        Fitment.OnChange += HandleFitmentChanged;

        if (_products is null)
            await LoadAsync();
    }

    // TEMP diagnostic logging (SPED accessories soft-nav investigation) — remove once the "no
    // products after picking a vehicle client-side" bug is root-caused. Confirms whether
    // Fitment.OnChange is actually firing/reaching this subscriber on soft nav, and — since the
    // catch below previously swallowed everything silently — surfaces any exception LoadAsync
    // throws (e.g. from CategoryService/ProductSearchService) that would otherwise look
    // identical to "nothing happened."
    private void HandleFitmentChanged()
    {
        _ = Log.LogInformation(
            $"HandleFitmentChanged fired: UkeyModel={Fitment.UkeyModel}, UkeyCategory={Fitment.UkeyCategory}",
            "AccessoryResults");
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try { await LoadAsync(); }
        catch (Exception ex)
        {
            // Previously an empty catch — leave the current results on a transient failure, but
            // log it now instead of swallowing silently (see HandleFitmentChanged comment above).
            await Log.LogError($"RefreshAsync/LoadAsync threw: {ex.Message}", "AccessoryResults", ex.StackTrace);
        }
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Fitment.OnChange -= HandleFitmentChanged;

    private async Task LoadAsync()
    {
        // Legacy's core case: a vehicle AND a category must both be resolved before accessories
        // render (ViewAllProducts.ascx.cs's "hasMakeOnly" make-only variant is a metadata-gated
        // edge case, not ported here).
        if (!Fitment.UkeyModel.HasValue || Fitment.UkeyModel <= 0
            || !Fitment.UkeyCategory.HasValue || Fitment.UkeyCategory <= 0)
        {
            await Log.LogInformation(
                $"LoadAsync gate failed — clearing results: UkeyModel={Fitment.UkeyModel}, UkeyCategory={Fitment.UkeyCategory}",
                "AccessoryResults");
            _categoryName = null;
            _products = null;
            return;
        }

        var categoryNameTask = CategoryService.GetCategoryNameAsync(new GetCategoryNameRequest
        {
            UkeyCategory = Fitment.UkeyCategory.Value,
            PageUrl = "/accessories"
        });

        var productsTask = ProductSearchService.GetSearchResultsAsync(new ProductSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyMake = Fitment.UkeyMake ?? 0,
            UkeyModel = Fitment.UkeyModel.Value,
            ModelYear = Fitment.Year ?? 0,
            UkeyDriveLine = Fitment.UkeyDriveline ?? 0,
            UkeyTrimLevel = Fitment.UkeyTrimLevel ?? 0,
            UkeyCategory = Fitment.UkeyCategory.Value,
            NumResults = 400,
            SortOrder = "Relevance",
            ShowAlternateImages = true,
            IsAccessory = false, // lifestyle merch (Lifestyle.razor) — not this page
            IsPerformance = true // vehicle-fitment-specific accessories — this page
        });

        await Task.WhenAll(categoryNameTask, productsTask);

        _categoryName = categoryNameTask.Result;
        _products = productsTask.Result.ToList();

        await Log.LogInformation(
            $"LoadAsync complete: category={_categoryName}, productCount={_products.Count}",
            "AccessoryResults");
    }

    // The category is always the last URL segment (/accessories/{make}/{category} or
    // /accessories/{make}/{year}-{model}/{category}), so dropping it is enough to "change
    // category" — mirrors AssemblyResults.ChangeCategory.
    private void ChangeCategory()
    {
        var path = Navigation.ToBaseRelativePath(Navigation.Uri).TrimEnd('/');
        var lastSlash = path.LastIndexOf('/');
        var target = lastSlash > 0 ? path[..lastSlash] : string.Empty;
        Navigation.NavigateTo("/" + target);
    }

    private Task HandleAddToCart(ProductSearchResult product) =>
        CartState.AddToCartAsync(productUkey: product.Ukey);
}
