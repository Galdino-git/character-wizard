using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CharacterWizard.Data.EntryRendering;

/// <summary>
/// Renders 5etools "entries" (strings with inline {@tag …} markers and recursive
/// structures) into safe HTML for consumption by Blazor &lt;MarkupString&gt;.
///
/// All raw text is HTML-encoded before composition; tag handlers are responsible
/// for emitting their own HTML structure (already-safe). This keeps callers from
/// having to think about XSS even though 5etools data is trusted — the same
/// renderer is used for user-supplied notes in the future.
/// </summary>
public static partial class EntryRenderer
{
    [GeneratedRegex(@"\{@(\w+)\s*([^}]*)\}", RegexOptions.Compiled)]
    private static partial Regex TagRegex();

    public static string RenderString(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var sb = new StringBuilder(text.Length + 16);
        var pos = 0;

        foreach (Match m in TagRegex().Matches(text))
        {
            if (m.Index > pos)
                sb.Append(WebUtility.HtmlEncode(text.Substring(pos, m.Index - pos)));

            var tag = m.Groups[1].Value;
            var args = m.Groups[2].Value;
            sb.Append(RenderTag(tag, args));

            pos = m.Index + m.Length;
        }

        if (pos < text.Length)
            sb.Append(WebUtility.HtmlEncode(text[pos..]));

        return sb.ToString();
    }

    /// <summary>
    /// Tags that resolve to an entity in the catalog. The string value is the
    /// "category" used by EntityResolver/data-cw-cat to route the click.
    /// Source defaults to "PHB" (5etools default for player content) when the
    /// data omits an explicit source.
    /// </summary>
    private static readonly Dictionary<string, string> EntityTagCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["spell"]            = "spell",
        ["item"]             = "item",
        ["condition"]        = "condition",
        ["feat"]             = "feat",
        ["skill"]            = "skill",
        ["action"]           = "action",
        ["sense"]            = "sense",
        ["creature"]         = "creature",
        ["feature"]          = "feature",
        ["classfeature"]     = "classfeature",
        ["subclassfeature"]  = "subclassfeature",
        ["optfeature"]       = "optfeature",
    };

    private const string DefaultEntitySource = "PHB";

    private static string RenderTag(string tag, string args)
    {
        var lower = tag.ToLowerInvariant();

        if (EntityTagCategories.TryGetValue(lower, out var category))
            return RenderEntityRef(category, args);

        return lower switch
        {
            "b" or "bold"      => $"<strong>{WebUtility.HtmlEncode(args)}</strong>",
            "i" or "italic"    => $"<em>{WebUtility.HtmlEncode(args)}</em>",
            "u" or "underline" => $"<u>{WebUtility.HtmlEncode(args)}</u>",
            _ => WebUtility.HtmlEncode(FirstSegment(args)),
        };
    }

    private static string RenderEntityRef(string category, string args)
    {
        var parts = args.Split('|');
        var name    = parts.Length > 0 ? parts[0] : "";
        var source  = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : DefaultEntitySource;
        var display = parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) ? parts[2] : name;

        return $"<span class=\"cw-ref\" data-cw-cat=\"{WebUtility.HtmlEncode(category)}\" " +
               $"data-cw-name=\"{WebUtility.HtmlEncode(name)}\" " +
               $"data-cw-source=\"{WebUtility.HtmlEncode(source)}\">" +
               $"{WebUtility.HtmlEncode(display)}</span>";
    }

    private static string FirstSegment(string args)
    {
        var pipe = args.IndexOf('|');
        return pipe < 0 ? args : args[..pipe];
    }
}
