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
    Lg,
    /// <summary>Square icon-only, small.</summary>
    IconSm,
    /// <summary>Square icon-only, default.</summary>
    Icon,
    /// <summary>Square icon-only, large.</summary>
    IconLg
}

/// <summary>HTML button types for <see cref="SfButton"/>.</summary>
public enum SfButtonType
{
    /// <summary>Plain button, no form interaction.</summary>
    Button,
    /// <summary>Submits the enclosing form.</summary>
    Submit,
    /// <summary>Resets the enclosing form.</summary>
    Reset
}

/// <summary>Icon placement inside <see cref="SfButton"/>.</summary>
public enum SfIconPosition
{
    /// <summary>Icon before the label.</summary>
    Start,
    /// <summary>Icon after the label.</summary>
    End
}

public partial class SfButton : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Button content: label, icon, or both.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Icon rendered alongside the label. Positioned via <see cref="IconPosition"/>.</summary>
    [Parameter] public RenderFragment? Icon { get; set; }

    /// <summary>Where <see cref="Icon"/> renders relative to the label.</summary>
    [Parameter] public SfIconPosition IconPosition { get; set; } = SfIconPosition.Start;

    /// <summary>Visual variant.</summary>
    [Parameter] public SfButtonVariant Variant { get; set; } = SfButtonVariant.Primary;

    /// <summary>Size. Icon* sizes render square icon-only buttons.</summary>
    [Parameter] public SfButtonSize Size { get; set; } = SfButtonSize.Md;

    /// <summary>HTML button type.</summary>
    [Parameter] public SfButtonType Type { get; set; } = SfButtonType.Button;

    /// <summary>When set, renders an &lt;a&gt; anchor instead of a &lt;button&gt;.</summary>
    [Parameter] public string? Href { get; set; }

    /// <summary>Anchor target. Only used when <see cref="Href"/> is set.</summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>Disables the button.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Shows a spinner and disables the button while an action runs.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Label shown while <see cref="Loading"/> instead of <see cref="ChildContent"/>.</summary>
    [Parameter] public string? LoadingText { get; set; }

    /// <summary>Accessible label. Required for icon-only buttons.</summary>
    [Parameter] public string? AriaLabel { get; set; }

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
        SfButtonSize.IconSm => "icon-sm",
        SfButtonSize.Icon => "icon",
        SfButtonSize.IconLg => "icon-lg",
        _ => "md"
    };

    private string TypeAttr => Type switch
    {
        SfButtonType.Submit => "submit",
        SfButtonType.Reset => "reset",
        _ => "button"
    };

    private bool IsDisabled => Disabled || Loading;

    private async Task HandleClick(MouseEventArgs args)
    {
        if (IsDisabled) return;
        await OnClick.InvokeAsync(args);
    }

    private RenderFragment ButtonContent => builder =>
    {
        var seq = 0;
        if (Loading)
            builder.AddContent(seq++, (RenderFragment)(b =>
            {
                b.OpenElement(seq++, "span");
                b.AddAttribute(seq++, "class", "sf-button-spinner");
                b.AddAttribute(seq++, "aria-hidden", "true");
                b.CloseElement();
            }));
        else if (Icon is not null && IconPosition == SfIconPosition.Start)
            builder.AddContent(seq++, Icon);

        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "sf-button-content");
        if (Loading && LoadingText is not null)
            builder.AddContent(seq++, LoadingText);
        else
            builder.AddContent(seq++, ChildContent);
        builder.CloseElement();

        if (!Loading && Icon is not null && IconPosition == SfIconPosition.End)
            builder.AddContent(seq++, Icon);
    };
}
