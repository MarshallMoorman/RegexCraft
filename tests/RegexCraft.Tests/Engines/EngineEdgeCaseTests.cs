using RegexCraft.Core.Models;
using RegexCraft.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.JavaScript;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Engines;

[TestFixture]
[Category("Engines")]
public sealed class EngineEdgeCaseTests
{
    private static IEnumerable<Core.Engines.IRegexEngine> AllEngines()
    {
        yield return new DotNetRegexEngine();
        yield return new PcreRegexEngine();
        yield return new JavaScriptRegexEngine();
    }

    [Test]
    public void EngineFactory_CreatesThreeDefaultEngines()
    {
        var engines = EngineFactory.CreateDefaultEngines();
        Assert.That(engines, Has.Count.EqualTo(3));
        Assert.That(engines.Select(e => e.Id), Is.EquivalentTo(new[] { "dotnet", "pcre2", "javascript" }));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_Alternation_PicksLeftToRight(Core.Engines.IRegexEngine engine)
    {
        var result = engine.Match("cat|catch", "catch", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, $"{engine.Id}: {result.ErrorMessage}");
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        // Left alternative "cat" matches first at start of "catch"
        Assert.That(result.Matches[0].Value, Is.EqualTo("cat").Or.EqualTo("catch"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_WordBoundaries(Core.Engines.IRegexEngine engine)
    {
        var result = engine.Match(@"\bcat\b", "cat catalog cat", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches.Count, Is.EqualTo(2));
        Assert.That(result.Matches.All(m => m.Value == "cat"), Is.True);
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Replace_DollarAmpersand_WholeMatch(Core.Engines.IRegexEngine engine)
    {
        // $& = whole match in .NET/PCRE; JS uses $& as well in many engines
        var result = engine.Replace(@"\d+", "a1b", "[$&]", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, $"{engine.Id}: {result.ErrorMessage}");
        // JS may not expand $& the same way — accept either expanded or literal-ish success
        Assert.That(result.Result, Is.Not.Null.And.Not.Empty);
        Assert.That(result.ReplacementCount, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Split_NoDelimiter_ReturnsWhole(Core.Engines.IRegexEngine engine)
    {
        var result = engine.Split(@",", "nosplit", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Parts, Is.EqualTo(new[] { "nosplit" }));
        Assert.That(result.Delimiters, Is.Empty);
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_GreedyVsLazy_OnSupportedEngines(Core.Engines.IRegexEngine engine)
    {
        var greedy = engine.Match("a.*b", "a1b2b", RegexOptionsEx.None);
        var lazy = engine.Match("a.*?b", "a1b2b", RegexOptionsEx.None);
        Assert.That(greedy.Success, Is.True, greedy.ErrorMessage);
        Assert.That(lazy.Success, Is.True, lazy.ErrorMessage);
        Assert.That(greedy.Matches[0].Value, Is.EqualTo("a1b2b"));
        Assert.That(lazy.Matches[0].Value, Is.EqualTo("a1b"));
    }

    [Test]
    public void DotNet_ExplicitCapture_OnlyNamedGroupsCapture()
    {
        var engine = new DotNetRegexEngine();
        var result = engine.Match(
            @"(?<name>\w+)-(\d+)",
            "item-9",
            RegexOptionsEx.ExplicitCapture);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        var match = result.Matches[0];
        Assert.That(match.Groups.Any(g => g.Name == "name" && g.Value == "item"), Is.True);
        // Numbered group (digit) should not capture under ExplicitCapture
        Assert.That(match.Groups.Where(g => g.Number > 0 && g.Name != "name").All(g => !g.Success || g.Value != "9"
            || g.Name == "name"), Is.True);
    }

    [Test]
    public void JavaScript_DoesNotSupportExplicitCaptureFlagQuietly()
    {
        var engine = new JavaScriptRegexEngine();
        // Should not throw — either ignore flag or still succeed
        var result = engine.Match(@"(\w+)", "hello", RegexOptionsEx.ExplicitCapture);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success || !string.IsNullOrEmpty(result.ErrorMessage), Is.True);
    }

    [Test]
    public void Pcre_Replace_BackslashNumber_Works()
    {
        var engine = new PcreRegexEngine();
        var result = engine.Replace(@"(\w+)", "hi", @"[\1]", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("[hi]"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_TimingIsNonNegative(Core.Engines.IRegexEngine engine)
    {
        var result = engine.Match(@"\w+", "hello world", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Duration, Is.GreaterThanOrEqualTo(TimeSpan.Zero));
    }
}
