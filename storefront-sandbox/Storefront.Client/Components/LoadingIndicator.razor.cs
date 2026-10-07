using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class LoadingIndicator : ComponentBase
{
    /// <summary>Label shown next to the spinner (e.g. "Loading results…", "Loading cart…").</summary>
    [Parameter] public string Text { get; set; } = "Loading…";
}
