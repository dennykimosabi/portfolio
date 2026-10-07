// Stand-ins for Storefront.Client.State. Behaviour follows the contracts described in the
// client code's comments (e.g. OnChange fires only on a real change, SetVehicleAndCategory is
// atomic), but storage is in-memory per circuit via the demo services.

using SimplePart.Shared.Models.Storefront;
using Portfolio.Demo;

namespace Storefront.Client.State;

public sealed class VehicleFitmentState
{
    public int? UkeyMake { get; private set; }
    public int? Year { get; private set; }
    public int? UkeyModel { get; private set; }
    public int? UkeyDriveline { get; private set; }
    public int? UkeyTrimLevel { get; private set; }
    public int? UkeyCategory { get; private set; }

    /// <summary>All-or-nothing: a vehicle needs both a year and a model.</summary>
    public bool HasVehicle => Year is > 0 && UkeyModel is > 0;

    public event Action? OnChange;

    /// <summary>Sets the vehicle, keeping the current category. A null make keeps the current make.</summary>
    public void Set(int? ukeyMake, int? year, int? ukeyModel, int? ukeyDriveline, int? ukeyTrimLevel) =>
        Apply(ukeyMake ?? UkeyMake, year, ukeyModel, ukeyDriveline, ukeyTrimLevel, UkeyCategory);

    public void SetMakeAndCategory(int? ukeyMake, int? ukeyCategory) =>
        Apply(ukeyMake, Year, UkeyModel, UkeyDriveline, UkeyTrimLevel, ukeyCategory);

    /// <summary>Vehicle + category in one step, so subscribers see a single OnChange.</summary>
    public void SetVehicleAndCategory(int? ukeyMake, int? year, int? ukeyModel, int? ukeyDriveline, int? ukeyTrimLevel, int? ukeyCategory) =>
        Apply(ukeyMake, year, ukeyModel, ukeyDriveline, ukeyTrimLevel, ukeyCategory);

    public void SetCategory(int? ukeyCategory) =>
        Apply(UkeyMake, Year, UkeyModel, UkeyDriveline, UkeyTrimLevel, ukeyCategory);

    /// <summary>Clears the vehicle (make and category are left alone).</summary>
    public void Clear() => Apply(UkeyMake, null, null, null, null, UkeyCategory);

    private void Apply(int? make, int? year, int? model, int? driveline, int? trim, int? category)
    {
        if (make == UkeyMake && year == Year && model == UkeyModel && driveline == UkeyDriveline
            && trim == UkeyTrimLevel && category == UkeyCategory)
            return;

        UkeyMake = make;
        Year = year;
        UkeyModel = model;
        UkeyDriveline = driveline;
        UkeyTrimLevel = trim;
        UkeyCategory = category;
        OnChange?.Invoke();
    }
}

public sealed class GarageState
{
    private List<CustomerVehicle> _vehicles = [];
    private bool _loaded;
    private long _nextUkey = 9000;

    public IReadOnlyList<CustomerVehicle> Vehicles => _vehicles;

    public event Action? OnChange;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        _loaded = true;
        await Task.Yield();
        _vehicles = DemoCatalog.SeedGarage();
        OnChange?.Invoke();
    }

    public void Seed(List<CustomerVehicle> vehicles)
    {
        _vehicles = vehicles.ToList();
        _loaded = true;
    }

    public Task AddAsync(int ukeyMake, int ukeyModel, int ukeyDriveline, int ukeyTrimLevel, int year)
    {
        var exists = _vehicles.Any(v => v.UkeyModel == ukeyModel && v.ModelYear == year
                                        && v.UkeyDriveline == ukeyDriveline && v.UkeyTrimLevel == ukeyTrimLevel);
        if (!exists)
        {
            _vehicles.Insert(0, DemoCatalog.CreateGarageVehicle(_nextUkey++, year, ukeyModel, ukeyDriveline, ukeyTrimLevel));
            OnChange?.Invoke();
        }
        return Task.CompletedTask;
    }

    public Task RenameAsync(long ukey, string name)
    {
        var vehicle = _vehicles.FirstOrDefault(v => v.Ukey == ukey);
        if (vehicle is not null)
        {
            vehicle.VehicleDescription = name;
            OnChange?.Invoke();
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(long ukey)
    {
        if (_vehicles.RemoveAll(v => v.Ukey == ukey) > 0)
            OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _vehicles = [];
        OnChange?.Invoke();
        return Task.CompletedTask;
    }
}

public sealed class CartState(DemoCartStore store)
{
    public int ItemCount { get; private set; }
    public bool IsLoadingContents { get; private set; }
    public IReadOnlyList<CartItem> Items { get; private set; } = [];
    public IReadOnlyList<CartSummaryLine> Summary { get; private set; } = [];

    public event Action? OnChange;

    public void SetItemCount(int count)
    {
        ItemCount = count;
        OnChange?.Invoke();
    }

    public Task RefreshCountAsync()
    {
        ItemCount = store.ItemCount;
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public async Task RefreshContentsAsync()
    {
        IsLoadingContents = true;
        OnChange?.Invoke();

        await Task.Delay(DemoCatalog.SimulatedLatencyMs); // lets the loading indicator show
        Snapshot();

        IsLoadingContents = false;
        OnChange?.Invoke();
    }

    public Task AddToCartAsync(long productUkey, string fitmentString = "")
    {
        store.Add(productUkey, 1, fitmentString);
        Snapshot();
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(CartItem item)
    {
        store.Remove(item.Ukey);
        Snapshot();
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task RemoveAllAsync()
    {
        store.Clear();
        Snapshot();
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(CartItem item, int quantity)
    {
        store.SetQuantity(item.Ukey, quantity);
        Snapshot();
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    private void Snapshot()
    {
        Items = store.Items.ToList();
        Summary = store.BuildSummary();
        ItemCount = store.ItemCount;
    }
}
