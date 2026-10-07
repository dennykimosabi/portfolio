using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ProductCard : ComponentBase
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? StockCode { get; set; }
    [Parameter] public decimal Price { get; set; }
    [Parameter] public string? Url { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public bool ShowAddToCart { get; set; } = true;
    [Parameter] public EventCallback OnAddToCart { get; set; }

    private Task HandleAddClick() =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync() : Task.CompletedTask;
}
