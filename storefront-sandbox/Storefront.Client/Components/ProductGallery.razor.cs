using Microsoft.AspNetCore.Components;

namespace Storefront.Client.Components;

public partial class ProductGallery : ComponentBase
{
    /// Image URLs, primary first. Entries may be empty while the data layer is stubbed —
    /// the placeholder glyph renders in their place.
    [Parameter] public IReadOnlyList<string> Images { get; set; } = [];

    /// Accessible name for the primary image, normally the product title.
    [Parameter] public string AltText { get; set; } = string.Empty;

    /// How many thumbnails to show before collapsing the rest into the "N More" tile.
    [Parameter] public int MaxThumbnails { get; set; } = 9;

    [Parameter] public EventCallback OnViewAll { get; set; }

    private int _selectedIndex;

    private string? SelectedImage =>
        _selectedIndex < Images.Count ? Images[_selectedIndex] : Images.FirstOrDefault();

    private IEnumerable<string> VisibleThumbnails => Images.Take(MaxThumbnails);

    private int OverflowCount => Math.Max(0, Images.Count - MaxThumbnails);

    private void Select(int index) => _selectedIndex = index;

    // Guard the selection when the image list is swapped out from under us (e.g. navigating
    // product-to-product reuses this component instance).
    protected override void OnParametersSet()
    {
        if (_selectedIndex >= Images.Count)
        {
            _selectedIndex = 0;
        }
    }
}
