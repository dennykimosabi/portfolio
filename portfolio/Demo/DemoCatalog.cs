using System.Text;
using SimplePart.Shared.Models.Storefront;

namespace Portfolio.Demo;

public sealed record DemoModel(int Ukey, string Name, string Slug, string Color, int[] Drivelines);

public sealed record DemoOption(int Ukey, string Name, string Slug);

public sealed record DemoCategory(int Ukey, string Name, string Slug, string Description, bool IsAccessory, string Color, string[] Assemblies);

public sealed record DemoProduct(long Ukey, string StockCode, string Description, decimal Price, decimal ListPrice, int UkeyCategory, string Blurb)
{
    public string Slug => DemoCatalog.Slugify(Description);
    public string Url => $"/product/{StockCode}/{Slug}";
    public DemoCategory Category => DemoCatalog.CategoryByUkey(UkeyCategory)!;
    public string Image(int variant = 0, int size = 400) =>
        DemoImages.Url("part", Description, DemoCatalog.Shade(Category.Color, variant), size, size);
}

/// <summary>
/// All dummy data for the preview: one fictional dealer, one make, a handful of models,
/// categories and products. Everything is invented — no real dealer, customer or pricing data.
/// </summary>
public static class DemoCatalog
{
    /// <summary>Artificial delay on "network" calls so loading states are visible.</summary>
    public const int SimulatedLatencyMs = 250;

    public const int UkeyWebsite = 1;
    public const int MakeUkey = 1;
    public const string MakeName = "Volvo";
    public const string MakeSlug = "volvo";

    public static readonly int[] Years = [2025, 2024, 2023, 2022, 2021, 2020];

    public static readonly DemoModel[] Models =
    [
        new(101, "XC40", "xc40", "2f6f9f", [201, 202]),
        new(102, "XC60", "xc60", "7a3b3b", [201, 202]),
        new(103, "XC90", "xc90", "2f4f3f", [201]),
        new(104, "S60", "s60", "4b4b6b", [201, 202]),
    ];

    public static readonly DemoOption[] Drivelines =
    [
        new(201, "AWD", "awd"),
        new(202, "FWD", "fwd"),
    ];

    public static readonly DemoOption[] Trims =
    [
        new(301, "Core", "core"),
        new(302, "Plus", "plus"),
        new(303, "Ultimate", "ultimate"),
    ];

    public static readonly DemoCategory[] Categories =
    [
        new(11, "Brakes", "brakes", "Pads, rotors, calipers and hardware", false, "a33b2f",
            ["Front Brakes", "Rear Brakes", "Parking Brake", "Brake Lines & Hoses"]),
        new(12, "Engine", "engine", "Belts, ignition and engine service parts", false, "3b5998",
            ["Engine Block", "Timing & Belt Drive", "Intake & Air Filter", "Ignition"]),
        new(13, "Suspension", "suspension", "Struts, links, bushings and arms", false, "4a6b3a",
            ["Front Suspension", "Rear Suspension", "Steering Linkage"]),
        new(14, "Electrical", "electrical", "Batteries, lighting and wiper systems", false, "b58a1b",
            ["Battery & Charging", "Lighting", "Wipers & Washers"]),
        new(15, "Body", "body", "Mirrors, grilles, bumpers and trim", false, "5b5b5b",
            ["Front Bumper", "Doors & Mirrors", "Grille & Trim"]),
        new(16, "Cooling", "cooling", "Radiators, hoses and reservoirs", false, "1f7a8c",
            ["Radiator", "Coolant Reservoir", "Water Pump"]),
        new(17, "Exhaust", "exhaust", "Manifolds, converters and mufflers", false, "6b4a2f",
            ["Exhaust Manifold", "Catalytic Converter", "Muffler & Tailpipe"]),
        new(18, "Filters & Maintenance", "filters", "Oil, air and cabin filters, service kits", false, "2f6f5f",
            ["Oil Filter", "Air Filters", "Service Kits"]),

        new(51, "Cargo & Storage", "cargo-storage", "Organisers, mats, roof boxes", true, "34495e", ["Cargo Area"]),
        new(52, "Exterior", "exterior", "Mud flaps, tread plates, styling", true, "8e3b46", ["Exterior Styling"]),
        new(53, "Interior", "interior", "Floor mats, seat covers, comfort", true, "5d4037", ["Interior Comfort"]),
        new(54, "Wheels & Tires", "wheels", "Alloy wheels and locking kits", true, "37474f", ["Wheels"]),
        new(55, "Electronics", "electronics", "Charging, cameras, connectivity", true, "283593", ["Electronics"]),
        new(56, "Protection", "protection", "Paint film, bumper and sill guards", true, "2e7d32", ["Protection"]),
    ];

    public static readonly DemoProduct[] Products =
    [
        new(1001, "VP-10421", "Front Brake Pad Set", 89.95m, 109.00m, 11, "Low-dust ceramic compound matched to the factory rotors."),
        new(1002, "VP-10422", "Rear Brake Rotor", 74.50m, 92.00m, 11, "Coated rotor for corrosion resistance; sold individually."),
        new(1003, "VP-10423", "Brake Caliper Bolt Kit", 18.25m, 22.00m, 11, "Replacement guide bolts and boots for one caliper."),
        new(1004, "VP-10424", "Brake Fluid DOT 4 (1L)", 21.40m, 26.00m, 11, "Low-viscosity DOT 4 fluid for ABS/ESC systems."),
        new(1005, "VP-20511", "Engine Oil Filter", 14.95m, 18.50m, 12, "Cartridge-style filter element with O-ring."),
        new(1006, "VP-20512", "Serpentine Belt", 46.80m, 58.00m, 12, "Multi-rib accessory drive belt."),
        new(1007, "VP-20513", "Spark Plug Set (4)", 62.00m, 75.00m, 12, "Iridium-tipped plugs, pre-gapped."),
        new(1008, "VP-20514", "Engine Air Filter", 32.10m, 39.00m, 12, "Pleated paper element for the intake box."),
        new(1009, "VP-30611", "Front Strut Assembly", 289.00m, 345.00m, 13, "Complete strut with spring and top mount."),
        new(1010, "VP-30612", "Stabilizer Link", 38.60m, 47.00m, 13, "Front sway-bar end link with nuts."),
        new(1011, "VP-30613", "Control Arm Bushing", 55.25m, 66.00m, 13, "Rear lower control arm bushing."),
        new(1012, "VP-40711", "12V AGM Battery", 229.00m, 269.00m, 14, "Start/stop capable AGM battery."),
        new(1013, "VP-40712", "Headlamp Bulb H7", 24.90m, 29.00m, 14, "Halogen low-beam bulb."),
        new(1014, "VP-40713", "Wiper Motor", 198.00m, 240.00m, 14, "Front windscreen wiper motor assembly."),
        new(1015, "VP-50811", "Door Mirror Cover", 64.00m, 78.00m, 15, "Paint-to-match mirror cap, driver side."),
        new(1016, "VP-50812", "Front Bumper Grille", 142.00m, 170.00m, 15, "Lower bumper grille insert."),
        new(1017, "VP-60911", "Coolant Reservoir", 58.70m, 70.00m, 16, "Expansion tank with level sensor port."),
        new(1018, "VP-60912", "Radiator Hose", 44.00m, 53.00m, 16, "Upper radiator hose, moulded."),
        new(1019, "VP-71011", "Exhaust Gasket Kit", 27.50m, 33.00m, 17, "Manifold-to-pipe gasket and hardware."),
        new(1020, "VP-81111", "Cabin Air Filter", 29.95m, 36.00m, 18, "Particulate filter for the climate system."),

        new(2001, "VA-90011", "Cargo Area Organizer", 86.40m, 99.99m, 51, "Folding organiser with super-grippy underside."),
        new(2002, "VA-90012", "Carpeted Cargo Area Protector", 129.00m, 149.00m, 51, "Tailored carpet protector for the load floor."),
        new(2003, "VA-90013", "Roof Box 350L", 699.00m, 799.00m, 51, "Aerodynamic lockable roof box."),
        new(2004, "VA-90021", "Mud Flaps Set", 89.00m, 105.00m, 52, "Front and rear moulded mud flaps."),
        new(2005, "VA-90022", "Illuminated Tread Plates", 249.00m, 289.00m, 52, "LED-lit stainless tread plates."),
        new(2006, "VA-90031", "All-Weather Floor Mats", 139.00m, 165.00m, 53, "Raised-edge rubber mats, set of four."),
        new(2007, "VA-90032", "Tailored Seat Covers", 329.00m, 379.00m, 53, "Water-resistant front seat covers."),
        new(2008, "VA-90041", "19\" Alloy Wheel", 610.00m, 695.00m, 54, "10-spoke diamond-cut alloy wheel."),
        new(2009, "VA-90042", "Wheel Lock Kit", 59.00m, 69.00m, 54, "Locking wheel bolts with coded key."),
        new(2010, "VA-90051", "Wireless Charging Pad", 149.00m, 175.00m, 55, "Qi charging pad for the centre console."),
        new(2011, "VA-90052", "Dash Camera Kit", 279.00m, 319.00m, 55, "Front and rear HD dash cameras."),
        new(2012, "VA-90061", "Paint Protection Film Kit", 189.00m, 219.00m, 56, "Pre-cut film for bonnet and bumper."),
        new(2013, "VA-90062", "Rear Bumper Protector", 79.00m, 94.00m, 56, "Stainless load-sill protector."),
    ];

    // ─── Lookups ──────────────────────────────────────────────────────────

    public static DemoModel? ModelByUkey(int? ukey) => Models.FirstOrDefault(m => m.Ukey == ukey);
    public static DemoOption? DrivelineByUkey(int? ukey) => Drivelines.FirstOrDefault(d => d.Ukey == ukey);
    public static DemoOption? TrimByUkey(int? ukey) => Trims.FirstOrDefault(t => t.Ukey == ukey);
    public static DemoCategory? CategoryByUkey(int? ukey) => Categories.FirstOrDefault(c => c.Ukey == ukey);
    public static DemoCategory? CategoryBySlug(string? slug) =>
        Categories.FirstOrDefault(c => string.Equals(c.Slug, slug, StringComparison.OrdinalIgnoreCase));
    public static DemoProduct? ProductByUkey(long ukey) => Products.FirstOrDefault(p => p.Ukey == ukey);
    public static DemoProduct? ProductByStockCode(string? stock) =>
        Products.FirstOrDefault(p => string.Equals(p.StockCode, stock, StringComparison.OrdinalIgnoreCase));

    // ─── Strings / URLs ───────────────────────────────────────────────────

    public static string FitmentString(int? year, int? model, int? driveline, int? trim)
    {
        var parts = new List<string>();
        if (year is > 0) parts.Add(year.Value.ToString());
        parts.Add(MakeName);
        if (ModelByUkey(model) is { } m) parts.Add(m.Name);
        if (DrivelineByUkey(driveline) is { } d) parts.Add(d.Name);
        if (TrimByUkey(trim) is { } t) parts.Add(t.Name);
        return string.Join(' ', parts);
    }

    /// <summary>"{year}-{model}[-{driveline}][-{trim}]", the vehicle segment Home/Accessories decompose.</summary>
    public static string VehicleSegment(int year, int model, int? driveline, int? trim)
    {
        var sb = new StringBuilder($"{year}-{ModelByUkey(model)?.Slug}");
        if (DrivelineByUkey(driveline) is { } d) sb.Append('-').Append(d.Slug);
        if (TrimByUkey(trim) is { } t) sb.Append('-').Append(t.Slug);
        return sb.ToString();
    }

    /// <summary>Same make-segmented URL shape pr_refineSearch / fn_returnStorefrontURL emit.</summary>
    public static string BuildUrl(bool accessory, int? year, int? model, int? driveline, int? trim, int? category)
    {
        var url = new StringBuilder(accessory ? "/accessories/" : "/").Append(MakeSlug);
        if (year is > 0 && model is > 0)
            url.Append('/').Append(VehicleSegment(year.Value, model.Value, driveline, trim));
        if (CategoryByUkey(category) is { } c)
            url.Append('/').Append(c.Slug);
        return url.ToString();
    }

    public static string Slugify(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    /// <summary>Lightens/darkens a hex colour a little per variant so image sets aren't identical.</summary>
    public static string Shade(string hex, int variant)
    {
        if (variant == 0) return hex;
        var rgb = Convert.ToInt32(hex, 16);
        int Adjust(int c) => Math.Clamp(c + variant * 18, 0, 255);
        var r = Adjust((rgb >> 16) & 0xff);
        var g = Adjust((rgb >> 8) & 0xff);
        var b = Adjust(rgb & 0xff);
        return $"{r:x2}{g:x2}{b:x2}";
    }

    public static string CarImage(int? model, int w = 320, int h = 180) =>
        ModelByUkey(model) is { } m
            ? DemoImages.Url("car", $"{MakeName} {m.Name}", m.Color, w, h)
            : DemoImages.Url("car", MakeName, "113458", w, h);

    // ─── Garage ───────────────────────────────────────────────────────────

    public static CustomerVehicle CreateGarageVehicle(long ukey, int year, int model, int driveline, int trim, string? nickname = null) =>
        new()
        {
            Ukey = ukey,
            UkeyMake = MakeUkey,
            UkeyModel = model,
            ModelYear = year,
            UkeyDriveline = driveline,
            UkeyTrimLevel = trim,
            ModelString = FitmentString(year, model, driveline, trim),
            VehicleDescription = nickname,
            ImageUrl = CarImage(model, 160, 90),
            LinkURL = BuildUrl(false, year, model, driveline, trim, null)
        };

    public static List<CustomerVehicle> SeedGarage() =>
    [
        CreateGarageVehicle(8001, 2024, 101, 201, 302, "Family XC40"),
        CreateGarageVehicle(8002, 2022, 103, 201, 303),
        CreateGarageVehicle(8003, 2021, 104, 202, 301),
    ];
}
