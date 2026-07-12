using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegexCraft.App.Services;
using RegexCraft.App.ViewModels;
using RegexCraft.App.Views;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Compare;
using RegexCraft.Core.Debug;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Library;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;
using RegexCraft.Engines;

namespace RegexCraft.App;

public partial class App : Application
{
    private ILoggerFactory? _loggerFactory;
    private ILogger<App>? _logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // macOS menu bar / system chrome uses Application.Name; default is "Avalonia Application".
        Name = "RegexCraft";
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _loggerFactory = LoggingBootstrap.CreateLoggerFactory(out IConfiguration configuration);
        _logger = _loggerFactory.CreateLogger<App>();

        var asm = Assembly.GetExecutingAssembly();
        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        string versionText;
        if (!string.IsNullOrWhiteSpace(infoVersion))
        {
            var plus = infoVersion.IndexOf('+');
            versionText = plus > 0 ? infoVersion[..plus] : infoVersion;
        }
        else
        {
            var version = asm.GetName().Version;
            versionText = version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        _logger.LogInformation("RegexCraft {Version} starting", versionText);
        _logger.LogDebug("Configuration loaded. Serilog section present: {HasSerilog}",
            configuration.GetSection("Serilog").Exists());

        var engines = EngineFactory.CreateDefaultEngines(_loggerFactory);
        var flavorService = new FlavorService(engines, _loggerFactory.CreateLogger<FlavorService>());
        var tokenCatalog = new TokenCatalog();
        var analysisService = new RegexAnalysisService();
        var codeGeneration = new CodeGenerationService();
        var libraryStore = new JsonLibraryStore(logger: _loggerFactory.CreateLogger<JsonLibraryStore>());
        var historyStore = new JsonHistoryStore(logger: _loggerFactory.CreateLogger<JsonHistoryStore>());
        var grepService = new GrepService(_loggerFactory.CreateLogger<GrepService>());
        var compareService = new RegexCompareService(
            flavorService, tokenCatalog, _loggerFactory.CreateLogger<RegexCompareService>());
        var debugService = new RegexDebugService(
            analysisService, _loggerFactory.CreateLogger<RegexDebugService>());
        var settingsStore = new JsonSettingsStore(logger: _loggerFactory.CreateLogger<JsonSettingsStore>());

        _logger.LogInformation(
            "Registered {EngineCount} engines: {Engines}",
            engines.Count,
            string.Join(", ", engines.Select(e => e.Id)));
        _logger.LogInformation("Library/History data directory: {Dir}", AppDataPaths.GetDataDirectory());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                Title = "RegexCraft",
                DataContext = new MainWindowViewModel(
                    flavorService,
                    tokenCatalog,
                    analysisService,
                    codeGeneration,
                    libraryStore,
                    historyStore,
                    grepService,
                    settingsStore,
                    _loggerFactory.CreateLogger<MainWindowViewModel>(),
                    compareService,
                    debugService),
            };

            desktop.Exit += (_, _) =>
            {
                _logger?.LogInformation("RegexCraft shutting down");
                LoggingBootstrap.Shutdown();
                _loggerFactory?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
        _logger.LogInformation("RegexCraft UI ready");
    }

    /// <summary>Shows the custom About RegexCraft dialog (replaces Avalonia's default About).</summary>
    private void AboutMenuItem_OnClick(object? sender, EventArgs e) => ShowAboutDialog();

    public static void ShowAboutDialog(Window? owner = null)
    {
        var about = new AboutWindow();
        if (owner is not null)
        {
            _ = about.ShowDialog(owner);
            return;
        }

        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            _ = about.ShowDialog(desktop.MainWindow);
            return;
        }

        about.Show();
    }
}
