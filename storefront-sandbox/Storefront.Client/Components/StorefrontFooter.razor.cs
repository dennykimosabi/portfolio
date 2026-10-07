using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;

namespace Storefront.Client.Components;

public partial class StorefrontFooter : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;

    protected int CopyrightYear => DateTime.Today.Year;

    // TODO: Inject IWebsiteSettingsService when implemented to provide:
    // - BusinessName       → copyright line, dealer info bar
    // - PhoneNumber        → dealer info bar
    // - FormattedAddress   → dealer info bar
    // - SalesEmail         → dealer info bar
    // - FooterDisclaimer   → disclaimer panel
    // - ShowDealerInfoBar  → conditional dealer info section
    // - ShowCultureDropdown, OneTrustId, social link URLs, etc.
}
