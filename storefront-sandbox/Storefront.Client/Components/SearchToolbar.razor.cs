using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Storefront.Client.Components;

public partial class SearchToolbar : ComponentBase
{
    [Parameter] public string FitmentString { get; set; } = string.Empty;
    [Parameter] public int CatalogCount { get; set; }
    [Parameter] public string CatalogTermLabel { get; set; } = string.Empty;
    [Parameter] public bool IsGridView { get; set; }
    [Parameter] public bool ShowSidebar { get; set; } = true;
    [Parameter] public int FilterCount { get; set; }
    [Parameter] public string SortOrder { get; set; } = "relevance";
    [Parameter] public EventCallback OnSetListView { get; set; }
    [Parameter] public EventCallback OnSetGridView { get; set; }
    [Parameter] public EventCallback<bool> OnShowSidebarChanged { get; set; }
    [Parameter] public EventCallback<string> OnSortOrderChanged { get; set; }

    // Falls back to "relevance" if the parent passes null/empty so the select always matches an option.
    private string SortValue => string.IsNullOrWhiteSpace(SortOrder) ? "relevance" : SortOrder;

    private Task HandleFiltersChange(ChangeEventArgs e) =>
        OnShowSidebarChanged.InvokeAsync(e.Value?.ToString() == "shown");

    private Task HandleSortChange(ChangeEventArgs e) =>
        OnSortOrderChanged.InvokeAsync(e.Value?.ToString() ?? "relevance");
}
