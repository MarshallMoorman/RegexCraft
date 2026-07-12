using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using RegexCraft.App.Views;

namespace RegexCraft.Tests.Headless;

/// <summary>
/// Captures high-quality PNGs of main app states for README and docs.
/// Run: dotnet test --filter Category=Screenshots
/// Output: docs/screenshots/*.png
/// </summary>
[TestFixture]
[Category("Screenshots")]
[Category("UI")]
[Category("Headless")]
public sealed class ScreenshotTests
{
    private string _tempDir = null!;
    private string _shotsDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "regexcraft-shots-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _shotsDir = HeadlessTestHelpers.ResolveScreenshotsDirectory();
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
    public void Capture_MainWindow_TestMode_Light()
    {
        CaptureMainWindow("main-test-light.png", "Test", ThemeVariant.Light, prepare: vm =>
        {
            vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
            vm.Subject = "Contact us at support@regexcraft.com or hello@example.org today.\nAlso try: admin@regexcraft.com";
            vm.RunTestCommand.Execute(null);
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_TestMode_Dark()
    {
        CaptureMainWindow("main-test-dark.png", "Test", ThemeVariant.Dark, prepare: vm =>
        {
            vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
            vm.Subject = "Contact us at support@regexcraft.com or hello@example.org today.\nAlso try: admin@regexcraft.com";
            vm.RunTestCommand.Execute(null);
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_ReplaceMode()
    {
        CaptureMainWindow("main-replace.png", "Replace", ThemeVariant.Light, prepare: vm =>
        {
            vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
            vm.Subject = "mail: a@b.com and c@d.org";
            vm.Replacement = "[${user}]";
            vm.RunReplaceCommand.Execute(null);
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_GenerateMode()
    {
        CaptureMainWindow("main-generate.png", "Generate", ThemeVariant.Light, prepare: vm =>
        {
            vm.Pattern = @"\d{3}-\d{2}-\d{4}";
            vm.Subject = "SSN-shaped 123-45-6789";
            vm.SelectRightTabCommand.Execute("Generate");
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_CompareMode()
    {
        CaptureMainWindow("main-compare.png", "Compare", ThemeVariant.Light, prepare: vm =>
        {
            vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
            vm.Subject = "Contact support@regexcraft.com or hello@example.org";
            vm.SelectRightTabCommand.Execute("Compare");
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_GrepMode()
    {
        CaptureMainWindow("main-grep.png", "Grep", ThemeVariant.Light, prepare: vm =>
        {
            vm.Pattern = @"\d+";
            vm.GrepRootPath = _tempDir;
            vm.GrepIncludeGlobs = "*.txt";
            vm.GrepSummary = "Select a folder and click Search.";
        });
    }

    [AvaloniaTest]
    public void Capture_MainWindow_LibrarySidebar()
    {
        CaptureMainWindow("main-library.png", "Test", ThemeVariant.Light, prepare: vm =>
        {
            vm.SelectLeftTabCommand.Execute("Library");
            vm.Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
            vm.Subject = "support@regexcraft.com";
            vm.RunTestCommand.Execute(null);
        });
    }

    [AvaloniaTest]
    public void Capture_AboutDialog_Light()
    {
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Light;

        var about = new AboutWindow
        {
            Width = 440,
            Height = 460,
        };
        about.Show();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        SaveFrame(about, "about-light.png");
        about.Close();
    }

    [AvaloniaTest]
    public void Capture_AboutDialog_Dark()
    {
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

        var about = new AboutWindow
        {
            Width = 440,
            Height = 460,
        };
        about.Show();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        SaveFrame(about, "about-dark.png");
        about.Close();

        // Restore light for subsequent tests
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Light;
    }

    private void CaptureMainWindow(
        string fileName,
        string modeTab,
        ThemeVariant theme,
        Action<App.ViewModels.MainWindowViewModel>? prepare = null)
    {
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = theme;

        var vm = HeadlessTestHelpers.CreateViewModel(_tempDir);
        // Align VM theme label with the requested variant
        var target = theme == ThemeVariant.Dark ? "Dark" : "Light";
        for (var i = 0; i < 4 && vm.ThemeLabel != target; i++)
            vm.CycleThemeCommand.Execute(null);

        // Force application theme again after VM cycle (VM also sets it)
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = theme;

        prepare?.Invoke(vm);
        vm.SelectRightTabCommand.Execute(modeTab);

        var window = HeadlessTestHelpers.CreateMainWindow(vm);
        window.Width = 1320;
        window.Height = 860;
        window.RequestedThemeVariant = theme;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // Re-assert theme after open (window chrome init can race with first paint)
        if (Avalonia.Application.Current is not null)
            Avalonia.Application.Current.RequestedThemeVariant = theme;
        window.RequestedThemeVariant = theme;
        for (var i = 0; i < 4 && vm.ThemeLabel != target; i++)
            vm.CycleThemeCommand.Execute(null);
        vm.ReapplyThemeFromSettings();
        Dispatcher.UIThread.RunJobs();

        // Let editors/layout settle
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        SaveFrame(window, fileName);
        window.Close();
    }

    private void SaveFrame(Window window, string fileName)
    {
        var frame = window.CaptureRenderedFrame();
        Assert.That(frame, Is.Not.Null, $"CaptureRenderedFrame returned null for {fileName}");
        Assert.That(frame!.PixelSize.Width, Is.GreaterThan(100), fileName);
        Assert.That(frame.PixelSize.Height, Is.GreaterThan(100), fileName);

        var path = Path.Combine(_shotsDir, fileName);
#pragma warning disable CS0618 // Save(string) still works; BitmapEncoderOptions overload varies by Avalonia patch
        frame.Save(path);
#pragma warning restore CS0618
        TestContext.Out.WriteLine($"Screenshot saved: {path} ({frame.PixelSize.Width}x{frame.PixelSize.Height})");
        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(5_000), $"Screenshot too small: {path}");
    }
}
