using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class ProductInformation : ComponentBase
{
    /// Compliance/merchandising tags from pr_returnProductTags.
    [Parameter] public IReadOnlyList<ProductTag> Tags { get; set; } = [];

    [Parameter] public IReadOnlyList<string> Features { get; set; } = [];

    /// Free-text line under the bullets, e.g. an "All w/ …" applicability note.
    [Parameter] public string? Note { get; set; }

    /// Part numbers this product supersedes.
    [Parameter] public IReadOnlyList<string> Supersessions { get; set; } = [];

    // TagType's distinct values aren't documented anywhere in the legacy code, so this is a
    // styling hook only — an unrecognised type just gets no extra styles. Non-alphanumerics
    // are collapsed to hyphens so the value can't break out of the class attribute.
    private static string? TagModifier(ProductTag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.TagType))
        {
            return null;
        }

        var slug = new string(tag.TagType
            .Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-')
            .ToArray());

        return $"product-information-tag--{slug}";
    }
}
