using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

/// <summary>
/// Assemblies grid + part types panel + keyword filter for a category-scoped page.
/// Ports legacy Components/AssemblyResults.ascx (Vue "AssemblyResults" component) — rendered
/// once a category is resolved (mirrors legacy's Interpret.UkeyCategory > 0 check).
/// </summary>
public partial class AssemblyResults : ComponentBase, IDisposable
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IHomeService HomeService { get; set; } = default!;
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [PersistentState] public GetAssemblyResultsResponse? _result { get; set; }

    private string _filterText = string.Empty;

    private bool HasAssemblies => _result?.Assemblies.Any() == true;
    private bool HasPartTypes => _result?.PartTypes.Any() == true;

    private IEnumerable<AssemblyResult> FilteredAssemblies =>
        Filter(_result?.Assemblies, a => !string.IsNullOrWhiteSpace(a.Keywords) ? a.Keywords : a.AssemblyName);

    private IEnumerable<CategoryPartType> FilteredPartTypes =>
        Filter(_result?.PartTypes, p => !string.IsNullOrWhiteSpace(p.Keywords) ? p.Keywords : p.CleanTypeName);

    protected override async Task OnInitializedAsync()
    {
        // Reload when the active vehicle/category changes — AssemblyResults is reused across
        // navigations so OnInitialized won't re-run on its own (mirrors RefineSearch).
        Fitment.OnChange += HandleFitmentChanged;

        if (_result is null)
            await LoadAsync();
    }

    private void HandleFitmentChanged() => _ = RefreshAsync();

    private async Task RefreshAsync()
    {
        try { await LoadAsync(); }
        catch { /* leave the current results on a transient failure */ }
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Fitment.OnChange -= HandleFitmentChanged;

    private async Task LoadAsync()
    {
        if (!Fitment.UkeyCategory.HasValue || Fitment.UkeyCategory <= 0)
        {
            _result = null;
            return;
        }

        var response = await HomeService.GetAssemblyResultsAsync(new GetAssemblyResultsRequest
        {
            UkeyCategory = Fitment.UkeyCategory.Value,
            UkeyMake = Fitment.UkeyMake,
            UkeyModel = Fitment.UkeyModel,
            ModelYear = Fitment.Year,
            UkeyDriveline = Fitment.UkeyDriveline,
            UkeyTrimLevel = Fitment.UkeyTrimLevel
        });

        // HomeService fails soft (returns null) rather than throwing on a transient error —
        // keep the last-good results on screen instead of clearing them.
        if (response is not null)
        {
            _result = response;
        }
    }

    // The category is always the last URL segment (/{make}/{category} or
    // /{make}/{year}-{model}/{category} — see Home.razor.cs DecomposeRoute), so dropping it
    // is enough to "change category" under the new route scheme.
    private void ChangeCategory()
    {
        var path = Navigation.ToBaseRelativePath(Navigation.Uri).TrimEnd('/');
        var lastSlash = path.LastIndexOf('/');
        var target = lastSlash > 0 ? path[..lastSlash] : string.Empty;
        Navigation.NavigateTo("/" + target);
    }

    private IEnumerable<T> Filter<T>(IEnumerable<T>? source, Func<T, string> keywordsSelector)
    {
        if (source is null) return Enumerable.Empty<T>();
        if (string.IsNullOrWhiteSpace(_filterText)) return source;

        return source.Where(item => keywordsSelector(item).Contains(_filterText, StringComparison.OrdinalIgnoreCase));
    }
}
