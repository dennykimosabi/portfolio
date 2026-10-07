using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ActiveFiltersChips : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Active model-year filter; chip hidden when 0.</summary>
    [Parameter] public int Year { get; set; }
    /// <summary>Active make key; chip hidden when 0.</summary>
    [Parameter] public int UkeyMake { get; set; }
    /// <summary>Active model key; chip hidden when 0.</summary>
    [Parameter] public int UkeyModel { get; set; }
    /// <summary>Active driveline key; chip hidden when 0.</summary>
    [Parameter] public int UkeyDriveline { get; set; }
    /// <summary>Active trim-level key; chip hidden when 0.</summary>
    [Parameter] public int UkeyTrimLevel { get; set; }
    /// <summary>Active category key; chip hidden when 0.</summary>
    [Parameter] public int UkeyCategory { get; set; }
    /// <summary>Display name for the make chip.</summary>
    [Parameter] public string MakeName { get; set; } = string.Empty;
    /// <summary>Display name for the model chip.</summary>
    [Parameter] public string ModelName { get; set; } = string.Empty;
    /// <summary>Display name for the driveline chip.</summary>
    [Parameter] public string DrivelineName { get; set; } = string.Empty;
    /// <summary>Display name for the trim-level chip.</summary>
    [Parameter] public string TrimLevelName { get; set; } = string.Empty;
    /// <summary>Display name for the category chip.</summary>
    [Parameter] public string CategoryName { get; set; } = string.Empty;
    /// <summary>Invoked when the year chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveYear { get; set; }
    /// <summary>Invoked when the make chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveMake { get; set; }
    /// <summary>Invoked when the model chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveModel { get; set; }
    /// <summary>Invoked when the driveline chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveDriveline { get; set; }
    /// <summary>Invoked when the trim-level chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveTrim { get; set; }
    /// <summary>Invoked when the category chip is dismissed.</summary>
    [Parameter] public EventCallback OnRemoveCategory { get; set; }
}
