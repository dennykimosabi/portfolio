using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ProductCard : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Product display name.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    /// <summary>Part number shown under the title.</summary>
    [Parameter] public string? StockCode { get; set; }
    /// <summary>Current selling price.</summary>
    [Parameter] public decimal Price { get; set; }
    /// <summary>Link to the product detail page.</summary>
    [Parameter] public string? Url { get; set; }
    /// <summary>Product image URL; placeholder shown when empty.</summary>
    [Parameter] public string? ImageUrl { get; set; }
    /// <summary>Whether the add-to-cart button renders.</summary>
    [Parameter] public bool ShowAddToCart { get; set; } = true;
    /// <summary>Invoked when add-to-cart is clicked.</summary>
    [Parameter] public EventCallback OnAddToCart { get; set; }

    private Task HandleAddClick() =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync() : Task.CompletedTask;
}
