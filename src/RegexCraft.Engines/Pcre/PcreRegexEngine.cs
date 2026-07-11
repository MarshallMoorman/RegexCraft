using System.Diagnostics;
using System.Text;
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
    public bool SupportsSplit => true;

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
            var spans = new List<ReplacementSpan>();
            var sb = new StringBuilder();
            var last = 0;
            var count = 0;

            foreach (var m in regex.Matches(subject))
            {
                if (m.Index > last)
                    sb.Append(subject, last, m.Index - last);

                var substituted = ExpandManual(m, regex, subject, replacement);
                var start = sb.Length;
                sb.Append(substituted);
                spans.Add(new ReplacementSpan
                {
                    Index = start,
                    Length = substituted.Length,
                    MatchIndex = count,
                });
                count++;
                last = m.Index + m.Length;
            }

            if (last < subject.Length)
                sb.Append(subject, last, subject.Length - last);

            var result = sb.ToString();
            sw.Stop();
            _logger.LogDebug(
                "PCRE2 Replace: {Count} replacement(s) in {ElapsedMs}ms",
                count,
                sw.ElapsedMilliseconds);

            return ReplaceResult.FromResult(Id, result, count, sw.Elapsed, spans);
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

    public SplitResult Split(string pattern, string subject, RegexOptionsEx options, bool removeEmptyEntries = false)
    {
        pattern ??= string.Empty;
        subject ??= string.Empty;

        var sw = Stopwatch.StartNew();
        try
        {
            var regex = CreateRegex(pattern, options);
            var delimiters = new List<SplitDelimiterRange>();
            var matches = regex.Matches(subject).ToList();

            foreach (var m in matches)
            {
                delimiters.Add(new SplitDelimiterRange
                {
                    Index = m.Index,
                    Length = m.Length,
                    Value = m.Value,
                });
            }

            // Manual split so we control empty entries and don't depend on PcreRegex.Split quirks.
            var parts = new List<string>();
            var last = 0;
            foreach (var m in matches)
            {
                parts.Add(subject[last..m.Index]);
                last = m.Index + m.Length;
            }

            parts.Add(subject[last..]);

            if (removeEmptyEntries)
                parts = parts.Where(p => p.Length > 0).ToList();

            sw.Stop();
            _logger.LogDebug(
                "PCRE2 Split: {PartCount} part(s) in {ElapsedMs}ms",
                parts.Count,
                sw.ElapsedMilliseconds);

            return SplitResult.FromParts(Id, parts, delimiters, sw.Elapsed);
        }
        catch (ArgumentException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Split: invalid pattern or argument");
            return SplitResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (PcreException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "PCRE2 Split: PCRE error");
            return SplitResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "PCRE2 Split: unexpected error");
            return SplitResult.Failed(Id, ex.Message, sw.Elapsed);
        }
    }

    /// <summary>
    /// Expand a replacement string with backreferences for a PCRE match.
    /// Supports $n, ${n}, ${name}, $&amp;, $`, $', \n digit refs.
    /// </summary>
    private static string ExpandManual(PcreMatch match, PcreRegex regex, string subject, string replacement)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < replacement.Length; i++)
        {
            var c = replacement[i];
            if (c == '$' && i + 1 < replacement.Length)
            {
                var next = replacement[i + 1];
                if (next == '$')
                {
                    sb.Append('$');
                    i++;
                    continue;
                }

                if (next == '&')
                {
                    sb.Append(match.Value);
                    i++;
                    continue;
                }

                if (next == '`')
                {
                    if (match.Index > 0)
                        sb.Append(subject, 0, match.Index);
                    i++;
                    continue;
                }

                if (next == '\'')
                {
                    var end = match.Index + match.Length;
                    if (end < subject.Length)
                        sb.Append(subject, end, subject.Length - end);
                    i++;
                    continue;
                }

                if (next == '{')
                {
                    var close = replacement.IndexOf('}', i + 2);
                    if (close > i + 2)
                    {
                        var name = replacement[(i + 2)..close];
                        if (int.TryParse(name, out var num))
                            sb.Append(GroupValue(match, num));
                        else
                            sb.Append(GroupValueByName(match, name));
                        i = close;
                        continue;
                    }
                }

                if (char.IsDigit(next))
                {
                    var j = i + 1;
                    while (j < replacement.Length && char.IsDigit(replacement[j]))
                        j++;
                    var digits = replacement[(i + 1)..j];
                    var chosen = -1;
                    for (var len = digits.Length; len >= 1; len--)
                    {
                        if (int.TryParse(digits[..len], out var g) && g <= match.CaptureCount)
                        {
                            chosen = g;
                            i += len;
                            break;
                        }
                    }

                    if (chosen >= 0)
                    {
                        sb.Append(GroupValue(match, chosen));
                        continue;
                    }
                }
            }

            if (c == '\\' && i + 1 < replacement.Length)
            {
                var next = replacement[i + 1];
                if (char.IsDigit(next))
                {
                    var j = i + 1;
                    while (j < replacement.Length && char.IsDigit(replacement[j]))
                        j++;
                    var digits = replacement[(i + 1)..j];
                    if (int.TryParse(digits, out var g) && g <= match.CaptureCount)
                    {
                        sb.Append(GroupValue(match, g));
                        i = j - 1;
                        continue;
                    }
                }

                if (next is 'n')
                {
                    sb.Append('\n');
                    i++;
                    continue;
                }

                if (next is 't')
                {
                    sb.Append('\t');
                    i++;
                    continue;
                }

                if (next is 'r')
                {
                    sb.Append('\r');
                    i++;
                    continue;
                }

                sb.Append(next);
                i++;
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static string GroupValue(PcreMatch match, int number)
    {
        if (number < 0 || number > match.CaptureCount)
            return string.Empty;
        var g = match.Groups[number];
        return g.Success ? g.Value : string.Empty;
    }

    private static string GroupValueByName(PcreMatch match, string name)
    {
        try
        {
            var g = match[name];
            return g.Success ? g.Value : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static Dictionary<int, string> BuildNameMap(PcreRegex regex)
    {
        var nameByNumber = new Dictionary<int, string>();
        foreach (var name in regex.PatternInfo.GroupNames)
        {
            var indexes = regex.PatternInfo.GetGroupIndexesByName(name);
            foreach (var index in indexes)
                nameByNumber[index] = name;
        }

        return nameByNumber;
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
        var nameByNumber = BuildNameMap(regex);

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
