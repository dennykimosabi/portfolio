using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Pages;

public partial class Home : ComponentBase
{
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;
    [Inject] public IVehicleService VehicleService { get; set; } = default!;
    [Inject] public GarageState Garage { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;

    // Make-segmented route: /{make}/{year}-{modelString}[/{category}], or /{make}/{category}, or /{make}.
    [Parameter] public string? Slug1 { get; set; }   // make
    [Parameter] public string? Slug2 { get; set; }   // vehicle ({year}-{modelString}) OR category
    [Parameter] public string? Slug3 { get; set; }   // category (present only when Slug2 is a vehicle)

    // interpret echoes the resolved ukeys back; persisted so WASM hydration restores them
    // without re-calling interpret (SSR already populated FitmentState for the first paint).
    [PersistentState] public InterpretSearchResult? _resolvedFitment { get; set; }

    // The route signature the _resolvedFitment corresponds to. Persisted alongside it so hydration
    // knows the restored resolution is still current and skips a re-interpret.
    [PersistentState] public string? _resolvedRoute { get; set; }

    // Decomposed from the route on each parameter change (see DecomposeRoute).
    private string? _routeMake;
    private int _routeYear;
    private string? _routeModelName;
    private string? _routeCategory;

    // In-session picks live in FitmentState; a deep-link / hard refresh carries the vehicle in the
    // route (make + {year}-{modelString}). Either signal means "show the picked card."
    private bool HasVehicle =>
        Fitment.HasVehicle || (_routeYear > 0 && !string.IsNullOrWhiteSpace(_routeModelName));

    // Runs on every parameter change — including vehicle→vehicle navigation, where Blazor reuses
    // this Home instance so OnInitializedAsync would NOT run again. Re-resolving here (and firing
    // FitmentState.OnChange via Set) is what refreshes the hero + picked card on each switch.
    protected override async Task OnParametersSetAsync()
    {
        DecomposeRoute();

        // No vehicle in the URL (site root, make landing, or make+category). Still resolve a bare
        // category (/{make}/{category}) so its ukey flows to refineSearch + placements + AssemblyResults,
        // and always resolve make too — interpret returns the dealer's make even with no year/model
        // chosen (mirrors legacy, which carries a nonzero make regardless of vehicle-selection state),
        // and refineSearch needs a real UkeyMake to scope fitment-gated results correctly.
        if (_routeYear <= 0 || string.IsNullOrWhiteSpace(_routeModelName))
        {
            var interpreted = await VehicleService.GetInterpretationAsync(new InterpretSearchRequest
            {
                SessionId = SessionState.SessionId,
                MachineId = SessionState.MachineId,
                UkeyWebsite = WebsiteContext.UkeyWebsite,
                MakeName = _routeMake ?? string.Empty,
                CategoryName = _routeCategory ?? string.Empty
            });
            Fitment.SetMakeAndCategory(
                interpreted.UkeyMake > 0 ? interpreted.UkeyMake : null,
                interpreted.UkeyCategory > 0 ? interpreted.UkeyCategory : null);
            // AssemblyResults reacts to Fitment.OnChange (fired once by SetMakeAndCategory above) and
            // loads/clears itself — nothing further to do here.
            return;
        }

        // The route is the source of truth for "which vehicle is active." Resolve the full fitment via
        // interpret — including UkeyMake, which the garage save requires. The make slug is seg1; the
        // model+driveline+trim ride inside the compound modelName that interpret parses back to ukeys
        // (the segments are already fn_cleanForURL slugs, built by fn_returnStorefrontURL).
        var route = $"{_routeMake}/{_routeYear}/{_routeModelName}/{_routeCategory}";

        // needResolve is false only when persisted state already matches this route (hydration) —
        // which also gates the garage save so it fires once per real resolution, not on hydration.
        var needResolve = _resolvedFitment is null || _resolvedRoute != route;
        if (needResolve)
        {
            _resolvedFitment = await VehicleService.GetInterpretationAsync(new InterpretSearchRequest
            {
                SessionId = SessionState.SessionId,
                MachineId = SessionState.MachineId,
                UkeyWebsite = WebsiteContext.UkeyWebsite,
                MakeName = _routeMake ?? string.Empty,
                ModelYear = _routeYear.ToString(),
                ModelName = _routeModelName,
                CategoryName = _routeCategory ?? string.Empty
            });
            _resolvedRoute = route;
        }

        if (_resolvedFitment is null || _resolvedFitment.UkeyModel <= 0) return;

        var year = int.TryParse(_resolvedFitment.ModelYear, out var yr) ? yr : _routeYear;

        // Category rides alongside the vehicle (Slug3, resolved by the same interpret call). Null when
        // the URL carries no category segment, which clears any previously-active category.
        // Set vehicle + category atomically (single OnChange) — a separate Set(...) + SetCategory(...)
        // pair here raced AssemblyResults/RefineSearch into transiently loading against a stale
        // category on client-side navigation (see VehicleFitmentState.SetVehicleAndCategory). Still
        // only fires OnChange on a real change, so redundant calls (e.g. a garage/dropdown pick
        // already set the same fitment before navigating) don't double-fetch.
        Fitment.SetVehicleAndCategory(
            _resolvedFitment.UkeyMake > 0 ? _resolvedFitment.UkeyMake : null,
            year,
            _resolvedFitment.UkeyModel,
            _resolvedFitment.UkeyDriveLine > 0 ? _resolvedFitment.UkeyDriveLine : null,
            _resolvedFitment.UkeyTrimLevel > 0 ? _resolvedFitment.UkeyTrimLevel : null,
            _resolvedFitment.UkeyCategory > 0 ? _resolvedFitment.UkeyCategory : null);

        // Auto-save the active vehicle to the garage (mirrors legacy VehiclePicked.vue). Only when we
        // actually resolved it and interpret gave us a make. Saving through GarageState inserts then
        // reloads and fires its OnChange, so the header's list reflects the new vehicle right after
        // the write — no save-timing lag. GarageService swallows failures so this never blocks.
        if (needResolve && _resolvedFitment.UkeyMake > 0)
        {
            await Garage.AddAsync(
                _resolvedFitment.UkeyMake,
                _resolvedFitment.UkeyModel,
                _resolvedFitment.UkeyDriveLine,
                _resolvedFitment.UkeyTrimLevel,
                year);
        }
    }

    // Decompose the make-segmented route:
    //   /{make}/{year}-{modelString}[/{category}]  → make + vehicle (+ optional category)
    //   /{make}/{category}                          → make + category (seg2 is not year-led)
    //   /{make}                                     → make only
    // The vehicle segment always starts with the 4-digit model year, which is how it's told apart
    // from a category segment (categories never lead with a 4-digit year).
    private void DecomposeRoute()
    {
        _routeMake = string.IsNullOrWhiteSpace(Slug1) ? null : Slug1;
        _routeYear = 0;
        _routeModelName = null;
        _routeCategory = null;

        if (string.IsNullOrWhiteSpace(Slug2)) return;

        var dash = Slug2.IndexOf('-');
        var firstToken = dash >= 0 ? Slug2[..dash] : Slug2;
        if (firstToken.Length == 4 && int.TryParse(firstToken, out var y) && y is >= 1900 and <= 2100)
        {
            _routeYear = y;
            _routeModelName = dash >= 0 ? Slug2[(dash + 1)..] : string.Empty;
            _routeCategory = string.IsNullOrWhiteSpace(Slug3) ? null : Slug3;
        }
        else
        {
            // seg2 is a category (make + category, no vehicle)
            _routeCategory = Slug2;
        }
    }
}
