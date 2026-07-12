using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Flavors;

/// <summary>
/// Core execution tests proving each selectable flavor maps to a working engine
/// and can Match / Replace / Split a portable pattern.
/// </summary>
[TestFixture]
[Category("Flavors")]
public sealed class FlavorMappingAndExecutionTests
{
    private FlavorService _service = null!;

    [SetUp]
    public void SetUp() => _service = new FlavorService(EngineFactory.CreateDefaultEngines());

    private static IEnumerable<string> AllFlavorIds() =>
        FlavorService.BuildDefaultFlavors().Select(f => f.Id);

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_ResolvesEngine_AndMatchesPortablePattern(string flavorId)
    {
        var flavor = _service.GetFlavor(flavorId);
        Assert.That(flavor, Is.Not.Null, flavorId);

        var engine = _service.GetEngineForFlavor(flavorId);
        Assert.That(engine, Is.Not.Null, flavorId);
        Assert.That(engine!.Id, Is.EqualTo(flavor!.EngineId).IgnoreCase);

        var result = engine.Match(@"\d+", "a12b34c", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, $"{flavorId}: {result.ErrorMessage}");
        Assert.That(result.Matches, Has.Count.EqualTo(2));
        Assert.That(result.Matches[0].Value, Is.EqualTo("12"));
        Assert.That(result.Matches[1].Value, Is.EqualTo("34"));
        Assert.That(result.EngineId, Is.EqualTo(engine.Id));
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_Replace_Works(string flavorId)
    {
        var engine = _service.GetEngineForFlavor(flavorId)!;
        var result = engine.Replace(@"\d+", "x1y2z", "#", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, $"{flavorId}: {result.ErrorMessage}");
        Assert.That(result.Result, Is.EqualTo("x#y#z"));
        Assert.That(result.ReplacementCount, Is.EqualTo(2));
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_Split_Works(string flavorId)
    {
        var engine = _service.GetEngineForFlavor(flavorId)!;
        var result = engine.Split(@",\s*", "a, b, c", RegexOptionsEx.None);
        Assert.That(result.Success, Is.True, $"{flavorId}: {result.ErrorMessage}");
        Assert.That(result.Parts, Is.EqualTo(new[] { "a", "b", "c" }));
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_NamedGroups_WhenSupportedByEngine(string flavorId)
    {
        var flavor = _service.GetFlavor(flavorId)!;
        var engine = _service.GetEngineForFlavor(flavorId)!;

        // Named groups work on all three real engines with (?<name>) syntax.
        var result = engine.Match(
            @"(?<word>\w+)-(?<num>\d+)",
            "item-42",
            RegexOptionsEx.None);

        Assert.That(result.Success, Is.True, $"{flavorId}: {result.ErrorMessage}");
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        var word = result.Matches[0].Groups.FirstOrDefault(g => g.Name == "word");
        Assert.That(word, Is.Not.Null, flavorId);
        Assert.That(word!.Value, Is.EqualTo("item"));
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_IgnoreCase_WhenSupported(string flavorId)
    {
        var flavor = _service.GetFlavor(flavorId)!;
        if (!flavor.SupportsOption(RegexOptionsEx.IgnoreCase))
            Assert.Ignore("IgnoreCase not supported");

        var engine = _service.GetEngineForFlavor(flavorId)!;
        var result = engine.Match("hello", "HELLO", RegexOptionsEx.IgnoreCase);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_InvalidPattern_ReturnsError(string flavorId)
    {
        var engine = _service.GetEngineForFlavor(flavorId)!;
        var result = engine.Match("(", "abc", RegexOptionsEx.None);
        Assert.That(result.Success, Is.False, flavorId);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void FlavorEngineMapping_Table()
    {
        var expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dotnet"] = "dotnet",
            ["pcre2"] = "pcre2",
            ["javascript"] = "javascript",
            ["typescript"] = "javascript",
            ["php"] = "pcre2",
            ["python"] = "dotnet",
            ["java"] = "dotnet",
            ["ruby"] = "pcre2",
            ["go"] = "dotnet",
            ["rust"] = "dotnet",
            ["perl"] = "pcre2",
            ["kotlin"] = "dotnet",
            ["swift"] = "dotnet",
        };

        foreach (var (flavorId, engineId) in expected)
        {
            var f = _service.GetFlavor(flavorId);
            Assert.That(f, Is.Not.Null, flavorId);
            Assert.That(f!.EngineId, Is.EqualTo(engineId).IgnoreCase);
        }
    }
}
