using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Engines.DotNet;

/// <summary>
/// Regex engine backed by <see cref="System.Text.RegularExpressions"/>.
/// </summary>
public sealed class DotNetRegexEngine : IRegexEngine
{
    public const string EngineId = "dotnet";

    private readonly ILogger<DotNetRegexEngine> _logger;

    public DotNetRegexEngine(ILogger<DotNetRegexEngine>? logger = null)
    {
        _logger = logger ?? NullLogger<DotNetRegexEngine>.Instance;
    }

    public string Id => EngineId;
    public string DisplayName => ".NET";
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
            var results = new List<MatchResult>(matches.Count);

            foreach (Match match in matches)
            {
                results.Add(MapMatch(match, regex));
            }

            sw.Stop();
            _logger.LogDebug(
                "DotNet Match: {MatchCount} match(es) in {ElapsedMs}ms (pattern length {PatternLen})",
                results.Count,
                sw.ElapsedMilliseconds,
                pattern.Length);

            return MatchCollectionResult.FromMatches(Id, results, sw.Elapsed);
        }
        catch (RegexParseException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "DotNet Match: invalid pattern");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (ArgumentException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "DotNet Match: argument error");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "DotNet Match: unexpected error");
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
            var count = 0;
            var result = regex.Replace(subject, m =>
            {
                count++;
                return m.Result(replacement);
            });

            sw.Stop();
            _logger.LogDebug(
                "DotNet Replace: {Count} replacement(s) in {ElapsedMs}ms",
                count,
                sw.ElapsedMilliseconds);

            return ReplaceResult.FromResult(Id, result, count, sw.Elapsed);
        }
        catch (RegexParseException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "DotNet Replace: invalid pattern");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (ArgumentException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "DotNet Replace: argument error");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "DotNet Replace: unexpected error");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
    }

    private static Regex CreateRegex(string pattern, RegexOptionsEx options)
    {
        var netOptions = MapOptions(options);
        // NonBacktracking is not used; keep classic engine for full feature parity.
        return new Regex(pattern, netOptions, TimeSpan.FromSeconds(5));
    }

    public static RegexOptions MapOptions(RegexOptionsEx options)
    {
        var result = RegexOptions.None;
        if (options.HasFlag(RegexOptionsEx.IgnoreCase))
            result |= RegexOptions.IgnoreCase;
        if (options.HasFlag(RegexOptionsEx.Multiline))
            result |= RegexOptions.Multiline;
        if (options.HasFlag(RegexOptionsEx.Singleline))
            result |= RegexOptions.Singleline;
        if (options.HasFlag(RegexOptionsEx.ExplicitCapture))
            result |= RegexOptions.ExplicitCapture;
        if (options.HasFlag(RegexOptionsEx.IgnorePatternWhitespace))
            result |= RegexOptions.IgnorePatternWhitespace;
        return result;
    }

    private static MatchResult MapMatch(Match match, Regex regex)
    {
        var groups = new List<GroupResult>(match.Groups.Count);
        var groupNames = regex.GetGroupNames();

        for (var i = 0; i < match.Groups.Count; i++)
        {
            var group = match.Groups[i];
            var name = i < groupNames.Length ? groupNames[i] : i.ToString();

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
