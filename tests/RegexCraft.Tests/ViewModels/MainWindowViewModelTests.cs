using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;
using RegexCraft.Core.Flavors;
using Microsoft.Extensions.Logging.Abstractions;

namespace RegexCraft.Tests.ViewModels;

[TestFixture]
[Category("ViewModels")]
public sealed class MainWindowViewModelTests
{
    private MainWindowViewModel CreateVm()
    {
        var engines = EngineFactory.CreateDefaultEngines();
        var flavors = new FlavorService(engines);
        return new MainWindowViewModel(
            flavors,
            new TokenCatalog(),
            new RegexAnalysisService(),
            NullLogger<MainWindowViewModel>.Instance);
    }

    [Test]
    public void Constructor_LoadsFlavorsAndRunsInitialTest()
    {
        var vm = CreateVm();
        Assert.That(vm.Flavors, Has.Count.EqualTo(2));
        Assert.That(vm.SelectedFlavor, Is.Not.Null);
        Assert.That(vm.TokenCategories, Is.Not.Empty);
        Assert.That(vm.AnalysisNodes, Is.Not.Empty);
        // Sample pattern should produce matches
        Assert.That(vm.Matches, Is.Not.Empty);
        Assert.That(vm.CurrentHighlights, Is.Not.Empty);
        Assert.That(vm.HasError, Is.False);
    }

    [Test]
    public void InsertToken_WithoutEditor_UpdatesPattern()
    {
        var vm = CreateVm();
        vm.Pattern = "ab";
        vm.PatternCaretOffset = 1;
        vm.PatternSelectionLength = 0;

        var digit = vm.TokenCategories
            .SelectMany(c => c.Tokens)
            .First(t => t.InsertText == @"\d");

        vm.InsertTokenCommand.Execute(digit);
        Assert.That(vm.Pattern, Is.EqualTo(@"a\db"));
    }

    [Test]
    public void SwitchFlavor_ReTests()
    {
        var vm = CreateVm();
        var pcre = vm.Flavors.First(f => f.Id == "pcre2");
        vm.SelectedFlavor = pcre;
        // Force immediate test (debounce is async; call command)
        vm.RunTestCommand.Execute(null);
        Assert.That(vm.StatusEngine, Does.Contain("PCRE2").Or.Contain("pcre2").IgnoreCase);
        Assert.That(vm.HasError, Is.False);
        Assert.That(vm.Matches, Is.Not.Empty);
    }

    [Test]
    public void InvalidPattern_SetsError()
    {
        var vm = CreateVm();
        vm.Pattern = "(";
        vm.RunTestCommand.Execute(null);
        Assert.That(vm.HasError, Is.True);
        Assert.That(vm.ErrorText, Is.Not.Empty);
        Assert.That(vm.Matches, Is.Empty);
        Assert.That(vm.CurrentHighlights, Is.Empty);
    }

    [Test]
    public void Replace_Works()
    {
        var vm = CreateVm();
        vm.Pattern = @"\d+";
        vm.Subject = "a1b2";
        vm.Replacement = "#";
        vm.RunReplaceCommand.Execute(null);
        Assert.That(vm.ReplacePreview, Is.EqualTo("a#b#"));
        Assert.That(vm.ReplaceCount, Is.EqualTo(2));
    }

    [Test]
    public void TokenSearch_FiltersCategories()
    {
        var vm = CreateVm();
        var before = vm.TokenCategories.Sum(c => c.Tokens.Count);
        vm.TokenSearch = "lookahead";
        var after = vm.TokenCategories.Sum(c => c.Tokens.Count);
        Assert.That(after, Is.LessThan(before));
        Assert.That(after, Is.GreaterThan(0));
    }

    [Test]
    public void SelectRightTab_Replace_SetsFlags()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Replace");
        Assert.That(vm.IsReplaceTab, Is.True);
        Assert.That(vm.IsTestTab, Is.False);
        vm.SelectRightTabCommand.Execute("Test");
        Assert.That(vm.IsTestTab, Is.True);
    }

    [Test]
    public void BothEngines_ProduceHighlightsForSample()
    {
        var vm = CreateVm();
        foreach (var flavor in vm.Flavors)
        {
            vm.SelectedFlavor = flavor;
            vm.RunTestCommand.Execute(null);
            Assert.That(vm.HasError, Is.False, flavor.Id);
            Assert.That(vm.CurrentHighlights, Is.Not.Empty, flavor.Id);
            Assert.That(vm.Matches.All(m => m.Groups.Count > 0), Is.True, flavor.Id);
        }
    }
}
