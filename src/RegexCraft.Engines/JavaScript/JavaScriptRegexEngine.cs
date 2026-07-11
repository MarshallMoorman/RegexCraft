using System.Diagnostics;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Engines.JavaScript;

/// <summary>
/// ECMAScript RegExp engine backed by Jint (pure .NET JavaScript interpreter).
/// Provides high-fidelity testing for JavaScript / TypeScript flavors.
/// </summary>
public sealed class JavaScriptRegexEngine : IRegexEngine
{
    public const string EngineId = "javascript";

    private readonly ILogger<JavaScriptRegexEngine> _logger;

    public JavaScriptRegexEngine(ILogger<JavaScriptRegexEngine>? logger = null)
    {
        _logger = logger ?? NullLogger<JavaScriptRegexEngine>.Instance;
    }

    public string Id => EngineId;
    public string DisplayName => "JavaScript (Jint)";
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
            var flags = BuildFlags(options, global: true, pattern);
            using var engine = CreateEngine();
            engine.SetValue("__pattern", pattern);
            engine.SetValue("__subject", subject);
            engine.SetValue("__flags", flags);

            var result = engine.Evaluate("""
                (function () {
                  try {
                    new RegExp(__pattern, __flags.replaceAll('g', ''));
                  } catch (e) {
                    return { ok: false, error: String(e && e.message ? e.message : e) };
                  }
                  const re = new RegExp(__pattern, __flags.includes('g') ? __flags : __flags + 'g');
                  const matches = [];
                  for (const m of __subject.matchAll(re)) {
                    const groups = [];
                    groups.push({
                      number: 0,
                      name: '0',
                      index: m.index,
                      length: m[0] ? m[0].length : 0,
                      value: m[0] ?? '',
                      success: true
                    });
                    for (let i = 1; i < m.length; i++) {
                      const val = m[i];
                      const success = val !== undefined && val !== null;
                      groups.push({
                        number: i,
                        name: String(i),
                        index: -1,
                        length: success ? String(val).length : 0,
                        value: success ? String(val) : '',
                        success: success
                      });
                    }
                    if (m.groups) {
                      for (const [name, val] of Object.entries(m.groups)) {
                        const success = val !== undefined && val !== null;
                        const existing = groups.find(g => g.name === name);
                        if (existing) {
                          existing.value = success ? String(val) : '';
                          existing.success = success;
                          existing.length = success ? String(val).length : 0;
                        } else {
                          groups.push({
                            number: groups.length,
                            name: name,
                            index: -1,
                            length: success ? String(val).length : 0,
                            value: success ? String(val) : '',
                            success: success
                          });
                        }
                      }
                    }
                    matches.push({
                      index: m.index,
                      length: m[0] ? m[0].length : 0,
                      value: m[0] ?? '',
                      groups: groups
                    });
                  }
                  return { ok: true, matches: matches };
                })()
                """);

            if (!TryReadOk(result, out var error, out var obj))
            {
                sw.Stop();
                return MatchCollectionResult.Failed(Id, error ?? "Invalid pattern", sw.Elapsed);
            }

            var list = new List<MatchResult>();
            var matchesArr = obj!.Get("matches");
            if (matchesArr.IsArray())
            {
                var arr = matchesArr.AsArray();
                for (var i = 0; i < (int)arr.Length; i++)
                {
                    var item = arr.Get((uint)i);
                    if (item.IsObject())
                        list.Add(MapMatch(item.AsObject()));
                }
            }

            sw.Stop();
            _logger.LogDebug(
                "JavaScript Match: {MatchCount} match(es) in {ElapsedMs}ms",
                list.Count,
                sw.ElapsedMilliseconds);
            return MatchCollectionResult.FromMatches(Id, list, sw.Elapsed);
        }
        catch (JavaScriptException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "JavaScript Match: JS error");
            return MatchCollectionResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "JavaScript Match: unexpected error");
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
            // Map .NET-style ${name} replacement to JS $<name> for named groups.
            var jsReplacement = MapReplacementToJs(replacement);
            var flags = BuildFlags(options, global: true, pattern);
            using var engine = CreateEngine();
            engine.SetValue("__pattern", pattern);
            engine.SetValue("__subject", subject);
            engine.SetValue("__replacement", jsReplacement);
            engine.SetValue("__flags", flags);

            var result = engine.Evaluate("""
                (function () {
                  try {
                    new RegExp(__pattern, __flags.replaceAll('g', ''));
                  } catch (e) {
                    return { ok: false, error: String(e && e.message ? e.message : e) };
                  }
                  const re = new RegExp(__pattern, __flags.includes('g') ? __flags : __flags + 'g');
                  let count = 0;
                  const spans = [];
                  const native = __subject.replace(re, function (...args) {
                    // last two args are offset and string (or groups object for named)
                    const offset = typeof args[args.length - 2] === 'number'
                      ? args[args.length - 2]
                      : args[args.length - 3];
                    const matchText = args[0];
                    // Compute substituted piece by running a one-match replace
                    const oneFlags = __flags.replaceAll('g', '');
                    const piece = String(matchText).replace(new RegExp(__pattern, oneFlags), __replacement);
                    // Approximate: when callback is used, return value is the substitution directly.
                    // String.replace with string replacement expands $1 itself when not using a function.
                    count++;
                    return null; // placeholder — we use string form below
                  });
                  // Correct path: string replacement expands $n
                  count = 0;
                  const outParts = [];
                  let last = 0;
                  const gre = new RegExp(__pattern, __flags.includes('g') ? __flags : __flags + 'g');
                  let m;
                  gre.lastIndex = 0;
                  while ((m = gre.exec(__subject)) !== null) {
                    outParts.push(__subject.slice(last, m.index));
                    // Expand $n / $& using a non-global replace on the match text alone is imperfect;
                    // use full-string approach: collect then use native replace for final text.
                    count++;
                    last = m.index + (m[0] ? m[0].length : 0);
                    if (!m[0] || m[0].length === 0) {
                      gre.lastIndex++;
                      if (gre.lastIndex > __subject.length) break;
                    }
                  }
                  const finalText = __subject.replace(re, __replacement);
                  // Build spans by walking matches and applying replace to each match value
                  last = 0;
                  let built = '';
                  const spanList = [];
                  gre.lastIndex = 0;
                  let mi = 0;
                  while ((m = gre.exec(__subject)) !== null) {
                    built += __subject.slice(last, m.index);
                    const oneFlags = __flags.replaceAll('g', '');
                    const sub = m[0].replace(new RegExp(__pattern, oneFlags), __replacement);
                    spanList.push({ index: built.length, length: sub.length, matchIndex: mi });
                    built += sub;
                    mi++;
                    last = m.index + m[0].length;
                    if (m[0].length === 0) {
                      gre.lastIndex++;
                      if (gre.lastIndex > __subject.length) break;
                    }
                  }
                  built += __subject.slice(last);
                  return {
                    ok: true,
                    result: finalText,
                    count: count,
                    spans: (built === finalText) ? spanList : []
                  };
                })()
                """);

            if (!TryReadOk(result, out var error, out var obj))
            {
                sw.Stop();
                return ReplaceResult.Failed(Id, error ?? "Invalid pattern", sw.Elapsed);
            }

            var replaced = obj!.Get("result").ToString() ?? string.Empty;
            var count = (int)obj.Get("count").AsNumber();
            var spans = new List<ReplacementSpan>();
            var spansJs = obj.Get("spans");
            if (spansJs.IsArray())
            {
                var arr = spansJs.AsArray();
                for (var i = 0; i < (int)arr.Length; i++)
                {
                    var s = arr.Get((uint)i).AsObject();
                    spans.Add(new ReplacementSpan
                    {
                        Index = (int)s.Get("index").AsNumber(),
                        Length = (int)s.Get("length").AsNumber(),
                        MatchIndex = (int)s.Get("matchIndex").AsNumber(),
                    });
                }
            }

            sw.Stop();
            _logger.LogDebug(
                "JavaScript Replace: {Count} replacement(s) in {ElapsedMs}ms",
                count,
                sw.ElapsedMilliseconds);
            return ReplaceResult.FromResult(Id, replaced, count, sw.Elapsed, spans);
        }
        catch (JavaScriptException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "JavaScript Replace: JS error");
            return ReplaceResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "JavaScript Replace: unexpected error");
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
            var flags = BuildFlags(options, global: false, pattern);
            using var engine = CreateEngine();
            engine.SetValue("__pattern", pattern);
            engine.SetValue("__subject", subject);
            engine.SetValue("__flags", flags);

            var result = engine.Evaluate("""
                (function () {
                  try {
                    new RegExp(__pattern, __flags);
                  } catch (e) {
                    return { ok: false, error: String(e && e.message ? e.message : e) };
                  }
                  const re = new RegExp(__pattern, __flags);
                  const greFlags = __flags.includes('g') ? __flags : __flags + 'g';
                  const gre = new RegExp(__pattern, greFlags);
                  const delimiters = [];
                  let m;
                  gre.lastIndex = 0;
                  while ((m = gre.exec(__subject)) !== null) {
                    delimiters.push({ index: m.index, length: m[0].length, value: m[0] });
                    if (m[0].length === 0) {
                      gre.lastIndex++;
                      if (gre.lastIndex > __subject.length) break;
                    }
                  }
                  const parts = __subject.split(re);
                  return { ok: true, parts: parts, delimiters: delimiters };
                })()
                """);

            if (!TryReadOk(result, out var error, out var obj))
            {
                sw.Stop();
                return SplitResult.Failed(Id, error ?? "Invalid pattern", sw.Elapsed);
            }

            var parts = new List<string>();
            var partsJs = obj!.Get("parts");
            if (partsJs.IsArray())
            {
                var arr = partsJs.AsArray();
                for (var i = 0; i < (int)arr.Length; i++)
                    parts.Add(arr.Get((uint)i).ToString() ?? string.Empty);
            }

            if (removeEmptyEntries)
                parts = parts.Where(p => p.Length > 0).ToList();

            var delimiters = new List<SplitDelimiterRange>();
            var delJs = obj.Get("delimiters");
            if (delJs.IsArray())
            {
                var arr = delJs.AsArray();
                for (var i = 0; i < (int)arr.Length; i++)
                {
                    var d = arr.Get((uint)i).AsObject();
                    delimiters.Add(new SplitDelimiterRange
                    {
                        Index = (int)d.Get("index").AsNumber(),
                        Length = (int)d.Get("length").AsNumber(),
                        Value = d.Get("value").ToString() ?? string.Empty,
                    });
                }
            }

            sw.Stop();
            _logger.LogDebug(
                "JavaScript Split: {PartCount} part(s) in {ElapsedMs}ms",
                parts.Count,
                sw.ElapsedMilliseconds);
            return SplitResult.FromParts(Id, parts, delimiters, sw.Elapsed);
        }
        catch (JavaScriptException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "JavaScript Split: JS error");
            return SplitResult.Failed(Id, ex.Message, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "JavaScript Split: unexpected error");
            return SplitResult.Failed(Id, ex.Message, sw.Elapsed);
        }
    }

    private static Engine CreateEngine() =>
        new Engine(cfg =>
        {
            cfg.TimeoutInterval(TimeSpan.FromSeconds(5));
            cfg.MaxStatements(100_000);
            cfg.LimitRecursion(64);
        });

    private static string BuildFlags(RegexOptionsEx options, bool global, string? pattern = null)
    {
        var sb = new StringBuilder();
        if (global) sb.Append('g');
        if (options.HasFlag(RegexOptionsEx.IgnoreCase)) sb.Append('i');
        if (options.HasFlag(RegexOptionsEx.Multiline)) sb.Append('m');
        if (options.HasFlag(RegexOptionsEx.Singleline)) sb.Append('s');
        // Unicode property escapes (\p{…}) and astral plane need the `u` flag in JS.
        if (pattern is not null &&
            (pattern.Contains(@"\p{", StringComparison.Ordinal) ||
             pattern.Contains(@"\P{", StringComparison.Ordinal)))
        {
            sb.Append('u');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Maps common .NET / PCRE replacement forms to ECMAScript where possible.
    /// <c>${name}</c> → <c>$&lt;name&gt;</c>; leaves <c>$1</c> / <c>$&amp;</c> alone.
    /// </summary>
    internal static string MapReplacementToJs(string replacement)
    {
        if (string.IsNullOrEmpty(replacement))
            return replacement ?? string.Empty;

        // ${name} → $<name>
        return System.Text.RegularExpressions.Regex.Replace(
            replacement,
            @"\$\{([A-Za-z_][A-Za-z0-9_]*)\}",
            m => "$<" + m.Groups[1].Value + ">");
    }

    private static bool TryReadOk(JsValue result, out string? error, out ObjectInstance? obj)
    {
        error = null;
        obj = null;
        if (!result.IsObject())
        {
            error = "Unexpected engine result";
            return false;
        }

        obj = result.AsObject();
        var ok = obj.Get("ok");
        if (ok.IsBoolean() && !ok.AsBoolean())
        {
            error = obj.Get("error").ToString() ?? "Invalid pattern";
            return false;
        }

        return true;
    }

    private static MatchResult MapMatch(ObjectInstance item)
    {
        var groups = new List<GroupResult>();
        var groupsJs = item.Get("groups");
        if (groupsJs.IsArray())
        {
            var arr = groupsJs.AsArray();
            for (var i = 0; i < (int)arr.Length; i++)
            {
                var g = arr.Get((uint)i).AsObject();
                groups.Add(new GroupResult
                {
                    Number = (int)g.Get("number").AsNumber(),
                    Name = g.Get("name").ToString() ?? i.ToString(),
                    Index = (int)g.Get("index").AsNumber(),
                    Length = (int)g.Get("length").AsNumber(),
                    Value = g.Get("value").ToString() ?? string.Empty,
                    Success = g.Get("success").AsBoolean(),
                });
            }
        }

        return new MatchResult
        {
            Index = (int)item.Get("index").AsNumber(),
            Length = (int)item.Get("length").AsNumber(),
            Value = item.Get("value").ToString() ?? string.Empty,
            Groups = groups,
        };
    }
}
