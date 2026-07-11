using RegexCraft.Core.Highlighting;
using RegexCraft.Core.Models;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Highlighting;

[TestFixture]
[Category("Highlighting")]
public sealed class MatchHighlightBuilderTests
{
    [Test]
    public void Build_EmptyOrFailed_ReturnsEmpty()
    {
        Assert.That(MatchHighlightBuilder.Build(null), Is.Empty);
        Assert.That(MatchHighlightBuilder.Build(MatchCollectionResult.Failed("dotnet", "x")), Is.Empty);
        Assert.That(MatchHighlightBuilder.Build(MatchCollectionResult.FromMatches("dotnet", [], TimeSpan.Zero)), Is.Empty);
    }

    [Test]
    public void Build_IncludesMatchAndGroups()
    {
        var engine = new DotNetRegexEngine();
        var result = engine.Match(@"(?<a>\w+)-(\d+)", "item-42", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True);

        var spans = MatchHighlightBuilder.Build(result);
        Assert.That(spans.Any(s => s.Kind == HighlightKind.Match && s.GroupNumber == 0), Is.True);
        Assert.That(spans.Any(s => s.GroupNumber == 1), Is.True);
        Assert.That(spans.Any(s => s.GroupNumber == 2), Is.True);

        var matchSpan = spans.First(s => s.GroupNumber == 0);
        Assert.That(matchSpan.Range.Start, Is.EqualTo(0));
        Assert.That(matchSpan.Range.Length, Is.EqualTo("item-42".Length));
    }

    [Test]
    public void Build_WorksForBothEngines()
    {
        const string pattern = @"(\w+)@(\w+)";
        const string subject = "a@b x@y";

        foreach (var engine in new Core.Engines.IRegexEngine[] { new DotNetRegexEngine(), new PcreRegexEngine() })
        {
            var result = engine.Match(pattern, subject, RegexOptionsEx.None);
            Assert.That(result.Success, Is.True, result.ErrorMessage);
            var spans = MatchHighlightBuilder.Build(result);
            Assert.That(spans.Count(s => s.Kind == HighlightKind.Match), Is.EqualTo(2), engine.Id);
            Assert.That(spans.Any(s => s.GroupNumber > 0), Is.True, engine.Id);
        }
    }

    [Test]
    public void KindForGroup_Cycles()
    {
        Assert.That(MatchHighlightBuilder.KindForGroup(1), Is.EqualTo(HighlightKind.Group0));
        Assert.That(MatchHighlightBuilder.KindForGroup(2), Is.EqualTo(HighlightKind.Group1));
        Assert.That(MatchHighlightBuilder.KindForGroup(3), Is.EqualTo(HighlightKind.Group2));
        Assert.That(MatchHighlightBuilder.KindForGroup(4), Is.EqualTo(HighlightKind.Group3));
        Assert.That(MatchHighlightBuilder.KindForGroup(5), Is.EqualTo(HighlightKind.Group0));
    }

    [Test]
    public void ForLine_FiltersOverlaps()
    {
        var spans = new[]
        {
            new HighlightSpan { Range = new TextRange(0, 5), Kind = HighlightKind.Match, MatchIndex = 0 },
            new HighlightSpan { Range = new TextRange(10, 3), Kind = HighlightKind.Match, MatchIndex = 1 },
        };
        var line = MatchHighlightBuilder.ForLine(spans, lineStart: 0, lineEnd: 8).ToList();
        Assert.That(line, Has.Count.EqualTo(1));
        Assert.That(line[0].Range.Start, Is.EqualTo(0));
    }

    [Test]
    public void Build_ExcludeGroups_OnlyFullMatches()
    {
        var engine = new DotNetRegexEngine();
        var result = engine.Match(@"(a)(b)", "ab", RegexOptionsEx.None);
        var spans = MatchHighlightBuilder.Build(result, includeGroups: false);
        Assert.That(spans.All(s => s.GroupNumber == 0), Is.True);
    }
}
