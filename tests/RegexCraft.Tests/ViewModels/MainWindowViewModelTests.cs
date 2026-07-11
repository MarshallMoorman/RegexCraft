using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Tokens;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Engines;
using RegexCraft.Core.Flavors;
using Microsoft.Extensions.Logging.Abstractions;

namespace RegexCraft.Tests.ViewModels;

[TestFixture]
[Category("ViewModels")]
public sealed class MainWindowViewModelTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
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
        return new MainWindowViewModel(
            flavors,
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(_tempDir, "settings.json")),
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
        Assert.That(vm.Matches, Is.Not.Empty);
        Assert.That(vm.CurrentHighlights, Is.Not.Empty);
        Assert.That(vm.HasError, Is.False);
        Assert.That(vm.CodeLanguages.Count, Is.GreaterThanOrEqualTo(6));
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
        Assert.That(vm.ReplaceHighlights, Is.Not.Empty);
    }

    [Test]
    public void Replace_NamedBackreference_Works()
    {
        var vm = CreateVm();
        vm.Pattern = @"(?<word>\w+)";
        vm.Subject = "hello world";
        vm.Replacement = "[${word}]";
        vm.RunReplaceCommand.Execute(null);
        Assert.That(vm.ReplacePreview, Is.EqualTo("[hello] [world]"));
        Assert.That(vm.ReplaceCount, Is.EqualTo(2));
    }

    [Test]
    public void Split_Works()
    {
        var vm = CreateVm();
        vm.Pattern = @",\s*";
        vm.Subject = "a, b, c";
        vm.SelectRightTabCommand.Execute("Split");
        vm.RunSplitCommand.Execute(null);
        Assert.That(vm.SplitPartCount, Is.EqualTo(3));
        Assert.That(vm.SplitParts.Select(p => p.Value), Is.EqualTo(new[] { "a", "b", "c" }));
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
    public void SelectRightTab_SetsFlags()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Replace");
        Assert.That(vm.IsReplaceTab, Is.True);
        Assert.That(vm.IsTestTab, Is.False);
        Assert.That(vm.IsReplaceMode, Is.True);

        vm.SelectRightTabCommand.Execute("Split");
        Assert.That(vm.IsSplitTab, Is.True);

        vm.SelectRightTabCommand.Execute("Generate");
        Assert.That(vm.IsGenerateTab, Is.True);
        Assert.That(vm.GeneratedCode, Is.Not.Empty);

        vm.SelectRightTabCommand.Execute("Grep");
        Assert.That(vm.IsGrepTab, Is.True);
        Assert.That(vm.IsGrepMode, Is.True);
        Assert.That(vm.WindowTitle, Does.Contain("GREP"));

        vm.SelectRightTabCommand.Execute("Test");
        Assert.That(vm.IsTestTab, Is.True);
        Assert.That(vm.WindowTitle, Is.EqualTo("RegexCraft"));
    }

    [Test]
    public async Task GrepSearch_FindsHitsInTempFolder()
    {
        var file = Path.Combine(_tempDir, "sample.txt");
        await File.WriteAllTextAsync(file, "alpha\nbeta 42 gamma\n42 again\n");

        var vm = CreateVm();
        vm.Pattern = @"\d+";
        vm.GrepRootPath = _tempDir;
        vm.GrepIncludeGlobs = "*.txt";
        vm.GrepExcludeGlobs = "";
        vm.GrepRecursive = false;
        vm.SelectRightTabCommand.Execute("Grep");

        await vm.RunGrepSearchCommand.ExecuteAsync(null);

        Assert.That(vm.HasError, Is.False, vm.ErrorText);
        Assert.That(vm.GrepHits.Count, Is.EqualTo(2));
        Assert.That(vm.GrepHits.All(h => h.MatchValue == "42"), Is.True);
    }

    [Test]
    public void Library_FavoriteAndCategory_Persist()
    {
        var vm = CreateVm();
        vm.Pattern = @"\w+";
        vm.LibraryName = "Words";
        vm.LibraryCategory = "Basics";
        vm.LibraryTags = "demo,words";
        vm.LibraryFavorite = true;
        vm.SaveToLibraryCommand.Execute(null);

        Assert.That(vm.LibraryItems, Has.Count.EqualTo(1));
        Assert.That(vm.LibraryItems[0].IsFavorite, Is.True);
        Assert.That(vm.LibraryItems[0].Category, Is.EqualTo("Basics"));

        vm.ToggleLibraryFavoriteCommand.Execute(vm.LibraryItems[0]);
        Assert.That(vm.LibraryItems[0].IsFavorite, Is.False);
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

    [Test]
    public void Library_SaveAndLoad()
    {
        var vm = CreateVm();
        vm.Pattern = @"\d+";
        vm.Subject = "x1y";
        vm.LibraryName = "Digits";
        vm.LibraryDescription = "Find digits";
        vm.SaveToLibraryCommand.Execute(null);

        Assert.That(vm.LibraryItems, Has.Count.EqualTo(1));
        Assert.That(vm.LibraryItems[0].Name, Is.EqualTo("Digits"));

        vm.Pattern = "changed";
        vm.LoadLibraryItemCommand.Execute(vm.LibraryItems[0]);
        Assert.That(vm.Pattern, Is.EqualTo(@"\d+"));
        Assert.That(vm.Subject, Is.EqualTo("x1y"));
    }

    [Test]
    public void Library_Delete()
    {
        var vm = CreateVm();
        vm.Pattern = "abc";
        vm.LibraryName = "Temp";
        vm.SaveToLibraryCommand.Execute(null);
        Assert.That(vm.LibraryItems, Has.Count.EqualTo(1));
        vm.DeleteLibraryItemCommand.Execute(vm.LibraryItems[0]);
        Assert.That(vm.LibraryItems, Is.Empty);
    }

    [Test]
    public void History_RecordsAfterTest()
    {
        var vm = CreateVm();
        vm.Pattern = @"hello\d+";
        vm.Subject = "hello1";
        vm.RunTestCommand.Execute(null);
        Assert.That(vm.HistoryItems.Any(h => h.Entry.Pattern == @"hello\d+"), Is.True);
    }

    [Test]
    public void HistorySearch_FiltersEntries()
    {
        var vm = CreateVm();
        vm.Pattern = @"unique_alpha_\d+";
        vm.Subject = "unique_alpha_1";
        vm.RunTestCommand.Execute(null);
        vm.Pattern = @"other_beta_\d+";
        vm.Subject = "other_beta_2";
        vm.RunTestCommand.Execute(null);

        var before = vm.HistoryItems.Count;
        Assert.That(before, Is.GreaterThanOrEqualTo(2));

        vm.HistorySearch = "unique_alpha";
        Assert.That(vm.HistoryItems.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(vm.HistoryItems.All(h =>
            h.Entry.Pattern.Contains("unique_alpha", StringComparison.OrdinalIgnoreCase)), Is.True);

        vm.HistorySearch = "zzz_no_match_zzz";
        Assert.That(vm.HistoryItems, Is.Empty);
        Assert.That(vm.HistoryEmptyMessage, Does.Contain("match").IgnoreCase);
    }

    [Test]
    public void GenerateCode_ProducesOutputForLanguages()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Generate");
        foreach (var lang in vm.CodeLanguages.ToList())
        {
            vm.SelectedCodeLanguage = lang;
            Assert.That(vm.GeneratedCode, Is.Not.Empty, lang.Id);
            Assert.That(vm.GeneratedCode, Does.Not.Contain("Unsupported"), lang.Id);
        }
    }

    [Test]
    public void Analysis_HasNestedChildrenForSample()
    {
        var vm = CreateVm();
        // Sample email pattern should produce named groups as nested nodes
        var flat = Flatten(vm.AnalysisNodes);
        Assert.That(flat.Any(n => n.Kind == AnalysisNodeKind.NamedGroup), Is.True);
        Assert.That(flat.Any(n => n.HasRange), Is.True);
        Assert.That(flat.Count(n => n.Children.Count > 0 || n.Kind != AnalysisNodeKind.Root), Is.GreaterThan(1));
    }

    private static IEnumerable<AnalysisNode> Flatten(IEnumerable<AnalysisNode> nodes)
    {
        foreach (var n in nodes)
        {
            yield return n;
            foreach (var c in Flatten(n.Children))
                yield return c;
        }
    }
}
