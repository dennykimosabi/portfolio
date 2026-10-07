using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;
using Storefront.Client.Utilities;

namespace Storefront.Client.Components;

public partial class GarageVehicleCard : ComponentBase
{
    [Inject] public GarageState Garage { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public IClientAssetResolver AssetResolver { get; set; } = default!;

    // Brand/UI icons live on the asset host, not the site origin — resolve the static paths.
    private string EditIcon => AssetResolver.Resolve("/images/default-assets/icons/mygarage-edit.png");
    private string DeleteIcon => AssetResolver.Resolve("/images/default-assets/icons/mygarage-delete.png");

    [Parameter, EditorRequired] public CustomerVehicle Vehicle { get; set; } = default!;

    /// <summary>Raised after a successful select (before navigation completes) so a host can react
    /// — e.g. the header closes its dropdown panel. Home leaves it unset.</summary>
    [Parameter] public EventCallback OnSelected { get; set; }

    private bool _editing;
    private string _nameInput = string.Empty;

    // The label: the user's nickname if set, otherwise the fitment string.
    private string DisplayName =>
        !string.IsNullOrWhiteSpace(Vehicle.VehicleDescription) ? Vehicle.VehicleDescription! : Vehicle.ModelString;

    // Highlighted when this row matches the active fitment. Recomputed on render; the host
    // re-renders on FitmentState.OnChange, so the highlight tracks the current vehicle.
    private bool IsCurrent =>
        FitmentState.HasVehicle
        && Vehicle.UkeyModel == (FitmentState.UkeyModel ?? -1)
        && Vehicle.ModelYear == (FitmentState.Year ?? -1)
        && Vehicle.UkeyDriveline == (FitmentState.UkeyDriveline ?? 0)
        && Vehicle.UkeyTrimLevel == (FitmentState.UkeyTrimLevel ?? 0);

    private async Task Select()
    {
        // Seed FitmentState from the exact ukeys for instant display, then navigate to the same
        // segmented URL shape the dropdowns produce (Home re-resolves it on arrival).
        FitmentState.Set(
            Vehicle.UkeyMake > 0 ? Vehicle.UkeyMake : null,
            Vehicle.ModelYear,
            Vehicle.UkeyModel,
            Vehicle.UkeyDriveline > 0 ? Vehicle.UkeyDriveline : null,
            Vehicle.UkeyTrimLevel > 0 ? Vehicle.UkeyTrimLevel : null);

        await OnSelected.InvokeAsync();
        // Navigate to the DB-built storefront URL for this saved vehicle (sproc's linkURL). Home
        // re-resolves it on arrival.
        if (!string.IsNullOrWhiteSpace(Vehicle.LinkURL))
            NavigationManager.NavigateTo(Vehicle.LinkURL);
    }

    private void StartEdit()
    {
        _nameInput = DisplayName;
        _editing = true;
    }

    private void CancelEdit() => _editing = false;

    private async Task SaveEdit()
    {
        var name = _nameInput.Trim();
        _editing = false;
        if (!string.IsNullOrEmpty(name))
            await Garage.RenameAsync(Vehicle.Ukey, name);
    }

    private async Task OnEditKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await SaveEdit();
        else if (e.Key == "Escape") CancelEdit();
    }

    private Task Delete() => Garage.RemoveAsync(Vehicle.Ukey);
}
