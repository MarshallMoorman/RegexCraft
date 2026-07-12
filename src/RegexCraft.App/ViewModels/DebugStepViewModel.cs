using RegexCraft.Core.Debug;

namespace RegexCraft.App.ViewModels;

/// <summary>
/// UI wrapper for a single educational debug step.
/// </summary>
public sealed class DebugStepViewModel
{
    public DebugStepViewModel(DebugStep step)
    {
        Step = step;
        Index = step.Index;
        Kind = step.Kind;
        Explanation = step.Explanation;
        PatternStart = step.PatternStart;
        PatternLength = step.PatternLength;
        SubjectStart = step.SubjectStart;
        SubjectLength = step.SubjectLength;
        Success = step.Success;
        MatchIndex = step.MatchIndex;
        GroupNumber = step.GroupNumber;
        GroupName = step.GroupName;
        StatusLabel = step.StatusLabel;
        KindLabel = step.KindLabel;
        SummaryLine = $"[{step.Index + 1}] {step.KindLabel}: {Truncate(step.Explanation, 100)}";
    }

    public DebugStep Step { get; }
    public int Index { get; }
    public DebugStepKind Kind { get; }
    public string Explanation { get; }
    public int PatternStart { get; }
    public int PatternLength { get; }
    public int SubjectStart { get; }
    public int SubjectLength { get; }
    public bool? Success { get; }
    public int? MatchIndex { get; }
    public int? GroupNumber { get; }
    public string? GroupName { get; }
    public string StatusLabel { get; }
    public string KindLabel { get; }
    public string SummaryLine { get; }

    public bool HasPatternRange => PatternStart >= 0 && PatternLength >= 0;
    public bool HasSubjectRange => SubjectStart >= 0 && SubjectLength >= 0;

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
