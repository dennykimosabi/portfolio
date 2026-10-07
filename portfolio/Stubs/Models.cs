// Stand-ins for SimplePart.Shared.Models.Storefront DTOs. Shapes are inferred from the members
// Storefront.Client actually reads/writes — only those members exist here.

namespace SimplePart.Shared.Models.Storefront;

/// <summary>Session/site context every storefront request carries.</summary>
public abstract class StorefrontRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public int UkeyWebsite { get; set; }
}

// ─── Refine search / vehicle ──────────────────────────────────────────────

public sealed class RefineSearchRequest : StorefrontRequest
{
    public string ShowFitmentLevel { get; set; } = string.Empty;
    public int? UkeyMake { get; set; }
    public int? UkeyModel { get; set; }
    public int? Year { get; set; }
    public int? UkeyDriveline { get; set; }
    public int? UkeyTrimLevel { get; set; }
    public int? UkeyCategory { get; set; }
    public bool ShowCategory { get; set; }
    public string PageURL { get; set; } = string.Empty;
}

public sealed class RefineSearchResult
{
    public int Ukey { get; set; }
    public string SimpleString { get; set; } = string.Empty;
    public string DisplayString { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? ImageURL { get; set; }
    public string? ExtendedDescription { get; set; }
    public string? LinkURL { get; set; }
    public string? AccessoryLinkURL { get; set; }
}

public sealed class InterpretSearchRequest : StorefrontRequest
{
    public string MakeName { get; set; } = string.Empty;
    public string ModelYear { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SearchString { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public int UkeyDriveLine { get; set; }
    public int UkeyTrimLevel { get; set; }
    public int UkeyCategory { get; set; }
}

public sealed class InterpretSearchResult
{
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public string ModelYear { get; set; } = string.Empty;
    public int UkeyDriveLine { get; set; }
    public int UkeyTrimLevel { get; set; }
    public int UkeyCategory { get; set; }
    public int UkeyModelRange { get; set; }
    public string? SearchTerm { get; set; }
}

public sealed class GetFitmentStringRequest : StorefrontRequest
{
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public string ModelYear { get; set; } = string.Empty;
    public int UkeyDriveLine { get; set; }
    public int UkeyTrimLevel { get; set; }
}

public sealed class GetRefineLinksRequest : StorefrontRequest
{
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public string ModelYear { get; set; } = string.Empty;
    public int UkeyCategory { get; set; }
    public string SearchString { get; set; } = string.Empty;
    public int UkeyDriveLine { get; set; }
    public int UkeyTrimLevel { get; set; }
    public int UkeyModelRange { get; set; }
    public string QueryString { get; set; } = string.Empty;
}

public sealed class RefineLink
{
    public string DisplayString { get; set; } = string.Empty;
}

public sealed class CustomerVehicle
{
    public long Ukey { get; set; }
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public int ModelYear { get; set; }
    public int UkeyDriveline { get; set; }
    public int UkeyTrimLevel { get; set; }
    public string ModelString { get; set; } = string.Empty;
    public string? VehicleDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkURL { get; set; }
}

// ─── Categories ───────────────────────────────────────────────────────────

public sealed class Category
{
    public int Ukey { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public sealed class GetCategoriesRequest : StorefrontRequest
{
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public int UkeyParent { get; set; }
}

public sealed class GetCategoryNameRequest : StorefrontRequest
{
    public int UkeyCategory { get; set; }
    public string PageUrl { get; set; } = string.Empty;
}

// ─── Product search ───────────────────────────────────────────────────────

public sealed class ProductSearchRequest : StorefrontRequest
{
    public string SearchString { get; set; } = string.Empty;
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public int ModelYear { get; set; }
    public int UkeyDriveLine { get; set; }
    public int UkeyTrimLevel { get; set; }
    public int UkeyModelRange { get; set; }
    public int UkeyCategory { get; set; }
    public string SortOrder { get; set; } = "Relevance";
    public string QueryString { get; set; } = string.Empty;
    public int NumResults { get; set; }
    public bool ShowAlternateImages { get; set; }
    public bool IsAccessory { get; set; }
    public bool IsPerformance { get; set; }
}

public sealed class ProductSearchResult
{
    public long Ukey { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ProductURL { get; set; } = string.Empty;
    public string ThumbnailURL { get; set; } = string.Empty;
    public string? CategoryList { get; set; }
    public string? AdditionalDescription { get; set; }
    public string? AssemblyList { get; set; }
}

public sealed class GetSearchAssembliesRequest : StorefrontRequest
{
    public long UkeyCachedSearch { get; set; }
}

public sealed class SearchAssembly
{
    public string AssemblyURL { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyDescription { get; set; } = string.Empty;
    public string ThumbnailURL { get; set; } = string.Empty;
}

// ─── Placements / banners / settings ──────────────────────────────────────

public sealed class GetPlacementRequest : StorefrontRequest
{
    public string Key { get; set; } = string.Empty;
    public int UkeyModel { get; set; }
    public string ModelYear { get; set; } = string.Empty;
    public int UkeyDriveline { get; set; }
    public int UkeyTrimLevel { get; set; }
    public int UkeyCategory { get; set; }
}

public sealed class PlacementText
{
    public string? Title { get; set; }
    public string? IntroTextTitle { get; set; }
    public string? IntroTextSubTitle { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string? TargetUrl { get; set; }
    public string? DisplayString { get; set; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(IntroTextTitle)
        && string.IsNullOrWhiteSpace(IntroTextSubTitle) && string.IsNullOrWhiteSpace(Text)
        && string.IsNullOrWhiteSpace(ImageUrl);
}

public sealed class GetBannerRequest : StorefrontRequest
{
    public string PageUrl { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
}

public sealed class Banner
{
    public string BannerMode { get; set; } = string.Empty;
    public string? BannerTitle { get; set; }
    public string? BannerText { get; set; }
    public string? BannerTarget { get; set; }
    public string? BannerTargetLocation { get; set; }
    public string DismissalKey { get; set; } = string.Empty;

    public bool IsEmpty => string.IsNullOrWhiteSpace(BannerTitle) && string.IsNullOrWhiteSpace(BannerText);
}

public sealed class HeadConfig
{
    public string? Favicon { get; set; }
    public List<string> Stylesheets { get; set; } = [];
    public List<HeadScript> Scripts { get; set; } = [];
}

public sealed class HeadScript
{
    public string Src { get; set; } = string.Empty;
    public bool Async { get; set; }
    public bool Defer { get; set; }
}

public sealed class WebsiteSettingsDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string FormattedAddress { get; set; } = string.Empty;
    public string DealerStreet { get; set; } = string.Empty;
    public string DealerCity { get; set; } = string.Empty;
    public string DealerState { get; set; } = string.Empty;
    public string DealerZip { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SalesEmail { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string PoliciesLink { get; set; } = string.Empty;
    public string? PriceBoxMessage { get; set; }
    public string? ConfidenceContainerTitle { get; set; }
    public string? ConfidenceContainerText { get; set; }
    public bool HideCart { get; set; }
    public bool HideProfile { get; set; }
    public bool NoCommerce { get; set; }
    public bool ShowAssemblySearch { get; set; }
    public bool ShowPerformanceSuggestions { get; set; }
    public bool ShowAccessorySuggestions { get; set; }
    public HeadConfig Head { get; set; } = new();
}

public sealed class PageHeadingRequest : StorefrontRequest
{
    public string PageUrl { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
}

public sealed class PageHeadingResponse
{
    public string H1TagText { get; set; } = string.Empty;
}

// ─── Home: assemblies / featured ──────────────────────────────────────────

public sealed class GetAssemblyResultsRequest : StorefrontRequest
{
    public int UkeyCategory { get; set; }
    public int? UkeyMake { get; set; }
    public int? UkeyModel { get; set; }
    public int? ModelYear { get; set; }
    public int? UkeyDriveline { get; set; }
    public int? UkeyTrimLevel { get; set; }
}

public sealed class GetAssemblyResultsResponse
{
    public string CategoryName { get; set; } = string.Empty;
    public List<AssemblyResult> Assemblies { get; set; } = [];
    public List<CategoryPartType> PartTypes { get; set; } = [];
}

public sealed class AssemblyResult
{
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyDescription { get; set; } = string.Empty;
    public string ShowAssemblyURL { get; set; } = string.Empty;
    public string? HoverText { get; set; }
    public string? ImageURL { get; set; }
    public string? Keywords { get; set; }
}

public sealed class CategoryPartType
{
    public string TypeName { get; set; } = string.Empty;
    public string CleanTypeName { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public string PageURL { get; set; } = string.Empty;
    public string? ImageURL { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int NumProducts { get; set; }
}

public sealed class GetFeaturedProductsRequest : StorefrontRequest
{
    public int ReturnNum { get; set; }
    public bool ShowApparelAccessories { get; set; }
    public bool ShowVehicleAccessories { get; set; }
    public int UkeyModel { get; set; }
    public int ModelYear { get; set; }
    public int UkeyDriveline { get; set; }
    public int UkeyTrimLevel { get; set; }
}

public sealed class FeaturedProduct
{
    public long UkeyProduct { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ProductURL { get; set; } = string.Empty;
    public string? FullsizeURL { get; set; }
    public string? ThumbnailURL { get; set; }
}

public sealed class LandingPageConfig
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string SearchString { get; set; } = string.Empty;
    public int UkeyCategory { get; set; }
    public int UkeyMake { get; set; }
    public int UkeyModel { get; set; }
    public int Year { get; set; }
}

// ─── Reviews ──────────────────────────────────────────────────────────────

public sealed class GetTestimonialsRequest : StorefrontRequest
{
    public bool ReturnAll { get; set; }
    public int NumReviews { get; set; }
}

public sealed class Review
{
    public string Rating { get; set; } = string.Empty;
    public string DateString { get; set; } = string.Empty;
    public string Testimonial { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Location { get; set; }
}

public sealed class AverageReviewsResult
{
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

// ─── Product detail page ──────────────────────────────────────────────────

public abstract class ProductRequest : StorefrontRequest
{
    public string StockNumber { get; set; } = string.Empty;
    public long UkeyProduct { get; set; }
}

public abstract class FitmentProductRequest : ProductRequest
{
    public int? UkeyModel { get; set; }
    public int? ModelYear { get; set; }
    public int? UkeyDriveline { get; set; }
    public int? UkeyTrimLevel { get; set; }
}

public sealed class GetProductDetailRequest : FitmentProductRequest { }
public sealed class GetProductAssembliesRequest : FitmentProductRequest { }
public sealed class GetProductTypesRequest : FitmentProductRequest { }
public sealed class GetProductServicesRequest : FitmentProductRequest { }
public sealed class GetProductReviewsRequest : ProductRequest { }
public sealed class GetProductApplicationsRequest : ProductRequest { }
public sealed class GetProductAttachmentsRequest : ProductRequest { }
public sealed class GetProductReviewScoreRequest : ProductRequest { }
public sealed class GetProductImagesRequest : ProductRequest { }
public sealed class GetInventoryMessageRequest : ProductRequest { }

public sealed class GetSuggestedProductsRequest : ProductRequest
{
    public bool IsRelated { get; set; }
    public bool Performance { get; set; }
    public bool Accessory { get; set; }
}

public sealed class ProductDetail
{
    public long UkeyProduct { get; set; }
    public string ProductHeading { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public int TimesViewed { get; set; }
    public decimal Price { get; set; }
    public decimal ListPrice { get; set; }
    public string? SavingsStringWithPercent { get; set; }
}

public sealed class ProductImage
{
    public string ImageURL { get; set; } = string.Empty;
    public string RawThumbnailURL { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
}

public sealed class ProductAssembly
{
    public string AssemblyName { get; set; } = string.Empty;
    public int AssemblyIndex { get; set; }
    public string? AssemblyURL { get; set; }
    public string? AssemblyImageURL { get; set; }
    public string? AssemblyDescription { get; set; }
    public int QuantityRequired { get; set; }
    public string? ApplicationNotes { get; set; }
}

public sealed class ProductReview
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime YMD { get; set; }
    public int ReviewScore { get; set; }
    public string ReviewText { get; set; } = string.Empty;
}

public sealed class ProductApplication
{
    public string Fitment { get; set; } = string.Empty;
    public string ModelYears { get; set; } = string.Empty;
}

public sealed class ProductType
{
    public string TypeName { get; set; } = string.Empty;
    public string? TypeDescription { get; set; }
    public string? TargetURL { get; set; }
    public string? ThumbnailURL { get; set; }
}

public sealed class StorefrontProductService
{
    public string AssemblyName { get; set; } = string.Empty;
    public string? AssemblyDescription { get; set; }
    public List<ServiceProduct> ServiceProducts { get; set; } = [];
}

public sealed class ServiceProduct
{
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public string? TargetURL { get; set; }
    public string? ThumbnailURL { get; set; }
}

public sealed class ProductAttachment
{
    public string AttachmentName { get; set; } = string.Empty;
    public string AttachmentUrl { get; set; } = string.Empty;
    public bool IsVideo { get; set; }
}

public sealed class ProductReviewScore
{
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
}

public enum InventoryLevel
{
    AvailableForOrder,
    InStock,
    LimitedStock
}

public sealed class InventoryMessage
{
    public InventoryLevel Level { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string MessageTextShipping { get; set; } = string.Empty;
    public string MessageTextLocalPickup { get; set; } = string.Empty;
}

public sealed class ProductTag
{
    public string TagTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TagType { get; set; }
    public string? ImageURL { get; set; }
    public string? TargetURL { get; set; }
}

public sealed class SuggestedProduct
{
    public long Ukey { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ListPrice { get; set; }
    public string? CategoryList { get; set; }
    public string? ImageURL { get; set; }
    public string? ProductURL { get; set; }
}

public sealed class GetGuidedNavigationRequest : StorefrontRequest
{
    public long UkeyProduct { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public int? UkeyMake { get; set; }
    public int? UkeyModel { get; set; }
    public int? ModelYear { get; set; }
    public int? UkeyDriveline { get; set; }
    public int? UkeyTrimLevel { get; set; }
}

public sealed class GuidedNavigationHeader
{
    public string? CssClass { get; set; }
    public string GuideHeader { get; set; } = string.Empty;
    public string? GuideBody { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkHover { get; set; }
    public string? LinkAnchor { get; set; }
    public string? ResultLabel1 { get; set; }
    public string? ResultLabel2 { get; set; }
    public string? ResultLabel3 { get; set; }
    public bool HasVipPromo { get; set; }
    public string? GuidedNavPromoTitle { get; set; }
    public string? GuidedNavPromoText { get; set; }
}

public sealed class GuidedNavigationDetailResponse
{
    public List<GuidedNavigationDetail> Details { get; set; } = [];
}

public sealed class GuidedNavigationDetail
{
    public string LinkAnchor { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkHover { get; set; }
}

// ─── Fulfilment ───────────────────────────────────────────────────────────

public sealed class GetShippingPostalCodeRequest : StorefrontRequest { }
public sealed class GetLocalPickupCostRequest : StorefrontRequest { }

public sealed class GetEstimatedShippingCostRequest : StorefrontRequest
{
    public long UkeyProduct { get; set; }
    public int Quantity { get; set; }
}

public sealed class UpdateShippingPostalCodeRequest : StorefrontRequest
{
    public string PostalCode { get; set; } = string.Empty;
}

public sealed class ShippingEstimate
{
    public bool IsTBD { get; set; }
    public decimal ShippingDifference { get; set; }
    public string? Disclaimer { get; set; }
}

// ─── Cart ─────────────────────────────────────────────────────────────────

public sealed class AddToCartRequest : StorefrontRequest
{
    public long UkeyProduct { get; set; }
    public int Quantity { get; set; } = 1;
}

public sealed class GetCartRequest : StorefrontRequest { }

public sealed class CartItem
{
    public long Ukey { get; set; }
    public long UkeyProduct { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public string ProductURL { get; set; } = string.Empty;
    public string ThumbnailURL { get; set; } = string.Empty;
    public string FitmentString { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ListPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineItemTotal => Price * Quantity;
}

public sealed class CartSummaryLine
{
    public string? PlainName { get; set; }
    public string? CartSummary { get; set; }
    public string DisplayAmount { get; set; } = string.Empty;
    public bool IsTotal { get; set; }
}

public sealed class RecommendedProduct
{
    public long Ukey { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ProductURL { get; set; } = string.Empty;
    public string? ImageURL { get; set; }
}
