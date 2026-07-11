using RegexCraft.Core.Engines;
using RegexCraft.Engines.DotNet;

namespace RegexCraft.Tests.Engines;

[TestFixture]
[Category("Engines")]
[Category("DotNet")]
public sealed class DotNetRegexEngineTests : EngineTestBase
{
    protected override IRegexEngine CreateEngine() => new DotNetRegexEngine();

    [Test]
    public void Id_IsDotnet()
    {
        Assert.That(CreateEngine().Id, Is.EqualTo("dotnet"));
        Assert.That(CreateEngine().DisplayName, Is.EqualTo(".NET"));
    }

    [Test]
    public void MapOptions_CombinesFlags()
    {
        var mapped = DotNetRegexEngine.MapOptions(
            RegexCraft.Core.Models.RegexOptionsEx.IgnoreCase
            | RegexCraft.Core.Models.RegexOptionsEx.Multiline
            | RegexCraft.Core.Models.RegexOptionsEx.Singleline
            | RegexCraft.Core.Models.RegexOptionsEx.ExplicitCapture
            | RegexCraft.Core.Models.RegexOptionsEx.IgnorePatternWhitespace);

        Assert.That(mapped.HasFlag(System.Text.RegularExpressions.RegexOptions.IgnoreCase), Is.True);
        Assert.That(mapped.HasFlag(System.Text.RegularExpressions.RegexOptions.Multiline), Is.True);
        Assert.That(mapped.HasFlag(System.Text.RegularExpressions.RegexOptions.Singleline), Is.True);
        Assert.That(mapped.HasFlag(System.Text.RegularExpressions.RegexOptions.ExplicitCapture), Is.True);
        Assert.That(mapped.HasFlag(System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace), Is.True);
    }
}
