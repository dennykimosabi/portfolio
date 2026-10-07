using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class AccordionSection : ComponentBase
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;

    /// Collapsed-row summary text. Ignored when MetaContent is supplied.
    [Parameter] public string? Meta { get; set; }

    /// Collapsed-row summary as markup, for cases like the star rating on Product Reviews.
    [Parameter] public RenderFragment? MetaContent { get; set; }

    [Parameter] public bool Expanded { get; set; }

    [Parameter] public EventCallback<bool> ExpandedChanged { get; set; }

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
