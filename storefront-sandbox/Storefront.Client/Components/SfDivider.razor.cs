using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

/// <summary>Vertical spacing for <see cref="SfDivider"/>.</summary>
public enum SfDividerSpacing
{
    /// <summary>Compact spacing.</summary>
    Sm,
    /// <summary>Default spacing.</summary>
    Md,
    /// <summary>Generous spacing.</summary>
    Lg
}

public partial class SfDivider : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Optional centered label text. When set, the divider renders a line on either
    /// side of the label; otherwise it renders a single full-width line.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>Vertical spacing around the divider.</summary>
    [Parameter] public SfDividerSpacing Spacing { get; set; } = SfDividerSpacing.Md;

    private bool HasLabel => !string.IsNullOrEmpty(Label);

    private string SpacingClass => Spacing switch
    {
        SfDividerSpacing.Sm => "sm",
        SfDividerSpacing.Lg => "lg",
        _ => "md"
    };
}
