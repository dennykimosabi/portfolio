using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using Storefront.Client;
using Storefront.Client.State;
using Storefront.Client.Utilities;
using Portfolio.Demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Context / session
builder.Services.AddScoped<IWebsiteContextProvider, DemoWebsiteContext>();
builder.Services.AddScoped<ISessionStateProvider, DemoSessionState>();
builder.Services.AddScoped<ILogService, DemoLogService>();
builder.Services.AddScoped<IClientAssetResolver, DemoAssetResolver>();

// Storefront services — all in-memory dummy data (see Demo/DemoCatalog.cs)
builder.Services.AddScoped<IProductSearchService, DemoProductSearchService>();
builder.Services.AddScoped<IPlacementService, DemoPlacementService>();
builder.Services.AddScoped<IWebsiteSettingsService, DemoWebsiteSettingsService>();
builder.Services.AddScoped<IVehicleService, DemoVehicleService>();
builder.Services.AddScoped<ICategoryService, DemoCategoryService>();
builder.Services.AddScoped<IProductService, DemoProductService>();
builder.Services.AddScoped<IFulfillmentService, DemoFulfillmentService>();
builder.Services.AddScoped<ICartService, DemoCartService>();
builder.Services.AddScoped<ILandingService, DemoLandingService>();
builder.Services.AddScoped<IHomeService, DemoHomeService>();
builder.Services.AddScoped<IFeaturedProductsService, DemoFeaturedProductsService>();
builder.Services.AddScoped<IBannerService, DemoBannerService>();
builder.Services.AddScoped<IReviewsService, DemoReviewsService>();
builder.Services.AddScoped<IGuidedNavigationService, DemoGuidedNavigationService>();
builder.Services.AddScoped<IPageTrackingService, DemoPageTrackingService>();

// Client state
builder.Services.AddScoped<DemoCartStore>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<GarageState>();
builder.Services.AddScoped<VehicleFitmentState>();

await builder.Build().RunAsync();
