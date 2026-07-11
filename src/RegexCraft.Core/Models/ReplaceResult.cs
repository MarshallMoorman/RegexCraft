namespace RegexCraft.Core.Models;

/// <summary>
/// Result of a Replace operation across any engine.
/// </summary>
public sealed class ReplaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string Result { get; init; } = string.Empty;
    public int ReplacementCount { get; init; }
    public TimeSpan Duration { get; init; }
    public string EngineId { get; init; } = string.Empty;

    /// <summary>
    /// Ranges in the replacement result string that were produced by substitutions.
    /// Used for highlighting changed parts in the Replace preview.
    /// </summary>
    public IReadOnlyList<ReplacementSpan> ReplacementSpans { get; init; } = Array.Empty<ReplacementSpan>();

    public static ReplaceResult Failed(string engineId, string errorMessage, TimeSpan duration = default) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            Result = string.Empty,
            ReplacementCount = 0,
            Duration = duration,
            EngineId = engineId,
            ReplacementSpans = Array.Empty<ReplacementSpan>(),
        };

    public static ReplaceResult FromResult(
        string engineId,
        string result,
        int replacementCount,
        TimeSpan duration,
        IReadOnlyList<ReplacementSpan>? replacementSpans = null) =>
        new()
        {
            Success = true,
            Result = result,
            ReplacementCount = replacementCount,
            Duration = duration,
            EngineId = engineId,
            ReplacementSpans = replacementSpans ?? Array.Empty<ReplacementSpan>(),
        };
}

/// <summary>A span in the replace result that came from a substitution.</summary>
public sealed class ReplacementSpan
{
    public int Index { get; init; }
    public int Length { get; init; }
    public int MatchIndex { get; init; }
}
