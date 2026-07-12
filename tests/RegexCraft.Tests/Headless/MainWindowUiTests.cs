using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using RegexCraft.App.Views;
using RegexCraft.App.ViewModels;

namespace RegexCraft.Tests.Headless;

[TestFixture]
[NonParallelizable]
[Category("UI")]
[Category("Headless")]
public sealed class MainWindowUiTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-ui-" + Guid.NewGuid().ToString("N"));
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

    [AvaloniaTest]
    public void MainWindow_Opens_WithRegexCraftTitle()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.That(window.Title, Does.Contain("RegexCraft").Or.EqualTo("RegexCraft"));
        Assert.That(vm.Flavors.Count, Is.GreaterThanOrEqualTo(10));
        Assert.That(vm.Matches, Is.Not.Empty);
        Assert.That(vm.HasError, Is.False);
        window.Close();
    }

    [AvaloniaTest]
    public void SwitchModes_UpdatesViewModelFlags()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        foreach (var (tab, check) in new (string, Func<MainWindowViewModel, bool>)[]
                 {
                     ("Test", v => v.IsTestTab),
                     ("Replace", v => v.IsReplaceTab),
                     ("Split", v => v.IsSplitTab),
                     ("Generate", v => v.IsGenerateTab),
                     ("Grep", v => v.IsGrepTab),
                     ("Compare", v => v.IsCompareTab),
                 })
        {
            vm.SelectRightTabCommand.Execute(tab);
            Dispatcher.UIThread.RunJobs();
            Assert.That(check(vm), Is.True, tab);
        }

        window.Close();
    }

    [AvaloniaTest]
    public void CompareMode_ShowsCardsForSelectedFlavors()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
        vm.Subject = "a@b.com and c@d.org";
        vm.SelectRightTabCommand.Execute("Compare");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.IsCompareTab, Is.True);
        Assert.That(vm.IsCompareMode, Is.True);
        Assert.That(vm.WindowTitle, Does.Contain("Compare"));
        Assert.That(vm.CompareFlavorChoices, Is.Not.Empty);
        Assert.That(vm.CompareCards.Count, Is.InRange(2, 4));
        Assert.That(vm.CompareCards.All(c => c.IsValid), Is.True, "all cards should be valid");
        Assert.That(vm.CompareExportText, Does.Contain("Flavor Comparison"));
        Assert.That(vm.CompareDifferenceNotes, Is.Not.Empty);

        window.Close();
    }

    [AvaloniaTest]
    public void CompareMode_ExpandsRightPanel_AndRestoresOnLeave()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Width = 1320;
        window.Height = 860;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // Force layout pass so Bounds are non-zero.
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var body = window.FindControl<Avalonia.Controls.Grid>("MainBodyGrid");
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.ColumnDefinitions.Count, Is.GreaterThanOrEqualTo(5));

        // Start on Test (Normal): right should be absolute, center star.
        Assert.That(vm.IsCompareTab, Is.False);
        Assert.That(body.ColumnDefinitions[2].Width.IsStar, Is.True);
        Assert.That(body.ColumnDefinitions[4].Width.IsAbsolute, Is.True);
        var normalRight = body.ColumnDefinitions[4].Width.Value;

        vm.SelectRightTabCommand.Execute("Compare");
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        // Compare: center collapses to a fixed strip; right is star (or large absolute).
        Assert.That(vm.IsCompareTab, Is.True);
        Assert.That(body.ColumnDefinitions[2].Width.IsAbsolute, Is.True,
            "center should shrink to a fixed strip in Compare");
        Assert.That(body.ColumnDefinitions[2].Width.Value,
            Is.EqualTo(RegexCraft.Core.Settings.LayoutDefaults.CenterWidthWhenCompare).Within(1));
        Assert.That(
            body.ColumnDefinitions[4].Width.IsStar
            || body.ColumnDefinitions[4].Width.Value > normalRight + 100,
            "right panel should expand to take most of the body in Compare");

        var rightHost = window.FindControl<Avalonia.Controls.Border>("RightPanelHost");
        Assert.That(rightHost, Is.Not.Null);
        if (rightHost!.Bounds.Width > 0 && body.Bounds.Width > 0)
        {
            var share = rightHost.Bounds.Width / body.Bounds.Width;
            Assert.That(share, Is.GreaterThan(0.45),
                $"Compare right host should dominate body (share={share:F2})");
        }

        vm.SelectRightTabCommand.Execute("Test");
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.That(body.ColumnDefinitions[2].Width.IsStar, Is.True, "center star restored");
        Assert.That(body.ColumnDefinitions[4].Width.IsAbsolute, Is.True, "normal absolute right restored");
        Assert.That(body.ColumnDefinitions[4].Width.Value, Is.EqualTo(normalRight).Within(2));

        window.Close();
    }

    [AvaloniaTest]
    public void ChangeFlavor_UpdatesEngineAndRetests()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var js = vm.Flavors.First(f => f.Id == "javascript");
        vm.SelectedFlavor = js;
        vm.Pattern = @"\d+";
        vm.Subject = "a12b";
        vm.RunTestCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.HasError, Is.False, vm.ErrorText);
        Assert.That(vm.Matches.Count, Is.EqualTo(1));
        Assert.That(vm.Matches[0].Value, Is.EqualTo("12"));
        Assert.That(vm.StatusEngine, Does.Contain("JavaScript").IgnoreCase);
        Assert.That(vm.ShowFidelityBanner, Is.True);
        Assert.That(vm.ExplicitCaptureEnabled, Is.False);
        Assert.That(vm.IgnorePatternWhitespaceEnabled, Is.False);

        var python = vm.Flavors.First(f => f.Id == "python");
        vm.SelectedFlavor = python;
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.ShowFidelityBanner, Is.True);
        Assert.That(vm.FidelityBannerText, Does.Contain("Python").IgnoreCase);

        var go = vm.Flavors.First(f => f.Id == "go");
        vm.SelectedFlavor = go;
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.ShowFidelityBanner, Is.True);
        var lookbehind = vm.TokenCategories.SelectMany(c => c.Tokens)
            .FirstOrDefault(t => t.Token.Id == "pos-lookbehind");
        Assert.That(lookbehind, Is.Not.Null);
        Assert.That(lookbehind!.IsSupported, Is.False);

        window.Close();
    }

    [AvaloniaTest]
    public void ApproximateFlavors_ShowFidelityBanner_InUi()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        foreach (var flavor in vm.Flavors.Where(f =>
                     f.Fidelity is RegexCraft.Core.Flavors.TestingFidelity.Approximate
                         or RegexCraft.Core.Flavors.TestingFidelity.High))
        {
            vm.SelectedFlavor = flavor;
            Dispatcher.UIThread.RunJobs();
            Assert.That(vm.ShowFidelityBanner, Is.True, flavor.Id);
            Assert.That(vm.FidelityBannerText, Is.Not.Empty, flavor.Id);
        }

        window.Close();
    }

    [AvaloniaTest]
    public void EnterPatternAndSubject_ProducesMatchesAndGroups()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.Pattern = @"(?<word>\w+)-(?<num>\d+)";
        vm.Subject = "item-42 and foo-7";
        vm.RunTestCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.HasError, Is.False, vm.ErrorText);
        Assert.That(vm.Matches.Count, Is.EqualTo(2));
        Assert.That(vm.Matches[0].Groups.Any(g => g.Name == "word" && g.Value == "item"), Is.True);
        Assert.That(vm.Matches[0].Groups.Any(g => g.Name == "num" && g.Value == "42"), Is.True);
        Assert.That(vm.CurrentHighlights, Is.Not.Empty);
        Assert.That(vm.AnalysisNodes, Is.Not.Empty);

        window.Close();
    }

    [AvaloniaTest]
    public void LibraryAndHistory_TabsPopulate()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.SelectLeftTabCommand.Execute("Library");
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.IsLibraryTab, Is.True);
        Assert.That(vm.LibraryItems.Count, Is.GreaterThanOrEqualTo(12));
        Assert.That(vm.LibraryItems.Any(i => i.IsBuiltIn), Is.True);

        vm.Pattern = @"history_probe_\d+";
        vm.Subject = "history_probe_1";
        vm.RunTestCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        vm.SelectLeftTabCommand.Execute("History");
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.IsHistoryTab, Is.True);
        Assert.That(vm.HistoryItems.Any(h => h.Entry.Pattern.Contains("history_probe")), Is.True);

        window.Close();
    }

    [AvaloniaTest]
    public void ThemeCycle_UpdatesLabel()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var initial = vm.ThemeLabel;
        vm.CycleThemeCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.ThemeLabel, Is.Not.EqualTo(initial));

        vm.CycleThemeCommand.Execute(null);
        vm.CycleThemeCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        // After three cycles from System: Light → Dark → System (or from wherever we started)
        Assert.That(vm.ThemeLabel, Is.AnyOf("System", "Light", "Dark"));

        window.Close();
    }

    [AvaloniaTest]
    public void GenerateTab_ProducesDefaultCSharpOutput()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.SelectRightTabCommand.Execute("Generate");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.IsGenerateTab, Is.True);
        Assert.That(vm.SelectedCodeLanguage?.Id, Is.EqualTo("csharp"));
        Assert.That(vm.GeneratedCode, Does.Contain("System.Text.RegularExpressions"));
        Assert.That(vm.GeneratedCode, Does.Contain("var pattern").Or.Contain("Regex"));

        window.Close();
    }

    [AvaloniaTest]
    [Retry(2)]
    public void AboutWindow_ShowsCorrectBranding()
    {
        // Warm the dispatcher before creating a second window type (CI race hardening).
        Dispatcher.UIThread.RunJobs();

        var about = new AboutWindow();
        about.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.That(about.Title, Is.EqualTo("About RegexCraft"));
        Assert.That(about.FindControl<TextBlock>("VersionTextBlock")?.Text,
            Does.StartWith("Version "));
        Assert.That(AboutWindow.GetAppVersion(), Does.Match(@"^\d+\.\d+\.\d+(-[A-Za-z0-9.]+)?$"));

        // Icon resource is set on the window
        Assert.That(about.Icon, Is.Not.Null);

        about.Close();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void ReplaceAndSplit_ModesWorkThroughUiHost()
    {
        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.Pattern = @"\d+";
        vm.Subject = "a1b2c3";
        vm.Replacement = "#";
        vm.SelectRightTabCommand.Execute("Replace");
        vm.RunReplaceCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.ReplacePreview, Is.EqualTo("a#b#c#"));
        Assert.That(vm.ReplaceCount, Is.EqualTo(3));

        vm.Pattern = @",\s*";
        vm.Subject = "one, two, three";
        vm.SelectRightTabCommand.Execute("Split");
        vm.RunSplitCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.SplitPartCount, Is.EqualTo(3));

        window.Close();
    }

    [AvaloniaTest]
    public void Application_Name_IsRegexCraft()
    {
        Assert.That(Avalonia.Application.Current?.Name, Is.EqualTo("RegexCraft"));
    }
}
