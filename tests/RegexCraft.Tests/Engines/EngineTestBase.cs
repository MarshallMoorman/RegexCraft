using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Tests.Engines;

/// <summary>
/// Shared Match/Replace scenarios exercised against every engine.
/// </summary>
public abstract class EngineTestBase
{
    protected abstract IRegexEngine CreateEngine();

    [Test]
    [Category("Engines")]
    public void Match_SimplePattern_ReturnsMatches()
    {
        var engine = CreateEngine();
        var result = engine.Match(@"\d+", "a1 b22 c333", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(3));
        Assert.That(result.Matches[0].Value, Is.EqualTo("1"));
        Assert.That(result.Matches[1].Value, Is.EqualTo("22"));
        Assert.That(result.Matches[2].Value, Is.EqualTo("333"));
        Assert.That(result.EngineId, Is.EqualTo(engine.Id));
    }

    [Test]
    [Category("Engines")]
    public void Match_NamedGroups_ArePopulated()
    {
        var engine = CreateEngine();
        var result = engine.Match(
            @"(?<user>\w+)@(?<domain>\w+\.\w+)",
            "mail me at alice@example.com please",
            RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));

        var match = result.Matches[0];
        Assert.That(match.Value, Is.EqualTo("alice@example.com"));

        var user = match.Groups.FirstOrDefault(g => g.Name == "user");
        var domain = match.Groups.FirstOrDefault(g => g.Name == "domain");

        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Success, Is.True);
        Assert.That(user.Value, Is.EqualTo("alice"));

        Assert.That(domain, Is.Not.Null);
        Assert.That(domain!.Success, Is.True);
        Assert.That(domain.Value, Is.EqualTo("example.com"));
    }

    [Test]
    [Category("Engines")]
    public void Match_NumberedGroups_ArePopulated()
    {
        var engine = CreateEngine();
        var result = engine.Match(@"(\w+)-(\d+)", "item-42", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));

        var groups = result.Matches[0].Groups;
        Assert.That(groups, Has.Count.GreaterThanOrEqualTo(3)); // 0 + 2 captures
        Assert.That(groups[0].Value, Is.EqualTo("item-42"));
        Assert.That(groups[1].Value, Is.EqualTo("item"));
        Assert.That(groups[2].Value, Is.EqualTo("42"));
    }

    [Test]
    [Category("Engines")]
    public void Match_IgnoreCase_Works()
    {
        var engine = CreateEngine();
        var result = engine.Match("hello", "Hello HELLO hello", RegexOptionsEx.IgnoreCase);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(3));
    }

    [Test]
    [Category("Engines")]
    public void Match_Multiline_AnchorsPerLine()
    {
        var engine = CreateEngine();
        var subject = "one\ntwo\nthree";
        var result = engine.Match("^\\w+", subject, RegexOptionsEx.Multiline);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(3));
        Assert.That(result.Matches.Select(m => m.Value), Is.EqualTo(new[] { "one", "two", "three" }));
    }

    [Test]
    [Category("Engines")]
    public void Match_Singleline_DotMatchesNewline()
    {
        var engine = CreateEngine();
        var result = engine.Match("a.b", "a\nb", RegexOptionsEx.Singleline);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        Assert.That(result.Matches[0].Value, Is.EqualTo("a\nb"));
    }

    [Test]
    [Category("Engines")]
    public void Match_InvalidPattern_ReturnsError()
    {
        var engine = CreateEngine();
        var result = engine.Match("(", "abc", RegexOptionsEx.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Matches, Is.Empty);
    }

    [Test]
    [Category("Engines")]
    public void Match_EmptySubject_ReturnsNoMatches()
    {
        var engine = CreateEngine();
        var result = engine.Match(@"\w+", string.Empty, RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Is.Empty);
    }

    [Test]
    [Category("Engines")]
    public void Match_EmptyPattern_IsHandled()
    {
        var engine = CreateEngine();
        // Empty pattern is valid for both engines (matches empty strings).
        var result = engine.Match(string.Empty, "ab", RegexOptionsEx.None);

        // Either success with empty-string matches, or a graceful failure — never throw.
        Assert.That(result, Is.Not.Null);
        if (result.Success)
        {
            Assert.That(result.Matches.Count, Is.GreaterThanOrEqualTo(0));
        }
        else
        {
            Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
        }
    }

    [Test]
    [Category("Engines")]
    public void Match_Unicode_Works()
    {
        var engine = CreateEngine();
        var result = engine.Match(@"\p{L}+", "café 東京", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(result.Matches.Any(m => m.Value.Contains('é') || m.Value == "caf\u00e9" || m.Value.StartsWith("caf")), Is.True);
    }

    [Test]
    [Category("Engines")]
    public void Match_LargeInput_Completes()
    {
        var engine = CreateEngine();
        var subject = string.Concat(Enumerable.Repeat("word123 ", 10_000));
        var result = engine.Match(@"\d+", subject, RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(10_000));
    }

    [Test]
    [Category("Engines")]
    public void Match_ProvidesHighlightFriendlyRanges()
    {
        var engine = CreateEngine();
        var subject = "xxABCyy";
        var result = engine.Match("ABC", subject, RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        Assert.That(result.Matches[0].Index, Is.EqualTo(2));
        Assert.That(result.Matches[0].Length, Is.EqualTo(3));
        Assert.That(subject.Substring(result.Matches[0].Index, result.Matches[0].Length),
            Is.EqualTo(result.Matches[0].Value));
    }

    [Test]
    [Category("Engines")]
    public void Replace_Simple_ReplacesAll()
    {
        var engine = CreateEngine();
        var result = engine.Replace(@"\d+", "a1b2c3", "#", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("a#b#c#"));
        Assert.That(result.ReplacementCount, Is.EqualTo(3));
        Assert.That(result.EngineId, Is.EqualTo(engine.Id));
    }

    [Test]
    [Category("Engines")]
    public void Replace_WithGroupReference_Works()
    {
        var engine = CreateEngine();
        var result = engine.Replace(@"(\w+)@(\w+)", "a@b x@y", "[$1]", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("[a] [x]"));
        Assert.That(result.ReplacementCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Engines")]
    public void Replace_InvalidPattern_ReturnsError()
    {
        var engine = CreateEngine();
        var result = engine.Replace("(", "abc", "x", RegexOptionsEx.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    [Category("Engines")]
    public void Replace_NoMatches_ReturnsOriginal()
    {
        var engine = CreateEngine();
        var result = engine.Replace(@"\d+", "no digits here", "X", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("no digits here"));
        Assert.That(result.ReplacementCount, Is.EqualTo(0));
    }

    [Test]
    [Category("Engines")]
    public void Engine_Metadata_IsCorrect()
    {
        var engine = CreateEngine();
        Assert.That(engine.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(engine.DisplayName, Is.Not.Null.And.Not.Empty);
        Assert.That(engine.SupportsFullTesting, Is.True);
        Assert.That(engine.SupportsReplace, Is.True);
        Assert.That(engine.SupportsSplit, Is.True);
    }

    [Test]
    [Category("Engines")]
    public void Replace_ProvidesReplacementSpans()
    {
        var engine = CreateEngine();
        var result = engine.Replace(@"\d+", "a1b2", "#", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.ReplacementSpans, Has.Count.EqualTo(2));
        Assert.That(result.ReplacementSpans[0].Length, Is.EqualTo(1));
        // Spans should point into the result string
        foreach (var span in result.ReplacementSpans)
        {
            Assert.That(span.Index, Is.GreaterThanOrEqualTo(0));
            Assert.That(span.Index + span.Length, Is.LessThanOrEqualTo(result.Result.Length));
            Assert.That(result.Result.Substring(span.Index, span.Length), Is.EqualTo("#"));
        }
    }

    [Test]
    [Category("Engines")]
    public void Replace_NamedGroupReference_Works()
    {
        var engine = CreateEngine();
        var result = engine.Replace(
            @"(?<user>\w+)@(?<domain>\w+)",
            "a@b c@d",
            "[${user}]",
            RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("[a] [c]"));
        Assert.That(result.ReplacementCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Engines")]
    public void Split_Simple_Works()
    {
        var engine = CreateEngine();
        var result = engine.Split(@",\s*", "one, two, three", RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Parts, Is.EqualTo(new[] { "one", "two", "three" }));
        Assert.That(result.Delimiters, Has.Count.EqualTo(2));
        Assert.That(result.EngineId, Is.EqualTo(engine.Id));
    }

    [Test]
    [Category("Engines")]
    public void Split_RemoveEmptyEntries()
    {
        var engine = CreateEngine();
        var result = engine.Split(@",", "a,,b", RegexOptionsEx.None, removeEmptyEntries: true);

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Parts, Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    [Category("Engines")]
    public void Split_InvalidPattern_ReturnsError()
    {
        var engine = CreateEngine();
        var result = engine.Split("(", "abc", RegexOptionsEx.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }
}
