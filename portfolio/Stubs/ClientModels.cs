// Stand-ins for Storefront.Client.Models (PDP view models), inferred from usage.

namespace Storefront.Client.Models;

public sealed record DealerInfo(
    string Name,
    string AddressLine1,
    string AddressLine2,
    string Phone,
    string Email,
    decimal Rating);

public sealed record RelatedProduct(
    string Title,
    string StockCode,
    decimal Price,
    long UkeyProduct,
    decimal? ListPrice = null,
    string? SavingsText = null,
    string? Category = null,
    string? ImageUrl = null,
    string? Url = null);

public sealed record DeliveryOption(
    string ZipCode,
    string Promise,
    string EstimateLinkText = "Estimate Shipping");

public sealed record PickupOption(
    string Promise,
    string CostText);

public enum AvailabilityLevel
{
    AvailableForOrder,
    InStock,
    LimitedStock
}

public sealed record ProductSection(
    string Title,
    string? Meta = null,
    bool ShowRating = false);
