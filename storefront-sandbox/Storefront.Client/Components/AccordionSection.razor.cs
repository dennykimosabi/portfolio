using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class AccordionSection : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Header text shown on the collapsed row.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;

    /// Collapsed-row summary text. Ignored when MetaContent is supplied.
    [Parameter] public string? Meta { get; set; }

    /// Collapsed-row summary as markup, for cases like the star rating on Product Reviews.
    [Parameter] public RenderFragment? MetaContent { get; set; }

    /// <summary>Whether the section body is open. Supports two-way binding.</summary>
    [Parameter] public bool Expanded { get; set; }

    /// <summary>Invoked when the open state toggles.</summary>
    [Parameter] public EventCallback<bool> ExpandedChanged { get; set; }

    /// <summary>Body content rendered only when the section is open.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private async Task Toggle()
    {
        Expanded = !Expanded;

        // Two-way bindable so a parent can drive/observe it (e.g. open one section at a time),
        // while still working uncontrolled when nobody binds.
        if (ExpandedChanged.HasDelegate)
        {
            await ExpandedChanged.InvokeAsync(Expanded);
        }
    }
}
