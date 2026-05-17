using System.Net;
using System.Text;
using System.Text.Json;
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
            "h"                => $"<mark>{WebUtility.HtmlEncode(args)}</mark>",
            "note"             => $"<em class=\"cw-note\">{WebUtility.HtmlEncode(args)}</em>",
            "damage" or "dice" => RenderRoll(args),
            "scaledamage" or "scaledice" => RenderRoll(FirstSegment(args)),
            "hit"              => RenderHit(args),
            "dc"               => $"DC {WebUtility.HtmlEncode(args)}",
            "atk"              => RenderAtk(args),
            _ => WebUtility.HtmlEncode(FirstSegment(args)),
        };
    }

    /// <summary>"+5" / "−2" — uses U+2212 (minus sign) for negatives to match the d&d style guide.</summary>
    private static string RenderHit(string args)
    {
        var trimmed = args.Trim();
        if (string.IsNullOrEmpty(trimmed)) return "";
        if (trimmed.StartsWith('-')) return "&minus;" + WebUtility.HtmlEncode(trimmed[1..]);
        return "+" + WebUtility.HtmlEncode(trimmed);
    }

    private static string RenderRoll(string args) =>
        $"<span class=\"cw-roll\">{WebUtility.HtmlEncode(args)}</span>";

    private static readonly Dictionary<string, string> AtkCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mw"]  = "Melee Weapon Attack",
        ["rw"]  = "Ranged Weapon Attack",
        ["mw,rw"] = "Melee or Ranged Weapon Attack",
        ["ms"]  = "Melee Spell Attack",
        ["rs"]  = "Ranged Spell Attack",
        ["ms,rs"] = "Melee or Ranged Spell Attack",
    };

    private static string RenderAtk(string args) =>
        AtkCodes.TryGetValue(args.Trim(), out var translated)
            ? WebUtility.HtmlEncode(translated)
            : WebUtility.HtmlEncode(args);

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

    // ───── structured entries ─────────────────────────────────────────────────

    /// <summary>
    /// Renders a 5etools "entries" node — which can be a string, an array, or a
    /// typed object (entries/list/inset/table/quote) — into HTML. Unknown types
    /// are dropped silently rather than raising, so a single malformed entry in
    /// a class file does not break the whole renderer.
    /// </summary>
    public static string Render(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Undefined => "",
            JsonValueKind.Null      => "",
            JsonValueKind.String    => RenderString(element.GetString()),
            JsonValueKind.Number    => WebUtility.HtmlEncode(element.GetRawText()),
            JsonValueKind.Array     => RenderArray(element),
            JsonValueKind.Object    => RenderObject(element),
            _                       => "",
        };
    }

    private static string RenderArray(JsonElement arr)
    {
        var sb = new StringBuilder();
        foreach (var item in arr.EnumerateArray())
        {
            var part = Render(item);
            if (string.IsNullOrEmpty(part)) continue;
            // Strings wrapped in <p> so consecutive entries don't run together;
            // structured items return their own block-level wrapper already.
            sb.Append(item.ValueKind == JsonValueKind.String
                ? $"<p>{part}</p>"
                : part);
        }
        return sb.ToString();
    }

    private static string RenderObject(JsonElement obj)
    {
        if (!obj.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.String)
            return ""; // not an entries-typed object, ignore

        var type = typeEl.GetString()!.ToLowerInvariant();
        return type switch
        {
            "entries" or "section" => RenderEntriesBlock(obj),
            "list"                  => RenderList(obj),
            "inset" or "insetReadaloud" => RenderInset(obj),
            "table"                 => RenderTable(obj),
            "quote"                 => RenderQuote(obj),
            _                       => "",
        };
    }

    private static string RenderEntriesBlock(JsonElement obj)
    {
        var sb = new StringBuilder("<div class=\"cw-entries\">");
        if (obj.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
            sb.Append($"<span class=\"cw-entry-name\">{RenderString(nameEl.GetString())}</span> ");
        if (obj.TryGetProperty("entries", out var entriesEl))
            sb.Append(Render(entriesEl));
        sb.Append("</div>");
        return sb.ToString();
    }

    private static string RenderList(JsonElement obj)
    {
        var sb = new StringBuilder("<ul class=\"cw-list\">");
        if (obj.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
            {
                var inner = item.ValueKind == JsonValueKind.String
                    ? RenderString(item.GetString())
                    : Render(item);
                sb.Append($"<li>{inner}</li>");
            }
        }
        sb.Append("</ul>");
        return sb.ToString();
    }

    private static string RenderInset(JsonElement obj)
    {
        var sb = new StringBuilder("<aside class=\"cw-inset\">");
        if (obj.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
            sb.Append($"<header class=\"cw-inset-name\">{RenderString(nameEl.GetString())}</header>");
        if (obj.TryGetProperty("entries", out var entriesEl))
            sb.Append(Render(entriesEl));
        sb.Append("</aside>");
        return sb.ToString();
    }

    private static string RenderQuote(JsonElement obj)
    {
        var sb = new StringBuilder("<blockquote class=\"cw-quote\">");
        if (obj.TryGetProperty("entries", out var entriesEl))
            sb.Append(Render(entriesEl));
        if (obj.TryGetProperty("by", out var byEl) && byEl.ValueKind == JsonValueKind.String)
            sb.Append($"<footer>— {RenderString(byEl.GetString())}</footer>");
        sb.Append("</blockquote>");
        return sb.ToString();
    }

    private static string RenderTable(JsonElement obj)
    {
        var sb = new StringBuilder("<table class=\"cw-table\">");

        if (obj.TryGetProperty("caption", out var capEl) && capEl.ValueKind == JsonValueKind.String)
            sb.Append($"<caption>{RenderString(capEl.GetString())}</caption>");

        if (obj.TryGetProperty("colLabels", out var cols) && cols.ValueKind == JsonValueKind.Array)
        {
            sb.Append("<thead><tr>");
            foreach (var c in cols.EnumerateArray())
                sb.Append($"<th>{(c.ValueKind == JsonValueKind.String ? RenderString(c.GetString()) : Render(c))}</th>");
            sb.Append("</tr></thead>");
        }

        if (obj.TryGetProperty("rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
        {
            sb.Append("<tbody>");
            foreach (var row in rows.EnumerateArray())
            {
                sb.Append("<tr>");
                if (row.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cell in row.EnumerateArray())
                        sb.Append($"<td>{(cell.ValueKind == JsonValueKind.String ? RenderString(cell.GetString()) : Render(cell))}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }
}
