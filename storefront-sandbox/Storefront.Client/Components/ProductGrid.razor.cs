using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class ProductGrid : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Search results rendered as cards.</summary>
    [Parameter, EditorRequired] public IEnumerable<ProductSearchResult> Results { get; set; } = [];
    /// <summary>Invoked with the result when its add-to-cart is clicked.</summary>
    [Parameter] public EventCallback<ProductSearchResult> OnAddToCart { get; set; }

    private Task InvokeAdd(ProductSearchResult product) =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync(product) : Task.CompletedTask;
}
