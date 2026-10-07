using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class ConfidencePanel : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IWebsiteSettingsService WebsiteSettingsService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;

    [PersistentState] public WebsiteSettingsDto? _settings { get; set; }

    private string Title => string.IsNullOrWhiteSpace(_settings?.ConfidenceContainerTitle)
        ? "Shop with Confidence"
        : _settings.ConfidenceContainerTitle;

    private string Text => string.IsNullOrWhiteSpace(_settings?.ConfidenceContainerText)
        ? $"Your purchases are 100% safe and secure. Buy with confidence from {_settings?.WebsiteUrl ?? "us"}."
        : _settings.ConfidenceContainerText;

    // Phone: legacy converts dots to dashes for both the tel: href and the display text.
    private string PhoneHref => _settings?.PhoneNumber.Replace(".", "-") ?? string.Empty;
    private string PhoneDisplay => PhoneHref;

    private bool _hasPhone => !string.IsNullOrWhiteSpace(_settings?.PhoneNumber);
    private bool _hasEmail => !string.IsNullOrWhiteSpace(_settings?.SalesEmail) && _settings?.HideCart != true;
    private bool _hasReturnPolicy => !string.IsNullOrWhiteSpace(_settings?.PoliciesLink) && _settings?.NoCommerce != true;
    private bool _showMyProfile => _settings?.HideCart != true && _settings?.HideProfile != true;
    private bool _hasAnyContactItem => _hasPhone || _hasEmail || _hasReturnPolicy || _showMyProfile;

    protected override async Task OnInitializedAsync()
    {
        if (_settings is not null) return;

        _settings = await WebsiteSettingsService.GetSettingsAsync(
            WebsiteContext.UkeyWebsite,
            SessionState.SessionId,
            SessionState.MachineId);
    }
}
