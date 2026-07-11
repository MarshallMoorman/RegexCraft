using RegexCraft.Core.Models;

namespace RegexCraft.Core.Highlighting;

/// <summary>
/// Builds highlight spans for the Replace preview (substituted regions).
/// </summary>
public static class ReplaceHighlightBuilder
{
    public static IReadOnlyList<HighlightSpan> Build(ReplaceResult? result)
    {
        if (result is null || !result.Success || result.ReplacementSpans.Count == 0)
            return Array.Empty<HighlightSpan>();

        var spans = new List<HighlightSpan>(result.ReplacementSpans.Count);
        foreach (var rs in result.ReplacementSpans)
        {
            if (rs.Index < 0 || rs.Length < 0)
                continue;

            spans.Add(new HighlightSpan
            {
                Range = new TextRange(rs.Index, rs.Length),
                Kind = HighlightKind.Match,
                MatchIndex = rs.MatchIndex,
                GroupNumber = 0,
                Label = $"Replacement {rs.MatchIndex}",
            });
        }

        return spans;
    }

    /// <summary>
    /// Builds highlight spans for split delimiters in the original subject.
    /// </summary>
    public static IReadOnlyList<HighlightSpan> BuildSplitDelimiters(SplitResult? result)
    {
        if (result is null || !result.Success || result.Delimiters.Count == 0)
            return Array.Empty<HighlightSpan>();

        var spans = new List<HighlightSpan>(result.Delimiters.Count);
        for (var i = 0; i < result.Delimiters.Count; i++)
        {
            var d = result.Delimiters[i];
            if (d.Index < 0 || d.Length < 0)
                continue;

            spans.Add(new HighlightSpan
            {
                Range = new TextRange(d.Index, d.Length),
                Kind = HighlightKind.Match,
                MatchIndex = i,
                GroupNumber = 0,
                Label = $"Delimiter {i}",
            });
        }

        return spans;
    }
}
