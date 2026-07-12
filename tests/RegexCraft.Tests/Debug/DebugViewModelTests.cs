using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.App.ViewModels;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Debug;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Debug;

[TestFixture]
[Category("Debug")]
[Category("ViewModels")]
public sealed class DebugViewModelTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-debug-vm-" + Guid.NewGuid().ToString("N"));
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
            NullLogger<MainWindowViewModel>.Instance,
            debugService: new RegexDebugService(new RegexAnalysisService()));
    }

    [Test]
    public void DebugTab_OnDotNet_BuildsSessionAndSteps()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "dotnet");
        vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
        vm.Subject = "a@b.com";

        vm.SelectRightTabCommand.Execute("Debug");

        Assert.That(vm.IsDebugTab, Is.True);
        Assert.That(vm.IsDebugMode, Is.True);
        Assert.That(vm.IsDebugAvailable, Is.True);
        Assert.That(vm.DebugSteps.Count, Is.GreaterThan(2));
        Assert.That(vm.DebugStepCounter, Does.Contain("Step"));
        Assert.That(vm.DebugExplanation, Is.Not.Empty);
        Assert.That(vm.WindowTitle, Does.Contain("Debug"));
    }

    [Test]
    public void Debug_StepForwardAndBackward()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "dotnet");
        vm.Pattern = @"\d+";
        vm.Subject = "x42y";
        vm.SelectRightTabCommand.Execute("Debug");

        Assert.That(vm.IsDebugAvailable, Is.True);
        var total = vm.DebugSteps.Count;
        Assert.That(total, Is.GreaterThan(2));

        Assert.That(vm.DebugStepIndex, Is.EqualTo(0));
        Assert.That(vm.CanDebugStepBack, Is.False);
        Assert.That(vm.CanDebugStepForward, Is.True);

        vm.DebugStepForwardCommand.Execute(null);
        Assert.That(vm.DebugStepIndex, Is.EqualTo(1));
        Assert.That(vm.CanDebugStepBack, Is.True);

        var explanation1 = vm.DebugExplanation;
        vm.DebugStepForwardCommand.Execute(null);
        Assert.That(vm.DebugStepIndex, Is.EqualTo(2));
        Assert.That(vm.DebugExplanation, Is.Not.EqualTo(explanation1).Or.Not.Empty);

        vm.DebugStepBackwardCommand.Execute(null);
        Assert.That(vm.DebugStepIndex, Is.EqualTo(1));

        vm.DebugGoToEndCommand.Execute(null);
        Assert.That(vm.DebugStepIndex, Is.EqualTo(total - 1));
        Assert.That(vm.CanDebugStepForward, Is.False);

        vm.DebugResetCommand.Execute(null);
        Assert.That(vm.DebugStepIndex, Is.EqualTo(0));
    }

    [Test]
    public void Debug_Unavailable_ForJavaScript()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "javascript");
        vm.Pattern = @"\d+";
        vm.Subject = "12";
        vm.SelectRightTabCommand.Execute("Debug");

        Assert.That(vm.IsDebugTab, Is.True);
        Assert.That(vm.IsDebugAvailable, Is.False);
        Assert.That(vm.DebugUnavailableMessage, Does.Contain(".NET").IgnoreCase);
        Assert.That(vm.DebugSteps, Is.Empty);
    }

    [Test]
    public void Debug_Available_ForPython_BecauseDotNetEngine()
    {
        var vm = CreateVm();
        var python = vm.Flavors.FirstOrDefault(f => f.Id == "python");
        if (python is null)
            Assert.Ignore("python flavor not registered");

        vm.SelectedFlavor = python;
        vm.Pattern = @"\w+";
        vm.Subject = "hello";
        vm.SelectRightTabCommand.Execute("Debug");

        // Python uses the .NET engine for approximate testing — Debug should work.
        Assert.That(python.EngineId, Is.EqualTo("dotnet"));
        Assert.That(vm.IsDebugAvailable, Is.True);
        Assert.That(vm.DebugSteps, Is.Not.Empty);
    }

    [Test]
    public void Debug_StepAppliesSubjectHighlight()
    {
        var vm = CreateVm();
        vm.SelectedFlavor = vm.Flavors.First(f => f.Id == "dotnet");
        vm.Pattern = @"\d+";
        vm.Subject = "ab99cd";
        vm.SelectRightTabCommand.Execute("Debug");

        // Advance until we find a step with a positive subject range.
        var found = false;
        for (var i = 0; i < vm.DebugSteps.Count; i++)
        {
            vm.DebugStepForwardCommand.Execute(null);
            if (vm.CurrentHighlights.Count > 0)
            {
                found = true;
                break;
            }
        }

        // At least match-success steps should set highlights.
        vm.DebugGoToEndCommand.Execute(null);
        // Complete step may clear or keep last; scan for any step with subject highlight
        for (var i = 0; i < vm.DebugSteps.Count; i++)
        {
            // Set via SelectedDebugStep
            vm.SelectedDebugStep = vm.DebugSteps[i];
            if (vm.DebugSteps[i].HasSubjectRange && vm.DebugSteps[i].SubjectLength > 0)
            {
                Assert.That(vm.CurrentHighlights, Is.Not.Empty);
                found = true;
                break;
            }
        }

        Assert.That(found, Is.True, "expected at least one step with subject highlight");
    }
}
