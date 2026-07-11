using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Analysis;
using RegexCraft.Core.Editing;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Highlighting;
using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;

namespace RegexCraft.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFlavorService _flavorService;
    private readonly ITokenCatalog _tokenCatalog;
    private readonly IRegexAnalysisService _analysisService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _debounceCts;
    private const int DebounceMs = 200;

    public MainWindowViewModel(
        IFlavorService flavorService,
        ITokenCatalog tokenCatalog,
        IRegexAnalysisService analysisService,
        ILogger<MainWindowViewModel> logger)
    {
        _flavorService = flavorService;
        _tokenCatalog = tokenCatalog;
        _analysisService = analysisService;
        _logger = logger;

        Flavors = new ObservableCollection<FlavorDefinition>(_flavorService.GetFlavors());
        SelectedFlavor = Flavors.FirstOrDefault();

        Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
        Subject = "Contact us at support@regexcraft.com or hello@example.org today.\nAlso try: admin@regexcraft.com";
        Replacement = "[$1]";

        VersionText = $"v{GetAppVersion()}";
        RebuildTokenList();
        RefreshAnalysis();
        RunTestCore(live: false);

        _logger.LogInformation("MainWindowViewModel initialized (Phase 1 shell)");
    }

    public MainWindowViewModel()
        : this(
            new FlavorService(Array.Empty<IRegexEngine>()),
            new TokenCatalog(),
            new RegexAnalysisService(),
            NullLogger<MainWindowViewModel>.Instance)
    {
    }

    /// <summary>Raised when a token should be inserted into the pattern editor.</summary>
    public event Action<string, int?>? InsertTokenRequested;

    /// <summary>Raised when match highlights should be applied to the subject editor.</summary>
    public event Action? HighlightsChanged;

    public ObservableCollection<FlavorDefinition> Flavors { get; }
    public ObservableCollection<TokenCategoryViewModel> TokenCategories { get; } = new();
    public ObservableCollection<MatchItemViewModel> Matches { get; } = new();
    public ObservableCollection<AnalysisNode> AnalysisNodes { get; } = new();
    public string VersionText { get; }

    public IReadOnlyList<HighlightSpan> CurrentHighlights { get; private set; } = Array.Empty<HighlightSpan>();

    [ObservableProperty] private FlavorDefinition? _selectedFlavor;
    [ObservableProperty] private string _pattern = string.Empty;
    [ObservableProperty] private string _subject = string.Empty;
    [ObservableProperty] private string _replacement = string.Empty;
    [ObservableProperty] private string _replacePreview = string.Empty;
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _statusEngine = "—";
    [ObservableProperty] private string _statusMatches = "Matches: —";
    [ObservableProperty] private string _statusTime = "Time: —";
    [ObservableProperty] private string _errorText = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _ignoreCase;
    [ObservableProperty] private bool _multiline;
    [ObservableProperty] private bool _singleline;
    [ObservableProperty] private bool _explicitCapture;
    [ObservableProperty] private bool _ignorePatternWhitespace;
    [ObservableProperty] private string _themeLabel = "System";
    [ObservableProperty] private string _tokenSearch = string.Empty;
    [ObservableProperty] private string _rightPanelTab = "Test";
    [ObservableProperty] private bool _isTestTab = true;
    [ObservableProperty] private bool _isReplaceTab;
    [ObservableProperty] private AnalysisNode? _analysisRoot;
    [ObservableProperty] private int _patternCaretOffset;
    [ObservableProperty] private int _patternSelectionLength;
    [ObservableProperty] private int _replaceCount;
    [ObservableProperty] private bool _optionsExpanded;

    partial void OnPatternChanged(string value) => ScheduleLiveUpdate();
    partial void OnSubjectChanged(string value) => ScheduleLiveUpdate();
    partial void OnReplacementChanged(string value) => ScheduleLiveUpdate();
    partial void OnIgnoreCaseChanged(bool value) => ScheduleLiveUpdate();
    partial void OnMultilineChanged(bool value) => ScheduleLiveUpdate();
    partial void OnSinglelineChanged(bool value) => ScheduleLiveUpdate();
    partial void OnExplicitCaptureChanged(bool value) => ScheduleLiveUpdate();
    partial void OnIgnorePatternWhitespaceChanged(bool value) => ScheduleLiveUpdate();

    partial void OnSelectedFlavorChanged(FlavorDefinition? value)
    {
        if (value is null) return;
        _logger.LogInformation("Flavor selected: {FlavorId}", value.Id);
        StatusEngine = $"Flavor: {value.DisplayName} | Engine: {value.EngineId}";
        ScheduleLiveUpdate();
    }

    partial void OnTokenSearchChanged(string value) => RebuildTokenList();

    [RelayCommand]
    private void RunTest() => RunTestCore(live: false);

    [RelayCommand]
    private void RunReplace() => RunReplaceCore();

    [RelayCommand]
    private void InsertToken(TokenItemViewModel? item)
    {
        if (item is null) return;
        _logger.LogDebug("Insert token {Id}: {Text}", item.Token.Id, item.InsertText);

        // Prefer editor-driven insert when the view is listening (real caret).
        if (InsertTokenRequested is not null)
        {
            InsertTokenRequested.Invoke(item.InsertText, item.Token.CaretOffsetInInsert);
            return;
        }

        // Headless / tests: pure string insertion using caret snapshot.
        var result = TokenInsertion.Insert(
            Pattern,
            PatternCaretOffset,
            PatternSelectionLength,
            item.InsertText,
            item.Token.CaretOffsetInInsert);

        Pattern = result.NewText;
        PatternCaretOffset = result.NewCaretOffset;
        PatternSelectionLength = 0;
    }

    [RelayCommand]
    private void SelectRightTab(string? tab)
    {
        if (string.IsNullOrWhiteSpace(tab))
            return;

        RightPanelTab = tab!;
        IsTestTab = tab == "Test";
        IsReplaceTab = tab == "Replace";

        if (IsReplaceTab)
            RunReplaceCore(live: false);
    }

    [RelayCommand]
    private void CycleTheme()
    {
        var app = Application.Current;
        if (app is null) return;

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
    }

    [RelayCommand]
    private void CopyPattern()
    {
        // Clipboard requires top-level; status only here — view can also wire.
        StatusText = "Pattern ready to copy (use OS clipboard from editor selection)";
    }

    [RelayCommand]
    private void ToggleOptions() => OptionsExpanded = !OptionsExpanded;

    private void ScheduleLiveUpdate()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceMs, token);
                if (token.IsCancellationRequested) return;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested) return;
                    RefreshAnalysis();
                    RunTestCore(live: true);
                    if (RightPanelTab == "Replace")
                        RunReplaceCore(live: true);
                });
            }
            catch (TaskCanceledException)
            {
                // expected
            }
        }, token);
    }

    private void RebuildTokenList()
    {
        TokenCategories.Clear();
        var found = _tokenCatalog.Search(TokenSearch);
        foreach (var group in found.GroupBy(t => t.Category))
        {
            TokenCategories.Add(new TokenCategoryViewModel(
                group.Key,
                group.Select(t => new TokenItemViewModel(t))));
        }
    }

    private void RefreshAnalysis()
    {
        AnalysisRoot = _analysisService.Analyze(Pattern);
        AnalysisNodes.Clear();
        if (AnalysisRoot is null)
            return;

        if (AnalysisRoot.Children.Count > 0)
        {
            foreach (var child in AnalysisRoot.Children)
                AnalysisNodes.Add(child);
        }
        else
        {
            AnalysisNodes.Add(AnalysisRoot);
        }
    }

    private void RunTestCore(bool live)
    {
        var engine = ResolveEngine();
        if (engine is null)
        {
            HasError = true;
            ErrorText = "No engine available for the selected flavor.";
            Matches.Clear();
            CurrentHighlights = Array.Empty<HighlightSpan>();
            HighlightsChanged?.Invoke();
            StatusMatches = "Matches: —";
            StatusTime = "Time: —";
            return;
        }

        StatusEngine = $"Flavor: {SelectedFlavor!.DisplayName} | Engine: {engine.DisplayName}";
        var options = BuildOptions();
        var result = engine.Match(Pattern, Subject, options);

        if (!result.Success)
        {
            HasError = true;
            ErrorText = result.ErrorMessage ?? "Invalid pattern";
            Matches.Clear();
            CurrentHighlights = Array.Empty<HighlightSpan>();
            HighlightsChanged?.Invoke();
            StatusMatches = "Matches: error";
            StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
            StatusText = live ? "Live test — pattern error" : "Test failed";
            if (!live)
                _logger.LogWarning("Match failed on {Engine}: {Error}", engine.Id, result.ErrorMessage);
            return;
        }

        HasError = false;
        ErrorText = string.Empty;
        Matches.Clear();
        for (var i = 0; i < result.Matches.Count; i++)
            Matches.Add(new MatchItemViewModel(i, result.Matches[i]));

        CurrentHighlights = MatchHighlightBuilder.Build(result, includeGroups: true);
        HighlightsChanged?.Invoke();

        StatusMatches = $"Matches: {result.Matches.Count}";
        StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
        StatusText = live
            ? $"Live — {result.Matches.Count} match(es) via {engine.DisplayName}"
            : $"Test OK — {result.Matches.Count} match(es)";

        if (!live)
            _logger.LogInformation("Match: {Count} via {Engine} in {Ms:F2}ms",
                result.Matches.Count, engine.Id, result.Duration.TotalMilliseconds);
    }

    private void RunReplaceCore(bool live = false)
    {
        var engine = ResolveEngine();
        if (engine is null)
        {
            ReplacePreview = string.Empty;
            return;
        }

        var result = engine.Replace(Pattern, Subject, Replacement, BuildOptions());
        if (!result.Success)
        {
            HasError = true;
            ErrorText = result.ErrorMessage ?? "Replace failed";
            ReplacePreview = string.Empty;
            ReplaceCount = 0;
            StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
            return;
        }

        // Keep match error state separate if only replace ran; clear if ok
        if (HasError && string.IsNullOrEmpty(ErrorText))
            HasError = false;

        ReplacePreview = result.Result;
        ReplaceCount = result.ReplacementCount;
        StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
        StatusText = live
            ? $"Live replace — {result.ReplacementCount} replacement(s)"
            : $"Replace OK — {result.ReplacementCount} replacement(s)";

        if (!live)
            _logger.LogInformation("Replace: {Count} via {Engine}", result.ReplacementCount, engine.Id);
    }

    private IRegexEngine? ResolveEngine()
    {
        if (SelectedFlavor is null)
            return null;
        return _flavorService.GetEngineForFlavor(SelectedFlavor.Id);
    }

    private RegexOptionsEx BuildOptions()
    {
        var o = RegexOptionsEx.None;
        if (IgnoreCase) o |= RegexOptionsEx.IgnoreCase;
        if (Multiline) o |= RegexOptionsEx.Multiline;
        if (Singleline) o |= RegexOptionsEx.Singleline;
        if (ExplicitCapture) o |= RegexOptionsEx.ExplicitCapture;
        if (IgnorePatternWhitespace) o |= RegexOptionsEx.IgnorePatternWhitespace;
        return o;
    }

    private static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "0.2.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
