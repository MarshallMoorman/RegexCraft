namespace RegexCraft.Core.Compare;

/// <summary>
/// Compares the same pattern/subject across multiple regex flavors.
/// </summary>
public interface IRegexCompareService
{
    /// <summary>
    /// Runs match (and difference analysis) for each requested flavor.
    /// Engines may run in parallel.
    /// </summary>
    CompareResult Compare(CompareRequest request);
}
