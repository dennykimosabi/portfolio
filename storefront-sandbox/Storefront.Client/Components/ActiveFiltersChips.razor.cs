using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ActiveFiltersChips : ComponentBase
{
    [Parameter] public int Year { get; set; }
    [Parameter] public int UkeyMake { get; set; }
    [Parameter] public int UkeyModel { get; set; }
    [Parameter] public int UkeyDriveline { get; set; }
    [Parameter] public int UkeyTrimLevel { get; set; }
    [Parameter] public int UkeyCategory { get; set; }
    [Parameter] public string MakeName { get; set; } = string.Empty;
    [Parameter] public string ModelName { get; set; } = string.Empty;
    [Parameter] public string DrivelineName { get; set; } = string.Empty;
    [Parameter] public string TrimLevelName { get; set; } = string.Empty;
    [Parameter] public string CategoryName { get; set; } = string.Empty;
    [Parameter] public EventCallback OnRemoveYear { get; set; }
    [Parameter] public EventCallback OnRemoveMake { get; set; }
    [Parameter] public EventCallback OnRemoveModel { get; set; }
    [Parameter] public EventCallback OnRemoveDriveline { get; set; }
    [Parameter] public EventCallback OnRemoveTrim { get; set; }
    [Parameter] public EventCallback OnRemoveCategory { get; set; }
}
