using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

// Generic placement renderer — mirrors legacy ~/Components/PlacementContent.ascx +
// PlacementContentStandard.vue. Fetches a placement by key and renders title + body
// (HTML, MarkupString) with an optional background-image div, all inside a wrapper
// whose only class is the caller-supplied ClassName so dealer stylesheets can hook in.
//
// Wrapper is a <div> (not the legacy <a> wrapping the whole thing) — Placement.Text
// commonly ships with its own <a> tags from the DB and nested anchors silently break
// rendering. The inner anchors carry the navigation themselves.
public partial class PlacementContent : ComponentBase
{
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    /// <summary>Placement key fetched from the server (legacy `placementTarget`).</summary>
    [Parameter, EditorRequired] public string PlacementKey { get; set; } = string.Empty;

    /// <summary>Class applied to the inner wrapper — dealer stylesheets target this.</summary>
    [Parameter, EditorRequired] public string ClassName { get; set; } = string.Empty;

    /// <summary>Render variant. Only "Standard" is implemented; add modes as DB rows surface them.</summary>
    [Parameter] public string Mode { get; set; } = "Standard";

    [PersistentState] public PlacementText? Placement { get; set; }

    private bool HasContent =>
        Placement is not null
        && (!string.IsNullOrWhiteSpace(Placement.IntroTextTitle)
            || !string.IsNullOrWhiteSpace(Placement.Text));

    protected override async Task OnInitializedAsync()
    {
        if (Placement is not null) return; // restored from prerender state
        if (string.IsNullOrWhiteSpace(PlacementKey)) return;

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
