using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;

namespace RegexCraft.Core.Compare;

/// <summary>
/// Request to compare a pattern/subject across multiple flavors.
/// </summary>
public sealed class CompareRequest
{
    public required string Pattern { get; init; }
    public required string Subject { get; init; }
    public RegexOptionsEx Options { get; init; } = RegexOptionsEx.None;

    /// <summary>Flavor ids to compare (2–4 recommended).</summary>
    public required IReadOnlyList<string> FlavorIds { get; init; }

    /// <summary>Max matches to include per flavor card (full count is still reported).</summary>
    public int MaxMatchesToShow { get; init; } = 5;
}

/// <summary>
/// A single match summary for display in a compare card.
/// </summary>
public sealed class CompareMatchSummary
{
    public int Index { get; init; }
    public int Start { get; init; }
    public int Length { get; init; }
    public string Value { get; init; } = string.Empty;
    public IReadOnlyList<string> GroupSummaries { get; init; } = Array.Empty<string>();

    public string SummaryLine =>
        $"[{Index}] {Start}+{Length}: {Truncate(Value, 60)}";

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

/// <summary>
/// Per-flavor comparison outcome.
/// </summary>
public sealed class FlavorCompareResult
{
    public required string FlavorId { get; init; }
    public required string FlavorDisplayName { get; init; }
    public required string EngineId { get; init; }
    public required string EngineDisplayName { get; init; }
    public TestingFidelity Fidelity { get; init; }
    public string? FidelityNote { get; init; }
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public int MatchCount { get; init; }
    public IReadOnlyList<CompareMatchSummary> Matches { get; init; } = Array.Empty<CompareMatchSummary>();
    public TimeSpan Duration { get; init; }
    public RegexOptionsEx AppliedOptions { get; init; }
    public RegexOptionsEx DroppedOptions { get; init; }

    /// <summary>Human-readable option flags that were requested but not applied.</summary>
    public IReadOnlyList<string> UnsupportedOptionLabels { get; init; } = Array.Empty<string>();

    /// <summary>Token labels detected in the pattern that this flavor does not support.</summary>
    public IReadOnlyList<string> UnsupportedTokensInPattern { get; init; } = Array.Empty<string>();

    /// <summary>Known dialect differences from the flavor definition (capped for UI).</summary>
    public IReadOnlyList<string> KnownDifferences { get; init; } = Array.Empty<string>();

    /// <summary>Short bullet notes for the card (options, tokens, fidelity, validity).</summary>
    public IReadOnlyList<string> KeyNotes { get; init; } = Array.Empty<string>();

    public string FidelityBadge => Fidelity.DisplayName();

    public string ValidityLabel => IsValid ? "Valid" : "Invalid";

    public string MatchCountLabel => IsValid ? $"{MatchCount} match(es)" : "—";

    public string DurationLabel => $"{Duration.TotalMilliseconds:F2} ms";

    public string HeaderLine =>
        $"{FlavorDisplayName} · {EngineDisplayName} · {FidelityBadge}";
}

/// <summary>
/// Aggregate multi-flavor comparison result.
/// </summary>
public sealed class CompareResult
{
    public IReadOnlyList<FlavorCompareResult> Flavors { get; init; } = Array.Empty<FlavorCompareResult>();

    /// <summary>Cross-flavor differences (match counts, validity, first divergence).</summary>
    public IReadOnlyList<string> CrossFlavorDifferences { get; init; } = Array.Empty<string>();

    /// <summary>Plain-text export of the comparison (for copy / clipboard).</summary>
    public string SummaryText { get; init; } = string.Empty;

    public TimeSpan TotalDuration { get; init; }

    public bool HasResults => Flavors.Count > 0;

    public string StatusLine
    {
        get
        {
            if (Flavors.Count == 0)
                return "Select 2–4 flavors to compare";

            var valid = Flavors.Count(f => f.IsValid);
            var diffs = CrossFlavorDifferences.Count;
            return $"{Flavors.Count} flavor(s) · {valid} valid · {diffs} difference note(s) · {TotalDuration.TotalMilliseconds:F1} ms";
        }
    }
}
