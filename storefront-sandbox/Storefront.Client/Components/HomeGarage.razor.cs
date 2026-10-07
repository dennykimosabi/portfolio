using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class HomeGarage : ComponentBase, IDisposable
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public GarageState Garage { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    private const int CollapsedCount = 2;
    private bool _expanded;

    // Collapsed shows the first two; expanded shows all.
    private IEnumerable<CustomerVehicle> Visible =>
        _expanded ? Garage.Vehicles : Garage.Vehicles.Take(CollapsedCount);

    protected override async Task OnInitializedAsync()
    {
        // Re-render on garage changes (add/remove/rename) and on active-vehicle changes (card
        // highlight). GarageState.EnsureLoadedAsync is idempotent — shared with the header.
        Garage.OnChange += HandleStateChanged;
        FitmentState.OnChange += HandleStateChanged;
        await Garage.EnsureLoadedAsync();
    }

    private void ToggleExpanded() => _expanded = !_expanded;

    // Action-based state-change handler — kept off the async-void path (unobserved
    // exceptions there can tear down the renderer). InvokeAsync(StateHasChanged) only
    // dispatches a render, so fire-and-forget is safe here.
    private void HandleStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Garage.OnChange -= HandleStateChanged;
        FitmentState.OnChange -= HandleStateChanged;
    }
}
