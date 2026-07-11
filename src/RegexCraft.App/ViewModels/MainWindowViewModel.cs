using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;

namespace RegexCraft.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFlavorService _flavorService;
    private readonly ILogger<MainWindowViewModel> _logger;

    public MainWindowViewModel(IFlavorService flavorService, ILogger<MainWindowViewModel> logger)
    {
        _flavorService = flavorService;
        _logger = logger;

        Flavors = new ObservableCollection<FlavorDefinition>(_flavorService.GetFlavors());
        SelectedFlavor = Flavors.FirstOrDefault();

        Pattern = @"(\w+)@(\w+)\.(\w+)";
        Subject = "Contact us at support@regexcraft.com or hello@example.org today.";
        Replacement = "[$1]";

        VersionText = $"v{GetAppVersion()}";
        StatusText = "Ready";

        _logger.LogInformation("MainWindowViewModel initialized with {FlavorCount} flavors", Flavors.Count);
    }

    /// <summary>Design-time / fallback constructor.</summary>
    public MainWindowViewModel()
        : this(
            new FlavorService(Array.Empty<IRegexEngine>()),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MainWindowViewModel>.Instance)
    {
    }

    public ObservableCollection<FlavorDefinition> Flavors { get; }

    public string VersionText { get; }

    [ObservableProperty]
    private FlavorDefinition? _selectedFlavor;

    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _replacement = string.Empty;

    [ObservableProperty]
    private string _resultsText = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _ignoreCase;

    [ObservableProperty]
    private bool _multiline;

    [ObservableProperty]
    private bool _singleline;

    [ObservableProperty]
    private string _themeLabel = "System";

    partial void OnSelectedFlavorChanged(FlavorDefinition? value)
    {
        if (value is not null)
        {
            _logger.LogInformation("Engine/flavor selected: {FlavorId} ({DisplayName})", value.Id, value.DisplayName);
            StatusText = $"Engine: {value.DisplayName}";
        }
    }

    [RelayCommand]
    private void RunMatch()
    {
        var engine = ResolveEngine();
        if (engine is null)
            return;

        _logger.LogInformation("Running Match with engine {EngineId}", engine.Id);
        var options = BuildOptions();
        var result = engine.Match(Pattern, Subject, options);

        if (!result.Success)
        {
            ResultsText = $"Error ({engine.DisplayName}):\n{result.ErrorMessage}";
            StatusText = "Match failed";
            _logger.LogWarning("Match failed on {EngineId}: {Error}", engine.Id, result.ErrorMessage);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Engine: {engine.DisplayName}");
        sb.AppendLine($"Matches: {result.Matches.Count}  |  Duration: {result.Duration.TotalMilliseconds:F2} ms");
        sb.AppendLine(new string('-', 48));

        for (var i = 0; i < result.Matches.Count; i++)
        {
            var m = result.Matches[i];
            sb.AppendLine($"[{i}] Index={m.Index}, Length={m.Length}, Value=\"{m.Value}\"");
            foreach (var g in m.Groups)
            {
                if (g.Number == 0)
                    continue;
                var label = g.Name != g.Number.ToString() ? $"{g.Number}/{g.Name}" : g.Number.ToString();
                if (g.Success)
                    sb.AppendLine($"    Group {label}: \"{g.Value}\" (Index={g.Index}, Length={g.Length})");
                else
                    sb.AppendLine($"    Group {label}: (no match)");
            }
        }

        if (result.Matches.Count == 0)
            sb.AppendLine("(no matches)");

        ResultsText = sb.ToString();
        StatusText = $"Match OK — {result.Matches.Count} match(es)";
        _logger.LogInformation("Match completed: {Count} match(es) in {Ms:F2}ms", result.Matches.Count, result.Duration.TotalMilliseconds);
    }

    [RelayCommand]
    private void RunReplace()
    {
        var engine = ResolveEngine();
        if (engine is null)
            return;

        _logger.LogInformation("Running Replace with engine {EngineId}", engine.Id);
        var options = BuildOptions();
        var result = engine.Replace(Pattern, Subject, Replacement, options);

        if (!result.Success)
        {
            ResultsText = $"Error ({engine.DisplayName}):\n{result.ErrorMessage}";
            StatusText = "Replace failed";
            _logger.LogWarning("Replace failed on {EngineId}: {Error}", engine.Id, result.ErrorMessage);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Engine: {engine.DisplayName}");
        sb.AppendLine($"Replacements: {result.ReplacementCount}  |  Duration: {result.Duration.TotalMilliseconds:F2} ms");
        sb.AppendLine(new string('-', 48));
        sb.AppendLine("Result:");
        sb.AppendLine(result.Result);

        ResultsText = sb.ToString();
        StatusText = $"Replace OK — {result.ReplacementCount} replacement(s)";
        _logger.LogInformation(
            "Replace completed: {Count} replacement(s) in {Ms:F2}ms",
            result.ReplacementCount,
            result.Duration.TotalMilliseconds);
    }

    [RelayCommand]
    private void CycleTheme()
    {
        var app = Application.Current;
        if (app is null)
            return;

        // Cycle: System → Light → Dark → System
        if (app.RequestedThemeVariant is null || app.RequestedThemeVariant == ThemeVariant.Default)
        {
            app.RequestedThemeVariant = ThemeVariant.Light;
            ThemeLabel = "Light";
        }
        else if (app.RequestedThemeVariant == ThemeVariant.Light)
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
            ThemeLabel = "Dark";
        }
        else
        {
            app.RequestedThemeVariant = ThemeVariant.Default;
            ThemeLabel = "System";
        }

        _logger.LogInformation("Theme set to {Theme}", ThemeLabel);
        StatusText = $"Theme: {ThemeLabel}";
    }

    private IRegexEngine? ResolveEngine()
    {
        if (SelectedFlavor is null)
        {
            ResultsText = "No flavor selected.";
            StatusText = "Select a flavor";
            return null;
        }

        var engine = _flavorService.GetEngineForFlavor(SelectedFlavor.Id);
        if (engine is null)
        {
            ResultsText = $"No engine registered for flavor '{SelectedFlavor.Id}'.";
            StatusText = "Engine missing";
            _logger.LogError("No engine for flavor {FlavorId}", SelectedFlavor.Id);
        }

        return engine;
    }

    private RegexOptionsEx BuildOptions()
    {
        var options = RegexOptionsEx.None;
        if (IgnoreCase) options |= RegexOptionsEx.IgnoreCase;
        if (Multiline) options |= RegexOptionsEx.Multiline;
        if (Singleline) options |= RegexOptionsEx.Singleline;
        return options;
    }

    private static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "0.1.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
