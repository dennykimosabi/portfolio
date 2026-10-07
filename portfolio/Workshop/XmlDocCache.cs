using System.Reflection;
using System.Xml.Linq;

namespace Storefront.Client.Workshop;

/// <summary>
/// Loads the assembly's XML documentation file (generated via
/// GenerateDocumentationFile) and serves &lt;summary&gt; text for reflected members.
/// </summary>
public static class XmlDocCache
{
    private static readonly Dictionary<string, string> _summaries = new(StringComparer.Ordinal);
    private static bool _loaded;

    public static string? GetSummary(MemberInfo member)
    {
        EnsureLoaded();
        var key = member is PropertyInfo p
            ? $"P:{p.DeclaringType!.FullName}.{p.Name}"
            : $"T:{member.DeclaringType?.FullName ?? member.Name}";
        return _summaries.TryGetValue(key, out var s) ? s : null;
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
            if (!File.Exists(xmlPath)) return;
            var doc = XDocument.Load(xmlPath);
            foreach (var m in doc.Descendants("member"))
            {
                var name = m.Attribute("name")?.Value;
                var summary = m.Element("summary")?.Value.Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(summary))
                    _summaries[name] = string.Join(" ", summary.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries));
            }
        }
        catch { /* docs are optional */ }
    }
}
