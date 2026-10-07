using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public enum VehiclePickerLayout
{
    /// <summary>Button + dropdown panel; used in the storefront header.</summary>
    Panel,

    /// <summary>Always-visible inline form; used on the home page below the hero.</summary>
    Inline
}

public partial class VehiclePicker : ComponentBase, IDisposable
{
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IProductSearchService ProductSearchService { get; set; } = default!;
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public GarageState Garage { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    [Parameter] public VehiclePickerLayout Layout { get; set; } = VehiclePickerLayout.Inline;
    [Parameter] public string? SubmitLabel { get; set; }
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// When false, only Year/Model render (no Driveline/Trim Level) — e.g. /accessories, which
    /// only needs a model-level match. IsVehicleComplete already treats an empty _drivelines/
    /// _trimLevels list as "nothing to require," so skipping their load calls is enough; no
    /// other completion/navigation logic needs to change.
    /// </summary>
    [Parameter] public bool ShowDrivelineAndTrim { get; set; } = true;

    /// <summary>
    /// pr_refineSearch computes both linkURL (home) and accessoryLinkURL (/accessories) on every
    /// fitment row. Home wants the former; /accessories wants the latter, so submitting a vehicle
    /// there stays on /accessories instead of bouncing to home.
    /// </summary>
    [Parameter] public bool UseAccessoryLinks { get; set; }

    /// <summary>
    /// Legacy: VehiclePicker.ascx reads ControlModifications.PlacementTarget and pulls
    /// pr_returnPlacementDynamicText for that key, using <c>IntroTextTitle</c> as the heading.
    /// Pass "VehiclePickerHeading" (or whatever the dealer's control modifications point at)
    /// to wire the same behavior here.
    /// </summary>
    [Parameter] public string? PlacementKey { get; set; }

    [PersistentState]
    public PlacementText? HeadingPlacement { get; set; }

    // Persisted across SSR → WASM hydration so the Inline picker doesn't re-fetch the
    // years list on first render. The dependent lists (_models / _drivelines / _trimLevels)
    // load on demand after the user picks a year, so they don't need persistence.
    [PersistentState]
    public List<RefineSearchResult>? Years { get; set; }

    // Prerender snapshot of the garage list, persisted so WASM hydration restores it and seeds
    // the shared GarageState instead of re-fetching — avoids the load-then-reload flash, matching
    // how the rest of the app uses [PersistentState]. Only the Panel (header) instance uses this;
    // it's always present (layout) and inits before page consumers (HomeGarage).
    [PersistentState]
    public List<CustomerVehicle>? GarageSnapshot { get; set; }

    // Within an open Panel: false = show the garage list, true = show the add-vehicle dropdowns.
    private bool _showAddVehicle;

    private string ResolvedSubmitLabel =>
        SubmitLabel ?? (Layout == VehiclePickerLayout.Panel ? "Shop Parts" : "+ Add Vehicle");

    private string ResolvedTitle
    {
        get
        {
            if (HeadingPlacement is not null)
            {
                if (!string.IsNullOrWhiteSpace(HeadingPlacement.IntroTextTitle))
                    return HeadingPlacement.IntroTextTitle;
                if (!string.IsNullOrWhiteSpace(HeadingPlacement.Title))
                    return HeadingPlacement.Title;
            }
            return Title ?? "Choose your Vehicle to start shopping:";
        }
    }

    private bool _panelOpen;
    private bool _loadingYears;
    private bool _loadingModels;
    private bool _loadingDrivelines;
    private bool _loadingTrimLevels;

    private List<RefineSearchResult> _models = [];
    private List<RefineSearchResult> _drivelines = [];
    private List<RefineSearchResult> _trimLevels = [];

    // Lets OnYearChanged/OnModelChanged/OnDrivelineChanged auto-open the next dropdown in the
    // cascade once it has options to show, instead of making the shopper click each one in turn.
    private SearchableSelect? _modelSelect;
    private SearchableSelect? _drivelineSelect;
    private SearchableSelect? _trimLevelSelect;

    private string _selectedYear = string.Empty;
    private string _selectedModel = string.Empty;
    private string _selectedDriveline = string.Empty;
    private string _selectedTrimLevel = string.Empty;

    private RefineSearchResult? _selectedModelResult;
    private RefineSearchResult? _selectedDrivelineResult;
    private RefineSearchResult? _selectedTrimLevelResult;

    private string _vinInput = string.Empty;

    private bool IsVehicleComplete =>
        !string.IsNullOrEmpty(_selectedYear) &&
        !string.IsNullOrEmpty(_selectedModel) &&
        (_drivelines.Count == 0 || !string.IsNullOrEmpty(_selectedDriveline)) &&
        (_trimLevels.Count == 0 || !string.IsNullOrEmpty(_selectedTrimLevel));

    // The saved vehicle matching the active fitment, if any — used to label the header button
    // and highlight the current row in the garage list.
    private CustomerVehicle? CurrentGarageVehicle =>
        !FitmentState.HasVehicle
            ? null
            : Garage.Vehicles.FirstOrDefault(v =>
                v.UkeyModel == (FitmentState.UkeyModel ?? -1) &&
                v.ModelYear == (FitmentState.Year ?? -1) &&
                v.UkeyDriveline == (FitmentState.UkeyDriveline ?? 0) &&
                v.UkeyTrimLevel == (FitmentState.UkeyTrimLevel ?? 0));

    private string PanelButtonLabel
    {
        get
        {
            // Prefer the active vehicle (post-pick / on a fitment page).
            var current = CurrentGarageVehicle;
            if (current is not null)
                return !string.IsNullOrWhiteSpace(current.VehicleDescription)
                    ? current.VehicleDescription!
                    : current.ModelString;

            // Else reflect an in-progress dropdown selection (mid-pick).
            if (!string.IsNullOrEmpty(_selectedYear))
            {
                var parts = new List<string> { _selectedYear };
                if (_selectedModelResult is not null) parts.Add(DisplayText(_selectedModelResult));
                if (_selectedDrivelineResult is not null) parts.Add(DisplayText(_selectedDrivelineResult));
                if (_selectedTrimLevelResult is not null) parts.Add(DisplayText(_selectedTrimLevelResult));
                return string.Join(" ", parts);
            }

            return "Select Your Vehicle";
        }
    }

    private bool HasGarage => Garage.Vehicles.Count > 0;

    protected override async Task OnInitializedAsync()
    {
        // Re-render when the active vehicle changes (label/highlight) or the garage list changes
        // (a save/remove elsewhere). GarageState.OnChange fires after the write completes, so the
        // header reflects a newly-saved vehicle with no save-timing lag.
        FitmentState.OnChange += HandleStateChanged;
        Garage.OnChange += HandleStateChanged;

        if (Layout == VehiclePickerLayout.Inline)
        {
            // Inline mode is always visible — load years + heading up front.
            var yearsTask = Years is null ? LoadYears() : Task.CompletedTask;
            var headingTask = LoadHeadingPlacement();
            await Task.WhenAll(yearsTask, headingTask);
            return;
        }

        // Panel mode (header) carries the persistence bridge for the shared GarageState: on WASM
        // hydration seed it from the prerendered snapshot (no re-fetch → no flash); otherwise load
        // it (SSR / cold client). Seeding is synchronous and the header inits before page consumers
        // like HomeGarage, so they never trigger a hydration fetch.
        if (GarageSnapshot is not null)
            Garage.Seed(GarageSnapshot);
        else
            await Garage.EnsureLoadedAsync();

        GarageSnapshot = Garage.Vehicles.ToList();
    }

    // Action-based state-change handler — kept off the async-void path (unobserved
    // exceptions there can tear down the renderer). InvokeAsync(StateHasChanged) only
    // dispatches a render, so fire-and-forget is safe here.
    private void HandleStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        FitmentState.OnChange -= HandleStateChanged;
        Garage.OnChange -= HandleStateChanged;
    }

    private async Task LoadHeadingPlacement()
    {
        if (HeadingPlacement is not null) return; // restored from prerender state
        if (string.IsNullOrWhiteSpace(PlacementKey)) return;

        HeadingPlacement = await PlacementService.GetAsync(new GetPlacementRequest
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

    private async Task TogglePanel()
    {
        _panelOpen = !_panelOpen;
        if (!_panelOpen) return;

        // Open to the garage list when there are saved vehicles; otherwise straight to the
        // add-vehicle dropdowns (which need the years list).
        _showAddVehicle = !HasGarage;
        if (_showAddVehicle && Years is null)
            await LoadYears();
    }

    private void ClosePanel() => _panelOpen = false;

    private async Task ShowAddVehicle()
    {
        _showAddVehicle = true;
        if (Years is null) await LoadYears();
    }

    private void ShowGarage() => _showAddVehicle = false;

    private async Task ClearGarage()
    {
        await Garage.ClearAsync();
        _showAddVehicle = true;
        FitmentState.Clear();
        if (Years is null) await LoadYears();
    }

    private void Clear()
    {
        _selectedYear = string.Empty;
        _selectedModel = string.Empty;
        _selectedDriveline = string.Empty;
        _selectedTrimLevel = string.Empty;
        _selectedModelResult = null;
        _selectedDrivelineResult = null;
        _selectedTrimLevelResult = null;
        _models = [];
        _drivelines = [];
        _trimLevels = [];
        _panelOpen = false;
        FitmentState.Clear();
    }

    private async Task OnYearChanged(string value)
    {
        _selectedYear = value;
        _selectedModel = string.Empty;
        _selectedDriveline = string.Empty;
        _selectedTrimLevel = string.Empty;
        _selectedModelResult = null;
        _selectedDrivelineResult = null;
        _selectedTrimLevelResult = null;
        _models = [];
        _drivelines = [];
        _trimLevels = [];

        if (!string.IsNullOrEmpty(_selectedYear))
        {
            await LoadModels();
            if (_models.Count > 0)
                _modelSelect?.Open();
        }
    }

    private async Task OnModelChanged(string value)
    {
        _selectedModel = value;
        _selectedModelResult = FindByUkey(_models, _selectedModel);
        _selectedDriveline = string.Empty;
        _selectedTrimLevel = string.Empty;
        _selectedDrivelineResult = null;
        _selectedTrimLevelResult = null;
        _drivelines = [];
        _trimLevels = [];

        if (ShowDrivelineAndTrim && _selectedModelResult is not null)
        {
            await LoadDrivelines();
            // LoadDrivelines already navigated away if the list came back empty.
            if (_drivelines.Count > 0)
                _drivelineSelect?.Open();
        }
    }

    private async Task OnDrivelineChanged(string value)
    {
        _selectedDriveline = value;
        _selectedDrivelineResult = FindByUkey(_drivelines, _selectedDriveline);
        _selectedTrimLevel = string.Empty;
        _selectedTrimLevelResult = null;
        _trimLevels = [];

        if (_selectedDrivelineResult is not null)
        {
            await LoadTrimLevels();
            // LoadTrimLevels already navigated away if the list came back empty.
            if (_trimLevels.Count > 0)
                _trimLevelSelect?.Open();
        }
    }

    private void OnTrimLevelChanged(string value)
    {
        _selectedTrimLevel = value;
        _selectedTrimLevelResult = FindByUkey(_trimLevels, _selectedTrimLevel);
    }

    private void Submit()
    {
        if (!IsVehicleComplete) return;
        SetFitmentAndNavigate();
    }

    private void SetFitmentAndNavigate()
    {
        var year = int.TryParse(_selectedYear, out var y) ? y : 0;
        FitmentState.Set(
            null,  // Make is not selected in picker; determined by route/site
            year,
            _selectedModelResult!.Ukey,
            _selectedDrivelineResult?.Ukey,
            _selectedTrimLevelResult?.Ukey);

        // Navigate to the DB-built storefront URL of the deepest level the shopper actually picked
        // — the refine sproc emits each fitment row's LinkURL/AccessoryLinkURL (make-segmented).
        // No client-side URL construction; the DB is the single source of shop URLs.
        var target = UseAccessoryLinks
            ? _selectedTrimLevelResult?.AccessoryLinkURL
                ?? _selectedDrivelineResult?.AccessoryLinkURL
                ?? _selectedModelResult?.AccessoryLinkURL
            : _selectedTrimLevelResult?.LinkURL
                ?? _selectedDrivelineResult?.LinkURL
                ?? _selectedModelResult?.LinkURL;
        if (!string.IsNullOrWhiteSpace(target))
            NavigationManager.NavigateTo(target);
        _panelOpen = false;
    }

    private void SearchVin()
    {
        // TODO: VIN lookup flow TBD
    }

    private void OnVinKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") SearchVin();
    }

    private async Task LoadYears()
    {
        _loadingYears = true;
        var results = await ProductSearchService.GetRefinedSearchAsync(new RefineSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyCategory = FitmentState.UkeyCategory,
            ShowFitmentLevel = "year",
            ShowCategory = false
        });
        Years = results.ToList();
        _loadingYears = false;
    }

    private async Task LoadModels()
    {
        _loadingModels = true;
        var year = int.TryParse(_selectedYear, out var y) ? y : (int?)null;
        var results = await ProductSearchService.GetRefinedSearchAsync(new RefineSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyCategory = FitmentState.UkeyCategory,
            ShowFitmentLevel = "model",
            Year = year,
            ShowCategory = false
        });
        _models = results.ToList();
        _loadingModels = false;
    }

    private async Task LoadDrivelines()
    {
        _loadingDrivelines = true;
        var year = int.TryParse(_selectedYear, out var y) ? y : (int?)null;
        var results = await ProductSearchService.GetRefinedSearchAsync(new RefineSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyCategory = FitmentState.UkeyCategory,
            ShowFitmentLevel = "driveline",
            Year = year,
            UkeyModel = _selectedModelResult is not null ? (int?)_selectedModelResult.Ukey : null,
            ShowCategory = false
        });
        _drivelines = results.ToList();
        _loadingDrivelines = false;

        if (_drivelines.Count == 0)
            SetFitmentAndNavigate();
    }

    private async Task LoadTrimLevels()
    {
        _loadingTrimLevels = true;
        var year = int.TryParse(_selectedYear, out var y) ? y : (int?)null;
        var results = await ProductSearchService.GetRefinedSearchAsync(new RefineSearchRequest
        {
            SessionId = SessionState.SessionId,
            MachineId = SessionState.MachineId,
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            UkeyCategory = FitmentState.UkeyCategory,
            ShowFitmentLevel = "trimlevel",
            Year = year,
            UkeyModel = _selectedModelResult is not null ? (int?)_selectedModelResult.Ukey : null,
            UkeyDriveline = _selectedDrivelineResult is not null ? (int?)_selectedDrivelineResult.Ukey : null,
            ShowCategory = false
        });
        _trimLevels = results.ToList();
        _loadingTrimLevels = false;

        if (_trimLevels.Count == 0)
            SetFitmentAndNavigate();
    }

    private static RefineSearchResult? FindByUkey(List<RefineSearchResult> list, string ukey) =>
        long.TryParse(ukey, out var id) ? list.FirstOrDefault(r => r.Ukey == id) : null;

    private static string DisplayText(RefineSearchResult r) =>
        !string.IsNullOrWhiteSpace(r.SimpleString) ? r.SimpleString : r.DisplayString;
}
