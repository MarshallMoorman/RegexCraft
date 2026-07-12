using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.JavaScript;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Flavors;

/// <summary>
/// Behavioral differences across real engines and RE2-style flavor limitations.
/// </summary>
[TestFixture]
[Category("Flavors")]
[Category("Engines")]
public sealed class FlavorBehavioralDifferenceTests
{
    private readonly DotNetRegexEngine _dotnet = new();
    private readonly PcreRegexEngine _pcre = new();
    private readonly JavaScriptRegexEngine _js = new();
    private FlavorService _flavors = null!;

    [SetUp]
    public void SetUp() => _flavors = new FlavorService(EngineFactory.CreateDefaultEngines());

    [Test]
    public void Pcre_SupportsPossessiveQuantifier_DotNetDoesNot()
    {
        // Possessive ++ is PCRE syntax; .NET rejects it.
        var pcre = _pcre.Match(@"a++b", "aaab", RegexOptionsEx.None);
        Assert.That(pcre.Success, Is.True, pcre.ErrorMessage);
        Assert.That(pcre.Matches, Has.Count.EqualTo(1));

        var dotnet = _dotnet.Match(@"a++b", "aaab", RegexOptionsEx.None);
        Assert.That(dotnet.Success, Is.False, "Expected .NET to reject possessive ++");
        Assert.That(dotnet.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Pcre_SupportsAtomicGroup()
    {
        var result = _pcre.Match(@"(?>a+)b", "aaab", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches[0].Value, Is.EqualTo("aaab"));
    }

    [Test]
    public void DotNet_SupportsBalancingGroups_PcreDoesNot()
    {
        // .NET balancing group: (?<close-open>…) pops 'open' and pushes 'close'
        var nest = _dotnet.Match(@"(?<open>\()[^)]*(?<close-open>\))", "(hi)", RegexOptionsEx.None);
        Assert.That(nest.Success, Is.True, nest.ErrorMessage);
        Assert.That(nest.Matches, Has.Count.EqualTo(1));

        var pcre = _pcre.Match(@"(?<open>\()[^)]*(?<close-open>\))", "(hi)", RegexOptionsEx.None);
        Assert.That(pcre.Success, Is.False, "PCRE should not accept .NET balancing group syntax");
        Assert.That(pcre.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void JavaScript_SupportsLookbehind_AndNamedGroups()
    {
        var lb = _js.Match(@"(?<=\$)\d+", "price $42", RegexOptionsEx.None);
        Assert.That(lb.Success, Is.True, lb.ErrorMessage);
        Assert.That(lb.Matches[0].Value, Is.EqualTo("42"));

        var named = _js.Match(@"(?<num>\d+)", "n=7", RegexOptionsEx.None);
        Assert.That(named.Success, Is.True, named.ErrorMessage);
        Assert.That(named.Matches[0].Groups.Any(g => g.Name == "num" && g.Value == "7"), Is.True);
    }

    [Test]
    public void JavaScript_RejectsPossessive()
    {
        var result = _js.Match(@"a++b", "aaab", RegexOptionsEx.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void AllEngines_AgreeOnSimpleWordMatch()
    {
        foreach (var engine in new Core.Engines.IRegexEngine[] { _dotnet, _pcre, _js })
        {
            var r = engine.Match(@"\b\w+\b", "one two", RegexOptionsEx.None);
            Assert.That(r.Success, Is.True, engine.Id);
            Assert.That(r.Matches.Select(m => m.Value), Is.EqualTo(new[] { "one", "two" }));
        }
    }

    [Test]
    public void AllEngines_AgreeOnBackreference()
    {
        // Backrefs work on all three real engines (not on real RE2).
        foreach (var engine in new Core.Engines.IRegexEngine[] { _dotnet, _pcre, _js })
        {
            var r = engine.Match(@"(\w+)\s+\1", "hello hello", RegexOptionsEx.None);
            Assert.That(r.Success, Is.True, $"{engine.Id}: {r.ErrorMessage}");
            Assert.That(r.Matches, Has.Count.EqualTo(1));
            Assert.That(r.Matches[0].Value, Is.EqualTo("hello hello"));
        }
    }

    [Test]
    public void GoFlavor_DocumentsRe2Limitations()
    {
        var go = _flavors.GetFlavor("go")!;
        Assert.That(go.Fidelity, Is.EqualTo(TestingFidelity.Approximate));
        Assert.That(go.FidelityNote, Does.Contain("RE2").IgnoreCase);
        Assert.That(go.KnownDifferences.Any(d =>
            d.Contains("backref", StringComparison.OrdinalIgnoreCase)
            || d.Contains("lookaround", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(go.UnsupportedTokenIds, Does.Contain("ref1"));
        Assert.That(go.UnsupportedTokenIds, Does.Contain("pos-lookbehind"));
    }

    [Test]
    public void RustFlavor_DocumentsRe2Limitations()
    {
        var rust = _flavors.GetFlavor("rust")!;
        Assert.That(rust.FidelityNote, Does.Contain("lookaround").Or.Contain("backref").IgnoreCase);
        Assert.That(rust.UnsupportedTokenIds, Does.Contain("ref-named"));
    }

    [Test]
    public void Php_UsesPcre2_HighFidelity()
    {
        var php = _flavors.GetFlavor("php")!;
        Assert.That(php.EngineId, Is.EqualTo("pcre2"));
        Assert.That(php.Fidelity, Is.EqualTo(TestingFidelity.High));

        var engine = _flavors.GetEngineForFlavor("php")!;
        // Possessive works via PCRE2 for PHP flavor
        var r = engine.Match(@"\d++", "123", RegexOptionsEx.None);
        Assert.That(r.Success, Is.True, r.ErrorMessage);
    }

    [Test]
    public void PythonFlavor_NotesDotNetApproximation()
    {
        var py = _flavors.GetFlavor("python")!;
        Assert.That(py.EngineId, Is.EqualTo("dotnet"));
        Assert.That(py.FidelityNote, Does.Contain(".NET").Or.Contain("closest").IgnoreCase);
        Assert.That(py.KnownDifferences.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void DotNet_ExplicitCapture_HidesNumberedGroups()
    {
        var with = _dotnet.Match(@"(?<n>\w+)-(\d+)", "a-1", RegexOptionsEx.ExplicitCapture);
        Assert.That(with.Success, Is.True, with.ErrorMessage);
        var match = with.Matches[0];
        Assert.That(match.Groups.Any(g => g.Name == "n" && g.Value == "a"), Is.True);
        // Numbered capture for digits should not succeed as a separate capture under ExplicitCapture
        var numberedDigit = match.Groups.Where(g => g.Number > 0 && g.Name != "n").ToList();
        Assert.That(numberedDigit.All(g => !g.Success || g.Name == "n"), Is.True);
    }
}
