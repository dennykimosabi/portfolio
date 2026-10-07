using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

/// <summary>Shapes for <see cref="SfSkeleton"/>.</summary>
public enum SfSkeletonShape
{
    /// <summary>Rounded rectangle. Default.</summary>
    Rectangle,
    /// <summary>Fully round. Best with equal width and height (e.g. avatars).</summary>
    Circle,
    /// <summary>Pill. Best for single-line text placeholders.</summary>
    Pill
}

public partial class SfSkeleton : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>CSS width of the placeholder, e.g. "100%" or "12rem".</summary>
    [Parameter] public string Width { get; set; } = "100%";

    /// <summary>CSS height of the placeholder, e.g. "1rem" or "48px".</summary>
    [Parameter] public string Height { get; set; } = "1rem";

    /// <summary>Shape of the placeholder.</summary>
    [Parameter] public SfSkeletonShape Shape { get; set; } = SfSkeletonShape.Rectangle;

    private string ShapeClass => Shape switch
    {
        SfSkeletonShape.Circle => "circle",
        SfSkeletonShape.Pill => "pill",
        _ => "rectangle"
    };
}
