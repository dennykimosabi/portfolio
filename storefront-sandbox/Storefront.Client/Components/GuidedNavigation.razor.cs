using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class GuidedNavigation : ComponentBase
{
    [Parameter]
    public long UkeyProduct { get; set; }

    [Parameter]
    public string PageUrl { get; set; } = "/productDetails.aspx";

    [Parameter]
    public string QueryString { get; set; } = string.Empty;

    [Parameter]
    public int? UkeyMake { get; set; }

    [Parameter]
    public int? UkeyModel { get; set; }

    [Parameter]
    public int? ModelYear { get; set; }

    [Parameter]
    public int? UkeyDriveline { get; set; }

    [Parameter]
    public int? UkeyTrimLevel { get; set; }

    [Inject]
    private IGuidedNavigationService GuidedNavigationService { get; set; } = null!;

    [Inject]
    private ILogService LogService { get; set; } = null!;

    [PersistentState]
    public GuidedNavigationHeader? _header { get; set; }

    [PersistentState]
    public GuidedNavigationDetailResponse? _detail { get; set; }

    private bool _isLoading = true;
    private bool _firstParamSet = true;
    private long _lastUkeyProduct = 0;
    private int? _lastUkeyMake;
    private int? _lastUkeyModel;
    private int? _lastModelYear;
    private int? _lastUkeyDriveline;
    private int? _lastUkeyTrimLevel;

    protected override async Task OnInitializedAsync()
    {
        // If data was already restored from prerender state, skip initial load.
        // _isLoading is set BEFORE the log's await — an await ahead of it would let Blazor
        // render mid-yield with _isLoading still at its default `true`, flashing a loading
        // state even though nothing is actually being (re)fetched.
        if (_header is not null && _detail is not null)
        {
            _isLoading = false;
            await LogService.LogInformation($"GuidedNavigation.OnInitializedAsync: Data already present (prerender restore), skipping load", "GuidedNavigation");
            return;
        }

        await LoadGuidedNavigationAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // On first param set (hydration), if data was restored from prerender state, skip re-loading.
        // No logging (or any other await) happens before this check — see the _isLoading-ordering
        // note in OnInitializedAsync above; the same flash bug applied here too.
        if (_firstParamSet)
        {
            _firstParamSet = false;
            if (_header is not null && _detail is not null)
            {
                _isLoading = false;
                await LogService.LogInformation($"GuidedNavigation: Data restored from prerender state for Product={UkeyProduct}, skipping reload", "GuidedNavigation");
                return;
            }
            else
            {
                await LogService.LogInformation($"GuidedNavigation: First param set but data NOT restored (header={_header is null}, detail={_detail is null}), proceeding to load", "GuidedNavigation");
            }
        }

        // Check if parameters actually changed (not just a re-render with same params)
        if (UkeyProduct == _lastUkeyProduct &&
            UkeyMake == _lastUkeyMake &&
            UkeyModel == _lastUkeyModel &&
            ModelYear == _lastModelYear &&
            UkeyDriveline == _lastUkeyDriveline &&
            UkeyTrimLevel == _lastUkeyTrimLevel)
        {
            _isLoading = false;
            await LogService.LogInformation($"GuidedNavigation: Parameters unchanged for Product={UkeyProduct}, skipping reload", "GuidedNavigation");
            return;
        }

        // Store current parameters for next comparison
        _lastUkeyProduct = UkeyProduct;
        _lastUkeyMake = UkeyMake;
        _lastUkeyModel = UkeyModel;
        _lastModelYear = ModelYear;
        _lastUkeyDriveline = UkeyDriveline;
        _lastUkeyTrimLevel = UkeyTrimLevel;

        // Reload if parameters actually changed
        await LoadGuidedNavigationAsync();
    }

    private async Task LoadGuidedNavigationAsync()
    {
        _isLoading = true;

        try
        {
            if (UkeyProduct <= 0)
            {
                _isLoading = false;
                await LogService.LogInformation($"GuidedNavigation: UkeyProduct is {UkeyProduct}, skipping", "GuidedNavigation");
                return;
            }

            var request = new GetGuidedNavigationRequest
            {
                UkeyProduct = UkeyProduct,
                PageUrl = PageUrl,
                QueryString = QueryString,
                UkeyMake = UkeyMake,
                UkeyModel = UkeyModel,
                ModelYear = ModelYear,
                UkeyDriveline = UkeyDriveline,
                UkeyTrimLevel = UkeyTrimLevel
            };

            await LogService.LogInformation($"GuidedNavigation: Loading with Product={UkeyProduct}, Make={UkeyMake}, Model={UkeyModel}, Year={ModelYear}, Driveline={UkeyDriveline}, Trim={UkeyTrimLevel}", "GuidedNavigation");

            var (header, detail) = await GuidedNavigationService.GetBothAsync(request);

            await LogService.LogInformation($"GuidedNavigation: Received from service: header type={header?.GetType().Name ?? "NULL"}, detail type={detail?.GetType().Name ?? "NULL"}", "GuidedNavigation");

            await LogService.LogInformation($"GuidedNavigation: Header={header?.GuideHeader ?? "NULL"}, Detail count={(detail?.Details.Count() ?? 0)}", "GuidedNavigation");

            _header = header;
            _detail = detail;
        }
        catch (Exception ex)
        {
            await LogService.LogError($"GuidedNavigation load error: {ex.Message}", "GuidedNavigation", ex.StackTrace);
            // Degrade gracefully — if guided nav fails, the page still loads
        }
        finally
        {
            _isLoading = false;
        }
    }
}
