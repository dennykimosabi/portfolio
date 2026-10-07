using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class MainBanner : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IBannerService BannerService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [PersistentState] public Banner? _banner { get; set; }
    private bool _dismissed;

    private string DismissalKey =>
        $"bannerDismissed:{WebsiteContext.UkeyWebsite}:{_banner?.DismissalKey}";

    protected override async Task OnInitializedAsync()
    {
        if (_banner is not null) return;

        var uri = new Uri(Nav.Uri);
        _banner = await BannerService.GetAsync(new GetBannerRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            PageUrl = uri.AbsolutePath,
            QueryString = uri.Query
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender
            || _banner is null
            || !string.Equals(_banner.BannerMode, "mainNav", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            var stored = await JS.InvokeAsync<string?>("localStorage.getItem", DismissalKey);
            if (stored == "1")
            {
                _dismissed = true;
                StateHasChanged();
            }
        }
        catch
        {
            // localStorage unavailable (private mode, SSR, etc.) — fall through and show the banner.
        }
    }

    private async Task HandleDismiss()
    {
        _dismissed = true;
        try
        {
            await JS.InvokeVoidAsync("localStorage.setItem", DismissalKey, "1");
        }
        catch { /* localStorage unavailable — dismissal won't persist past page reload */ }
    }
}
