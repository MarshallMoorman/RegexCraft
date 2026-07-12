using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Compare;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Compare;

[TestFixture]
[Category("Compare")]
[Category("ViewModels")]
public sealed class CompareViewModelTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-compare-vm-" + Guid.NewGuid().ToString("N"));
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
        var flavors = new FlavorService(engines);
        var tokens = new TokenCatalog();
        return new MainWindowViewModel(
            flavors,
            tokens,
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(_tempDir, "settings.json")),
            NullLogger<MainWindowViewModel>.Instance,
            new RegexCompareService(flavors, tokens));
    }

    [Test]
    public void Constructor_InitializesCompareFlavorChoices_WithDefaults()
    {
        var vm = CreateVm();
        Assert.That(vm.CompareFlavorChoices.Count, Is.EqualTo(vm.Flavors.Count));
        var selected = vm.CompareFlavorChoices.Count(c => c.IsSelected);
        Assert.That(selected, Is.InRange(2, 4));
    }

    [Test]
    public void SelectCompareTab_RunsComparison()
    {
        var vm = CreateVm();
        vm.Pattern = @"\d+";
        vm.Subject = "a1b22";
        vm.SelectRightTabCommand.Execute("Compare");

        Assert.That(vm.IsCompareTab, Is.True);
        Assert.That(vm.IsCompareMode, Is.True);
        Assert.That(vm.ModeLabel, Is.EqualTo("Compare"));
        Assert.That(vm.WindowTitle, Does.Contain("Compare"));
        Assert.That(vm.CompareCards.Count, Is.InRange(2, 4));
        Assert.That(vm.CompareCards.All(c => c.IsValid), Is.True);
        Assert.That(vm.CompareExportText, Does.Contain("Flavor Comparison"));
        Assert.That(vm.CompareDifferenceNotes, Is.Not.Empty);
    }

    [Test]
    public void Compare_LiveUpdate_OnPatternChange()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Compare");
        Assert.That(vm.CompareCards, Is.Not.Empty);

        vm.Pattern = @"xyz-no-match-zzz";
        vm.Subject = "hello";
        // Debounced path — call RunCompare directly for deterministic unit test
        vm.RunCompareCommand.Execute(null);

        Assert.That(vm.CompareCards.All(c => c.IsValid), Is.True);
        Assert.That(vm.CompareCards.All(c => c.Result.MatchCount == 0), Is.True);
    }

    [Test]
    public void Compare_EnforcesMaxFourFlavors()
    {
        var vm = CreateVm();
        foreach (var c in vm.CompareFlavorChoices)
            c.IsSelected = false;

        var toSelect = vm.CompareFlavorChoices.Take(5).ToList();
        for (var i = 0; i < toSelect.Count; i++)
        {
            toSelect[i].IsSelected = true;
            vm.CompareFlavorSelectionChangedCommand.Execute(toSelect[i]);
        }

        Assert.That(vm.CompareFlavorChoices.Count(c => c.IsSelected), Is.LessThanOrEqualTo(4));
    }

    [Test]
    public void Compare_FewerThanTwo_ShowsEmptyGuidance()
    {
        var vm = CreateVm();
        foreach (var c in vm.CompareFlavorChoices)
            c.IsSelected = false;

        vm.SelectRightTabCommand.Execute("Compare");
        Assert.That(vm.CompareCards, Is.Empty);
        Assert.That(vm.CompareEmptyMessage, Does.Contain("2").IgnoreCase);
    }

    [Test]
    public void CopyCompareSummary_InvokesCopyWhenTextPresent()
    {
        var vm = CreateVm();
        string? copied = null;
        vm.CopyTextRequested += t => copied = t;
        vm.SelectRightTabCommand.Execute("Compare");
        vm.CopyCompareSummaryCommand.Execute(null);
        Assert.That(copied, Is.Not.Null.And.Not.Empty);
        Assert.That(copied, Does.Contain("Pattern:"));
    }
}
