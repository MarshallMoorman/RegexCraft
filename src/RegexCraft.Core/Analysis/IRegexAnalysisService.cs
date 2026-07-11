namespace RegexCraft.Core.Analysis;

public interface IRegexAnalysisService
{
    /// <summary>
    /// Parses <paramref name="pattern"/> into a hierarchical explanation tree.
    /// Incomplete/invalid patterns return a partial tree with error nodes rather than throwing.
    /// </summary>
    AnalysisNode Analyze(string? pattern);
}
