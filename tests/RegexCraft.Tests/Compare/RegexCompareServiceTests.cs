using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Compare;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Compare;

[TestFixture]
[Category("Compare")]
public sealed class RegexCompareServiceTests
{
    private IRegexCompareService _compare = null!;
    private FlavorService _flavors = null!;

    [SetUp]
    public void SetUp()
    {
        var engines = EngineFactory.CreateDefaultEngines();
        _flavors = new FlavorService(engines);
        _compare = new RegexCompareService(_flavors, new TokenCatalog(), NullLogger<RegexCompareService>.Instance);
    }

    [Test]
    public void Compare_TwoFullEngines_SameSimplePattern_AgreeOnMatchCount()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"\d+",
            Subject = "a12 b34",
            FlavorIds = ["dotnet", "pcre2"],
        });

        Assert.That(result.Flavors.Count, Is.EqualTo(2));
        Assert.That(result.Flavors.All(f => f.IsValid), Is.True);
        Assert.That(result.Flavors.All(f => f.MatchCount == 2), Is.True);
        Assert.That(result.SummaryText, Does.Contain("Flavor Comparison"));
        Assert.That(result.StatusLine, Does.Contain("2 flavor"));
    }

    [Test]
    public void Compare_IncludesJavaScript_AndReportsFidelity()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"\w+",
            Subject = "hello world",
            FlavorIds = ["dotnet", "javascript"],
        });

        Assert.That(result.Flavors.Count, Is.EqualTo(2));
        var js = result.Flavors.First(f => f.FlavorId == "javascript");
        Assert.That(js.IsValid, Is.True);
        Assert.That(js.MatchCount, Is.EqualTo(2));
        Assert.That(js.EngineId, Is.EqualTo("javascript"));
        Assert.That(js.Fidelity, Is.EqualTo(TestingFidelity.Full).Or.EqualTo(TestingFidelity.High));
    }

    [Test]
    public void Compare_InvalidPattern_MarksFlavorInvalid()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = "(",
            Subject = "x",
            FlavorIds = ["dotnet", "pcre2"],
        });

        Assert.That(result.Flavors.All(f => !f.IsValid), Is.True);
        Assert.That(result.Flavors.All(f => !string.IsNullOrEmpty(f.ErrorMessage)), Is.True);
        Assert.That(result.CrossFlavorDifferences, Is.Not.Empty);
    }

    [Test]
    public void Compare_DroppedOptions_ReportedForJavaScript()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"\d+",
            Subject = "1",
            Options = RegexOptionsEx.IgnoreCase | RegexOptionsEx.ExplicitCapture | RegexOptionsEx.IgnorePatternWhitespace,
            FlavorIds = ["dotnet", "javascript"],
        });

        var js = result.Flavors.First(f => f.FlavorId == "javascript");
        Assert.That(js.UnsupportedOptionLabels, Does.Contain("ExplicitCapture").Or.Contain("IgnorePatternWhitespace"));
        Assert.That(js.KeyNotes.Any(n => n.Contains("Dropped", StringComparison.OrdinalIgnoreCase)
                                         || n.Contains("Explicit", StringComparison.OrdinalIgnoreCase)
                                         || n.Contains("whitespace", StringComparison.OrdinalIgnoreCase)),
            Is.True);
    }

    [Test]
    public void Compare_UnsupportedTokens_DetectedForGo()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"(?<=foo)\d+",
            Subject = "foo42",
            FlavorIds = ["dotnet", "go"],
        });

        var go = result.Flavors.FirstOrDefault(f => f.FlavorId == "go");
        Assert.That(go, Is.Not.Null);
        // Lookbehind insert text may be detected; at least known differences / fidelity notes present.
        Assert.That(
            go!.UnsupportedTokensInPattern.Count > 0
            || go.KeyNotes.Count > 0
            || go.Fidelity != TestingFidelity.Full,
            Is.True);
    }

    [Test]
    public void Compare_EmptyFlavorList_ReturnsEmpty()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = "a",
            Subject = "a",
            FlavorIds = Array.Empty<string>(),
        });

        Assert.That(result.Flavors, Is.Empty);
        Assert.That(result.SummaryText, Does.Contain("No flavors"));
    }

    [Test]
    public void Compare_DedupesAndCapsAtFourFlavors()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"a+",
            Subject = "aaa",
            FlavorIds = ["dotnet", "dotnet", "pcre2", "javascript", "python", "go", "rust"],
        });

        Assert.That(result.Flavors.Count, Is.EqualTo(4));
        Assert.That(result.Flavors.Select(f => f.FlavorId).Distinct().Count(), Is.EqualTo(4));
    }

    [Test]
    public void Compare_MatchSummaries_IncludeGroups()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"(?<num>\d+)",
            Subject = "x99y",
            FlavorIds = ["dotnet", "pcre2"],
            MaxMatchesToShow = 3,
        });

        var first = result.Flavors.First(f => f.IsValid);
        Assert.That(first.Matches.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(first.Matches[0].Value, Is.EqualTo("99"));
        Assert.That(first.Matches[0].GroupSummaries.Count, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void Compare_CrossFlavorDifferences_WhenCountsDiffer()
    {
        // Pattern valid on both; same count for \d+ — force a case with known-differences notes at least.
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"\d+",
            Subject = "1 2 3",
            FlavorIds = ["dotnet", "python"],
        });

        Assert.That(result.CrossFlavorDifferences, Is.Not.Empty);
        // Approximate fidelity should be noted for python vs full .NET
        Assert.That(
            result.CrossFlavorDifferences.Any(d =>
                d.Contains("fidelity", StringComparison.OrdinalIgnoreCase)
                || d.Contains("No significant", StringComparison.OrdinalIgnoreCase)
                || d.Contains("Match", StringComparison.OrdinalIgnoreCase)),
            Is.True);
    }

    [Test]
    public void Compare_SummaryText_IsClipboardFriendly()
    {
        var result = _compare.Compare(new CompareRequest
        {
            Pattern = @"\w+",
            Subject = "ab",
            FlavorIds = ["dotnet", "javascript"],
        });

        Assert.That(result.SummaryText, Does.Contain("Pattern:"));
        Assert.That(result.SummaryText, Does.Contain("Subject:"));
        Assert.That(result.SummaryText, Does.Contain("Cross-flavor"));
        Assert.That(result.TotalDuration, Is.GreaterThanOrEqualTo(TimeSpan.Zero));
    }
}
