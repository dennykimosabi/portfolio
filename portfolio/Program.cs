using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using Storefront.Client;
using Storefront.Client.State;
using Storefront.Client.Utilities;
using Portfolio.Demo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(o => o.DetailedErrors = true);

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

// Client state (per circuit)
builder.Services.AddScoped<DemoCartStore>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<GarageState>();
builder.Services.AddScoped<VehicleFitmentState>();

var app = builder.Build();

// Serve the real storefront stylesheet straight from the sandbox Server wwwroot
// (css/main.css, app.css) so components always reflect the committed styles.
// no-cache so a plain refresh picks up a recompiled main.css.
var serverWwwroot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath,
    "..", "storefront-sandbox", "Storefront.Server", "wwwroot"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(serverWwwroot),
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});

app.UseRouting();
app.UseAntiforgery();

app.MapStaticAssets();
DemoImages.Map(app);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
