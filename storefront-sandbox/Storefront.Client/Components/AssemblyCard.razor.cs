using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class AssemblyCard : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>The assembly result rendered as a card.</summary>
    [Parameter, EditorRequired] public AssemblyResult Assembly { get; set; } = default!;

    private bool _isExpanded;
    private readonly string _descriptionId = $"assembly-description-{Guid.NewGuid():N}";

    private bool HasDescription => !string.IsNullOrWhiteSpace(Assembly.AssemblyDescription);

    private void ToggleExpand() => _isExpanded = !_isExpanded;
}
