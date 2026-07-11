namespace RegexCraft.Core.Models;

/// <summary>
/// A single match with its groups (including named groups).
/// Index/Length are highlighting-friendly ranges.
/// </summary>
public sealed class MatchResult
{
    public int Index { get; init; }
    public int Length { get; init; }
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// All groups for this match. Group 0 is the full match.
    /// Named groups appear with their names in <see cref="GroupResult.Name"/>.
    /// </summary>
    public IReadOnlyList<GroupResult> Groups { get; init; } = Array.Empty<GroupResult>();
}
