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
}
