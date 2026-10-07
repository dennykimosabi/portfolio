using Microsoft.AspNetCore.Components;
using Storefront.Client.Models;

namespace Storefront.Client.Components;

public partial class DealerInfoCard : ComponentBase
{
    [Parameter, EditorRequired] public DealerInfo Dealer { get; set; } = default!;

    [Parameter] public string? WarrantyUrl { get; set; }

    [Parameter] public string? ReturnPolicyUrl { get; set; }
}
