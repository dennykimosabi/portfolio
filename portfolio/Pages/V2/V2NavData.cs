

namespace Storefront.Client.Pages.V2;

/// <summary>
/// Shared navigation data for the v2 shell: section trees and palette entries.
/// Kept in one place so the rail, section sidebar, and ⌘K palette stay in sync.
/// </summary>
public static class V2NavData
{
    public static readonly List<(string Href, string Label)> Blueprints = new()
    {
        ("v2/blueprints/home", "Home"),
        ("v2/blueprints/search", "Search"),
        ("v2/blueprints/product", "Product"),
        ("v2/blueprints/cart", "Cart"),
        ("v2/blueprints/accessories", "Accessories"),
    };

    public static readonly List<V2NavGroup> WorkshopGroups = new()
    {
        new("Foundation", new() { ("v2/workshop/design-tokens", "Design Tokens") }),
        new("Atomic", new()
        {
            ("v2/workshop/sf-button", "SfButton"),
            ("v2/workshop/sf-badge", "SfBadge"),
            ("v2/workshop/sf-chip", "SfChip"),
            ("v2/workshop/sf-input", "SfInput"),
            ("v2/workshop/sf-select", "SfSelect"),
            ("v2/workshop/sf-icon", "SfIcon"),
            ("v2/workshop/sf-link", "SfLink"),
            ("v2/workshop/sf-quantity-stepper", "SfQuantityStepper"),
            ("v2/workshop/sf-divider", "SfDivider"),
            ("v2/workshop/sf-skeleton", "SfSkeleton"),
            ("v2/workshop/sf-tooltip", "SfTooltip"),
        }),
        new("Browse", new()
        {
            ("v2/workshop/assembly-results", "AssemblyResults"),
            ("v2/workshop/vehicle-picker", "VehiclePicker"),
            ("v2/workshop/featured-products", "FeaturedProducts"),
        }),
        new("Search", new()
        {
            ("v2/workshop/search-toolbar", "SearchToolbar"),
            ("v2/workshop/product-grid", "ProductGrid"),
            ("v2/workshop/product-card", "ProductCard"),
        }),
        new("Product page", new()
        {
            ("v2/workshop/product-gallery", "ProductGallery"),
            ("v2/workshop/price-box", "ProductPriceBox"),
            ("v2/workshop/related-products", "RelatedProducts"),
        }),
    };

    public static readonly List<(string Href, string Label)> Docs = new()
    {
        ("v2/docs/build-log", "Build log"),
        ("v2/docs/flows", "Flows"),
        ("v2/docs/experiments", "Experiments"),
        ("v2/docs/tokens", "Design tokens"),
    };

    public static IEnumerable<(string Href, string Label, string Section)> AllEntries()
    {
        foreach (var b in Blueprints) yield return (b.Href, b.Label, "Blueprints");
        foreach (var g in WorkshopGroups)
            foreach (var i in g.Items) yield return (i.Href, i.Label, "Workshop");
        foreach (var d in Docs) yield return (d.Href, d.Label, "Docs");
    }
}
