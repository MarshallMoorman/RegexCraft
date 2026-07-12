using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Flavors;

[TestFixture]
[Category("Flavors")]
[Category("Tokens")]
public sealed class FlavorTokenAndOptionTests
{
    private FlavorService _service = null!;
    private TokenCatalog _catalog = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new FlavorService(EngineFactory.CreateDefaultEngines());
        _catalog = new TokenCatalog();
    }

    private static IEnumerable<string> AllFlavorIds() =>
        FlavorService.BuildDefaultFlavors().Select(f => f.Id);

    [TestCaseSource(nameof(AllFlavorIds))]
    public void CommonTokens_SupportedOnAllFlavors(string flavorId)
    {
        var flavor = _service.GetFlavor(flavorId)!;
        foreach (var id in new[] { "digit", "word", "capture", "zero-or-more", "start", "end" })
        {
            var token = _catalog.GetAllTokens().First(t => t.Id == id);
            Assert.That(flavor.IsTokenSupported(token), Is.True, $"{flavorId}/{id}");
        }
    }

    [Test]
    public void Go_DisablesLookaroundAndBackrefTokens()
    {
        var go = _service.GetFlavor("go")!;
        foreach (var id in new[] { "pos-lookahead", "pos-lookbehind", "ref1", "ref-named", "possessive-plus" })
        {
            var token = _catalog.GetAllTokens().First(t => t.Id == id);
            Assert.That(go.IsTokenSupported(token), Is.False, id);
        }
    }

    [Test]
    public void JavaScript_DisablesPossessiveAndFreeSpacingTokens()
    {
        var js = _service.GetFlavor("javascript")!;
        foreach (var id in new[] { "possessive-plus", "atomic", "balancing", "ignore-ws", "start-abs" })
        {
            var token = _catalog.GetAllTokens().First(t => t.Id == id);
            Assert.That(js.IsTokenSupported(token), Is.False, id);
        }

        // Lookahead is available in modern JS
        var la = _catalog.GetAllTokens().First(t => t.Id == "pos-lookahead");
        Assert.That(js.IsTokenSupported(la), Is.True);
    }

    [Test]
    public void Pcre2_DisablesDotNetOnlyTokens_SupportsPossessive()
    {
        var pcre = _service.GetFlavor("pcre2")!;
        var balancing = _catalog.GetAllTokens().First(t => t.Id == "balancing");
        var possessive = _catalog.GetAllTokens().First(t => t.Id == "possessive-plus");
        Assert.That(pcre.IsTokenSupported(balancing), Is.False);
        Assert.That(pcre.IsTokenSupported(possessive), Is.True);
    }

    [Test]
    public void Python_DisablesBalancingAndPossessive()
    {
        var py = _service.GetFlavor("python")!;
        Assert.That(py.IsTokenSupported(_catalog.GetAllTokens().First(t => t.Id == "balancing")), Is.False);
        Assert.That(py.IsTokenSupported(_catalog.GetAllTokens().First(t => t.Id == "possessive-plus")), Is.False);
        Assert.That(py.SupportsOption(RegexOptionsEx.ExplicitCapture), Is.False);
        Assert.That(py.SupportsOption(RegexOptionsEx.IgnorePatternWhitespace), Is.True);
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void AtLeastHalfOfCatalog_IsSupported(string flavorId)
    {
        var flavor = _service.GetFlavor(flavorId)!;
        var all = _catalog.GetAllTokens();
        var supported = all.Count(flavor.IsTokenSupported);
        Assert.That(supported, Is.GreaterThan(all.Count / 2),
            $"{flavorId}: only {supported}/{all.Count} tokens supported");
    }

    [Test]
    public void TokenCatalog_EngineSupport_PcreOnlyTokens()
    {
        var hSpace = _catalog.GetAllTokens().First(t => t.Id == "h-space");
        Assert.That(hSpace.IsSupportedBy("pcre2"), Is.True);
        Assert.That(hSpace.IsSupportedBy("dotnet"), Is.False);
        Assert.That(hSpace.IsSupportedBy("javascript"), Is.False);
    }

    [Test]
    public void TokenCatalog_EngineSupport_BalancingIsDotNetOnly()
    {
        var bal = _catalog.GetAllTokens().First(t => t.Id == "balancing");
        Assert.That(bal.IsSupportedBy("dotnet"), Is.True);
        Assert.That(bal.IsSupportedBy("pcre2"), Is.False);
        Assert.That(bal.IsSupportedBy("javascript"), Is.False);
    }
}
