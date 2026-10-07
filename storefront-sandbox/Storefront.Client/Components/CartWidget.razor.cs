using Microsoft.AspNetCore.Components;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class CartWidget : ComponentBase, IDisposable
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public CartState CartState { get; set; } = default!;

    /// <summary>Hides the cart link, e.g. on the cart page itself.</summary>
    [Parameter] public bool HideCart { get; set; }

    [PersistentState] public int? PersistedItemCount { get; set; }

    private bool _isOpen;

    protected override async Task OnInitializedAsync()
    {
        CartState.OnChange += HandleStateChange;

        if (PersistedItemCount.HasValue)
        {
            // Restored from SSR prerender — seed CartState and skip the API call.
            CartState.SetItemCount(PersistedItemCount.Value);
        }
        else
        {
            await CartState.RefreshCountAsync();
            PersistedItemCount = CartState.ItemCount;
        }
    }

    private async Task TogglePanel()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
            await CartState.RefreshContentsAsync();
    }

    private void ClosePanel() => _isOpen = false;

    private void HandleStateChange() => InvokeAsync(StateHasChanged);

    public void Dispose() => CartState.OnChange -= HandleStateChange;
}
