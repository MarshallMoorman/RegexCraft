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
        Assert.That(vm.Flavors.Count, Is.GreaterThanOrEqualTo(10));
        Assert.That(vm.SelectedFlavor, Is.Not.Null);
        Assert.That(vm.TokenCategories, Is.Not.Empty);
        Assert.That(vm.AnalysisNodes, Is.Not.Empty);
        Assert.That(vm.Matches, Is.Not.Empty);
        Assert.That(vm.CurrentHighlights, Is.Not.Empty);
        Assert.That(vm.HasError, Is.False);
        Assert.That(vm.CodeLanguages.Count, Is.GreaterThanOrEqualTo(10));
        Assert.That(vm.GeneratedCode, Is.Not.Empty);
        Assert.That(vm.GeneratedCode, Does.Contain("Regex").Or.Contain("regex").IgnoreCase);
    }

    [Test]
    public void GenerateTab_AutoGeneratesForDefaultCSharp()
    {
        var vm = CreateVm();
        Assert.That(vm.SelectedCodeLanguage?.Id, Is.EqualTo("csharp"));
        Assert.That(vm.GeneratedCode, Does.Contain("System.Text.RegularExpressions"));

        vm.SelectRightTabCommand.Execute("Generate");
        Assert.That(vm.IsGenerateTab, Is.True);
        Assert.That(vm.GeneratedCode, Does.Contain("System.Text.RegularExpressions"));
        Assert.That(vm.GeneratedCode, Does.Contain("var pattern"));
    }

    [Test]
    public void Generate_UpdatesWhenPatternChanges()
    {
        var vm = CreateVm();
        vm.SelectRightTabCommand.Execute("Generate");
        vm.Pattern = @"\d{4}";
        Assert.That(vm.GeneratedCode, Does.Contain(@"\d{4}").Or.Contain("d{4}"));
    }

    [Test]
    public void Settings_ThemeRoundTrip_ViaStore()
    {
        var settingsPath = Path.Combine(_tempDir, "settings.json");
        var store = new JsonSettingsStore(settingsPath);
        store.Save(new AppSettings { Theme = "Dark", FlavorId = "javascript" });

        var engines = EngineFactory.CreateDefaultEngines();
        var flavors = new FlavorService(engines);
        var vm = new MainWindowViewModel(
            flavors,
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library-theme.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history-theme.json")),
            new GrepService(),
            store,
            NullLogger<MainWindowViewModel>.Instance);

        Assert.That(vm.ThemeLabel, Is.EqualTo("Dark"));
        Assert.That(vm.SelectedFlavor?.Id, Is.EqualTo("javascript"));
    }

    [Test]
    public void Library_HasBuiltInPatterns()
    {
        var vm = CreateVm();
        vm.SelectLeftTabCommand.Execute("Library");
        Assert.That(vm.LibraryItems.Count, Is.GreaterThanOrEqualTo(12));
        Assert.That(vm.LibraryItems.Any(i => i.IsBuiltIn), Is.True);
    }

    [Test]
    public void SwitchToPython_ShowsFidelityBanner()
    {
        var vm = CreateVm();
        var python = vm.Flavors.First(f => f.Id == "python");
        vm.SelectedFlavor = python;
        Assert.That(vm.ShowFidelityBanner, Is.True);
        Assert.That(vm.FidelityBannerText, Does.Contain("Python").IgnoreCase);
        Assert.That(vm.StatusEngine, Does.Contain("Approximate").Or.Contain("Python"));
    }

    [Test]
    public void SwitchToJavaScript_TestsWithJsEngine()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "javascript");
        vm.Pattern = @"\d+";
        vm.Subject = "a1b2";
        vm.RunTestCommand.Execute(null);
        Assert.That(vm.HasError, Is.False);
        Assert.That(vm.Matches.Count, Is.EqualTo(2));
        Assert.That(vm.StatusEngine, Does.Contain("JavaScript").IgnoreCase);
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

        var words = vm.LibraryItems.First(i => i.Name == "Words");
        Assert.That(words.IsFavorite, Is.True);
        Assert.That(words.Category, Is.EqualTo("Basics"));
        Assert.That(words.IsBuiltIn, Is.False);

        vm.ToggleLibraryFavoriteCommand.Execute(words);
        Assert.That(vm.LibraryItems.First(i => i.Name == "Words").IsFavorite, Is.False);
    }

    [Test]
    public void BothEngines_ProduceHighlightsForSample()
    {
        var vm = CreateVm();
        // Full-fidelity engines only — approximate flavors still run but may diverge on edge patterns.
        foreach (var flavor in vm.Flavors.Where(f =>
                     f.Id is "dotnet" or "pcre2" or "javascript" or "typescript" or "php"))
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

        var digits = vm.LibraryItems.First(i => i.Name == "Digits");
        Assert.That(digits.IsBuiltIn, Is.False);

        vm.Pattern = "changed";
        vm.LoadLibraryItemCommand.Execute(digits);
        Assert.That(vm.Pattern, Is.EqualTo(@"\d+"));
        Assert.That(vm.Subject, Is.EqualTo("x1y"));
    }

    [Test]
    public void Library_Delete()
    {
        var vm = CreateVm();
        var before = vm.LibraryItems.Count;
        vm.Pattern = "abc";
        vm.LibraryName = "Temp";
        vm.SaveToLibraryCommand.Execute(null);
        Assert.That(vm.LibraryItems.Count, Is.EqualTo(before + 1));
        var temp = vm.LibraryItems.First(i => i.Name == "Temp");
        vm.DeleteLibraryItemCommand.Execute(temp);
        Assert.That(vm.LibraryItems.Any(i => i.Name == "Temp"), Is.False);
        Assert.That(vm.LibraryItems.Count, Is.EqualTo(before));
    }

    [Test]
    public void Library_CannotDeleteBuiltIn()
    {
        var vm = CreateVm();
        var builtin = vm.LibraryItems.First(i => i.IsBuiltIn);
        var count = vm.LibraryItems.Count;
        vm.DeleteLibraryItemCommand.Execute(builtin);
        Assert.That(vm.LibraryItems.Count, Is.EqualTo(count));
        Assert.That(vm.LibraryItems.Any(i => i.Id == builtin.Id), Is.True);
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
