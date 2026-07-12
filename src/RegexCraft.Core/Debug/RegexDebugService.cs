using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Models;

namespace RegexCraft.Core.Debug;

/// <summary>
/// Hybrid educational debugger: walks the pattern analysis tree and overlays
/// real engine Match / capture results. Explains the intended matching process
/// for teaching; it is <b>not</b> a cycle-accurate re-implementation of .NET's NFA.
/// </summary>
public sealed class RegexDebugService : IRegexDebugService
{
    public const string SupportedEngineId = "dotnet";

    private readonly IRegexAnalysisService _analysis;
    private readonly ILogger<RegexDebugService> _logger;

    public RegexDebugService(
        IRegexAnalysisService? analysis = null,
        ILogger<RegexDebugService>? logger = null)
    {
        _analysis = analysis ?? new RegexAnalysisService();
        _logger = logger ?? NullLogger<RegexDebugService>.Instance;
    }

    public bool SupportsEngine(string engineId) =>
        string.Equals(engineId, SupportedEngineId, StringComparison.OrdinalIgnoreCase);

    public DebugSession BuildSession(
        string pattern,
        string subject,
        RegexOptionsEx options,
        string engineId,
        string engineDisplayName,
        MatchCollectionResult matchResult)
    {
        pattern ??= string.Empty;
        subject ??= string.Empty;
        engineId ??= string.Empty;
        engineDisplayName ??= engineId;

        if (!SupportsEngine(engineId))
        {
            return DebugSession.Unavailable(
                engineId,
                engineDisplayName,
                $"Step-through Debug is currently available only for the .NET engine. " +
                $"The selected flavor uses {engineDisplayName}. Switch to .NET (or another flavor backed by .NET) to use Debug.");
        }

        try
        {
            var session = BuildAvailableSession(pattern, subject, options, engineId, engineDisplayName, matchResult);
            _logger.LogDebug(
                "Debug session built: {StepCount} steps, {MatchCount} matches, valid={Valid}",
                session.Steps.Count, session.MatchCount, session.PatternValid);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build debug session");
            var steps = new List<DebugStep>
            {
                new()
                {
                    Index = 0,
                    Kind = DebugStepKind.Error,
                    Explanation = $"Could not build debug session: {ex.Message}",
                    Success = false,
                },
            };
            return new DebugSession
            {
                IsAvailable = true,
                EngineId = engineId,
                EngineDisplayName = engineDisplayName,
                Steps = steps,
                PatternValid = false,
                PatternError = ex.Message,
                ApproachNote = ApproachNoteText,
            };
        }
    }

    private const string ApproachNoteText =
        "Educational walk-through using the real .NET Match results plus the Analysis Tree. " +
        "Not a cycle-accurate simulation of the engine’s internal NFA.";

    private DebugSession BuildAvailableSession(
        string pattern,
        string subject,
        RegexOptionsEx options,
        string engineId,
        string engineDisplayName,
        MatchCollectionResult matchResult)
    {
        var steps = new List<DebugStep>();
        var idx = 0;

        DebugStep Make(
            DebugStepKind kind,
            string explanation,
            bool? success = null,
            int patternStart = -1,
            int patternLength = 0,
            int subjectStart = -1,
            int subjectLength = 0,
            int? matchIndex = null,
            int? groupNumber = null,
            string? groupName = null)
        {
            var step = new DebugStep
            {
                Index = idx++,
                Kind = kind,
                Explanation = explanation,
                Success = success,
                PatternStart = patternStart,
                PatternLength = patternLength,
                SubjectStart = subjectStart,
                SubjectLength = subjectLength,
                MatchIndex = matchIndex,
                GroupNumber = groupNumber,
                GroupName = groupName,
            };
            steps.Add(step);
            return step;
        }

        Make(
            DebugStepKind.Start,
            $"Starting educational debug for .NET. Pattern length {pattern.Length}, subject length {subject.Length}. " +
            $"Options: {DescribeOptions(options)}.",
            success: null,
            patternStart: pattern.Length > 0 ? 0 : -1,
            patternLength: pattern.Length,
            subjectStart: subject.Length > 0 ? 0 : -1,
            subjectLength: Math.Min(subject.Length, 40));

        if (string.IsNullOrEmpty(pattern))
        {
            Make(DebugStepKind.Error, "Pattern is empty — enter a regular expression to debug.", success: false);
            Make(DebugStepKind.Complete, "Debug complete — nothing to step through.", success: false);
            return Finish(steps, engineId, engineDisplayName, 0, true, null);
        }

        if (!matchResult.Success)
        {
            var err = matchResult.ErrorMessage ?? "Invalid pattern";
            Make(
                DebugStepKind.Error,
                $"Pattern is invalid for .NET: {err}",
                success: false,
                patternStart: 0,
                patternLength: pattern.Length);
            Make(DebugStepKind.Complete, "Debug complete — fix the pattern to step through matching.", success: false);
            return Finish(steps, engineId, engineDisplayName, 0, false, err);
        }

        var root = _analysis.Analyze(pattern);
        var walkable = CollectWalkableNodes(root);

        Make(
            DebugStepKind.Overview,
            walkable.Count == 0
                ? "Analysis Tree is empty or could not parse the pattern structure."
                : $"Pattern structure: {walkable.Count} notable construct(s). Walking them in left-to-right order with real match results overlaid.",
            success: true,
            patternStart: 0,
            patternLength: pattern.Length);

        var matches = matchResult.Matches;
        if (matches.Count == 0)
        {
            Make(
                DebugStepKind.Attempt,
                "Starting search at subject position 0. The engine will try the pattern at successive positions until a match or the end of the subject.",
                success: null,
                patternStart: 0,
                patternLength: pattern.Length,
                subjectStart: 0,
                subjectLength: Math.Min(subject.Length, 1));

            foreach (var node in walkable)
            {
                Make(
                    DebugStepKind.PatternNode,
                    BuildNodeExplanation(node, attempting: true, subjectPos: 0),
                    success: null,
                    patternStart: node.StartIndex,
                    patternLength: node.Length,
                    subjectStart: 0,
                    subjectLength: Math.Min(subject.Length, Math.Max(1, EstimateNodeConsume(node, subject, 0))));
            }

            Make(
                DebugStepKind.Failure,
                "No match — the .NET engine found zero matches in the subject for this pattern and options.",
                success: false,
                patternStart: 0,
                patternLength: pattern.Length,
                subjectStart: 0,
                subjectLength: subject.Length);

            Make(DebugStepKind.Complete, "Debug complete — 0 match(es).", success: false);
            return Finish(steps, engineId, engineDisplayName, 0, true, null);
        }

        for (var mi = 0; mi < matches.Count; mi++)
        {
            var match = matches[mi];
            Make(
                DebugStepKind.Attempt,
                $"Match #{mi}: the engine reported a match at subject position {match.Index} (length {match.Length}): “{Truncate(match.Value, 60)}”.",
                success: true,
                patternStart: 0,
                patternLength: pattern.Length,
                subjectStart: match.Index,
                subjectLength: match.Length,
                matchIndex: mi);

            // Walk pattern nodes while highlighting the real match span (and groups when known).
            var cursor = match.Index;
            foreach (var node in walkable)
            {
                var (subStart, subLen, note) = MapNodeToSubject(node, match, cursor, subject);
                if (subLen > 0 && subStart >= match.Index)
                    cursor = Math.Max(cursor, subStart + subLen);

                Make(
                    DebugStepKind.PatternNode,
                    BuildNodeExplanation(node, attempting: false, subjectPos: subStart) +
                    (string.IsNullOrEmpty(note) ? "" : " " + note),
                    success: true,
                    patternStart: node.StartIndex,
                    patternLength: node.Length,
                    subjectStart: subStart,
                    subjectLength: subLen,
                    matchIndex: mi);
            }

            foreach (var g in match.Groups.Where(g => g.Number > 0 && g.Success))
            {
                var label = g.Name != g.Number.ToString()
                    ? $"Group {g.Number} / “{g.Name}”"
                    : $"Group {g.Number}";
                Make(
                    DebugStepKind.Capture,
                    $"{label} captured “{Truncate(g.Value, 50)}” at position {g.Index}+{g.Length}.",
                    success: true,
                    patternStart: 0,
                    patternLength: pattern.Length,
                    subjectStart: g.Index,
                    subjectLength: g.Length,
                    matchIndex: mi,
                    groupNumber: g.Number,
                    groupName: g.Name);
            }

            Make(
                DebugStepKind.MatchSuccess,
                $"Match #{mi} complete — full match “{Truncate(match.Value, 60)}” at {match.Index}+{match.Length}.",
                success: true,
                patternStart: 0,
                patternLength: pattern.Length,
                subjectStart: match.Index,
                subjectLength: match.Length,
                matchIndex: mi);
        }

        Make(
            DebugStepKind.Complete,
            $"Debug complete — {matches.Count} match(es). Use Step Back/Forward to revisit any step.",
            success: true,
            patternStart: 0,
            patternLength: pattern.Length);

        return Finish(steps, engineId, engineDisplayName, matches.Count, true, null);
    }

    private static DebugSession Finish(
        List<DebugStep> steps,
        string engineId,
        string engineDisplayName,
        int matchCount,
        bool patternValid,
        string? patternError) =>
        new()
        {
            IsAvailable = true,
            EngineId = engineId,
            EngineDisplayName = engineDisplayName,
            Steps = steps,
            MatchCount = matchCount,
            PatternValid = patternValid,
            PatternError = patternError,
            ApproachNote = ApproachNoteText,
        };

    /// <summary>
    /// Significant analysis nodes in document order (groups, atoms, quantifiers, etc.).
    /// Skips pure root wrappers; includes sequences only when they have no walkable children detail.
    /// </summary>
    public static List<AnalysisNode> CollectWalkableNodes(AnalysisNode root)
    {
        var list = new List<AnalysisNode>();
        void Visit(AnalysisNode node, bool isRoot)
        {
            if (node.IsError)
                return;

            var isStructuralOnly = node.Kind is AnalysisNodeKind.Root or AnalysisNodeKind.Sequence
                && node.Children.Count > 0;

            if (!isRoot && !isStructuralOnly && node.HasRange)
            {
                list.Add(node);
                // Still descend into groups/quantifiers/lookarounds so inner atoms appear too,
                // but for simple leaves stop.
                if (node.Kind is AnalysisNodeKind.Group or AnalysisNodeKind.NamedGroup
                    or AnalysisNodeKind.NonCapturingGroup or AnalysisNodeKind.Lookaround
                    or AnalysisNodeKind.Quantifier or AnalysisNodeKind.Alternation
                    or AnalysisNodeKind.Modifier)
                {
                    foreach (var child in node.Children)
                        Visit(child, isRoot: false);
                }

                return;
            }

            foreach (var child in node.Children)
                Visit(child, isRoot: false);
        }

        Visit(root, isRoot: true);

        // Cap for usability on huge patterns
        if (list.Count > 80)
            return list.Take(80).ToList();
        return list;
    }

    private static string BuildNodeExplanation(AnalysisNode node, bool attempting, int subjectPos)
    {
        var frag = string.IsNullOrEmpty(node.PatternFragment)
            ? node.Title
            : Truncate(node.PatternFragment, 40);
        var detail = string.IsNullOrEmpty(node.Detail) ? "" : $" — {node.Detail}";
        var verb = attempting ? "Trying" : "Applying";
        return $"{verb} {node.Title} (`{frag}`){detail} at subject position {subjectPos}.";
    }

    private static (int Start, int Length, string Note) MapNodeToSubject(
        AnalysisNode node,
        MatchResult match,
        int cursor,
        string subject)
    {
        // Prefer real capture ranges for groups.
        if (node.Kind is AnalysisNodeKind.Group or AnalysisNodeKind.NamedGroup)
        {
            var name = ExtractGroupName(node);
            GroupResult? g = null;
            if (!string.IsNullOrEmpty(name))
                g = match.Groups.FirstOrDefault(x =>
                    x.Success && string.Equals(x.Name, name, StringComparison.Ordinal));
            g ??= match.Groups.FirstOrDefault(x => x.Success && x.Number > 0
                && x.Index >= match.Index && x.Index + x.Length <= match.Index + match.Length);

            if (g is not null && g.Success)
                return (g.Index, g.Length, $"Captured “{Truncate(g.Value, 40)}”.");
        }

        if (node.Kind is AnalysisNodeKind.Anchor)
            return (node.Title.Contains("Start", StringComparison.OrdinalIgnoreCase)
                ? match.Index
                : match.Index + match.Length, 0, "Zero-width assertion.");

        // Try to locate the fragment inside the remaining match text.
        var frag = node.PatternFragment ?? string.Empty;
        if (frag.Length > 0 && !IsMetaHeavy(frag) && match.Length > 0)
        {
            var matchText = match.Value;
            var localCursor = Math.Clamp(cursor - match.Index, 0, matchText.Length);
            var rel = matchText.IndexOf(frag, localCursor, StringComparison.Ordinal);
            if (rel < 0 && localCursor > 0)
                rel = matchText.IndexOf(frag, StringComparison.Ordinal);
            if (rel >= 0)
                return (match.Index + rel, frag.Length, string.Empty);
        }

        // Fall back: highlight from cursor within the match, small window.
        var remain = match.Index + match.Length - cursor;
        if (remain < 0)
        {
            return (match.Index, match.Length, "Within overall match span.");
        }

        var window = Math.Min(Math.Max(1, EstimateNodeConsume(node, subject, cursor)), Math.Max(1, remain));
        var start = Math.Clamp(cursor, match.Index, match.Index + match.Length);
        if (start + window > match.Index + match.Length)
            window = match.Index + match.Length - start;
        return (start, Math.Max(0, window), "Position estimated within the successful match.");
    }

    private static string? ExtractGroupName(AnalysisNode node)
    {
        // Title like "Named group 'user'" or PatternFragment (?<user>...)
        var frag = node.PatternFragment ?? string.Empty;
        var m = System.Text.RegularExpressions.Regex.Match(frag, @"\(\?<([A-Za-z_][A-Za-z0-9_]*)>");
        if (m.Success)
            return m.Groups[1].Value;
        m = System.Text.RegularExpressions.Regex.Match(node.Title, @"'([^']+)'");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static bool IsMetaHeavy(string frag) =>
        frag.IndexOfAny(new[] { '\\', '*', '+', '?', '[', '(', ')', '|', '.', '^', '$', '{' }) >= 0;

    private static int EstimateNodeConsume(AnalysisNode node, string subject, int pos)
    {
        if (node.Kind is AnalysisNodeKind.Anchor or AnalysisNodeKind.Lookaround)
            return 0;
        if (node.Kind == AnalysisNodeKind.Literal && !string.IsNullOrEmpty(node.PatternFragment))
            return node.PatternFragment.Length;
        if (pos >= 0 && pos < subject.Length)
            return 1;
        return 0;
    }

    private static string DescribeOptions(RegexOptionsEx options)
    {
        if (options == RegexOptionsEx.None)
            return "none";
        var parts = new List<string>();
        if (options.HasFlag(RegexOptionsEx.IgnoreCase)) parts.Add("i");
        if (options.HasFlag(RegexOptionsEx.Multiline)) parts.Add("m");
        if (options.HasFlag(RegexOptionsEx.Singleline)) parts.Add("s");
        if (options.HasFlag(RegexOptionsEx.ExplicitCapture)) parts.Add("n");
        if (options.HasFlag(RegexOptionsEx.IgnorePatternWhitespace)) parts.Add("x");
        return parts.Count == 0 ? "none" : string.Join(", ", parts);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
