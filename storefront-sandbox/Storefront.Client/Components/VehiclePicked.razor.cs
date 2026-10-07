using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class VehiclePicked : ComponentBase, IDisposable
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [PersistentState] public PlacementText? _placement { get; set; }

    private bool _loaded;

    // IntroTextTitle is the picked-state heading in the legacy control; fall back to Title,
    // then to a generic label so the card (and its Change Vehicle escape hatch) always renders.
    private string DisplayTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_placement?.IntroTextTitle)) return _placement.IntroTextTitle;
            if (!string.IsNullOrWhiteSpace(_placement?.Title)) return _placement.Title;
            return "Your Vehicle";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        // Re-fetch whenever the active vehicle changes (Home is reused across vehicle navigations,
        // so this component's OnInitialized won't run again on its own).
        Fitment.OnChange += HandleFitmentChanged;

        if (_placement is not null)
        {
            _loaded = true;
            return; // restored from prerender state
        }

        _placement = await FetchAsync();
        _loaded = true;
    }

    private Task<PlacementText?> FetchAsync() =>
        PlacementService.GetAsync(new GetPlacementRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            Key = "VehiclePickerHeading",
            UkeyModel = Fitment.UkeyModel ?? 0,
            ModelYear = Fitment.Year?.ToString() ?? string.Empty,
            UkeyDriveline = Fitment.UkeyDriveline ?? 0,
            UkeyTrimLevel = Fitment.UkeyTrimLevel ?? 0,
            UkeyCategory = Fitment.UkeyCategory ?? 0
        });

    // Action-based event handler: keep it off async-void and observe the fetch, so a
    // transient failure degrades gracefully (DisplayTitle falls back to "Your Vehicle")
    // instead of surfacing as an unobserved exception that can tear down the renderer.
    private void HandleFitmentChanged() => _ = RefreshPlacementAsync();

    private async Task RefreshPlacementAsync()
    {
        try { _placement = await FetchAsync(); }
        catch { _placement = null; } // best-effort heading; fall back rather than crash
        _loaded = true;
        await InvokeAsync(StateHasChanged);
    }

    private void ChangeVehicle()
    {
        Fitment.Clear();
        NavigationManager.NavigateTo("/");
    }

    public void Dispose() => Fitment.OnChange -= HandleFitmentChanged;
}
