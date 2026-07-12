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
    public void GetTargetRightPanelWidth_Defaults_WhenUnstored()
    {
        var vm = CreateVm();
        Assert.That(vm.GetTargetRightPanelWidth(compareMode: false),
            Is.EqualTo(LayoutDefaults.RightPanelNormalDefault));
        Assert.That(vm.GetTargetRightPanelWidth(compareMode: true),
            Is.EqualTo(LayoutDefaults.RightPanelCompareDefault));
    }

    [Test]
    public void GetTargetRightPanelWidth_Compare_UsesBodyShare()
    {
        var vm = CreateVm();
        var w = vm.GetTargetRightPanelWidth(compareMode: true, bodyWidth: 1320);
        Assert.That(w, Is.GreaterThan(700), "Compare should claim most of the body content row");
    }

    [Test]
    public void GetTargetRightPanelWidth_UsesStoredSettings()
    {
        var vm = CreateVm(new AppSettings
        {
            RightPanelNormalWidth = 360,
            RightPanelCompareWidth = 900,
        });
        Assert.That(vm.GetTargetRightPanelWidth(false), Is.EqualTo(360));
        // Large stored Compare width is usable
        Assert.That(vm.GetTargetRightPanelWidth(true), Is.EqualTo(900));
    }

    [Test]
    public void GetTargetRightPanelWidth_IgnoresStaleNarrowCompareStore()
    {
        var vm = CreateVm(new AppSettings
        {
            RightPanelNormalWidth = 400,
            RightPanelCompareWidth = 520, // old default — not usable
        });
        Assert.That(vm.GetTargetRightPanelWidth(true, bodyWidth: 1320), Is.GreaterThan(700));
    }

    [Test]
    public void RememberRightPanelWidth_PersistsSeparately_AndSurvivesReload()
    {
        var vm = CreateVm();
        vm.RememberRightPanelWidth(370, compareMode: false);
        vm.RememberRightPanelWidth(850, compareMode: true);

        var store = new JsonSettingsStore(Path.Combine(_tempDir, "settings.json"));
        var loaded = store.Load();
        Assert.That(loaded.RightPanelNormalWidth, Is.EqualTo(370));
        Assert.That(loaded.RightPanelCompareWidth, Is.EqualTo(850));

        var vm2 = CreateVm(); // same settings path via CreateVm without seed reloads file
        Assert.That(vm2.GetTargetRightPanelWidth(false), Is.EqualTo(370));
        Assert.That(vm2.GetTargetRightPanelWidth(true), Is.EqualTo(850));
    }

    [Test]
    public void RememberRightPanelWidth_DoesNotPersistTooNarrowCompare()
    {
        var vm = CreateVm();
        vm.RememberRightPanelWidth(400, compareMode: true); // below CompareMin
        var store = new JsonSettingsStore(Path.Combine(_tempDir, "settings.json"));
        Assert.That(store.Load().RightPanelCompareWidth, Is.Null);
    }

    [Test]
    public void SelectRightTab_RaisesRightPanelModeChanged_WithPreviousTab()
    {
        var vm = CreateVm();
        string? previous = null;
        vm.RightPanelModeChanged += tab => previous = tab;

        vm.SelectRightTabCommand.Execute("Compare");
        Assert.That(previous, Is.EqualTo("Test"));
        Assert.That(vm.IsCompareTab, Is.True);

        previous = null;
        vm.SelectRightTabCommand.Execute("Replace");
        Assert.That(previous, Is.EqualTo("Compare"));
        Assert.That(vm.IsCompareTab, Is.False);

        // Same tab again should not raise
        previous = "sentinel";
        vm.SelectRightTabCommand.Execute("Replace");
        Assert.That(previous, Is.EqualTo("sentinel"));
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
