using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Storefront.Client.Components;

/// <summary>Visual variants for <see cref="SfButton"/>.</summary>
public enum SfButtonVariant
{
    /// <summary>Brand blue fill. Default for primary actions.</summary>
    Primary,
    /// <summary>Dark brand fill, used for add-to-cart and submit actions.</summary>
    Dark,
    /// <summary>Subtle fill for secondary actions.</summary>
    Secondary,
    /// <summary>Transparent with border.</summary>
    Outline,
    /// <summary>Transparent, no border; hover shows a subtle fill.</summary>
    Ghost,
    /// <summary>Destructive actions.</summary>
    Danger
}

/// <summary>Sizes for <see cref="SfButton"/>.</summary>
public enum SfButtonSize
{
    /// <summary>Compact, e.g. table rows and chips.</summary>
    Sm,
    /// <summary>Default.</summary>
    Md,
    /// <summary>Large, e.g. full-width purchase actions.</summary>
    Lg
}

public partial class SfButton : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Button content: label, icon, or both.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Visual variant.</summary>
    [Parameter] public SfButtonVariant Variant { get; set; } = SfButtonVariant.Primary;

    /// <summary>Size.</summary>
    [Parameter] public SfButtonSize Size { get; set; } = SfButtonSize.Md;

    /// <summary>HTML button type.</summary>
    [Parameter] public string Type { get; set; } = "button";

    /// <summary>Disables the button.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Shows a spinner and disables the button while an action runs.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Invoked on click. Not fired while disabled or loading.</summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    private string VariantClass => Variant switch
    {
        SfButtonVariant.Dark => "dark",
        SfButtonVariant.Secondary => "secondary",
        SfButtonVariant.Outline => "outline",
        SfButtonVariant.Ghost => "ghost",
        SfButtonVariant.Danger => "danger",
        _ => "primary"
    };

    private string SizeClass => Size switch
    {
        SfButtonSize.Sm => "sm",
        SfButtonSize.Lg => "lg",
        _ => "md"
    };

    private async Task HandleClick(MouseEventArgs args)
    {
        if (Disabled || Loading) return;
        await OnClick.InvokeAsync(args);
    }
}
