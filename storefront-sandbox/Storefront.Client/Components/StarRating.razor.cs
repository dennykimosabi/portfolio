using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class StarRating : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Rating value from 0 to 5; supports halves.</summary>
    [Parameter] public decimal Rating { get; set; }

    /// Optional trailing text, e.g. "45 Reviews".
    [Parameter] public string? Label { get; set; }

    // A star is full once the rating reaches it, half once it reaches the midpoint below it.
    private string StarClass(int position) =>
        Rating >= position ? "is-full"
        : Rating >= position - 0.5m ? "is-half"
        : "is-empty";
}
