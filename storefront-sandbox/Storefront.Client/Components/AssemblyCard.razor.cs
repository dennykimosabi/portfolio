using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class AssemblyCard : ComponentBase
{
    [Parameter, EditorRequired] public AssemblyResult Assembly { get; set; } = default!;

    private bool _isExpanded;
    private readonly string _descriptionId = $"assembly-description-{Guid.NewGuid():N}";

    private bool HasDescription => !string.IsNullOrWhiteSpace(Assembly.AssemblyDescription);

    private void ToggleExpand() => _isExpanded = !_isExpanded;
}
