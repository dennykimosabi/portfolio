using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class FeaturedProducts : ComponentBase
{
    [Inject] public IFeaturedProductsService FeaturedService { get; set; } = default!;
    [Inject] public IPlacementService PlacementService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;
    [Inject] public CartState CartState { get; set; } = default!;

    /// <summary>
    /// Placement key for the section heading. Defaults to the legacy
    /// "HomeFeaturedAccessories" key.
    /// </summary>
    [Parameter] public string PlacementKey { get; set; } = "HomeFeaturedAccessories";

    /// <summary>Number of products to fetch. Legacy default is 6.</summary>
    [Parameter] public int ReturnNum { get; set; } = 6;

    /// <summary>Section heading shown when the placement returns no IntroTextTitle.</summary>
    [Parameter] public string FallbackHeading { get; set; } = "Featured Parts and Accessories";

    [PersistentState] public List<FeaturedProduct>? Products { get; set; }
    [PersistentState] public PlacementText? Heading { get; set; }

    private bool HasProducts => Products is { Count: > 0 };

    private string ResolvedHeading
    {
        get
        {
            if (Heading is not null)
            {
                if (!string.IsNullOrWhiteSpace(Heading.IntroTextTitle))
                    return Heading.IntroTextTitle;
                if (!string.IsNullOrWhiteSpace(Heading.Title))
                    return Heading.Title;
            }
            return FallbackHeading;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var productsTask = Products is null ? LoadProducts() : Task.CompletedTask;
        var headingTask = Heading is null ? LoadHeading() : Task.CompletedTask;
        await Task.WhenAll(productsTask, headingTask);
    }

    private async Task LoadProducts()
    {
        var rows = await FeaturedService.GetAsync(new GetFeaturedProductsRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            ReturnNum = ReturnNum,
            ShowApparelAccessories = false,
            ShowVehicleAccessories = true,
            UkeyModel = FitmentState.UkeyModel ?? 0,
            ModelYear = FitmentState.Year ?? 0,
            UkeyDriveline = FitmentState.UkeyDriveline ?? 0,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel ?? 0
        });
        Products = rows.ToList();
    }

    private async Task LoadHeading()
    {
        if (string.IsNullOrWhiteSpace(PlacementKey)) return;

        Heading = await PlacementService.GetAsync(new GetPlacementRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            Key = PlacementKey,
            UkeyModel = FitmentState.UkeyModel ?? 0,
            ModelYear = FitmentState.Year?.ToString() ?? string.Empty,
            UkeyDriveline = FitmentState.UkeyDriveline ?? 0,
            UkeyTrimLevel = FitmentState.UkeyTrimLevel ?? 0,
            UkeyCategory = FitmentState.UkeyCategory ?? 0
        });
    }

    // Legacy truncates ProductDescription server-side to 60 chars + "...". Mirror that
    // server-side trimming here so we don't ship overly long strings into the markup.
    private static string TruncateDescription(string description)
    {
        if (string.IsNullOrEmpty(description)) return string.Empty;
        var trimmed = description.Trim();
        return trimmed.Length > 60 ? string.Concat(trimmed.AsSpan(0, 55), "...") : trimmed;
    }

    // Legacy appends ?partner=featuredAccessories to the product URL for analytics.
    private static string BuildProductHref(string productUrl)
    {
        if (string.IsNullOrEmpty(productUrl)) return string.Empty;
        var separator = productUrl.Contains('?') ? "&" : "?";
        return $"{productUrl}{separator}partner=featuredAccessories";
    }

    // Default no-image placeholder swap from the legacy server-side logic
    // ((FullsizeURL contains "singlepixel" or "no-image") → use placeholder).
    // PlaceholderImage from website settings isn't surfaced in the new stack yet —
    // for now we just return null so the markup skips the <img> entirely.
    private static string? ResolveImageSrc(FeaturedProduct p)
    {
        var src = !string.IsNullOrWhiteSpace(p.FullsizeURL) ? p.FullsizeURL : p.ThumbnailURL;
        if (string.IsNullOrWhiteSpace(src)) return null;
        if (src.Contains("singlepixel", StringComparison.OrdinalIgnoreCase)) return null;
        if (src.Contains("no-image", StringComparison.OrdinalIgnoreCase)) return null;
        return src;
    }

    private async Task HandleAddToCart(FeaturedProduct product)
    {
        await CartState.AddToCartAsync(product.UkeyProduct);
    }
}
