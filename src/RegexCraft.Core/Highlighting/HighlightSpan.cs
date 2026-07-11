namespace RegexCraft.Core.Highlighting;

/// <summary>
/// A highlight range ready for UI coloring. Kind selects theme brush.
/// </summary>
public sealed class HighlightSpan
{
    public required TextRange Range { get; init; }
    public required HighlightKind Kind { get; init; }
    public int MatchIndex { get; init; }
    public int GroupNumber { get; init; }
    public string? Label { get; init; }
}

public enum HighlightKind
{
    Match,
    Group0,
    Group1,
    Group2,
    Group3,
}
