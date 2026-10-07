using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class RecommendedProductsPanel : ComponentBase
{
    [Inject] public ICartService CartService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public CartState CartState { get; set; } = default!;

    [PersistentState] public List<RecommendedProduct>? _products { get; set; }

    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        if (_products is not null)
        {
            _loaded = true;
            return;
        }

        var rows = await CartService.GetCartRecommendationsAsync(new GetCartRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite
        });

        // The cart-suggestions sproc can return placeholder/blank rows mixed in
        // with real recommendations. Drop anything without a real description or
        // a non-zero price so the grid doesn't render $0.00 ghost cards.
        _products = rows
            .Where(p => p.Price > 0 && !string.IsNullOrWhiteSpace(p.ProductDescription))
            .ToList();
        _loaded = true;
    }

    private async Task HandleAddToCart(RecommendedProduct product)
    {
        await CartState.AddToCartAsync(product.Ukey);
    }
}
