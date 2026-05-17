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

    private static string RenderTag(string tag, string args)
    {
        // The default behavior for unknown tags is to emit the first pipe segment
        // (the display text) so we never silently drop content.
        return tag.ToLowerInvariant() switch
        {
            "b" or "bold"      => $"<strong>{WebUtility.HtmlEncode(args)}</strong>",
            "i" or "italic"    => $"<em>{WebUtility.HtmlEncode(args)}</em>",
            "u" or "underline" => $"<u>{WebUtility.HtmlEncode(args)}</u>",
            _ => WebUtility.HtmlEncode(FirstSegment(args)),
        };
    }

    private static string FirstSegment(string args)
    {
        var pipe = args.IndexOf('|');
        return pipe < 0 ? args : args[..pipe];
    }
}
