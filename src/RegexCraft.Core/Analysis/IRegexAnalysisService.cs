namespace RegexCraft.Core.Analysis;

public interface IRegexAnalysisService
{
    AnalysisNode Analyze(string? pattern);
}
