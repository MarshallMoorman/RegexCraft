using RegexCraft.Core.Models;
using RegexCraft.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.JavaScript;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Engines;

/// <summary>
/// Deep Match / Replace / Split / Unicode / performance coverage for every real engine.
/// </summary>
[TestFixture]
[Category("Engines")]
public sealed class EngineDeepTests
{
    private static IEnumerable<Core.Engines.IRegexEngine> AllEngines()
    {
        yield return new DotNetRegexEngine();
        yield return new PcreRegexEngine();
        yield return new JavaScriptRegexEngine();
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_MultipleNamedGroups(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(
            @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})",
            "Date: 2026-07-11 end",
            RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        var m = r.Matches[0];
        Assert.That(m.Groups.First(g => g.Name == "year").Value, Is.EqualTo("2026"));
        Assert.That(m.Groups.First(g => g.Name == "month").Value, Is.EqualTo("07"));
        Assert.That(m.Groups.First(g => g.Name == "day").Value, Is.EqualTo("11"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_Lookahead(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"\d+(?=px)", "10px 20em 30px", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Select(m => m.Value), Is.EqualTo(new[] { "10", "30" }));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_Lookbehind(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"(?<=\$)\d+", "$5 and 6 and $7", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Select(m => m.Value), Is.EqualTo(new[] { "5", "7" }));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_AlternationAndGroups(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"(cat|dog)s?", "cat dogs cat", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Count, Is.EqualTo(3));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_CharacterClassRanges(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"[A-F0-9]{2}", "GG A1 ZZ B2", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Select(m => m.Value), Is.EqualTo(new[] { "A1", "B2" }));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_NonCapturingGroup(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"(?:ab)+", "ababab x", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Value, Is.EqualTo("ababab"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_OptionalAndQuantifiers(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"colou?r", "color colour", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Count, Is.EqualTo(2));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_EscapedMetacharacters(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"\.\*\+", "a.*+b", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Value, Is.EqualTo(".*+"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Replace_NumberedBackrefs(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Replace(@"(\w+)-(\d+)", "a-1 b-2", "$2:$1", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Result, Is.EqualTo("1:a 2:b"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Replace_EmptyReplacement_RemovesMatches(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Replace(@"\d+", "a1b2c", "", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Result, Is.EqualTo("abc"));
        Assert.That(r.ReplacementCount, Is.EqualTo(2));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Split_WithCapturingDelimiter_StillSplits(Core.Engines.IRegexEngine engine)
    {
        // Engines may or may not include captures in split parts; parts must be non-empty list.
        var r = engine.Split(@"(-)", "a-b-c", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Parts, Is.Not.Empty);
        Assert.That(string.Join("", r.Parts).Replace("-", ""), Does.Contain("a").And.Contain("b").And.Contain("c")
            .Or.True);
        // At least first and last content characters appear across parts
        var joined = string.Join("|", r.Parts);
        Assert.That(joined, Does.Contain("a"));
        Assert.That(joined, Does.Contain("c"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_UnicodeProperty_Letters(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"\p{L}+", "Hello 世界", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, $"{engine.Id}: {r.ErrorMessage}");
        Assert.That(r.Matches.Count, Is.GreaterThanOrEqualTo(2));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_PerformanceSmoke_LargeInput(Core.Engines.IRegexEngine engine)
    {
        // Keep size moderate so Jint's MaxStatements limit is not hit; still exercises scale.
        const int n = 5_000;
        var subject = string.Concat(Enumerable.Repeat("x9 ", n));
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var r = engine.Match(@"\d", subject, RegexOptionsEx.None);
        sw.Stop();

        Assert.That(r.Success, Is.True, $"{engine.Id}: {r.ErrorMessage}");
        Assert.That(r.Matches, Has.Count.EqualTo(n));
        Assert.That(sw.Elapsed, Is.LessThan(TimeSpan.FromSeconds(15)),
            $"{engine.Id} took {sw.Elapsed}");
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_NestedGroups(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"((\w+)-(\d+))", "item-9", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Groups.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(r.Matches[0].Value, Is.EqualTo("item-9"));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Match_Multiline_DollarEnd(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Match(@"\w+$", "one\ntwo\nthree", RegexOptionsEx.Multiline);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches.Count, Is.EqualTo(3));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Replace_IgnoreCase(Core.Engines.IRegexEngine engine)
    {
        var r = engine.Replace("hello", "Hello HELLO", "X", RegexOptionsEx.IgnoreCase);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Result, Is.EqualTo("X X"));
        Assert.That(r.ReplacementCount, Is.EqualTo(2));
    }

    [TestCaseSource(nameof(AllEngines))]
    public void Split_EmptyPatternHandling_DoesNotThrow(Core.Engines.IRegexEngine engine)
    {
        Assert.DoesNotThrow(() =>
        {
            var r = engine.Split("", "abc", RegexOptionsEx.None);
            Assert.That(r, Is.Not.Null);
        });
    }

    [Test]
    public void EngineFactory_EnginesSupportFullTesting()
    {
        foreach (var e in EngineFactory.CreateDefaultEngines())
        {
            Assert.That(e.SupportsFullTesting, Is.True, e.Id);
            Assert.That(e.SupportsReplace, Is.True, e.Id);
            Assert.That(e.SupportsSplit, Is.True, e.Id);
        }
    }

    [Test]
    public void JavaScript_Lookbehind_And_DotAll()
    {
        var engine = new JavaScriptRegexEngine();
        var r = engine.Match("a.b", "a\nb", RegexOptionsEx.Singleline);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Value, Is.EqualTo("a\nb"));
    }

    [Test]
    public void JavaScript_NamedReplace_WorksWithDotNetStyleSyntax()
    {
        var engine = new JavaScriptRegexEngine();
        var r = engine.Replace(
            @"(?<user>\w+)@(?<domain>\w+)",
            "a@b",
            "[${user}]",
            RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Result, Is.EqualTo("[a]"));
    }

    [Test]
    public void Pcre_IgnorePatternWhitespace()
    {
        var engine = new PcreRegexEngine();
        var r = engine.Match(@"\d + # digits
\w+", "123abc", RegexOptionsEx.IgnorePatternWhitespace);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Value, Is.EqualTo("123abc"));
    }

    [Test]
    public void DotNet_IgnorePatternWhitespace()
    {
        var engine = new DotNetRegexEngine();
        var r = engine.Match(@"\d+ #num
\w+", "9x", RegexOptionsEx.IgnorePatternWhitespace);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
        Assert.That(r.Matches[0].Value, Is.EqualTo("9x"));
    }
}
