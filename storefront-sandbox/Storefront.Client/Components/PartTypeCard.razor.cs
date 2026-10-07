using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class PartTypeCard : ComponentBase
{
    [Parameter, EditorRequired] public CategoryPartType PartType { get; set; } = default!;

    private string PriceRange => $"{PartType.MinPrice:C} - {PartType.MaxPrice:C}";
}
