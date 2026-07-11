namespace RegexCraft.Core.Models;

/// <summary>
/// Result of a Match operation across any engine.
/// On failure, <see cref="Success"/> is false and <see cref="ErrorMessage"/> is set.
/// </summary>
public sealed class MatchCollectionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<MatchResult> Matches { get; init; } = Array.Empty<MatchResult>();
    public TimeSpan Duration { get; init; }
    public string EngineId { get; init; } = string.Empty;

    public static MatchCollectionResult Failed(string engineId, string errorMessage, TimeSpan duration = default) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            Matches = Array.Empty<MatchResult>(),
            Duration = duration,
            EngineId = engineId,
        };

    public static MatchCollectionResult FromMatches(
        string engineId,
        IReadOnlyList<MatchResult> matches,
        TimeSpan duration) =>
        new()
        {
            Success = true,
            Matches = matches,
            Duration = duration,
            EngineId = engineId,
        };
}
