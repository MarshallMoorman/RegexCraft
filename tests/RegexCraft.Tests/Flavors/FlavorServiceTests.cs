using RegexCraft.Core.Flavors;
using RegexCraft.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.JavaScript;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Tests.Flavors;

[TestFixture]
[Category("Core")]
[Category("Flavors")]
public sealed class FlavorServiceTests
{
    private FlavorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new FlavorService(EngineFactory.CreateDefaultEngines());
    }

    [Test]
    public void GetFlavors_IncludesCoreAndExpandedSet()
    {
        var flavors = _service.GetFlavors();
        Assert.That(flavors.Count, Is.GreaterThanOrEqualTo(10));
        var ids = flavors.Select(f => f.Id).ToList();
        Assert.That(ids, Does.Contain("dotnet"));
        Assert.That(ids, Does.Contain("pcre2"));
        Assert.That(ids, Does.Contain("javascript"));
        Assert.That(ids, Does.Contain("python"));
        Assert.That(ids, Does.Contain("java"));
        Assert.That(ids, Does.Contain("php"));
        Assert.That(ids, Does.Contain("typescript"));
        Assert.That(ids, Does.Contain("ruby"));
        Assert.That(ids, Does.Contain("go"));
        Assert.That(ids, Does.Contain("rust"));
    }

    [Test]
    public void GetFlavor_KnownId_ReturnsDefinition()
    {
        var flavor = _service.GetFlavor("dotnet");
        Assert.That(flavor, Is.Not.Null);
        Assert.That(flavor!.DisplayName, Is.EqualTo(".NET"));
        Assert.That(flavor.EngineId, Is.EqualTo("dotnet"));
        Assert.That(flavor.SupportsFullTesting, Is.True);
        Assert.That(flavor.Fidelity, Is.EqualTo(TestingFidelity.Full));
    }

    [Test]
    public void GetFlavor_Python_IsApproximate()
    {
        var flavor = _service.GetFlavor("python");
        Assert.That(flavor, Is.Not.Null);
        Assert.That(flavor!.Fidelity, Is.EqualTo(TestingFidelity.Approximate));
        Assert.That(flavor.ShowFidelityBanner, Is.True);
        Assert.That(flavor.FidelityNote, Does.Contain("Python").IgnoreCase);
        Assert.That(flavor.EngineId, Is.EqualTo("dotnet"));
    }

    [Test]
    public void GetFlavor_Php_UsesPcre2()
    {
        var flavor = _service.GetFlavor("php");
        Assert.That(flavor, Is.Not.Null);
        Assert.That(flavor!.EngineId, Is.EqualTo("pcre2"));
        Assert.That(flavor.Fidelity, Is.EqualTo(TestingFidelity.High));
    }

    [Test]
    public void GetFlavor_JavaScript_UsesJsEngine()
    {
        var flavor = _service.GetFlavor("javascript");
        Assert.That(flavor, Is.Not.Null);
        Assert.That(flavor!.EngineId, Is.EqualTo("javascript"));
        Assert.That(flavor.Fidelity, Is.EqualTo(TestingFidelity.High));
    }

    [Test]
    public void GetFlavor_UnknownId_ReturnsNull()
    {
        Assert.That(_service.GetFlavor("cobol-regex"), Is.Null);
    }

    [Test]
    public void GetEngineForFlavor_ResolvesCorrectEngine()
    {
        Assert.That(_service.GetEngineForFlavor("dotnet"), Is.InstanceOf<DotNetRegexEngine>());
        Assert.That(_service.GetEngineForFlavor("pcre2"), Is.InstanceOf<PcreRegexEngine>());
        Assert.That(_service.GetEngineForFlavor("javascript"), Is.InstanceOf<JavaScriptRegexEngine>());
        Assert.That(_service.GetEngineForFlavor("typescript"), Is.InstanceOf<JavaScriptRegexEngine>());
        Assert.That(_service.GetEngineForFlavor("php"), Is.InstanceOf<PcreRegexEngine>());
        Assert.That(_service.GetEngineForFlavor("python"), Is.InstanceOf<DotNetRegexEngine>());
    }

    [Test]
    public void GetEngineForFlavor_Unknown_ReturnsNull()
    {
        Assert.That(_service.GetEngineForFlavor("missing"), Is.Null);
    }

    [Test]
    public void GetEngine_ById_Works()
    {
        Assert.That(_service.GetEngine("dotnet"), Is.InstanceOf<DotNetRegexEngine>());
        Assert.That(_service.GetEngine("PCRE2"), Is.InstanceOf<PcreRegexEngine>());
        Assert.That(_service.GetEngine("javascript"), Is.InstanceOf<JavaScriptRegexEngine>());
    }

    [Test]
    public void GetEngines_ReturnsThree()
    {
        var engines = _service.GetEngines();
        Assert.That(engines, Has.Count.EqualTo(3));
        Assert.That(engines.Select(e => e.Id), Is.EquivalentTo(new[] { "dotnet", "pcre2", "javascript" }));
    }

    [Test]
    public void MissingEngine_HidesDependentFlavors()
    {
        // Without JS engine, javascript/typescript flavors should not appear.
        var custom = new FlavorService(
        [
            new DotNetRegexEngine(),
            new PcreRegexEngine(),
        ]);

        var ids = custom.GetFlavors().Select(f => f.Id).ToList();
        Assert.That(ids, Does.Contain("dotnet"));
        Assert.That(ids, Does.Contain("pcre2"));
        Assert.That(ids, Does.Not.Contain("javascript"));
        Assert.That(ids, Does.Not.Contain("typescript"));
        Assert.That(ids, Does.Contain("python")); // maps to dotnet
        Assert.That(ids, Does.Contain("php")); // maps to pcre2
    }

    [Test]
    public void BuildDefaultFlavors_AllHaveNotesOrFidelity()
    {
        foreach (var f in FlavorService.BuildDefaultFlavors())
        {
            Assert.That(f.Id, Is.Not.Empty);
            Assert.That(f.DisplayName, Is.Not.Empty);
            Assert.That(f.EngineId, Is.Not.Empty);
            if (f.Fidelity != TestingFidelity.Full)
                Assert.That(f.FidelityNote, Is.Not.Null.And.Not.Empty, f.Id);
        }
    }
}
