using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;
using Storefront.Client.State;

namespace Storefront.Client.Components;

public partial class StorefrontHeader : ComponentBase
{
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;
    [Inject] public IWebsiteSettingsService WebsiteSettingsService { get; set; } = default!;
    [Inject] public ISessionStateProvider SessionState { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public VehicleFitmentState FitmentState { get; set; } = default!;

    [Parameter] public string H1TagText { get; set; } = string.Empty;

    [PersistentState]
    public WebsiteSettingsDto? Settings { get; set; }

    private string _searchInput = string.Empty;
    private bool _isSearchOpen;
    private bool _hasFocusedSearch;
    private ElementReference _searchInputRef;

    private void OpenSearch() => _isSearchOpen = true;

    private void CloseSearch()
    {
        _isSearchOpen = false;
        _hasFocusedSearch = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isSearchOpen && !_hasFocusedSearch)
        {
            _hasFocusedSearch = true;
            await _searchInputRef.FocusAsync();
        }
    }

    private void HandleSearch()
    {
        var q = _searchInput.Trim();
        if (string.IsNullOrEmpty(q)) return;

        var url = $"/search?q={Uri.EscapeDataString(q)}";

        if (FitmentState.HasVehicle)
        {
            url += $"&modelYear={FitmentState.Year}&ukey_model={FitmentState.UkeyModel}";
            if (FitmentState.UkeyDriveline.HasValue)
                url += $"&ukey_driveline={FitmentState.UkeyDriveline}";
            if (FitmentState.UkeyTrimLevel.HasValue)
                url += $"&ukey_trimlevel={FitmentState.UkeyTrimLevel}";
        }

        NavigationManager.NavigateTo(url);
    }

    protected override async Task OnInitializedAsync()
    {
        if (Settings is not null) return; // restored from prerender state, skip service call

        Settings = await WebsiteSettingsService.GetSettingsAsync(
            WebsiteContext.UkeyWebsite,
            SessionState.SessionId,
            SessionState.MachineId
        );
    }
}
