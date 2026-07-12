using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Flavors;

/// <summary>
/// Ensures every selectable flavor has a complete, accurate definition.
/// </summary>
[TestFixture]
[Category("Flavors")]
public sealed class FlavorDefinitionCompletenessTests
{
    private static readonly string[] RequiredFlavorIds =
    [
        "dotnet", "pcre2", "javascript", "typescript", "php",
        "python", "java", "ruby", "go", "rust", "perl", "kotlin", "swift",
    ];

    private FlavorService _service = null!;
    private TokenCatalog _tokens = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new FlavorService(EngineFactory.CreateDefaultEngines());
        _tokens = new TokenCatalog();
    }

    [Test]
    public void AllRequiredFlavors_AreSelectable()
    {
        var ids = _service.GetFlavors().Select(f => f.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var id in RequiredFlavorIds)
            Assert.That(ids, Does.Contain(id), $"Missing flavor: {id}");
    }

    [Test]
    public void EveryFlavor_HasCompleteMetadata()
    {
        foreach (var f in _service.GetFlavors())
        {
            Assert.That(f.Id, Is.Not.Null.And.Not.Empty, "Id");
            Assert.That(f.DisplayName, Is.Not.Null.And.Not.Empty, f.Id);
            Assert.That(f.EngineId, Is.Not.Null.And.Not.Empty, f.Id);
            Assert.That(f.Description, Is.Not.Null.And.Not.Empty, f.Id);
            Assert.That(f.Notes, Is.Not.Null.And.Not.Empty, f.Id);
            Assert.That(f.CodegenLanguageId, Is.Not.Null.And.Not.Empty, f.Id);
            Assert.That(f.KnownDifferences, Is.Not.Null.And.Not.Empty, $"{f.Id} KnownDifferences");
            Assert.That(f.KnownDifferences.Count, Is.GreaterThanOrEqualTo(1), f.Id);
            Assert.That(f.SupportedOptions, Is.Not.EqualTo(RegexOptionsEx.None), f.Id);

            if (f.Fidelity != TestingFidelity.Full)
            {
                Assert.That(f.FidelityNote, Is.Not.Null.And.Not.Empty, f.Id);
                Assert.That(f.ShowFidelityBanner, Is.True, f.Id);
            }
        }
    }

    [Test]
    public void EveryFlavor_EngineIsRegistered()
    {
        foreach (var f in _service.GetFlavors())
        {
            var engine = _service.GetEngine(f.EngineId);
            Assert.That(engine, Is.Not.Null, f.Id);
            Assert.That(_service.GetEngineForFlavor(f.Id), Is.SameAs(engine));
        }
    }

    [Test]
    public void EveryFlavor_CodegenLanguageIsSupported()
    {
        var langs = new CodeGenerationService().SupportedLanguages
            .Select(l => l.Id())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var f in _service.GetFlavors())
            Assert.That(langs, Does.Contain(f.CodegenLanguageId), f.Id);
    }

    [Test]
    public void EveryFlavor_UnsupportedTokenIds_ExistInCatalog()
    {
        var catalogIds = _tokens.GetAllTokens().Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var f in _service.GetFlavors())
        {
            foreach (var tokenId in f.UnsupportedTokenIds)
            {
                Assert.That(catalogIds, Does.Contain(tokenId),
                    $"Flavor {f.Id} references unknown token id '{tokenId}'");
            }
        }
    }

    [Test]
    public void Fidelity_FullFlavors_AreDotNetAndPcre2()
    {
        var full = _service.GetFlavors().Where(f => f.Fidelity == TestingFidelity.Full).Select(f => f.Id).ToList();
        Assert.That(full, Is.EquivalentTo(new[] { "dotnet", "pcre2" }));
    }

    [Test]
    public void Fidelity_HighFlavors_AreJsTsPhp()
    {
        var high = _service.GetFlavors().Where(f => f.Fidelity == TestingFidelity.High).Select(f => f.Id).ToList();
        Assert.That(high, Is.EquivalentTo(new[] { "javascript", "typescript", "php" }));
    }

    [Test]
    public void Fidelity_ApproximateFlavors_IncludeExpectedSet()
    {
        var approx = _service.GetFlavors()
            .Where(f => f.Fidelity == TestingFidelity.Approximate)
            .Select(f => f.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var id in new[] { "python", "java", "ruby", "go", "rust", "perl", "kotlin", "swift" })
            Assert.That(approx, Does.Contain(id));
    }

    [Test]
    public void JavaScript_DoesNotSupportFreeSpacingOrExplicitCapture()
    {
        var js = _service.GetFlavor("javascript")!;
        Assert.That(js.SupportsOption(RegexOptionsEx.IgnoreCase), Is.True);
        Assert.That(js.SupportsOption(RegexOptionsEx.Multiline), Is.True);
        Assert.That(js.SupportsOption(RegexOptionsEx.Singleline), Is.True);
        Assert.That(js.SupportsOption(RegexOptionsEx.ExplicitCapture), Is.False);
        Assert.That(js.SupportsOption(RegexOptionsEx.IgnorePatternWhitespace), Is.False);
    }

    [Test]
    public void GoAndRust_MarkLookaroundAndBackrefsUnsupported()
    {
        foreach (var id in new[] { "go", "rust" })
        {
            var f = _service.GetFlavor(id)!;
            Assert.That(f.UnsupportedTokenIds, Does.Contain("pos-lookbehind"));
            Assert.That(f.UnsupportedTokenIds, Does.Contain("neg-lookbehind"));
            Assert.That(f.UnsupportedTokenIds, Does.Contain("pos-lookahead"));
            Assert.That(f.UnsupportedTokenIds, Does.Contain("ref1"));
            Assert.That(f.UnsupportedTokenIds, Does.Contain("ref-named"));
        }
    }

    [Test]
    public void DotNet_SupportsBalancingAndExplicitCaptureTokens()
    {
        var f = _service.GetFlavor("dotnet")!;
        var catalog = _tokens.GetAllTokens();
        var balancing = catalog.First(t => t.Id == "balancing");
        var explicitCap = catalog.First(t => t.Id == "explicit-capture");
        Assert.That(f.IsTokenSupported(balancing), Is.True);
        Assert.That(f.IsTokenSupported(explicitCap), Is.True);
    }

    [Test]
    public void FilterOptions_StripsUnsupportedFlags()
    {
        var js = _service.GetFlavor("javascript")!;
        var filtered = js.FilterOptions(
            RegexOptionsEx.IgnoreCase
            | RegexOptionsEx.ExplicitCapture
            | RegexOptionsEx.IgnorePatternWhitespace);

        Assert.That(filtered, Is.EqualTo(RegexOptionsEx.IgnoreCase));
    }

    [Test]
    public void BuildDefaultFlavors_CountMatchesSelectableWhenAllEnginesPresent()
    {
        Assert.That(FlavorService.BuildDefaultFlavors().Count, Is.EqualTo(13));
        Assert.That(_service.GetFlavors().Count, Is.EqualTo(13));
    }
}
