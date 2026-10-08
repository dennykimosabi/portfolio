using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

/// <summary>
/// Describes a single action (button or link) as data, so repeated action
/// patterns — dialog footers, page headers, toolbars, empty states — share one
/// shape instead of hand-rolled buttons. Render with <see cref="SfActionButton"/>.
/// </summary>
public sealed record SfActionDescriptor
{
    /// <summary>Visible label of the action.</summary>
    [Parameter] public string Content { get; init; } = string.Empty;

    /// <summary>
    /// When set, the action renders as a link to this URL instead of a button.
    /// Use only for real navigation.
    /// </summary>
    [Parameter] public string? Href { get; init; }

    /// <summary>Invoked when the action is clicked. Not fired while disabled.</summary>
    [Parameter] public EventCallback OnClick { get; init; }

    /// <summary>
    /// Accessible label for the action. Falls back to <see cref="Content"/> when not set.
    /// Set it when the visible label alone is ambiguous (e.g. several "Remove" buttons).
    /// </summary>
    [Parameter] public string? AccessibilityLabel { get; init; }

    /// <summary>Disables the action.</summary>
    [Parameter] public bool Disabled { get; init; }

    /// <summary>Visual variant of the rendered button.</summary>
    [Parameter] public SfButtonVariant Variant { get; init; } = SfButtonVariant.Primary;

    /// <summary>Additional CSS classes applied to the rendered button.</summary>
    [Parameter] public string? Class { get; init; }

    /// <summary>Additional attributes splatted onto the rendered button.</summary>
    [Parameter] public Dictionary<string, object>? AdditionalAttributes { get; init; }
}
