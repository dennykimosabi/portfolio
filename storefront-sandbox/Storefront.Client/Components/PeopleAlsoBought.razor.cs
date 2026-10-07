using Microsoft.AspNetCore.Components;
using Storefront.Client.Models;

namespace Storefront.Client.Components;

public partial class PeopleAlsoBought : ComponentBase
{
    [Parameter] public IReadOnlyList<RelatedProduct> Products { get; set; } = [];

    [Parameter] public string Heading { get; set; } = "People Also Bought";

    /// TODO: wire to ICartService once the cart slice is in place.
    [Parameter] public EventCallback<RelatedProduct> OnAddToCart { get; set; }

    private Task HandleAdd(RelatedProduct product) =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync(product) : Task.CompletedTask;
}
