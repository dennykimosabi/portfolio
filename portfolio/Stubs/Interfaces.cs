// Stand-ins for SimplePart.Shared.Models interfaces. Signatures are inferred from how the
// Storefront.Client components call them — they are not the real contracts.

using SimplePart.Shared.Models.Storefront;

namespace SimplePart.Shared.Models
{
    // Storefront.Client code imports this namespace; the real types aren't needed by the preview.
    internal static class NamespaceAnchor { }
}

namespace SimplePart.Shared.Models.Interfaces
{
    public interface IWebsiteContextProvider
    {
        int UkeyWebsite { get; }
    }

    public interface ISessionStateProvider
    {
        string SessionId { get; }
        string MachineId { get; }
    }

    public interface ILogService
    {
        Task LogInformation(string message, string source);
        Task LogError(string message, string source, string? stackTrace = null);
    }
}

namespace SimplePart.Shared.Models.Interfaces.Storefront
{
    public interface IProductSearchService
    {
        Task<IEnumerable<RefineSearchResult>> GetRefinedSearchAsync(RefineSearchRequest request);
        Task<IEnumerable<ProductSearchResult>> GetSearchResultsAsync(ProductSearchRequest request);
        Task<IEnumerable<SearchAssembly>> GetSearchAssembliesAsync(GetSearchAssembliesRequest request);
    }

    public interface IPlacementService
    {
        Task<PlacementText?> GetAsync(GetPlacementRequest request);
    }

    public interface IWebsiteSettingsService
    {
        Task<WebsiteSettingsDto> GetSettingsAsync(int ukeyWebsite, string sessionId, string machineId);
    }

    public interface IVehicleService
    {
        Task<InterpretSearchResult> GetInterpretationAsync(InterpretSearchRequest request);
        Task<string> GetFitmentStringAsync(GetFitmentStringRequest request);
        Task<IEnumerable<RefineLink>> GetModelRefineLinksAsync(GetRefineLinksRequest request);
        Task<IEnumerable<RefineLink>> GetYearRefineLinksAsync(GetRefineLinksRequest request);
        Task<IEnumerable<RefineLink>> GetTrimLevelRefineLinksAsync(GetRefineLinksRequest request);
        Task<IEnumerable<RefineLink>> GetDrivelineRefineLinksAsync(GetRefineLinksRequest request);
    }

    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync(GetCategoriesRequest request);
        Task<string> GetCategoryNameAsync(GetCategoryNameRequest request);
    }

    public interface IProductService
    {
        Task<ProductDetail?> GetDetailsAsync(GetProductDetailRequest request);
        Task<IEnumerable<ProductAssembly>> GetAssembliesAsync(GetProductAssembliesRequest request);
        Task<IEnumerable<ProductReview>> GetReviewsAsync(GetProductReviewsRequest request);
        Task<IEnumerable<ProductApplication>> GetApplicationsAsync(GetProductApplicationsRequest request);
        Task<IEnumerable<ProductType>> GetTypesAsync(GetProductTypesRequest request);
        Task<IEnumerable<StorefrontProductService>> GetServicesAsync(GetProductServicesRequest request);
        Task<IEnumerable<ProductAttachment>> GetAttachmentsAsync(GetProductAttachmentsRequest request);
        Task<ProductReviewScore?> GetReviewScoreAsync(GetProductReviewScoreRequest request);
        Task<IEnumerable<ProductImage>> GetAdditionalImagesAsync(GetProductImagesRequest request);
        Task<InventoryMessage?> GetInventoryMessageAsync(GetInventoryMessageRequest request);
        Task<IEnumerable<SuggestedProduct>> GetSuggestedProductsAsync(GetSuggestedProductsRequest request);
    }

    public interface IFulfillmentService
    {
        Task<string?> GetShippingPostalCodeAsync(GetShippingPostalCodeRequest request);
        Task<decimal?> GetLocalPickupCostAsync(GetLocalPickupCostRequest request);
        Task<ShippingEstimate?> GetEstimatedShippingCostAsync(GetEstimatedShippingCostRequest request);
        Task UpdateShippingPostalCodeAsync(UpdateShippingPostalCodeRequest request);
    }

    public interface ICartService
    {
        Task AddToCartAsync(AddToCartRequest request);
        Task<IEnumerable<RecommendedProduct>> GetCartRecommendationsAsync(GetCartRequest request);
    }

    public interface ILandingService
    {
        Task<LandingPageConfig?> GetLandingBySlugAsync(string slug);
    }

    public interface IHomeService
    {
        Task<GetAssemblyResultsResponse?> GetAssemblyResultsAsync(GetAssemblyResultsRequest request);
    }

    public interface IFeaturedProductsService
    {
        Task<IEnumerable<FeaturedProduct>> GetAsync(GetFeaturedProductsRequest request);
    }

    public interface IBannerService
    {
        Task<Banner?> GetAsync(GetBannerRequest request);
    }

    public interface IReviewsService
    {
        Task<IEnumerable<Review>> GetTestimonialsAsync(GetTestimonialsRequest request);
        Task<AverageReviewsResult?> GetAverageReviewsAsync(int ukeyWebsite);
    }

    public interface IGuidedNavigationService
    {
        Task<(GuidedNavigationHeader? Header, GuidedNavigationDetailResponse? Detail)> GetBothAsync(GetGuidedNavigationRequest request);
    }

    public interface IPageTrackingService
    {
        Task<PageHeadingResponse> GetPageHeadingAsync(PageHeadingRequest request);
    }
}
