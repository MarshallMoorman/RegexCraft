using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.ViewModels;

[TestFixture]
[Category("ViewModels")]
public sealed class MainWindowViewModelExpandedTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-vmx-" + Guid.NewGuid().ToString("N"));
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

    private MainWindowViewModel CreateVm(AppSettings? seed = null)
    {
        var settingsPath = Path.Combine(_tempDir, "settings.json");
        var store = new JsonSettingsStore(settingsPath);
        if (seed is not null)
            store.Save(seed);

        var engines = EngineFactory.CreateDefaultEngines();
        return new MainWindowViewModel(
            new FlavorService(engines),
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history.json")),
            new GrepService(),
            store,
            NullLogger<MainWindowViewModel>.Instance);
    }

    [Test]
    public void CycleTheme_RotatesSystemLightDark()
    {
        var vm = CreateVm(new AppSettings { Theme = "System" });
        Assert.That(vm.ThemeLabel, Is.EqualTo("System"));
        vm.CycleThemeCommand.Execute(null);
        Assert.That(vm.ThemeLabel, Is.EqualTo("Light"));
        vm.CycleThemeCommand.Execute(null);
        Assert.That(vm.ThemeLabel, Is.EqualTo("Dark"));
        vm.CycleThemeCommand.Execute(null);
        Assert.That(vm.ThemeLabel, Is.EqualTo("System"));
    }

    [Test]
    public void ThemePersistence_SurvivesNewVmInstance()
    {
        var vm1 = CreateVm(new AppSettings { Theme = "Light" });
        vm1.CycleThemeCommand.Execute(null); // Light → Dark
        Assert.That(vm1.ThemeLabel, Is.EqualTo("Dark"));

        var vm2 = CreateVm(); // loads from same settings path... wait, CreateVm creates new empty if no seed
        // Re-load from same file
        var store = new JsonSettingsStore(Path.Combine(_tempDir, "settings.json"));
        var engines = EngineFactory.CreateDefaultEngines();
        var vm3 = new MainWindowViewModel(
            new FlavorService(engines),
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library2.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history2.json")),
            new GrepService(),
            store,
            NullLogger<MainWindowViewModel>.Instance);

        Assert.That(vm3.ThemeLabel, Is.EqualTo("Dark"));
    }

    [Test]
    public void Options_PersistToSettingsStore()
    {
        var vm = CreateVm();
        vm.IgnoreCase = true;
        vm.Multiline = true;
        vm.Singleline = true;

        var store = new JsonSettingsStore(Path.Combine(_tempDir, "settings.json"));
        var loaded = store.Load();
        Assert.That(loaded.IgnoreCase, Is.True);
        Assert.That(loaded.Multiline, Is.True);
        Assert.That(loaded.Singleline, Is.True);
    }

    [Test]
    public void SelectLeftTab_SwitchesSidebar()
    {
        var vm = CreateVm();
        vm.SelectLeftTabCommand.Execute("Library");
        Assert.That(vm.IsLibraryTab, Is.True);
        Assert.That(vm.IsTokensTab, Is.False);

        vm.SelectLeftTabCommand.Execute("History");
        Assert.That(vm.IsHistoryTab, Is.True);

        vm.SelectLeftTabCommand.Execute("Tokens");
        Assert.That(vm.IsTokensTab, Is.True);
    }

    [Test]
    public void AllApproximateFlavors_ShowFidelityBanner()
    {
        var vm = CreateVm();
        foreach (var flavor in vm.Flavors.Where(f => f.Fidelity == TestingFidelity.Approximate))
        {
            vm.SelectedFlavor = flavor;
            Assert.That(vm.ShowFidelityBanner, Is.True, flavor.Id);
            Assert.That(vm.FidelityBannerText, Is.Not.Empty, flavor.Id);
        }
    }

    [Test]
    public void FullFidelityFlavors_HideBanner()
    {
        var vm = CreateVm();
        foreach (var flavor in vm.Flavors.Where(f => f.Fidelity == TestingFidelity.Full))
        {
            vm.SelectedFlavor = flavor;
            Assert.That(vm.ShowFidelityBanner, Is.False, flavor.Id);
        }
    }

    [Test]
    public void LoadHistoryEntry_RestoresPatternSubject()
    {
        var vm = CreateVm();
        vm.Pattern = @"hist_unique_\d+";
        vm.Subject = "hist_unique_99";
        vm.RunTestCommand.Execute(null);

        var entry = vm.HistoryItems.First(h => h.Entry.Pattern.Contains("hist_unique"));
        vm.Pattern = "changed";
        vm.Subject = "changed";
        vm.LoadHistoryItemCommand.Execute(entry);
        Assert.That(vm.Pattern, Is.EqualTo(@"hist_unique_\d+"));
        Assert.That(vm.Subject, Is.EqualTo("hist_unique_99"));
    }

    [Test]
    public void Generate_Operations_ChangeSnippet()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Generate");
        vm.SelectedCodeLanguage = vm.CodeLanguages.First(l => l.Id == "csharp");

        vm.SelectedCodegenOperation = vm.CodegenOperations.First(o => o.Id == "Replace");
        Assert.That(vm.GeneratedCode, Does.Contain("Replace").IgnoreCase);

        vm.SelectedCodegenOperation = vm.CodegenOperations.First(o => o.Id == "Split");
        Assert.That(vm.GeneratedCode, Does.Contain("Split").IgnoreCase);
    }

    [Test]
    public void EmptySubject_NoMatches_NoError()
    {
        var vm = CreateVm();
        vm.Pattern = @"\d+";
        vm.Subject = "";
        vm.RunTestCommand.Execute(null);
        Assert.That(vm.HasError, Is.False);
        Assert.That(vm.Matches, Is.Empty);
    }

    [Test]
    public void VersionText_IsNonEmpty()
    {
        var vm = CreateVm();
        Assert.That(vm.VersionText, Does.StartWith("v"));
        Assert.That(vm.VersionText.Length, Is.GreaterThan(2));
    }

    [Test]
    public void ToggleOptions_FlipsExpanded()
    {
        var vm = CreateVm();
        var before = vm.OptionsExpanded;
        vm.ToggleOptionsCommand.Execute(null);
        Assert.That(vm.OptionsExpanded, Is.EqualTo(!before));
    }
}
