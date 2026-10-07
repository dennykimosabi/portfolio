using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class HeadConfigInjector : ComponentBase
{
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public IWebsiteSettingsService Settings { get; set; } = default!;

    [PersistentState] public HeadConfig? _head { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (_head is not null) return; // restored from prerender state

        var settings = await Settings.GetSettingsAsync(
            WebsiteContext.UkeyWebsite,
            SessionState.SessionId,
            SessionState.MachineId);
        _head = settings.Head;
    }
}
