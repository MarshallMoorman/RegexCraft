namespace RegexCraft.Core.Debug;

/// <summary>
/// Kind of educational debug step (not a 1:1 mapping of engine opcodes).
/// </summary>
public enum DebugStepKind
{
    /// <summary>Session start / intro.</summary>
    Start,
    /// <summary>Pattern structure or overview.</summary>
    Overview,
    /// <summary>Considering a pattern construct (node).</summary>
    PatternNode,
    /// <summary>Search / match attempt at a subject position.</summary>
    Attempt,
    /// <summary>Capture group recorded.</summary>
    Capture,
    /// <summary>A full match succeeded.</summary>
    MatchSuccess,
    /// <summary>No match / step failure.</summary>
    Failure,
    /// <summary>Session complete.</summary>
    Complete,
    /// <summary>Pattern invalid or debug unavailable.</summary>
    Error,
}

/// <summary>
/// One human-readable step in a debug walk-through.
/// </summary>
public sealed class DebugStep
{
    public required int Index { get; init; }
    public required DebugStepKind Kind { get; init; }
    public required string Explanation { get; init; }

    /// <summary>Pattern range currently under consideration (inclusive start, length).</summary>
    public int PatternStart { get; init; } = -1;
    public int PatternLength { get; init; }

    /// <summary>Subject range currently under consideration.</summary>
    public int SubjectStart { get; init; } = -1;
    public int SubjectLength { get; init; }

    /// <summary>True = success, False = failure, null = informational.</summary>
    public bool? Success { get; init; }

    public int? MatchIndex { get; init; }
    public int? GroupNumber { get; init; }
    public string? GroupName { get; init; }

    public bool HasPatternRange => PatternStart >= 0 && PatternLength >= 0;
    public bool HasSubjectRange => SubjectStart >= 0 && SubjectLength >= 0;

    public string StatusLabel => Success switch
    {
        true => "OK",
        false => "Fail",
        null => "Info",
    };

    public string KindLabel => Kind switch
    {
        DebugStepKind.Start => "Start",
        DebugStepKind.Overview => "Overview",
        DebugStepKind.PatternNode => "Pattern",
        DebugStepKind.Attempt => "Attempt",
        DebugStepKind.Capture => "Capture",
        DebugStepKind.MatchSuccess => "Match",
        DebugStepKind.Failure => "No match",
        DebugStepKind.Complete => "Done",
        DebugStepKind.Error => "Error",
        _ => Kind.ToString(),
    };
}

/// <summary>
/// A full debug session: ordered steps plus availability metadata.
/// </summary>
public sealed class DebugSession
{
    public bool IsAvailable { get; init; }
    public string? UnavailableReason { get; init; }
    public string EngineId { get; init; } = string.Empty;
    public string EngineDisplayName { get; init; } = string.Empty;
    public IReadOnlyList<DebugStep> Steps { get; init; } = Array.Empty<DebugStep>();
    public int MatchCount { get; init; }
    public bool PatternValid { get; init; } = true;
    public string? PatternError { get; init; }

    /// <summary>Short approach note shown in the UI.</summary>
    public string ApproachNote { get; init; } = string.Empty;

    public bool HasSteps => Steps.Count > 0;

    public static DebugSession Unavailable(string engineId, string engineDisplayName, string reason) =>
        new()
        {
            IsAvailable = false,
            UnavailableReason = reason,
            EngineId = engineId,
            EngineDisplayName = engineDisplayName,
            ApproachNote = string.Empty,
            Steps = Array.Empty<DebugStep>(),
        };
}
