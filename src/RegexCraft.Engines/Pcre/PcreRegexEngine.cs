using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PCRE;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Engines.Pcre;

/// <summary>
/// Regex engine backed by PCRE2 via PCRE.NET.
/// </summary>
public sealed class PcreRegexEngine : IRegexEngine
{
    public const string EngineId = "pcre2";

    private readonly ILogger<PcreRegexEngine> _logger;

    public PcreRegexEngine(ILogger<PcreRegexEngine>? logger = null)
    {
        _logger = logger ?? NullLogger<PcreRegexEngine>.Instance;
    }

    public string Id => EngineId;
    public string DisplayName => "PCRE2";
    public bool SupportsFullTesting => true;
    public bool SupportsReplace => true;

    public MatchCollectionResult Match(string pattern, string subject, RegexOptionsEx options)
    {
        pattern ??= string.Empty;
        subject ??= string.Empty;

        var sw = Stopwatch.StartNew();
        try
        {
            var regex = CreateRegex(pattern, options);
            var matches = regex.Matches(subject);
            var results = new List<MatchResult>();

            foreach (var match in matches)
            {
                results.Add(MapMatch(match, regex));
            }

            sw.Stop();
            _logger.LogDebug(
                "PCRE2 Match: {MatchCount} match(es) in {ElapsedMs}ms (pattern length {PatternLen})",
                results.Count,
                sw.ElapsedMilliseconds,
                pattern.Length);

            return MatchCollectionResult.FromMatches(Id, results, sw.Elapsed);
        }
        catch (ArgumentException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Match: invalid pattern or argument");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (PcreException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Match: PCRE error");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "PCRE2 Match: unexpected error");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
    }

    public ReplaceResult Replace(string pattern, string subject, string replacement, RegexOptionsEx options)
    {
        pattern ??= string.Empty;
        subject ??= string.Empty;
        replacement ??= string.Empty;

        var sw = Stopwatch.StartNew();
        try
        {
            var regex = CreateRegex(pattern, options);

            // Count matches first so ReplacementCount is accurate, then replace.
            var count = regex.Matches(subject).Count();
            var result = regex.Replace(subject, replacement);

            sw.Stop();
            _logger.LogDebug(
                "PCRE2 Replace: {Count} replacement(s) in {ElapsedMs}ms",
                count,
                sw.ElapsedMilliseconds);

            return ReplaceResult.FromResult(Id, result, count, sw.Elapsed);
        }
        catch (ArgumentException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Replace: invalid pattern or argument");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (PcreException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Replace: PCRE error");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "PCRE2 Replace: unexpected error");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
    }

    private static PcreRegex CreateRegex(string pattern, RegexOptionsEx options)
    {
        var pcreOptions = MapOptions(options);
        return new PcreRegex(pattern, pcreOptions);
    }

    public static PcreOptions MapOptions(RegexOptionsEx options)
    {
        var result = PcreOptions.None;
        if (options.HasFlag(RegexOptionsEx.IgnoreCase))
            result |= PcreOptions.IgnoreCase;
        if (options.HasFlag(RegexOptionsEx.Multiline))
            result |= PcreOptions.MultiLine;
        if (options.HasFlag(RegexOptionsEx.Singleline))
            result |= PcreOptions.Singleline;
        if (options.HasFlag(RegexOptionsEx.ExplicitCapture))
            result |= PcreOptions.ExplicitCapture;
        if (options.HasFlag(RegexOptionsEx.IgnorePatternWhitespace))
            result |= PcreOptions.IgnorePatternWhitespace;
        return result;
    }

    private static MatchResult MapMatch(PcreMatch match, PcreRegex regex)
    {
        var captureCount = match.CaptureCount;
        var groups = new List<GroupResult>(captureCount + 1);

        // Build number → name map from pattern info (named groups).
        var nameByNumber = new Dictionary<int, string>();
        foreach (var name in regex.PatternInfo.GroupNames)
        {
            var indexes = regex.PatternInfo.GetGroupIndexesByName(name);
            foreach (var index in indexes)
            {
                nameByNumber[index] = name;
            }
        }

        for (var i = 0; i <= captureCount; i++)
        {
            var group = match.Groups[i];
            var name = nameByNumber.TryGetValue(i, out var n) ? n : i.ToString();

            groups.Add(new GroupResult
            {
                Number = i,
                Name = name,
                Index = group.Success ? group.Index : -1,
                Length = group.Success ? group.Length : 0,
                Value = group.Success ? group.Value : string.Empty,
                Success = group.Success,
            });
        }

        return new MatchResult
        {
            Index = match.Index,
            Length = match.Length,
            Value = match.Value,
            Groups = groups,
        };
    }
}
