using System.Collections.ObjectModel;

namespace RegexCraft.Core.Analysis;

/// <summary>
/// Hierarchical explanation of a regex pattern (analysis tree).
/// </summary>
public sealed class AnalysisNode
{
    public required string Title { get; init; }
    public string? Detail { get; set; }
    public string? PatternFragment { get; set; }
    public AnalysisNodeKind Kind { get; init; }
    public ObservableCollection<AnalysisNode> Children { get; init; } = new();
    public bool IsError { get; init; }

    /// <summary>Start offset of this node in the original pattern (inclusive).</summary>
    public int StartIndex { get; set; } = -1;

    /// <summary>Length of this node in the original pattern.</summary>
    public int Length { get; set; }

    /// <summary>End offset (exclusive) when <see cref="StartIndex"/> is known.</summary>
    public int EndIndex => StartIndex >= 0 ? StartIndex + Length : -1;

    /// <summary>Display line for tree UI: title + optional detail.</summary>
    public string DisplayTitle =>
        string.IsNullOrEmpty(Detail) ? Title : $"{Title} — {Detail}";

    /// <summary>True when the node maps to a selectable range in the pattern editor.</summary>
    public bool HasRange => StartIndex >= 0 && Length > 0;
}

public enum AnalysisNodeKind
{
    Root,
    Sequence,
    Alternation,
    Group,
    NamedGroup,
    NonCapturingGroup,
    Lookaround,
    Quantifier,
    CharacterClass,
    Escape,
    Anchor,
    Literal,
    Wildcard,
    Modifier,
    Error,
    Incomplete,
    Comment,
    Backreference,
}
