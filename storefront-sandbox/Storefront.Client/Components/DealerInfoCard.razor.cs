using Microsoft.AspNetCore.Components;
using Storefront.Client.Models;

namespace Storefront.Client.Components;

public partial class DealerInfoCard : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Dealer contact and location details.</summary>
    [Parameter, EditorRequired] public DealerInfo Dealer { get; set; } = default!;

    /// <summary>Optional link to the warranty policy.</summary>
    [Parameter] public string? WarrantyUrl { get; set; }

    /// <summary>Optional link to the return policy.</summary>
    [Parameter] public string? ReturnPolicyUrl { get; set; }
}
