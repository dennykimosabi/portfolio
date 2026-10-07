using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class StarRating : ComponentBase
{
    [Parameter] public decimal Rating { get; set; }

    /// Optional trailing text, e.g. "45 Reviews".
    [Parameter] public string? Label { get; set; }

    // A star is full once the rating reaches it, half once it reaches the midpoint below it.
    private string StarClass(int position) =>
        Rating >= position ? "is-full"
        : Rating >= position - 0.5m ? "is-half"
        : "is-empty";
}
