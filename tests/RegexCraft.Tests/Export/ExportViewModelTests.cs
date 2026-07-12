using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Export;

[TestFixture]
[Category("Export")]
[Category("ViewModels")]
public sealed class ExportViewModelTests
{
    private string _tempDir = null!;
    private MainWindowViewModel _vm = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-export-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var engines = EngineFactory.CreateDefaultEngines();
        var flavorService = new FlavorService(engines);
        _vm = new MainWindowViewModel(
            flavorService,
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(_tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(_tempDir, "history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(_tempDir, "settings.json")),
            NullLogger<MainWindowViewModel>.Instance);
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
            // best-effort cleanup
        }
    }

    [Test]
    public void AfterSuccessfulTest_CanExportMatches()
    {
        _vm.Pattern = @"\d+";
        _vm.Subject = "a 12 b 34";
        _vm.RunTestCommand.Execute(null);

        Assert.That(_vm.CanExportMatches, Is.True);
        Assert.That(_vm.Matches.Count, Is.EqualTo(2));

        var csv = _vm.BuildMatchesCsv();
        Assert.That(csv, Does.Contain("MatchIndex"));
        Assert.That(csv, Does.Contain("12"));
        Assert.That(csv, Does.Contain("34"));

        var json = _vm.BuildMatchesJson();
        Assert.That(json, Does.Contain("\"pattern\""));
        Assert.That(json, Does.Contain("12"));
        Assert.That(json, Does.Contain("flavorId"));
    }

    [Test]
    public void ExportMatchesCsv_WritesFileViaSaveCallback()
    {
        _vm.Pattern = @"\w+";
        _vm.Subject = "hello world";
        _vm.RunTestCommand.Execute(null);

        var outPath = Path.Combine(_tempDir, "out.csv");
        _vm.SaveFileRequested += (_, _, _) => Task.FromResult<string?>(outPath);

        Assert.That(_vm.ExportMatchesCsvCommand.CanExecute(null), Is.True);
        _vm.ExportMatchesCsvCommand.Execute(null);

        Assert.That(File.Exists(outPath), Is.True);
        var text = File.ReadAllText(outPath);
        Assert.That(text, Does.Contain("hello"));
        Assert.That(text, Does.Contain("world"));
        Assert.That(_vm.StatusText, Does.Contain("CSV"));
    }

    [Test]
    public void ExportMatchesJson_WritesFileViaSaveCallback()
    {
        _vm.Pattern = @"\d+";
        _vm.Subject = "x9y";
        _vm.RunTestCommand.Execute(null);

        var outPath = Path.Combine(_tempDir, "out.json");
        _vm.SaveFileRequested += (_, _, _) => Task.FromResult<string?>(outPath);

        _vm.ExportMatchesJsonCommand.Execute(null);

        Assert.That(File.Exists(outPath), Is.True);
        var text = File.ReadAllText(outPath);
        Assert.That(text, Does.Contain("\"matches\""));
        Assert.That(text, Does.Contain("9"));
        Assert.That(_vm.StatusText, Does.Contain("JSON"));
    }

    [Test]
    public void CopyMatchesJson_InvokesClipboardCallback()
    {
        _vm.Pattern = @"ab";
        _vm.Subject = "ab";
        _vm.RunTestCommand.Execute(null);

        string? copied = null;
        _vm.CopyTextRequested += t => copied = t;
        _vm.CopyMatchesJsonCommand.Execute(null);

        Assert.That(copied, Is.Not.Null);
        Assert.That(copied, Does.Contain("\"matches\""));
        Assert.That(copied, Does.Contain("ab"));
    }

    [Test]
    public void Export_CancelledSave_SetsStatus()
    {
        _vm.Pattern = @"x";
        _vm.Subject = "x";
        _vm.RunTestCommand.Execute(null);

        _vm.SaveFileRequested += (_, _, _) => Task.FromResult<string?>(null);
        _vm.ExportMatchesCsvCommand.Execute(null);

        Assert.That(_vm.StatusText, Does.Contain("cancelled").IgnoreCase);
    }
}
