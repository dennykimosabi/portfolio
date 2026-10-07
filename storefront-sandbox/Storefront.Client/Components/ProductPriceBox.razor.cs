using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Storefront.Client.Models;
using Storefront.Client.Utilities;

namespace Storefront.Client.Components;

public partial class ProductPriceBox : ComponentBase
{

    /// <summary>Additional CSS classes applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Additional attributes splatted onto the component root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Inject] public IClientAssetResolver AssetResolver { get; set; } = default!;

    // Brand/UI icons live on the asset host, not the site origin — same convention as
    // GarageVehicleCard's edit/delete icons.
    private string DeliveryIcon => AssetResolver.Resolve("/images/2022-base-template-assets/icons/icon-delivery-active.png");
    private string PickupIcon => AssetResolver.Resolve("/images/2022-base-template-assets/icons/icon-pickup-active.png");

    /// <summary>Current selling price.</summary>
    [Parameter] public decimal Price { get; set; }

    /// <summary>Manufacturer suggested retail price for the savings line.</summary>
    [Parameter] public decimal Msrp { get; set; }

    /// <summary>Dealer star rating shown beside the price.</summary>
    [Parameter] public decimal DealerRating { get; set; }

    /// <summary>Pre-formatted savings text, e.g. "You save $12.00".</summary>
    [Parameter] public string? SavingsText { get; set; }

    /// <summary>Promotional message rendered under the price.</summary>
    [Parameter] public string? PriceBoxMessage { get; set; }

    /// <summary>Stock level driving the availability badge.</summary>
    [Parameter] public AvailabilityLevel Availability { get; set; } = AvailabilityLevel.AvailableForOrder;

    /// <summary>Availability label, e.g. "In Stock".</summary>
    [Parameter] public string AvailabilityText { get; set; } = string.Empty;

    /// <summary>Delivery option with ETA and cost.</summary>
    [Parameter, EditorRequired] public DeliveryOption Delivery { get; set; } = default!;

    /// <summary>Pickup option with store details.</summary>
    [Parameter, EditorRequired] public PickupOption Pickup { get; set; } = default!;

    /// <summary>Selected quantity. Supports two-way binding.</summary>
    [Parameter] public int Quantity { get; set; } = 1;

    /// <summary>Invoked when the quantity changes.</summary>
    [Parameter] public EventCallback<int> QuantityChanged { get; set; }

    /// <summary>Invoked when add-to-cart is clicked.</summary>
    [Parameter] public EventCallback OnAddToCart { get; set; }

    /// <summary>Invoked when buy-now is clicked.</summary>
    [Parameter] public EventCallback OnBuyNow { get; set; }

    /// <summary>Invoked to estimate shipping for the entered ZIP.</summary>
    [Parameter] public EventCallback OnEstimateShipping { get; set; }

    /// <summary>Shows the estimating spinner.</summary>
    [Parameter] public bool IsEstimatingShipping { get; set; }

    /// <summary>Result text after a shipping estimate.</summary>
    [Parameter] public string? ShippingEstimateText { get; set; }

    /// <summary>Invoked when the ZIP code input changes.</summary>
    [Parameter] public EventCallback<string> OnZipCodeChanged { get; set; }

    private const int MinQuantity = 1;

    // Which fulfilment option is currently selected — a pure display toggle, not persisted
    // and not sent anywhere. Mirrors legacy's sharedVariable.fulfillmentType, but since both
    // halves already live in this one component there's no cross-component wiring needed.
    // Default is Delivery, matching legacy's selectedButton default; legacy also defaults to
    // Pickup when retail shipping isn't offered at all, but that show/hide flag isn't wired
    // yet (see ProductPageViewModels.cs's placeholder-layer note), so it's a known gap.
    private enum FulfillmentSelection { Delivery, Pickup }

    private FulfillmentSelection _selected = FulfillmentSelection.Delivery;

    private string SelectedFulfillmentPromise =>
        _selected == FulfillmentSelection.Pickup ? Pickup.Promise : Delivery.Promise;

    private void SelectFulfillment(FulfillmentSelection option) => _selected = option;

    private bool _isEditingZip;
    private string _zipInput = string.Empty;
    private string? _zipError;

    private void BeginZipEdit()
    {
        _zipInput = Delivery.ZipCode;
        _zipError = null;
        _isEditingZip = true;
    }

    private void CancelZipEdit()
    {
        _isEditingZip = false;
        _zipError = null;
    }

    private void HandleZipInput(ChangeEventArgs args) => _zipInput = args.Value?.ToString() ?? string.Empty;

    private Task HandleZipKeyDown(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Enter": return SaveZipCode();
            case "Escape": CancelZipEdit(); break;
        }

        return Task.CompletedTask;
    }

    // Loose on purpose — accepts US ZIP/ZIP+4 and other countries' alphanumeric postal
    // codes alike; the server owns real validation. This just catches empty/junk input
    // before spending a round trip on it.
    private async Task SaveZipCode()
    {
        var trimmed = _zipInput.Trim();
        if (trimmed.Length is < 3 or > 10)
        {
            _zipError = "Enter a valid postal code.";
            return;
        }

        _isEditingZip = false;
        _zipError = null;

        if (OnZipCodeChanged.HasDelegate)
        {
            await OnZipCodeChanged.InvokeAsync(trimmed);
        }
    }

    private string AvailabilityClass => Availability switch
    {
        AvailabilityLevel.InStock => "is-in-stock",
        AvailabilityLevel.LimitedStock => "is-limited",
        _ => "is-orderable"
    };

    private Task ChangeQuantity(int delta) => SetQuantity(Quantity + delta);

    // Typed input is clamped rather than rejected — a blank or junk value falls back to the
    // minimum instead of leaving the field in an unusable state.
    private Task HandleQuantityInput(ChangeEventArgs args)
    {
        var parsed = int.TryParse(args.Value?.ToString(), out var value) ? value : MinQuantity;
        return SetQuantity(parsed);
    }

    private Task SetQuantity(int value)
    {
        Quantity = Math.Max(MinQuantity, value);

        return QuantityChanged.HasDelegate ? QuantityChanged.InvokeAsync(Quantity) : Task.CompletedTask;
    }
}
