using RegexCraft.Core.Models;

namespace RegexCraft.Core.Highlighting;

/// <summary>
/// Builds ordered highlight spans from engine match results for the Test panel.
/// Full-match highlights are painted first; group captures (1+) sit on top by kind rotation.
/// </summary>
public static class MatchHighlightBuilder
{
    /// <summary>
    /// Creates highlight spans. Group 0 (full match) uses <see cref="HighlightKind.Match"/>.
    /// Capturing groups 1+ cycle through Group0–Group3 kinds for distinct colors.
    /// </summary>
    public static IReadOnlyList<HighlightSpan> Build(MatchCollectionResult? result, bool includeGroups = true)
    {
        if (result is null || !result.Success || result.Matches.Count == 0)
            return Array.Empty<HighlightSpan>();

        var spans = new List<HighlightSpan>();

        for (var mi = 0; mi < result.Matches.Count; mi++)
        {
            var match = result.Matches[mi];
            if (match.Length < 0 || match.Index < 0)
                continue;

            spans.Add(new HighlightSpan
            {
                Range = new TextRange(match.Index, match.Length),
                Kind = HighlightKind.Match,
                MatchIndex = mi,
                GroupNumber = 0,
                Label = $"Match {mi}",
            });

            if (!includeGroups)
                continue;

            foreach (var group in match.Groups)
            {
                if (group.Number == 0 || !group.Success || group.Length <= 0 || group.Index < 0)
                    continue;

                spans.Add(new HighlightSpan
                {
                    Range = new TextRange(group.Index, group.Length),
                    Kind = KindForGroup(group.Number),
                    MatchIndex = mi,
                    GroupNumber = group.Number,
                    Label = group.Name != group.Number.ToString()
                        ? $"M{mi} G{group.Number}/{group.Name}"
                        : $"M{mi} G{group.Number}",
                });
            }
        }

        // Stable paint order: matches first, then groups by number (so groups layer on top when drawing reverse).
        return spans
            .OrderBy(s => s.MatchIndex)
            .ThenBy(s => s.GroupNumber)
            .ToList();
    }

    public static HighlightKind KindForGroup(int groupNumber)
    {
        if (groupNumber <= 0)
            return HighlightKind.Match;

        return ((groupNumber - 1) % 4) switch
        {
            0 => HighlightKind.Group0,
            1 => HighlightKind.Group1,
            2 => HighlightKind.Group2,
            _ => HighlightKind.Group3,
        };
    }

    /// <summary>
    /// Returns spans that overlap a document line [lineStart, lineEnd).
    /// </summary>
    public static IEnumerable<HighlightSpan> ForLine(IReadOnlyList<HighlightSpan> spans, int lineStart, int lineEnd) =>
        spans.Where(s => s.Range.Overlaps(lineStart, lineEnd));
}
