using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class LoadingIndicator : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Label shown next to the spinner (e.g. "Loading results…", "Loading cart…").</summary>
    [Parameter] public string Text { get; set; } = "Loading…";
}
