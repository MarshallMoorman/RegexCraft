namespace RegexCraft.Core.Models;

/// <summary>
/// Result of a Split operation across any engine.
/// </summary>
public sealed class SplitResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> Parts { get; init; } = Array.Empty<string>();
    public TimeSpan Duration { get; init; }
    public string EngineId { get; init; } = string.Empty;

    /// <summary>
    /// Ranges in the original subject that matched the delimiter (split points).
    /// </summary>
    public IReadOnlyList<SplitDelimiterRange> Delimiters { get; init; } = Array.Empty<SplitDelimiterRange>();

    public static SplitResult Failed(string engineId, string errorMessage, TimeSpan duration = default) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            Parts = Array.Empty<string>(),
            Duration = duration,
            EngineId = engineId,
        };

    public static SplitResult FromParts(
        string engineId,
        IReadOnlyList<string> parts,
        IReadOnlyList<SplitDelimiterRange> delimiters,
        TimeSpan duration) =>
        new()
        {
            Success = true,
            Parts = parts,
            Delimiters = delimiters,
            Duration = duration,
            EngineId = engineId,
        };
}

/// <summary>A delimiter match that caused a split in the subject.</summary>
public sealed class SplitDelimiterRange
{
    public int Index { get; init; }
    public int Length { get; init; }
    public string Value { get; init; } = string.Empty;
}
