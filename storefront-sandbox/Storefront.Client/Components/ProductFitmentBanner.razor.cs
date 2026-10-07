using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ProductFitmentBanner : ComponentBase
{
    [Parameter] public bool Fits { get; set; } = true;

    /// Full fitment string, e.g. "2022 Volvo XC90 AWD T6 Momentum".
    [Parameter] public string VehicleDescription { get; set; } = string.Empty;

    [Parameter] public EventCallback OnMoreProducts { get; set; }

    [Parameter] public EventCallback OnChangeVehicle { get; set; }
}
