using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class HeroPlacement : ComponentBase, IDisposable
{
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;

    [Parameter] public string PlacementKey { get; set; } = "HeroArea";

    [PersistentState] public PlacementText? _placement { get; set; }

    private bool _loaded;

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
            Key = PlacementKey,
            UkeyModel = Fitment.UkeyModel ?? 0,
            ModelYear = Fitment.Year?.ToString() ?? string.Empty,
            UkeyDriveline = Fitment.UkeyDriveline ?? 0,
            UkeyTrimLevel = Fitment.UkeyTrimLevel ?? 0,
            UkeyCategory = Fitment.UkeyCategory ?? 0
        });

    // Action-based event handler: keep it off async-void and observe the fetch, so a
    // transient failure degrades gracefully (the hero simply hides when placement is
    // null/empty) instead of surfacing as an unobserved exception.
    private void HandleFitmentChanged() => _ = RefreshPlacementAsync();

    private async Task RefreshPlacementAsync()
    {
        try { _placement = await FetchAsync(); }
        catch { _placement = null; } // best-effort hero; fall back rather than crash
        _loaded = true;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Fitment.OnChange -= HandleFitmentChanged;
}
