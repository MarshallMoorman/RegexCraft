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

    public static ReplaceResult Failed(string engineId, string errorMessage, TimeSpan duration = default) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            Result = string.Empty,
            ReplacementCount = 0,
            Duration = duration,
            EngineId = engineId,
        };

    public static ReplaceResult FromResult(
        string engineId,
        string result,
        int replacementCount,
        TimeSpan duration) =>
        new()
        {
            Success = true,
            Result = result,
            ReplacementCount = replacementCount,
            Duration = duration,
            EngineId = engineId,
        };
}
