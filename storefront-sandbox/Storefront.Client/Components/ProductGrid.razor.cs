using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class ProductGrid : ComponentBase
{
    [Parameter, EditorRequired] public IEnumerable<ProductSearchResult> Results { get; set; } = [];
    [Parameter] public EventCallback<ProductSearchResult> OnAddToCart { get; set; }

    private Task InvokeAdd(ProductSearchResult product) =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync(product) : Task.CompletedTask;
}
