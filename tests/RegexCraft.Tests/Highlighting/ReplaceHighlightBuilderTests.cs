using RegexCraft.Core.Highlighting;
using RegexCraft.Core.Models;
using RegexCraft.Engines.DotNet;

namespace RegexCraft.Tests.Highlighting;

[TestFixture]
[Category("Highlighting")]
public sealed class ReplaceHighlightBuilderTests
{
    [Test]
    public void Build_FromReplaceResult_CreatesSpans()
    {
        var engine = new DotNetRegexEngine();
        var result = engine.Replace(@"\d+", "a1b2", "#", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True);

        var spans = ReplaceHighlightBuilder.Build(result);
        Assert.That(spans, Has.Count.EqualTo(2));
        foreach (var s in spans)
        {
            Assert.That(s.Range.Start, Is.GreaterThanOrEqualTo(0));
            Assert.That(s.Range.Length, Is.GreaterThan(0));
        }
    }

    [Test]
    public void Build_NullOrFailed_ReturnsEmpty()
    {
        Assert.That(ReplaceHighlightBuilder.Build(null), Is.Empty);
        Assert.That(ReplaceHighlightBuilder.Build(ReplaceResult.Failed("dotnet", "err")), Is.Empty);
    }

    [Test]
    public void Build_NoReplacements_ReturnsEmpty()
    {
        var engine = new DotNetRegexEngine();
        var result = engine.Replace(@"\d+", "no digits", "#", RegexOptionsEx.None);
        var spans = ReplaceHighlightBuilder.Build(result);
        Assert.That(spans, Is.Empty);
    }
}
