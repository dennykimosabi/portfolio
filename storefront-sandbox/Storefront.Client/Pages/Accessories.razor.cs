using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Pages;

// Route decomposition, interpret resolution, and garage auto-save mirror Home.razor.cs exactly
// (same make-segmented URL shape, just under the /accessories prefix) — see that file for the
// reasoning behind each step. What renders once a category resolves differs: AccessoryResults
// (a product grid) instead of AssemblyResults (assemblies + part types).
public partial class Accessories : ComponentBase
{
    [Inject] public VehicleFitmentState Fitment { get; set; } = default!;
    [Inject] public IVehicleService VehicleService { get; set; } = default!;
    [Inject] public GarageState Garage { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ILogService Log { get; set; } = default!;

    [Parameter] public string? Slug1 { get; set; }   // make
    [Parameter] public string? Slug2 { get; set; }   // vehicle ({year}-{modelString}) OR category
    [Parameter] public string? Slug3 { get; set; }   // category (present only when Slug2 is a vehicle)

    [PersistentState] public InterpretSearchResult? _resolvedFitment { get; set; }
    [PersistentState] public string? _resolvedRoute { get; set; }

    private string? _routeMake;
    private int _routeYear;
    private string? _routeModelName;
    private string? _routeCategory;

    private bool HasVehicle =>
        Fitment.HasVehicle || (_routeYear > 0 && !string.IsNullOrWhiteSpace(_routeModelName));

    protected override async Task OnParametersSetAsync()
    {
        DecomposeRoute();

        // TEMP diagnostic logging (SPED accessories soft-nav investigation) — remove once the
        // "no products after picking a vehicle client-side" bug is root-caused. Confirms this
        // lifecycle method actually runs on soft nav, which branch it takes, and — via the
        // try/catch below — surfaces any exception that would otherwise silently stop the
        // Fitment.Set*/OnChange chain (Blazor WASM does not reliably surface an unhandled
        // exception from OnParametersSetAsync back to the page).
        await Log.LogInformation(
            $"OnParametersSetAsync: Slug1={Slug1}, Slug2={Slug2}, Slug3={Slug3} -> route Make={_routeMake}, Year={_routeYear}, Model={_routeModelName}, Category={_routeCategory}",
            "Accessories");

        try
        {
            // Still resolve make + a bare category with no vehicle in the URL — interpret returns the
            // dealer's make even with no year/model chosen (mirrors legacy, which carries a nonzero make
            // regardless of vehicle-selection state), and refineSearch needs a real UkeyMake to scope
            // fitment-gated results correctly. Mirrors Home.razor.cs's identical branch.
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
                await Log.LogInformation(
                    $"OnParametersSetAsync (no-vehicle branch): calling SetMakeAndCategory(UkeyMake={interpreted.UkeyMake}, UkeyCategory={interpreted.UkeyCategory})",
                    "Accessories");
                Fitment.SetMakeAndCategory(
                    interpreted.UkeyMake > 0 ? interpreted.UkeyMake : null,
                    interpreted.UkeyCategory > 0 ? interpreted.UkeyCategory : null);
                return;
            }

            var route = $"{_routeMake}/{_routeYear}/{_routeModelName}/{_routeCategory}";

            var needResolve = _resolvedFitment is null || _resolvedRoute != route;
            await Log.LogInformation(
                $"OnParametersSetAsync (vehicle branch): route={route}, needResolve={needResolve}, prior _resolvedRoute={_resolvedRoute}",
                "Accessories");
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

            if (_resolvedFitment is null || _resolvedFitment.UkeyModel <= 0)
            {
                await Log.LogInformation(
                    $"OnParametersSetAsync: bailing out, _resolvedFitment={(_resolvedFitment is null ? "null" : $"UkeyModel={_resolvedFitment.UkeyModel}")}",
                    "Accessories");
                return;
            }

            var year = int.TryParse(_resolvedFitment.ModelYear, out var yr) ? yr : _routeYear;

            await Log.LogInformation(
                $"OnParametersSetAsync: calling SetVehicleAndCategory(UkeyMake={_resolvedFitment.UkeyMake}, Year={year}, UkeyModel={_resolvedFitment.UkeyModel}, UkeyDriveLine={_resolvedFitment.UkeyDriveLine}, UkeyTrimLevel={_resolvedFitment.UkeyTrimLevel}, UkeyCategory={_resolvedFitment.UkeyCategory}); prior Fitment: UkeyMake={Fitment.UkeyMake}, Year={Fitment.Year}, UkeyModel={Fitment.UkeyModel}, UkeyDriveline={Fitment.UkeyDriveline}, UkeyTrimLevel={Fitment.UkeyTrimLevel}, UkeyCategory={Fitment.UkeyCategory}",
                "Accessories");

            // Set vehicle + category atomically (single OnChange) — see VehicleFitmentState.
            // SetVehicleAndCategory's doc comment for why a separate Set(...) + SetCategory(...)
            // pair here raced AccessoryResults into showing no products on client-side navigation.
            Fitment.SetVehicleAndCategory(
                _resolvedFitment.UkeyMake > 0 ? _resolvedFitment.UkeyMake : null,
                year,
                _resolvedFitment.UkeyModel,
                _resolvedFitment.UkeyDriveLine > 0 ? _resolvedFitment.UkeyDriveLine : null,
                _resolvedFitment.UkeyTrimLevel > 0 ? _resolvedFitment.UkeyTrimLevel : null,
                _resolvedFitment.UkeyCategory > 0 ? _resolvedFitment.UkeyCategory : null);

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
        catch (Exception ex)
        {
            await Log.LogError($"OnParametersSetAsync threw: {ex.Message}", "Accessories", ex.StackTrace);
            throw;
        }
    }

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
            _routeCategory = Slug2;
        }
    }
}
