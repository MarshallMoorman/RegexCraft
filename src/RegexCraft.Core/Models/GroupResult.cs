namespace RegexCraft.Core.Models;

/// <summary>
/// A single capture group within a match, including named groups.
/// Index/Length ranges are ready for future UI highlighting.
/// </summary>
public sealed class GroupResult
{
    /// <summary>Group number (0 = entire match).</summary>
    public int Number { get; init; }

    /// <summary>Group name, or the number as string when unnamed.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Start index in the subject string, or -1 if unsuccessful.</summary>
    public int Index { get; init; }

    /// <summary>Length of the captured text.</summary>
    public int Length { get; init; }

    /// <summary>Captured value (empty when unsuccessful).</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Whether this group participated in the match.</summary>
    public bool Success { get; init; }
}
