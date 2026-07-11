using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegexCraft.App.Services;
using RegexCraft.App.ViewModels;
using RegexCraft.App.Views;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Flavors;
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
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _loggerFactory = LoggingBootstrap.CreateLoggerFactory(out IConfiguration configuration);
        _logger = _loggerFactory.CreateLogger<App>();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionText = version is null ? "0.2.0" : $"{version.Major}.{version.Minor}.{version.Build}";

        _logger.LogInformation("RegexCraft {Version} starting", versionText);
        _logger.LogDebug("Configuration loaded. Serilog section present: {HasSerilog}",
            configuration.GetSection("Serilog").Exists());

        var engines = EngineFactory.CreateDefaultEngines(_loggerFactory);
        var flavorService = new FlavorService(engines, _loggerFactory.CreateLogger<FlavorService>());
        var tokenCatalog = new TokenCatalog();
        var analysisService = new RegexAnalysisService();

        _logger.LogInformation(
            "Registered {EngineCount} engines: {Engines}",
            engines.Count,
            string.Join(", ", engines.Select(e => e.Id)));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    flavorService,
                    tokenCatalog,
                    analysisService,
                    _loggerFactory.CreateLogger<MainWindowViewModel>()),
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
}
