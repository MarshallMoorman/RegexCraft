using RegexCraft.Core.Models;

namespace RegexCraft.Core.Debug;

/// <summary>
/// Builds educational step-through sessions for regex matching.
/// Primary support: .NET engine. Architecture allows more engines later.
/// </summary>
public interface IRegexDebugService
{
    /// <summary>Engine ids that currently support step-through Debug.</summary>
    bool SupportsEngine(string engineId);

    /// <summary>
    /// Build a debug session from pattern/subject and a real Match result.
    /// Pass the engine's MatchCollectionResult so explanations align with live Test.
    /// </summary>
    DebugSession BuildSession(
        string pattern,
        string subject,
        RegexOptionsEx options,
        string engineId,
        string engineDisplayName,
        MatchCollectionResult matchResult);
}
