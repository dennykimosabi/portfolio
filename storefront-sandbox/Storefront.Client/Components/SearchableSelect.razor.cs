using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Storefront.Client.Components;

public record SearchableSelectOption(string Value, string Text);

public partial class SearchableSelect : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Element id linking the button and listbox.</summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>Button text when nothing is selected.</summary>
    [Parameter, EditorRequired] public string Placeholder { get; set; } = string.Empty;
    /// <summary>Placeholder inside the search input.</summary>
    [Parameter] public string SearchPlaceholder { get; set; } = "Search...";
    /// <summary>Selectable options.</summary>
    [Parameter] public List<SearchableSelectOption> Options { get; set; } = [];
    /// <summary>Selected option value. Supports two-way binding.</summary>
    [Parameter] public string Value { get; set; } = string.Empty;
    /// <summary>Invoked when the selection changes.</summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    /// <summary>Disables the control.</summary>
    [Parameter] public bool Disabled { get; set; }

    private bool _isOpen;
    private bool _hasFocusedSearch;
    private string _search = string.Empty;
    private int _highlightedIndex;
    private ElementReference _searchInputRef;

    private SearchableSelectOption? SelectedOption =>
        Options.FirstOrDefault(o => o.Value == Value);

    private string SelectedText => SelectedOption?.Text ?? Placeholder;

    private List<SearchableSelectOption> FilteredOptions =>
        string.IsNullOrWhiteSpace(_search)
            ? Options
            : Options.Where(o => o.Text.Contains(_search, StringComparison.OrdinalIgnoreCase)).ToList();

    private void Toggle()
    {
        if (Disabled) return;
        _isOpen = !_isOpen;
        if (_isOpen) PrepareOpen();
    }

    // Lets a parent (e.g. a cascading Year → Model → Driveline chain) open this dropdown
    // programmatically right after it becomes enabled, instead of waiting for a click. Called
    // before the parent's own re-render pushes the (now-false) Disabled parameter down to us,
    // so — unlike Toggle — this intentionally does not gate on Disabled's current (stale) value.
    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        PrepareOpen();
        StateHasChanged();
    }

    private void PrepareOpen()
    {
        _search = string.Empty;
        _highlightedIndex = Math.Max(0, Options.FindIndex(o => o.Value == Value));
        _hasFocusedSearch = false;
    }

    private void Close()
    {
        _isOpen = false;
        _hasFocusedSearch = false;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? string.Empty;
        _highlightedIndex = 0;
    }

    private async Task Select(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        Close();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        var filtered = FilteredOptions;
        switch (e.Key)
        {
            case "ArrowDown":
                _highlightedIndex = Math.Min(_highlightedIndex + 1, filtered.Count - 1);
                break;
            case "ArrowUp":
                _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
                break;
            case "Enter":
                if (_highlightedIndex >= 0 && _highlightedIndex < filtered.Count)
                    await Select(filtered[_highlightedIndex].Value);
                break;
            case "Escape":
                Close();
                break;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isOpen && !_hasFocusedSearch)
        {
            _hasFocusedSearch = true;
            await _searchInputRef.FocusAsync();
        }
    }
}
