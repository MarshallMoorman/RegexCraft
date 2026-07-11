using RegexCraft.Core.Flavors;
using RegexCraft.Engines;
using RegexCraft.Engines.DotNet;
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
    public void GetFlavors_ReturnsDotNetAndPcre2()
    {
        var flavors = _service.GetFlavors();
        Assert.That(flavors, Has.Count.EqualTo(2));
        Assert.That(flavors.Select(f => f.Id), Is.EquivalentTo(new[] { "dotnet", "pcre2" }));
    }

    [Test]
    public void GetFlavor_KnownId_ReturnsDefinition()
    {
        var flavor = _service.GetFlavor("dotnet");
        Assert.That(flavor, Is.Not.Null);
        Assert.That(flavor!.DisplayName, Is.EqualTo(".NET"));
        Assert.That(flavor.EngineId, Is.EqualTo("dotnet"));
        Assert.That(flavor.SupportsFullTesting, Is.True);
    }

    [Test]
    public void GetFlavor_UnknownId_ReturnsNull()
    {
        Assert.That(_service.GetFlavor("python"), Is.Null);
    }

    [Test]
    public void GetEngineForFlavor_ResolvesCorrectEngine()
    {
        var dotnet = _service.GetEngineForFlavor("dotnet");
        var pcre = _service.GetEngineForFlavor("pcre2");

        Assert.That(dotnet, Is.Not.Null);
        Assert.That(dotnet, Is.InstanceOf<DotNetRegexEngine>());
        Assert.That(pcre, Is.Not.Null);
        Assert.That(pcre, Is.InstanceOf<PcreRegexEngine>());
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
        Assert.That(_service.GetEngine("PCRE2"), Is.InstanceOf<PcreRegexEngine>()); // case-insensitive
    }

    [Test]
    public void GetEngines_ReturnsBoth()
    {
        var engines = _service.GetEngines();
        Assert.That(engines, Has.Count.EqualTo(2));
        Assert.That(engines.Select(e => e.Id), Is.EquivalentTo(new[] { "dotnet", "pcre2" }));
    }

    [Test]
    public void AddingFlavorOnlyRequiresDefinitionAndEngine()
    {
        // Demonstrates the Phase 0 design: new engines register via the constructor list.
        var custom = new FlavorService(
        [
            new DotNetRegexEngine(),
            new PcreRegexEngine(),
        ]);

        Assert.That(custom.GetFlavors().All(f => custom.GetEngineForFlavor(f.Id) is not null), Is.True);
    }
}
