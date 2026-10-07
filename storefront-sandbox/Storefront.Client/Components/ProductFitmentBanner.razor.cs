using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ProductFitmentBanner : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Whether the product fits the active vehicle.</summary>
    [Parameter] public bool Fits { get; set; } = true;

    /// Full fitment string, e.g. "2022 Volvo XC90 AWD T6 Momentum".
    [Parameter] public string VehicleDescription { get; set; } = string.Empty;

    /// <summary>Invoked from the fits state CTA.</summary>
    [Parameter] public EventCallback OnMoreProducts { get; set; }

    /// <summary>Invoked from the change-vehicle CTA.</summary>
    [Parameter] public EventCallback OnChangeVehicle { get; set; }
}
