using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class ExtraHomeContent : ComponentBase
{
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    /// <summary>
    /// Placement key. Legacy ExtraDefaultContent.ascx hardcodes "ExtraDefaultContent" —
    /// keep that as the default so existing dealer DB content works without changes.
    /// </summary>
    [Parameter] public string PlacementKey { get; set; } = "ExtraDefaultContent";

    [PersistentState] public PlacementText? Placement { get; set; }

    private bool HasContent =>
        Placement is not null
        && (!string.IsNullOrWhiteSpace(Placement.IntroTextTitle)
            || !string.IsNullOrWhiteSpace(Placement.Text));

    protected override async Task OnInitializedAsync()
    {
        if (Placement is not null) return; // restored from prerender state

        Placement = await PlacementService.GetAsync(new GetPlacementRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            Key = PlacementKey,
            UkeyModel = FitmentState.UkeyModel ?? 0,
            ModelYear = FitmentState.Year?.ToString() ?? string.Empty,
            UkeyDriveline = FitmentState.UkeyDriveline ?? 0,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel ?? 0,
            UkeyCategory = FitmentState.UkeyCategory ?? 0
        });
    }
}
