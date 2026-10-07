using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

/// <summary>Placement of the tooltip relative to its trigger.</summary>
public enum SfTooltipPosition
{
    /// <summary>Above the trigger. Default.</summary>
    Top,
    /// <summary>Below the trigger.</summary>
    Bottom,
    /// <summary>To the left of the trigger.</summary>
    Left,
    /// <summary>To the right of the trigger.</summary>
    Right
}

public partial class SfTooltip : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The trigger element (button, icon, text). For keyboard users, the trigger
    /// should be focusable (e.g. a button or an element with tabindex="0").
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>The tooltip text, shown on hover and keyboard focus.</summary>
    [Parameter, EditorRequired] public string Text { get; set; } = string.Empty;

    /// <summary>Placement of the tooltip relative to the trigger.</summary>
    [Parameter] public SfTooltipPosition Position { get; set; } = SfTooltipPosition.Top;

    private string PositionClass => Position switch
    {
        SfTooltipPosition.Bottom => "bottom",
        SfTooltipPosition.Left => "left",
        SfTooltipPosition.Right => "right",
        _ => "top"
    };
}
