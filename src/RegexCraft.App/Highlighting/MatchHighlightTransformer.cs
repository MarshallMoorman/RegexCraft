using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using RegexCraft.Core.Highlighting;

namespace RegexCraft.App.Highlighting;

/// <summary>
/// Colors subject text ranges from <see cref="HighlightSpan"/> lists (match + group layers).
/// </summary>
public sealed class MatchHighlightTransformer : DocumentColorizingTransformer
{
    private IReadOnlyList<HighlightSpan> _spans = Array.Empty<HighlightSpan>();
    private IBrush _match = Brushes.Yellow;
    private IBrush _g0 = Brushes.LightBlue;
    private IBrush _g1 = Brushes.LightGreen;
    private IBrush _g2 = Brushes.LightSalmon;
    private IBrush _g3 = Brushes.Plum;

    public void SetBrushes(IBrush match, IBrush g0, IBrush g1, IBrush g2, IBrush g3)
    {
        _match = match;
        _g0 = g0;
        _g1 = g1;
        _g2 = g2;
        _g3 = g3;
    }

    public void SetSpans(IReadOnlyList<HighlightSpan>? spans)
    {
        _spans = spans ?? Array.Empty<HighlightSpan>();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (_spans.Count == 0 || line.Length == 0)
            return;

        var lineStart = line.Offset;
        var lineEnd = line.EndOffset;

        // Paint full matches first, then groups so group colors sit on top where they overlap.
        foreach (var span in _spans.OrderBy(s => s.GroupNumber == 0 ? 0 : 1).ThenBy(s => s.GroupNumber))
        {
            if (!span.Range.Overlaps(lineStart, lineEnd))
                continue;

            var start = Math.Max(span.Range.Start, lineStart);
            var end = Math.Min(span.Range.End, lineEnd);
            if (end <= start)
                continue;

            var brush = BrushFor(span.Kind);
            ChangeLinePart(start, end, element =>
            {
                element.BackgroundBrush = brush;
            });
        }
    }

    private IBrush BrushFor(HighlightKind kind) => kind switch
    {
        HighlightKind.Match => _match,
        HighlightKind.Group0 => _g0,
        HighlightKind.Group1 => _g1,
        HighlightKind.Group2 => _g2,
        HighlightKind.Group3 => _g3,
        _ => _match,
    };
}
