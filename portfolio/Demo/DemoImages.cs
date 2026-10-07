using System.Globalization;
using System.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Portfolio.Demo;

/// <summary>
/// Generates placeholder SVG artwork locally so the preview needs no external image hosts.
///   /demo-img/{kind}.svg?t=Label&amp;c=hex&amp;w=400&amp;h=300   kinds: part, tile, car, hero, promo, diagram, logo
///   /demo-assets/**  and  /imgs/**                          small UI icons (edit, delete, delivery, pickup, cart)
/// </summary>
public static class DemoImages
{
    public static string Url(string kind, string text, string color, int w, int h) =>
        $"/demo-img/{kind}.svg?t={Uri.EscapeDataString(text)}&c={color}&w={w}&h={h}";

    public static void Map(WebApplication app)
    {
        app.MapGet("/demo-img/{kind}.svg", (string kind, string? t, string? c, int? w, int? h) =>
            Results.Text(Render(kind, t ?? string.Empty, c, w ?? 400, h ?? 300), "image/svg+xml"));

        // GarageVehicleCard / ProductPriceBox resolve their icons through IClientAssetResolver → here.
        app.MapGet("/demo-assets/{**path}", (string path) => Results.Text(Icon(path), "image/svg+xml"));

        // Search/Landing list view reference /imgs/cart-add-primary.png directly.
        app.MapGet("/imgs/{**path}", (string path) => Results.Text(Icon(path), "image/svg+xml"));
    }

    private static string Render(string kind, string text, string? color, int w, int h)
    {
        w = Math.Clamp(w, 16, 2400);
        h = Math.Clamp(h, 16, 1600);
        var c = color is { Length: 6 } && color.All(Uri.IsHexDigit) ? "#" + color : "#113458";
        var label = SecurityElement.Escape(text) ?? string.Empty;
        var fs = F(Math.Max(10, Math.Min(w * 0.85 / Math.Max(1, text.Length * 0.56), h * 0.08)));

        var body = kind switch
        {
            "hero" => $"""
                <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="{c}"/><stop offset="1" stop-color="#0b1a2b"/></linearGradient></defs>
                <rect width="{w}" height="{h}" fill="url(#g)"/>
                <circle cx="{F(w * .82)}" cy="{F(h * .3)}" r="{F(h * .55)}" fill="#fff" opacity=".06"/>
                <circle cx="{F(w * .9)}" cy="{F(h * .85)}" r="{F(h * .35)}" fill="#fff" opacity=".05"/>
                <path d="M0 {F(h * .86)} L{w} {F(h * .7)} L{w} {h} L0 {h} Z" fill="#000" opacity=".18"/>
                <g transform="translate({F(w * .58)} {F(h * .38)}) scale({F(h / 260.0)})" fill="#fff" opacity=".22">{CarPath}</g>
                """,
            "promo" => $"""
                <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="0">
                  <stop offset="0" stop-color="{c}"/><stop offset="1" stop-color="#1b1b1b"/></linearGradient></defs>
                <rect width="{w}" height="{h}" fill="url(#g)"/>
                <circle cx="{F(w * .3)}" cy="{F(h * .5)}" r="{F(h * .36)}" fill="#fff" opacity=".08"/>
                <text x="{F(w * .3)}" y="{F(h * .5)}" dy=".35em" text-anchor="middle" font-family="Helvetica, Arial, sans-serif"
                      font-size="{F(h * .1)}" font-weight="600" fill="#fff" opacity=".85">{label}</text>
                """,
            "tile" => $"""
                <rect width="{w}" height="{h}" fill="{c}"/>
                <circle cx="{F(w * .5)}" cy="{F(h * .45)}" r="{F(Math.Min(w, h) * .26)}" fill="#fff" opacity=".14"/>
                <circle cx="{F(w * .5)}" cy="{F(h * .45)}" r="{F(Math.Min(w, h) * .12)}" fill="none" stroke="#fff" stroke-width="{F(Math.Min(w, h) * .03)}" opacity=".5"/>
                """,
            "car" => $"""
                <rect width="{w}" height="{h}" fill="#eef1f5"/>
                <g transform="translate({F(w * .5 - h * .62)} {F(h * .14)}) scale({F(h / 150.0)})" fill="{c}">{CarPath}</g>
                <text x="{F(w / 2.0)}" y="{F(h * .92)}" text-anchor="middle" font-family="Helvetica, Arial, sans-serif"
                      font-size="{F(h * .1)}" fill="#445">{label}</text>
                """,
            "diagram" => $"""
                <rect width="{w}" height="{h}" fill="#fff"/>
                <g fill="none" stroke="{c}" stroke-width="2" opacity=".85">
                  <circle cx="{F(w * .3)}" cy="{F(h * .42)}" r="{F(h * .2)}"/>
                  <circle cx="{F(w * .3)}" cy="{F(h * .42)}" r="{F(h * .08)}"/>
                  <rect x="{F(w * .55)}" y="{F(h * .22)}" width="{F(w * .22)}" height="{F(h * .4)}" rx="6"/>
                  <line x1="{F(w * .5)}" y1="{F(h * .42)}" x2="{F(w * .55)}" y2="{F(h * .42)}" stroke-dasharray="4 4"/>
                  <line x1="{F(w * .66)}" y1="{F(h * .62)}" x2="{F(w * .66)}" y2="{F(h * .74)}" stroke-dasharray="4 4"/>
                  <circle cx="{F(w * .66)}" cy="{F(h * .78)}" r="{F(h * .04)}"/>
                </g>
                <g font-family="Helvetica, Arial, sans-serif" font-size="{F(h * .06)}" fill="{c}">
                  <text x="{F(w * .12)}" y="{F(h * .16)}">1</text><text x="{F(w * .8)}" y="{F(h * .2)}">2</text><text x="{F(w * .72)}" y="{F(h * .86)}">3</text>
                </g>
                <text x="{F(w / 2.0)}" y="{F(h * .95)}" text-anchor="middle" font-family="Helvetica, Arial, sans-serif"
                      font-size="{F(h * .07)}" fill="#556">{label}</text>
                """,
            "logo" => $"""
                <text x="0" y="{F(h * .62)}" font-family="Helvetica, Arial, sans-serif" font-size="{F(h * .5)}" font-weight="700"
                      letter-spacing="{F(h * .12)}" fill="{c}">{label}</text>
                """,
            _ => $"""
                <rect width="{w}" height="{h}" fill="#f4f6f9"/>
                <rect x="{F(w * .22)}" y="{F(h * .16)}" width="{F(w * .56)}" height="{F(h * .5)}" rx="{F(h * .06)}" fill="{c}" opacity=".9"/>
                <circle cx="{F(w * .5)}" cy="{F(h * .41)}" r="{F(h * .13)}" fill="#fff" opacity=".35"/>
                <circle cx="{F(w * .5)}" cy="{F(h * .41)}" r="{F(h * .05)}" fill="#fff" opacity=".8"/>
                <text x="{F(w / 2.0)}" y="{F(h * .84)}" text-anchor="middle" font-family="Helvetica, Arial, sans-serif"
                      font-size="{fs}" fill="#334">{label}</text>
                """
        };

        return $"""<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}" preserveAspectRatio="xMidYMid slice">{body}</svg>""";
    }

    // Side-profile hatchback, ~200×110 units.
    private const string CarPath =
        """<path d="M12 78 L24 56 Q34 40 58 36 L112 32 Q134 32 150 46 L176 52 Q194 56 196 70 L196 80 L12 80 Z"/>""" +
        """<path d="M62 42 L108 38 Q126 38 138 48 L66 50 Z" fill="#fff" opacity=".45"/>""" +
        """<circle cx="52" cy="82" r="15"/><circle cx="160" cy="82" r="15"/>""" +
        """<circle cx="52" cy="82" r="6" fill="#fff" opacity=".6"/><circle cx="160" cy="82" r="6" fill="#fff" opacity=".6"/>""";

    private static string Icon(string path)
    {
        var p = path.ToLowerInvariant();
        var shape =
            p.Contains("edit") ? """<path d="M4 20h4L19 9l-4-4L4 16v4Z"/><path d="M14 6l4 4"/>""" :
            p.Contains("delete") ? """<path d="M5 7h14"/><path d="M9 7V4h6v3"/><path d="M7 7l1 13h8l1-13"/>""" :
            p.Contains("delivery") ? """<path d="M2 6h11v10H2z"/><path d="M13 10h4l3 3v3h-7z"/><circle cx="6" cy="18" r="2"/><circle cx="17" cy="18" r="2"/>""" :
            p.Contains("pickup") ? """<path d="M3 10l2-6h14l2 6"/><path d="M4 10v10h16V10"/><path d="M9 20v-6h6v6"/>""" :
            p.Contains("cart") ? """<path d="M2 3h3l2 12h12l2-8H6"/><circle cx="9" cy="20" r="1.5"/><circle cx="18" cy="20" r="1.5"/><path d="M13 7v4M11 9h4"/>""" :
            """<circle cx="12" cy="12" r="6"/>""";
        // "…-primary" icons sit on the navy primary button, so draw them white.
        var stroke = p.Contains("primary") ? "#ffffff" : "#113458";
        return $"""<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="{stroke}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">{shape}</svg>""";
    }

    private static string F(double v) => Math.Round(v, 1).ToString(CultureInfo.InvariantCulture);
}
