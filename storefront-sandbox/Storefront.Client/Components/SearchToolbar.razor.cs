using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Storefront.Client.Components;

public partial class SearchToolbar : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Active vehicle label, e.g. "2024 Honda Civic".</summary>
    [Parameter] public string FitmentString { get; set; } = string.Empty;
    /// <summary>Total result count.</summary>
    [Parameter] public int CatalogCount { get; set; }
    /// <summary>Search term or category label.</summary>
    [Parameter] public string CatalogTermLabel { get; set; } = string.Empty;
    /// <summary>Whether the grid view is active. Supports two-way binding.</summary>
    [Parameter] public bool IsGridView { get; set; }
    /// <summary>Whether the filter sidebar is visible.</summary>
    [Parameter] public bool ShowSidebar { get; set; } = true;
    /// <summary>Number of active filters shown on the filter toggle.</summary>
    [Parameter] public int FilterCount { get; set; }
    /// <summary>Current sort key. Supports two-way binding.</summary>
    [Parameter] public string SortOrder { get; set; } = "relevance";
    /// <summary>Invoked when list view is selected.</summary>
    [Parameter] public EventCallback OnSetListView { get; set; }
    /// <summary>Invoked when grid view is selected.</summary>
    [Parameter] public EventCallback OnSetGridView { get; set; }
    /// <summary>Invoked when sidebar visibility toggles.</summary>
    [Parameter] public EventCallback<bool> OnShowSidebarChanged { get; set; }
    /// <summary>Invoked when the sort order changes.</summary>
    [Parameter] public EventCallback<string> OnSortOrderChanged { get; set; }

    // Falls back to "relevance" if the parent passes null/empty so the select always matches an option.
    private string SortValue => string.IsNullOrWhiteSpace(SortOrder) ? "relevance" : SortOrder;

    private Task HandleFiltersChange(ChangeEventArgs e) =>
        OnShowSidebarChanged.InvokeAsync(e.Value?.ToString() == "shown");

    private Task HandleSortChange(ChangeEventArgs e) =>
        OnSortOrderChanged.InvokeAsync(e.Value?.ToString() ?? "relevance");
}
