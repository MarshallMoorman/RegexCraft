using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Models;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Flavors;

/// <summary>
/// ViewModel fidelity banner / token / option behavior and GREP engine routing per flavor.
/// </summary>
[TestFixture]
[Category("Flavors")]
[Category("ViewModels")]
public sealed class FlavorViewModelAndGrepTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-flavor-vm-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // ignore
        }
    }

    private MainWindowViewModel CreateVm()
    {
        var engines = EngineFactory.CreateDefaultEngines();
        return new MainWindowViewModel(
            new FlavorService(engines),
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(_tempDir, "settings.json")),
            NullLogger<MainWindowViewModel>.Instance);
    }

    private static IEnumerable<string> AllFlavorIds() =>
        FlavorService.BuildDefaultFlavors().Select(f => f.Id);

    [TestCaseSource(nameof(AllFlavorIds))]
    public void SelectingFlavor_UpdatesBannerAndStatus(string flavorId)
    {
        var vm = CreateVm();
        var flavor = vm.Flavors.First(f => f.Id == flavorId);
        vm.SelectedFlavor = flavor;

        Assert.That(vm.SelectedFlavor!.Id, Is.EqualTo(flavorId));
        Assert.That(vm.StatusEngine, Does.Contain(flavor.DisplayName).Or.Contain(flavor.StatusLabel).IgnoreCase
            .Or.Contain(flavorId).IgnoreCase);

        if (flavor.Fidelity == TestingFidelity.Full)
        {
            Assert.That(vm.ShowFidelityBanner, Is.False, flavorId);
        }
        else
        {
            Assert.That(vm.ShowFidelityBanner, Is.True, flavorId);
            Assert.That(vm.FidelityBannerText, Is.Not.Null.And.Not.Empty, flavorId);
        }
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void SelectingFlavor_RebuildsTokens_WithSupportFlags(string flavorId)
    {
        var vm = CreateVm();
        var flavor = vm.Flavors.First(f => f.Id == flavorId);
        vm.SelectedFlavor = flavor;

        var items = vm.TokenCategories.SelectMany(c => c.Tokens).ToList();
        Assert.That(items, Is.Not.Empty, flavorId);

        foreach (var item in items)
        {
            var expected = flavor.IsTokenSupported(item.Token);
            Assert.That(item.IsSupported, Is.EqualTo(expected),
                $"{flavorId}/{item.Token.Id}");
            Assert.That(item.Opacity, Is.EqualTo(expected ? 1.0 : 0.45).Within(0.01));
        }
    }

    [Test]
    public void JavaScript_DisablesExplicitCaptureAndFreeSpacingOptions()
    {
        var vm = CreateVm();
        vm.ExplicitCapture = true;
        vm.IgnorePatternWhitespace = true;
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "javascript");

        Assert.That(vm.ExplicitCaptureEnabled, Is.False);
        Assert.That(vm.IgnorePatternWhitespaceEnabled, Is.False);
        Assert.That(vm.ExplicitCapture, Is.False);
        Assert.That(vm.IgnorePatternWhitespace, Is.False);
        Assert.That(vm.IgnoreCaseEnabled, Is.True);
    }

    [Test]
    public void DotNet_EnablesAllCommonOptions()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "dotnet");
        Assert.That(vm.ExplicitCaptureEnabled, Is.True);
        Assert.That(vm.IgnorePatternWhitespaceEnabled, Is.True);
        Assert.That(vm.IgnoreCaseEnabled, Is.True);
        Assert.That(vm.MultilineEnabled, Is.True);
        Assert.That(vm.SinglelineEnabled, Is.True);
    }

    [Test]
    public void SelectingPython_SelectsPythonCodegenLanguage()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "python");
        Assert.That(vm.SelectedCodeLanguage?.Id, Is.EqualTo("python"));
        Assert.That(vm.GeneratedCode, Does.Contain("re").IgnoreCase);
    }

    [Test]
    public void SelectingJavaScript_SelectsJsCodegenLanguage()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "javascript");
        Assert.That(vm.SelectedCodeLanguage?.Id, Is.EqualTo("javascript"));
    }

    [Test]
    public void Go_TokensDimLookbehind()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "go");
        var lookbehind = vm.TokenCategories.SelectMany(c => c.Tokens)
            .First(t => t.Token.Id == "pos-lookbehind");
        Assert.That(lookbehind.IsSupported, Is.False);
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void Flavor_MatchViaViewModel_Works(string flavorId)
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == flavorId);
        vm.Pattern = @"\d+";
        vm.Subject = "ab12cd";
        vm.RunTestCommand.Execute(null);

        Assert.That(vm.HasError, Is.False, $"{flavorId}: {vm.ErrorText}");
        Assert.That(vm.Matches.Count, Is.EqualTo(1));
        Assert.That(vm.Matches[0].Value, Is.EqualTo("12"));
    }

    [Test]
    public async Task Grep_UsesFlavorEngine_JavaScript()
    {
        var file = Path.Combine(_tempDir, "sample.js");
        await File.WriteAllTextAsync(file, "const n = 42;\nconst s = 'x';\n");

        var flavors = new FlavorService(EngineFactory.CreateDefaultEngines());
        var engine = flavors.GetEngineForFlavor("javascript")!;
        Assert.That(engine.Id, Is.EqualTo("javascript"));

        var grep = new GrepService();
        var result = await grep.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _tempDir,
            Pattern = @"\d+",
            Options = RegexOptionsEx.None,
            Recursive = false,
            IncludeGlobs = "*.js",
            ExcludeGlobs = "",
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches.Any(m => m.MatchValue == "42"), Is.True);
    }

    [Test]
    public async Task Grep_UsesFlavorEngine_PhpMapsToPcre2()
    {
        var file = Path.Combine(_tempDir, "a.txt");
        await File.WriteAllTextAsync(file, "id=99\n");

        var flavors = new FlavorService(EngineFactory.CreateDefaultEngines());
        var engine = flavors.GetEngineForFlavor("php")!;
        Assert.That(engine.Id, Is.EqualTo("pcre2"));

        var grep = new GrepService();
        var result = await grep.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _tempDir,
            Pattern = @"\d++",
            Options = RegexOptionsEx.None,
            Recursive = false,
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches.Any(m => m.MatchValue == "99"), Is.True);
    }

    [Test]
    public void HighFidelityFlavors_ShowBanner()
    {
        var vm = CreateVm();
        foreach (var flavor in vm.Flavors.Where(f => f.Fidelity == TestingFidelity.High))
        {
            vm.SelectedFlavor = flavor;
            Assert.That(vm.ShowFidelityBanner, Is.True, flavor.Id);
            Assert.That(vm.FidelityBannerText, Is.Not.Empty, flavor.Id);
        }
    }
}
