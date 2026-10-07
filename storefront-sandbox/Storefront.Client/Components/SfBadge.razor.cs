using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

/// <summary>Visual variants for <see cref="SfBadge"/>.</summary>
public enum SfBadgeVariant
{
    /// <summary>Neutral gray.</summary>
    Default,
    /// <summary>Blue tint. Info, fitment matches.</summary>
    Primary,
    /// <summary>Green tint. In stock, success.</summary>
    Success,
    /// <summary>Amber tint. Low stock, warnings.</summary>
    Warning,
    /// <summary>Red tint. Out of stock, errors.</summary>
    Danger
}

public partial class SfBadge : ComponentBase
{
    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Badge content.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Visual variant.</summary>
    [Parameter] public SfBadgeVariant Variant { get; set; } = SfBadgeVariant.Default;

    private string VariantClass => Variant switch
    {
        SfBadgeVariant.Primary => "primary",
        SfBadgeVariant.Success => "success",
        SfBadgeVariant.Warning => "warning",
        SfBadgeVariant.Danger => "danger",
        _ => "default"
    };
}
