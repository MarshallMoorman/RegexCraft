using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;

namespace RegexCraft.Core.Compare;

/// <summary>
/// Multi-flavor comparison using the existing <see cref="IFlavorService"/> and engines.
/// </summary>
public sealed class RegexCompareService : IRegexCompareService
{
    private readonly IFlavorService _flavorService;
    private readonly ITokenCatalog _tokenCatalog;
    private readonly ILogger<RegexCompareService> _logger;

    public const int MinFlavors = 2;
    public const int MaxFlavors = 4;
    public const int DefaultMaxMatches = 5;

    public RegexCompareService(
        IFlavorService flavorService,
        ITokenCatalog tokenCatalog,
        ILogger<RegexCompareService>? logger = null)
    {
        _flavorService = flavorService ?? throw new ArgumentNullException(nameof(flavorService));
        _tokenCatalog = tokenCatalog ?? throw new ArgumentNullException(nameof(tokenCatalog));
        _logger = logger ?? NullLogger<RegexCompareService>.Instance;
    }

    public CompareResult Compare(CompareRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.FlavorIds is null || request.FlavorIds.Count == 0)
        {
            return new CompareResult
            {
                SummaryText = "No flavors selected.",
            };
        }

        var pattern = request.Pattern ?? string.Empty;
        var subject = request.Subject ?? string.Empty;
        var maxMatches = request.MaxMatchesToShow > 0 ? request.MaxMatchesToShow : DefaultMaxMatches;

        // Dedupe flavor ids while preserving order; clamp to MaxFlavors.
        var flavorIds = request.FlavorIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxFlavors)
            .ToList();

        var totalSw = Stopwatch.StartNew();

        // Resolve definitions first (main thread / sequential — cheap).
        var resolved = new List<(FlavorDefinition Flavor, IRegexEngine? Engine)>();
        foreach (var id in flavorIds)
        {
            var flavor = _flavorService.GetFlavor(id);
            if (flavor is null)
            {
                _logger.LogWarning("Compare: unknown flavor id {FlavorId}", id);
                continue;
            }

            var engine = _flavorService.GetEngine(flavor.EngineId);
            resolved.Add((flavor, engine));
        }

        if (resolved.Count == 0)
        {
            return new CompareResult
            {
                SummaryText = "No valid flavors to compare.",
            };
        }

        // Evaluate each flavor. Keep sequential for predictable UI/test threading
        // (2–4 flavors is fast enough without Parallel.For side effects on Avalonia headless).
        var flavors = new List<FlavorCompareResult>(resolved.Count);
        foreach (var (flavor, engine) in resolved)
            flavors.Add(EvaluateFlavor(flavor, engine, pattern, subject, request.Options, maxMatches));

        totalSw.Stop();
        var cross = BuildCrossFlavorDifferences(flavors);
        var summary = BuildSummaryText(pattern, subject, request.Options, flavors, cross);

        _logger.LogDebug(
            "Compare complete: {Count} flavors, {Diffs} diffs, {Ms:F1}ms",
            flavors.Count, cross.Count, totalSw.Elapsed.TotalMilliseconds);

        return new CompareResult
        {
            Flavors = flavors,
            CrossFlavorDifferences = cross,
            SummaryText = summary,
            TotalDuration = totalSw.Elapsed,
        };
    }

    private FlavorCompareResult EvaluateFlavor(
        FlavorDefinition flavor,
        IRegexEngine? engine,
        string pattern,
        string subject,
        RegexOptionsEx requestedOptions,
        int maxMatches)
    {
        var applied = flavor.FilterOptions(requestedOptions);
        var dropped = requestedOptions & ~applied;
        var unsupportedOpts = DescribeOptions(dropped);
        var unsupportedTokens = DetectUnsupportedTokensInPattern(pattern, flavor);
        var known = flavor.KnownDifferences.Take(4).ToList();

        if (engine is null)
        {
            var notes = BuildKeyNotes(
                isValid: false,
                error: "No engine registered for this flavor.",
                flavor,
                unsupportedOpts,
                unsupportedTokens,
                known);

            return new FlavorCompareResult
            {
                FlavorId = flavor.Id,
                FlavorDisplayName = flavor.DisplayName,
                EngineId = flavor.EngineId,
                EngineDisplayName = flavor.EngineId,
                Fidelity = flavor.Fidelity,
                FidelityNote = flavor.FidelityNote,
                IsValid = false,
                ErrorMessage = "No engine available for this flavor.",
                AppliedOptions = applied,
                DroppedOptions = dropped,
                UnsupportedOptionLabels = unsupportedOpts,
                UnsupportedTokensInPattern = unsupportedTokens,
                KnownDifferences = known,
                KeyNotes = notes,
            };
        }

        MatchCollectionResult result;
        try
        {
            result = engine.Match(pattern, subject, applied);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Compare match threw for flavor {FlavorId}", flavor.Id);
            result = MatchCollectionResult.Failed(engine.Id, ex.Message);
        }

        var matchSummaries = new List<CompareMatchSummary>();
        if (result.Success)
        {
            var take = Math.Min(maxMatches, result.Matches.Count);
            for (var i = 0; i < take; i++)
            {
                var m = result.Matches[i];
                var groups = m.Groups
                    .Where(g => g.Number > 0 && g.Success)
                    .Select(g =>
                    {
                        var name = g.Name != g.Number.ToString()
                            ? $"G{g.Number}/{g.Name}"
                            : $"G{g.Number}";
                        return $"{name}={Truncate(g.Value, 40)}";
                    })
                    .ToList();

                matchSummaries.Add(new CompareMatchSummary
                {
                    Index = i,
                    Start = m.Index,
                    Length = m.Length,
                    Value = m.Value,
                    GroupSummaries = groups,
                });
            }
        }

        var notes2 = BuildKeyNotes(
            result.Success,
            result.ErrorMessage,
            flavor,
            unsupportedOpts,
            unsupportedTokens,
            known);

        return new FlavorCompareResult
        {
            FlavorId = flavor.Id,
            FlavorDisplayName = flavor.DisplayName,
            EngineId = engine.Id,
            EngineDisplayName = engine.DisplayName,
            Fidelity = flavor.Fidelity,
            FidelityNote = flavor.FidelityNote,
            IsValid = result.Success,
            ErrorMessage = result.Success ? null : (result.ErrorMessage ?? "Invalid pattern"),
            MatchCount = result.Success ? result.Matches.Count : 0,
            Matches = matchSummaries,
            Duration = result.Duration,
            AppliedOptions = applied,
            DroppedOptions = dropped,
            UnsupportedOptionLabels = unsupportedOpts,
            UnsupportedTokensInPattern = unsupportedTokens,
            KnownDifferences = known,
            KeyNotes = notes2,
        };
    }

    private List<string> DetectUnsupportedTokensInPattern(string pattern, FlavorDefinition flavor)
    {
        if (string.IsNullOrEmpty(pattern) || flavor.UnsupportedTokenIds.Count == 0)
            return new List<string>();

        var unsupported = new HashSet<string>(flavor.UnsupportedTokenIds, StringComparer.OrdinalIgnoreCase);
        var found = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in _tokenCatalog.GetAllTokens())
        {
            if (!unsupported.Contains(token.Id))
                continue;

            var insert = token.InsertText;
            if (string.IsNullOrEmpty(insert) || insert.Length < 2)
                continue;

            // Prefer distinctive inserts (≥2 chars of regex syntax).
            if (pattern.Contains(insert, StringComparison.Ordinal) && seen.Add(token.Id))
            {
                found.Add(token.Label);
                if (found.Count >= 6)
                    break;
            }
        }

        return found;
    }

    private static IReadOnlyList<string> BuildKeyNotes(
        bool isValid,
        string? error,
        FlavorDefinition flavor,
        IReadOnlyList<string> unsupportedOpts,
        IReadOnlyList<string> unsupportedTokens,
        IReadOnlyList<string> known)
    {
        var notes = new List<string>();

        if (!isValid && !string.IsNullOrWhiteSpace(error))
            notes.Add($"Error: {error}");

        if (flavor.Fidelity != TestingFidelity.Full)
            notes.Add($"Fidelity: {flavor.Fidelity.DisplayName()}" +
                      (string.IsNullOrWhiteSpace(flavor.FidelityNote) ? "" : $" — {flavor.FidelityNote}"));

        if (unsupportedOpts.Count > 0)
            notes.Add("Dropped options: " + string.Join(", ", unsupportedOpts));

        if (unsupportedTokens.Count > 0)
            notes.Add("Unsupported tokens in pattern: " + string.Join(", ", unsupportedTokens));

        foreach (var d in known.Take(2))
            notes.Add(d);

        return notes;
    }

    private static List<string> BuildCrossFlavorDifferences(IReadOnlyList<FlavorCompareResult> flavors)
    {
        var diffs = new List<string>();
        if (flavors.Count < 2)
            return diffs;

        var validities = flavors.GroupBy(f => f.IsValid).ToList();
        if (validities.Count > 1)
        {
            var valid = string.Join(", ", flavors.Where(f => f.IsValid).Select(f => f.FlavorDisplayName));
            var invalid = string.Join(", ", flavors.Where(f => !f.IsValid).Select(f => f.FlavorDisplayName));
            diffs.Add($"Validity differs — valid: [{valid}]; invalid: [{invalid}]");
        }

        var validFlavors = flavors.Where(f => f.IsValid).ToList();
        if (validFlavors.Count >= 2)
        {
            var counts = validFlavors.Select(f => f.MatchCount).Distinct().ToList();
            if (counts.Count > 1)
            {
                var detail = string.Join("; ",
                    validFlavors.Select(f => $"{f.FlavorDisplayName}={f.MatchCount}"));
                diffs.Add($"Match counts differ: {detail}");
            }

            // First-match divergence (position or value).
            var firsts = validFlavors
                .Where(f => f.Matches.Count > 0)
                .Select(f => (f.FlavorDisplayName, f.Matches[0]))
                .ToList();
            if (firsts.Count >= 2)
            {
                var starts = firsts.Select(x => x.Item2.Start).Distinct().ToList();
                var values = firsts.Select(x => x.Item2.Value).Distinct(StringComparer.Ordinal).ToList();
                if (starts.Count > 1 || values.Count > 1)
                {
                    var detail = string.Join("; ",
                        firsts.Select(x =>
                            $"{x.FlavorDisplayName}@[{x.Item2.Start}+{x.Item2.Length}]={Truncate(x.Item2.Value, 30)}"));
                    diffs.Add($"First match differs: {detail}");
                }
            }
            else if (validFlavors.Any(f => f.Matches.Count == 0) && validFlavors.Any(f => f.Matches.Count > 0))
            {
                diffs.Add("Some flavors match while others find no matches.");
            }
        }

        // Option / token differences among selections.
        var withDropped = flavors.Where(f => f.UnsupportedOptionLabels.Count > 0).ToList();
        if (withDropped.Count > 0 && withDropped.Count < flavors.Count)
        {
            diffs.Add("Some flavors drop requested options that others apply: " +
                      string.Join("; ", withDropped.Select(f =>
                          $"{f.FlavorDisplayName} drops [{string.Join(", ", f.UnsupportedOptionLabels)}]")));
        }

        var withTokens = flavors.Where(f => f.UnsupportedTokensInPattern.Count > 0).ToList();
        if (withTokens.Count > 0)
        {
            diffs.Add("Pattern uses constructs unsupported on: " +
                      string.Join("; ", withTokens.Select(f =>
                          $"{f.FlavorDisplayName} [{string.Join(", ", f.UnsupportedTokensInPattern)}]")));
        }

        var fidelities = flavors.Select(f => f.Fidelity).Distinct().ToList();
        if (fidelities.Count > 1)
        {
            diffs.Add("Testing fidelity differs: " +
                      string.Join("; ", flavors.Select(f => $"{f.FlavorDisplayName}={f.FidelityBadge}")));
        }

        if (diffs.Count == 0)
            diffs.Add("No significant differences detected for this pattern and subject.");

        return diffs;
    }

    private static string BuildSummaryText(
        string pattern,
        string subject,
        RegexOptionsEx options,
        IReadOnlyList<FlavorCompareResult> flavors,
        IReadOnlyList<string> cross)
    {
        var sb = new StringBuilder();
        sb.AppendLine("RegexCraft — Flavor Comparison");
        sb.AppendLine(new string('=', 40));
        sb.AppendLine($"Pattern: {pattern}");
        sb.AppendLine($"Subject: {Truncate(subject.Replace("\r", "").Replace("\n", "\\n"), 200)}");
        var optionLabels = DescribeOptions(options);
        sb.AppendLine($"Options: {(optionLabels.Count > 0 ? string.Join(", ", optionLabels) : "(none)")}");
        sb.AppendLine();

        foreach (var f in flavors)
        {
            sb.AppendLine($"--- {f.FlavorDisplayName} ({f.EngineDisplayName}, {f.FidelityBadge}) ---");
            if (!f.IsValid)
            {
                sb.AppendLine($"  Invalid: {f.ErrorMessage}");
            }
            else
            {
                sb.AppendLine($"  Matches: {f.MatchCount} in {f.DurationLabel}");
                foreach (var m in f.Matches)
                {
                    sb.AppendLine($"    {m.SummaryLine}");
                    foreach (var g in m.GroupSummaries)
                        sb.AppendLine($"      {g}");
                }
            }

            foreach (var note in f.KeyNotes)
                sb.AppendLine($"  · {note}");
            sb.AppendLine();
        }

        sb.AppendLine("Cross-flavor differences");
        sb.AppendLine(new string('-', 24));
        foreach (var d in cross)
            sb.AppendLine($"• {d}");

        return sb.ToString().TrimEnd();
    }

    private static List<string> DescribeOptions(RegexOptionsEx options)
    {
        var list = new List<string>();
        if (options.HasFlag(RegexOptionsEx.IgnoreCase)) list.Add("IgnoreCase");
        if (options.HasFlag(RegexOptionsEx.Multiline)) list.Add("Multiline");
        if (options.HasFlag(RegexOptionsEx.Singleline)) list.Add("Singleline");
        if (options.HasFlag(RegexOptionsEx.ExplicitCapture)) list.Add("ExplicitCapture");
        if (options.HasFlag(RegexOptionsEx.IgnorePatternWhitespace)) list.Add("IgnorePatternWhitespace");
        return list;
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
