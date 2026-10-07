using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class PartTypeCard : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>The category part type rendered as a tile.</summary>
    [Parameter, EditorRequired] public CategoryPartType PartType { get; set; } = default!;

    private string PriceRange => $"{PartType.MinPrice:C} - {PartType.MaxPrice:C}";
}
