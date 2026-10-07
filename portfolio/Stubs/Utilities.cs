using System.Net;
using System.Text.RegularExpressions;

namespace Storefront.Client.Utilities;

public interface IClientAssetResolver
{
    /// <summary>Maps a site-relative brand/UI asset path onto the asset host.</summary>
    string Resolve(string path);
}

public static partial class HtmlHelper
{
    [GeneratedRegex("<[^>]*>")]
    private static partial Regex TagPattern();

    /// <summary>Removes markup and decodes entities, e.g. "&lt;b&gt;A &amp;amp; B&lt;/b&gt;" → "A &amp; B".</summary>
    public static string StripHtml(string? html) =>
        string.IsNullOrEmpty(html)
            ? string.Empty
            : WebUtility.HtmlDecode(TagPattern().Replace(html, string.Empty)).Trim();
}
