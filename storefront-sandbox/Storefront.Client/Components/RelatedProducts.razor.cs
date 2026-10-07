using Microsoft.AspNetCore.Components;
using Storefront.Client.Models;

namespace Storefront.Client.Components;

public partial class RelatedProducts : ComponentBase
{
    [Parameter] public IReadOnlyList<RelatedProduct> Products { get; set; } = [];

    [Parameter] public string Heading { get; set; } = "Related Products";

    /// TODO: wire to ICartService once the cart slice is in place.
    [Parameter] public EventCallback<RelatedProduct> OnAddToCart { get; set; }

    private const int ItemsPerPage = 3;
    private int CurrentPageIndex { get; set; } = 0;

    private int TotalPages => (Products.Count + ItemsPerPage - 1) / ItemsPerPage;

    private IReadOnlyList<RelatedProduct> CurrentPageProducts =>
        Products
            .Skip(CurrentPageIndex * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToList();

    private void NextPage()
    {
        if (CurrentPageIndex < TotalPages - 1)
        {
            CurrentPageIndex++;
        }
    }

    private void PreviousPage()
    {
        if (CurrentPageIndex > 0)
        {
            CurrentPageIndex--;
        }
    }

    private Task HandleAdd(RelatedProduct product) =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync(product) : Task.CompletedTask;
}
