using RegexCraft.Core.Engines;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Engines;

[TestFixture]
[Category("Engines")]
[Category("Pcre")]
public sealed class PcreRegexEngineTests : EngineTestBase
{
    protected override IRegexEngine CreateEngine() => new PcreRegexEngine();

    [Test]
    public void Id_IsPcre2()
    {
        Assert.That(CreateEngine().Id, Is.EqualTo("pcre2"));
        Assert.That(CreateEngine().DisplayName, Is.EqualTo("PCRE2"));
    }

    [Test]
    public void MapOptions_CombinesFlags()
    {
        var mapped = PcreRegexEngine.MapOptions(
            RegexCraft.Core.Models.RegexOptionsEx.IgnoreCase
            | RegexCraft.Core.Models.RegexOptionsEx.Multiline
            | RegexCraft.Core.Models.RegexOptionsEx.Singleline
            | RegexCraft.Core.Models.RegexOptionsEx.ExplicitCapture
            | RegexCraft.Core.Models.RegexOptionsEx.IgnorePatternWhitespace);

        Assert.That(mapped.HasFlag(PCRE.PcreOptions.IgnoreCase), Is.True);
        Assert.That(mapped.HasFlag(PCRE.PcreOptions.MultiLine), Is.True);
        Assert.That(mapped.HasFlag(PCRE.PcreOptions.Singleline), Is.True);
        Assert.That(mapped.HasFlag(PCRE.PcreOptions.ExplicitCapture), Is.True);
        Assert.That(mapped.HasFlag(PCRE.PcreOptions.IgnorePatternWhitespace), Is.True);
    }
}
