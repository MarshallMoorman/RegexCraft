using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;
using RegexCraft.Engines.JavaScript;

namespace RegexCraft.Tests.Engines;

[TestFixture]
[Category("Engines")]
public sealed class JavaScriptRegexEngineTests : EngineTestBase
{
    protected override IRegexEngine CreateEngine() => new JavaScriptRegexEngine();

    [Test]
    public void Engine_Metadata()
    {
        var engine = CreateEngine();
        Assert.That(engine.Id, Is.EqualTo("javascript"));
        Assert.That(engine.SupportsFullTesting, Is.True);
        Assert.That(engine.SupportsReplace, Is.True);
        Assert.That(engine.SupportsSplit, Is.True);
    }

    [Test]
    public void Replace_Backreference()
    {
        var engine = CreateEngine();
        var result = engine.Replace(@"(\w+)", "hi there", "[$1]", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Result, Is.EqualTo("[hi] [there]"));
    }

    [Test]
    public void Split_CommaSeparated()
    {
        var engine = CreateEngine();
        var result = engine.Split(@",\s*", "a, b, c", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Parts, Is.EqualTo(new[] { "a", "b", "c" }));
    }
}
