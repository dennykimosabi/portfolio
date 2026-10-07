using Microsoft.AspNetCore.Components;
using Storefront.Client.Models;

namespace Storefront.Client.Components;

public partial class PeopleAlsoBought : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>Related products shown in the carousel.</summary>
    [Parameter] public IReadOnlyList<RelatedProduct> Products { get; set; } = [];

    /// <summary>Section heading text.</summary>
    [Parameter] public string Heading { get; set; } = "People Also Bought";

    /// TODO: wire to ICartService once the cart slice is in place.
    [Parameter] public EventCallback<RelatedProduct> OnAddToCart { get; set; }

    private Task HandleAdd(RelatedProduct product) =>
        OnAddToCart.HasDelegate ? OnAddToCart.InvokeAsync(product) : Task.CompletedTask;
}
