using RegexCraft.Core.Models;

namespace RegexCraft.Core.Engines;

/// <summary>
/// Abstraction over a concrete regular-expression engine (e.g. .NET, PCRE2).
/// All engines return consistent result models so the UI stays engine-agnostic.
/// </summary>
public interface IRegexEngine
{
    /// <summary>Stable engine id used in flavor mappings (e.g. "dotnet", "pcre2").</summary>
    string Id { get; }

    /// <summary>Human-readable display name.</summary>
    string DisplayName { get; }

    /// <summary>True for Tier 1 engines that support full testing workflows.</summary>
    bool SupportsFullTesting { get; }

    /// <summary>True when Replace is supported.</summary>
    bool SupportsReplace { get; }

    /// <summary>True when Split is supported.</summary>
    bool SupportsSplit { get; }

    /// <summary>
    /// Finds all matches of <paramref name="pattern"/> in <paramref name="subject"/>.
    /// Invalid patterns return a failed result rather than throwing when possible.
    /// </summary>
    MatchCollectionResult Match(string pattern, string subject, RegexOptionsEx options);

    /// <summary>
    /// Replaces all matches of <paramref name="pattern"/> in <paramref name="subject"/>
    /// with <paramref name="replacement"/>.
    /// </summary>
    ReplaceResult Replace(string pattern, string subject, string replacement, RegexOptionsEx options);

    /// <summary>
    /// Splits <paramref name="subject"/> on matches of <paramref name="pattern"/>.
    /// </summary>
    /// <param name="removeEmptyEntries">When true, empty parts are omitted from the result.</param>
    SplitResult Split(string pattern, string subject, RegexOptionsEx options, bool removeEmptyEntries = false);
}
