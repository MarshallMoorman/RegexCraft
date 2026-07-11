using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.App.ViewModels;
using RegexCraft.App.Views;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Headless;

internal static class HeadlessTestHelpers
{
    public static MainWindowViewModel CreateViewModel(string? tempDir = null)
    {
        tempDir ??= Path.Combine(Path.GetTempPath(), "regexcraft-ui-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var engines = EngineFactory.CreateDefaultEngines();
        var flavors = new FlavorService(engines);
        return new MainWindowViewModel(
            flavors,
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(tempDir, "library.json")),
            new JsonHistoryStore(Path.Combine(tempDir, "history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(tempDir, "settings.json")),
            NullLogger<MainWindowViewModel>.Instance);
    }

    public static MainWindow CreateMainWindow(MainWindowViewModel? vm = null)
    {
        vm ??= CreateViewModel();
        var window = new MainWindow
        {
            Width = 1320,
            Height = 860,
            DataContext = vm,
        };
        return window;
    }

    public static string ResolveScreenshotsDirectory()
    {
        // Prefer repo docs/screenshots relative to the solution when running from bin/
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "screenshots");
            if (Directory.Exists(Path.Combine(dir.FullName, "docs"))
                || File.Exists(Path.Combine(dir.FullName, "RegexCraft.sln")))
            {
                var shots = Path.Combine(dir.FullName, "docs", "screenshots");
                Directory.CreateDirectory(shots);
                return shots;
            }

            dir = dir.Parent;
        }

        var fallback = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}
