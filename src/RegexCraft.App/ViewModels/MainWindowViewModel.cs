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
using RegexCraft.Core.Codegen;
using RegexCraft.Core.Compare;
using RegexCraft.Core.Editing;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Grep;
using RegexCraft.Core.Highlighting;
using RegexCraft.Core.Library;
using RegexCraft.Core.Models;
using RegexCraft.Core.Settings;
using RegexCraft.Core.Tokens;

namespace RegexCraft.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFlavorService _flavorService;
    private readonly ITokenCatalog _tokenCatalog;
    private readonly IRegexAnalysisService _analysisService;
    private readonly ICodeGenerationService _codeGeneration;
    private readonly ILibraryStore _libraryStore;
    private readonly IHistoryStore _historyStore;
    private readonly IGrepService _grepService;
    private readonly IRegexCompareService _compareService;
    private readonly ISettingsStore _settingsStore;
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _debounceCts;
    private CancellationTokenSource? _grepCts;
    private const int DebounceMs = 200;
    private const int CompareMinFlavors = RegexCompareService.MinFlavors;
    private const int CompareMaxFlavors = RegexCompareService.MaxFlavors;
    private string? _lastHistoryPattern;
    private string? _lastHistorySubject;
    private string? _lastHistoryFlavor;
    private AppSettings _settings;
    private bool _suppressSettingsSave;

    public MainWindowViewModel(
        IFlavorService flavorService,
        ITokenCatalog tokenCatalog,
        IRegexAnalysisService analysisService,
        ICodeGenerationService codeGeneration,
        ILibraryStore libraryStore,
        IHistoryStore historyStore,
        IGrepService grepService,
        ISettingsStore settingsStore,
        ILogger<MainWindowViewModel> logger,
        IRegexCompareService? compareService = null)
    {
        _flavorService = flavorService;
        _tokenCatalog = tokenCatalog;
        _analysisService = analysisService;
        _codeGeneration = codeGeneration;
        _libraryStore = libraryStore;
        _historyStore = historyStore;
        _grepService = grepService;
        _settingsStore = settingsStore;
        _logger = logger;
        _compareService = compareService ?? new RegexCompareService(flavorService, tokenCatalog);
        _settings = _settingsStore.Load();

        // Suppress until all settings-backed properties are applied; otherwise
        // SelectedFlavor / option property changers overwrite theme (and other prefs)
        // with defaults before ApplyThemeFromSettings runs.
        _suppressSettingsSave = true;

        Flavors = new ObservableCollection<FlavorDefinition>(_flavorService.GetFlavors());
        SelectedFlavor = Flavors.FirstOrDefault(f => f.Id == _settings.FlavorId)
            ?? Flavors.FirstOrDefault();

        foreach (var lang in _codeGeneration.SupportedLanguages)
            CodeLanguages.Add(new CodeLanguageItem(lang.Id(), lang.DisplayName()));
        SelectedCodeLanguage = CodeLanguages.FirstOrDefault();

        CodegenOperations.Add(new CodegenOperationItem("IsMatch", "IsMatch"));
        CodegenOperations.Add(new CodegenOperationItem("Match", "Match (first)"));
        CodegenOperations.Add(new CodegenOperationItem("Matches", "Matches (all)"));
        CodegenOperations.Add(new CodegenOperationItem("Replace", "Replace"));
        CodegenOperations.Add(new CodegenOperationItem("Split", "Split"));
        SelectedCodegenOperation = CodegenOperations.FirstOrDefault(o => o.Id == "Matches")
            ?? CodegenOperations.FirstOrDefault();

        Pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
        Subject = "Contact us at support@regexcraft.com or hello@example.org today.\nAlso try: admin@regexcraft.com";
        Replacement = "[$1]";
        IgnoreCase = _settings.IgnoreCase;
        Multiline = _settings.Multiline;
        Singleline = _settings.Singleline;
        ExplicitCapture = _settings.ExplicitCapture;
        IgnorePatternWhitespace = _settings.IgnorePatternWhitespace;
        OptionsExpanded = _settings.OptionsExpanded;
        GrepRootPath = _settings.LastGrepRoot ?? string.Empty;
        GrepIncludeGlobs = string.IsNullOrWhiteSpace(_settings.GrepIncludeGlobs)
            ? "*.cs;*.json;*.md;*.txt;*.xml;*.yml;*.yaml;*.html;*.js;*.ts;*.py"
            : _settings.GrepIncludeGlobs;
        GrepExcludeGlobs = string.IsNullOrWhiteSpace(_settings.GrepExcludeGlobs)
            ? "bin/**;obj/**;.git/**;node_modules/**;*.dll;*.exe;*.pdb"
            : _settings.GrepExcludeGlobs;
        GrepRecursive = _settings.GrepRecursive;
        GrepCreateBackup = _settings.GrepCreateBackup;
        ApplyThemeFromSettings(_settings.Theme);
        _suppressSettingsSave = false;

        VersionText = $"v{GetAppVersion()}";
        WindowTitle = "RegexCraft";
        InitializeCompareFlavorChoices();
        RebuildTokenList();
        RefreshLibrary();
        RefreshHistory();
        RefreshAnalysis();
        UpdateFlavorUiState();
        RunTestCore(live: false);
        RefreshGeneratedCode(force: true);
        UpdateOptionsEnabledState();

        _logger.LogInformation(
            "MainWindowViewModel initialized (v{Version}, {FlavorCount} flavors, theme={Theme})",
            VersionText,
            Flavors.Count,
            ThemeLabel);
    }

    /// <summary>Design-time / test convenience constructor.</summary>
    public MainWindowViewModel()
        : this(
            new FlavorService(Array.Empty<IRegexEngine>()),
            new TokenCatalog(),
            new RegexAnalysisService(),
            new CodeGenerationService(),
            new JsonLibraryStore(Path.Combine(Path.GetTempPath(), "regexcraft-design-library.json")),
            new JsonHistoryStore(Path.Combine(Path.GetTempPath(), "regexcraft-design-history.json")),
            new GrepService(),
            new JsonSettingsStore(Path.Combine(Path.GetTempPath(), "regexcraft-design-settings.json")),
            NullLogger<MainWindowViewModel>.Instance)
    {
    }

    public event Action<string, int?>? InsertTokenRequested;
    public event Action? HighlightsChanged;
    public event Action<int, int>? SelectPatternRangeRequested;
    public event Action<int, int>? SelectSubjectRangeRequested;
    public event Action<string>? CopyTextRequested;
    /// <summary>Raised when GREP preview text/highlights change (view updates preview editor).</summary>
    public event Action? GrepPreviewChanged;
    /// <summary>Raised to request a folder picker; view sets GrepRootPath.</summary>
    public event Func<Task<string?>>? PickFolderRequested;
    /// <summary>
    /// Raised after a right-panel mode switch so the view can apply Normal vs Compare widths.
    /// Argument is the previous tab name (before the switch).
    /// </summary>
    public event Action<string>? RightPanelModeChanged;

    public ObservableCollection<FlavorDefinition> Flavors { get; }
    public ObservableCollection<TokenCategoryViewModel> TokenCategories { get; } = new();
    public ObservableCollection<MatchItemViewModel> Matches { get; } = new();
    public ObservableCollection<AnalysisNode> AnalysisNodes { get; } = new();
    public ObservableCollection<LibraryItemViewModel> LibraryItems { get; } = new();
    public ObservableCollection<HistoryItemViewModel> HistoryItems { get; } = new();
    public ObservableCollection<SplitPartViewModel> SplitParts { get; } = new();
    public ObservableCollection<CodeLanguageItem> CodeLanguages { get; } = new();
    public ObservableCollection<CodegenOperationItem> CodegenOperations { get; } = new();
    public ObservableCollection<GrepHitViewModel> GrepHits { get; } = new();
    public ObservableCollection<CompareFlavorChoiceViewModel> CompareFlavorChoices { get; } = new();
    public ObservableCollection<CompareCardViewModel> CompareCards { get; } = new();
    public ObservableCollection<string> CompareDifferenceNotes { get; } = new();
    public string VersionText { get; }

    public IReadOnlyList<HighlightSpan> CurrentHighlights { get; private set; } = Array.Empty<HighlightSpan>();
    public IReadOnlyList<HighlightSpan> ReplaceHighlights { get; private set; } = Array.Empty<HighlightSpan>();
    public IReadOnlyList<HighlightSpan> GrepPreviewHighlights { get; private set; } = Array.Empty<HighlightSpan>();

    /// <summary>Window geometry from settings (applied by the view on open).</summary>
    public AppSettings LoadedSettings => _settings;

    [ObservableProperty] private string _windowTitle = "RegexCraft";
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
    [ObservableProperty] private bool _explicitCaptureEnabled = true;
    [ObservableProperty] private bool _ignorePatternWhitespaceEnabled = true;
    [ObservableProperty] private bool _ignoreCaseEnabled = true;
    [ObservableProperty] private bool _multilineEnabled = true;
    [ObservableProperty] private bool _singlelineEnabled = true;
    [ObservableProperty] private string _themeLabel = "System";
    [ObservableProperty] private string _tokenSearch = string.Empty;
    [ObservableProperty] private string _rightPanelTab = "Test";
    [ObservableProperty] private bool _isTestTab = true;
    [ObservableProperty] private bool _isReplaceTab;
    [ObservableProperty] private bool _isSplitTab;
    [ObservableProperty] private bool _isGenerateTab;
    [ObservableProperty] private bool _isGrepTab;
    [ObservableProperty] private bool _isCompareTab;
    [ObservableProperty] private AnalysisNode? _analysisRoot;
    [ObservableProperty] private AnalysisNode? _selectedAnalysisNode;
    [ObservableProperty] private int _patternCaretOffset;
    [ObservableProperty] private int _patternSelectionLength;
    [ObservableProperty] private int _replaceCount;
    [ObservableProperty] private bool _optionsExpanded = true;
    [ObservableProperty] private bool _removeEmptySplitEntries;
    [ObservableProperty] private int _splitPartCount;
    [ObservableProperty] private string _generatedCode = string.Empty;
    [ObservableProperty] private CodeLanguageItem? _selectedCodeLanguage;
    [ObservableProperty] private CodegenOperationItem? _selectedCodegenOperation;
    [ObservableProperty] private bool _showFidelityBanner;
    [ObservableProperty] private string _fidelityBannerText = string.Empty;
    [ObservableProperty] private string _flavorTooltip = "Select regex flavor";
    [ObservableProperty] private string _librarySearch = string.Empty;
    [ObservableProperty] private string _historySearch = string.Empty;
    [ObservableProperty] private string _historyEmptyMessage = "History is empty. Patterns appear here after you test them.";
    [ObservableProperty] private string _libraryName = string.Empty;
    [ObservableProperty] private string _libraryDescription = string.Empty;
    [ObservableProperty] private string _libraryCategory = string.Empty;
    [ObservableProperty] private string _libraryTags = string.Empty;
    [ObservableProperty] private bool _libraryFavorite;
    [ObservableProperty] private string _emptyMatchesMessage = "No matches yet — enter a pattern and subject.";
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _leftSidebarTab = "Tokens";
    [ObservableProperty] private bool _isTokensTab = true;
    [ObservableProperty] private bool _isLibraryTab;
    [ObservableProperty] private bool _isHistoryTab;
    [ObservableProperty] private string _modeLabel = "Match";
    [ObservableProperty] private bool _isMatchMode = true;
    [ObservableProperty] private bool _isReplaceMode;
    [ObservableProperty] private bool _isSplitMode;
    [ObservableProperty] private bool _isGenerateMode;
    [ObservableProperty] private bool _isGrepMode;
    [ObservableProperty] private bool _isCompareMode;
    [ObservableProperty] private string _optionsContextLabel = "Options apply to the current engine";
    [ObservableProperty] private string _shortcutHints = "Ctrl+Enter run · Ctrl+1–6 modes";

    // GREP
    [ObservableProperty] private string _grepRootPath = string.Empty;
    [ObservableProperty] private string _grepIncludeGlobs = "*.cs;*.json;*.md;*.txt";
    [ObservableProperty] private string _grepExcludeGlobs = "bin/**;obj/**;.git/**;node_modules/**";
    [ObservableProperty] private bool _grepRecursive = true;
    [ObservableProperty] private bool _grepDryRun = true;
    [ObservableProperty] private bool _grepCreateBackup = true;
    [ObservableProperty] private bool _isGrepRunning;
    [ObservableProperty] private double _grepProgressValue;
    [ObservableProperty] private string _grepProgressText = string.Empty;
    [ObservableProperty] private string _grepSummary = "Select a folder and click Search.";
    [ObservableProperty] private string _grepEmptyMessage = "No GREP results yet. Choose a folder, then Search.";
    [ObservableProperty] private GrepHitViewModel? _selectedGrepHit;
    [ObservableProperty] private string _grepPreviewText = string.Empty;
    [ObservableProperty] private string _grepPreviewPath = string.Empty;
    [ObservableProperty] private string _grepReplaceSummary = string.Empty;

    // Compare
    [ObservableProperty] private bool _isCompareRunning;
    [ObservableProperty] private string _compareSummary = "Select 2–4 flavors and run Compare.";
    [ObservableProperty] private string _compareEmptyMessage = "Select 2–4 flavors below, then compare the current pattern and subject.";
    [ObservableProperty] private string _compareSelectionHint = "Select 2–4 flavors";
    [ObservableProperty] private string _compareExportText = string.Empty;
    [ObservableProperty] private bool _compareShowCodeSnippets;

    partial void OnPatternChanged(string value)
    {
        ScheduleLiveUpdate();
        if (IsGenerateTab)
            RefreshGeneratedCode();
        else
            RefreshGeneratedCode();
    }

    partial void OnSubjectChanged(string value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnReplacementChanged(string value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnIgnoreCaseChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
        PersistSettings();
    }

    partial void OnMultilineChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
        PersistSettings();
    }

    partial void OnSinglelineChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
        PersistSettings();
    }

    partial void OnExplicitCaptureChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
        PersistSettings();
    }

    partial void OnIgnorePatternWhitespaceChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
        PersistSettings();
    }

    partial void OnRemoveEmptySplitEntriesChanged(bool value) => ScheduleLiveUpdate();
    partial void OnOptionsExpandedChanged(bool value) => PersistSettings();

    partial void OnSelectedFlavorChanged(FlavorDefinition? value)
    {
        if (value is null) return;
        _logger.LogInformation("Flavor selected: {FlavorId} (engine={EngineId}, fidelity={Fidelity})",
            value.Id, value.EngineId, value.Fidelity);
        UpdateFlavorUiState();
        UpdateOptionsEnabledState();
        ApplyPreferredCodegenLanguage(value);
        RebuildTokenList();
        ScheduleLiveUpdate();
        RefreshGeneratedCode(force: true);
        PersistSettings();
        UpdateWindowTitle();
    }

    partial void OnTokenSearchChanged(string value) => RebuildTokenList();
    partial void OnLibrarySearchChanged(string value) => RefreshLibrary();
    partial void OnHistorySearchChanged(string value) => RefreshHistory();
    partial void OnSelectedCodeLanguageChanged(CodeLanguageItem? value) => RefreshGeneratedCode(force: true);
    partial void OnSelectedCodegenOperationChanged(CodegenOperationItem? value) => RefreshGeneratedCode(force: true);

    partial void OnSelectedAnalysisNodeChanged(AnalysisNode? value)
    {
        if (value is null || !value.HasRange) return;
        SelectPatternRangeRequested?.Invoke(value.StartIndex, value.Length);
    }

    partial void OnSelectedGrepHitChanged(GrepHitViewModel? value)
    {
        if (value is null)
        {
            GrepPreviewText = string.Empty;
            GrepPreviewPath = string.Empty;
            GrepPreviewHighlights = Array.Empty<HighlightSpan>();
            GrepPreviewChanged?.Invoke();
            return;
        }

        _ = LoadGrepPreviewAsync(value);
    }

    partial void OnGrepRootPathChanged(string value) => PersistSettings();
    partial void OnGrepIncludeGlobsChanged(string value) => PersistSettings();
    partial void OnGrepExcludeGlobsChanged(string value) => PersistSettings();
    partial void OnGrepRecursiveChanged(bool value) => PersistSettings();
    partial void OnGrepCreateBackupChanged(bool value) => PersistSettings();

    [RelayCommand]
    private void RunTest() => RunTestCore(live: false);

    [RelayCommand]
    private void RunReplace() => RunReplaceCore(live: false);

    [RelayCommand]
    private void RunSplit() => RunSplitCore(live: false);

    [RelayCommand]
    private void InsertToken(TokenItemViewModel? item)
    {
        if (item is null) return;
        _logger.LogDebug("Insert token {Id}: {Text}", item.Token.Id, item.InsertText);

        if (InsertTokenRequested is not null)
        {
            InsertTokenRequested.Invoke(item.InsertText, item.Token.CaretOffsetInInsert);
            return;
        }

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

        var previousTab = RightPanelTab;
        var modeChanged = !string.Equals(previousTab, tab, StringComparison.Ordinal);

        RightPanelTab = tab!;
        IsTestTab = tab == "Test";
        IsReplaceTab = tab == "Replace";
        IsSplitTab = tab == "Split";
        IsGenerateTab = tab == "Generate";
        IsGrepTab = tab == "Grep";
        IsCompareTab = tab == "Compare";

        IsMatchMode = IsTestTab;
        IsReplaceMode = IsReplaceTab;
        IsSplitMode = IsSplitTab;
        IsGenerateMode = IsGenerateTab;
        IsGrepMode = IsGrepTab;
        IsCompareMode = IsCompareTab;
        ModeLabel = tab switch
        {
            "Replace" => "Replace",
            "Split" => "Split",
            "Generate" => "Generate",
            "Grep" => "GREP",
            "Compare" => "Compare",
            _ => "Match",
        };

        UpdateWindowTitle();

        // Notify the view so it can capture the previous mode's width and apply the new one.
        // Only when Compare enters or leaves does the width typically change; the view still
        // captures on every mode switch so manual resizes stay associated with Normal vs Compare.
        if (modeChanged)
            RightPanelModeChanged?.Invoke(previousTab);

        if (IsReplaceTab)
            RunReplaceCore(live: false);
        else if (IsSplitTab)
            RunSplitCore(live: false);
        else if (IsGenerateTab)
        {
            RefreshGeneratedCode(force: true);
            StatusText = $"Generate — {SelectedCodeLanguage?.DisplayName ?? "C#"} snippet ready";
        }
        else if (IsTestTab)
            RunTestCore(live: false);
        else if (IsGrepTab)
            StatusText = string.IsNullOrWhiteSpace(GrepRootPath)
                ? "GREP — pick a folder to search"
                : $"GREP ready — {GrepRootPath}";
        else if (IsCompareTab)
            RunCompareCore(live: false);
    }

    /// <summary>
    /// Target pixel width for the right panel given the current (or specified) mode.
    /// Pass <paramref name="bodyWidth"/> when available so Compare can claim ~72% of the body.
    /// </summary>
    public double GetTargetRightPanelWidth(bool? compareMode = null, double bodyWidth = 0)
    {
        var compare = compareMode ?? IsCompareTab;
        return LayoutDefaults.ResolveRightPanelWidth(
            compare,
            _settings.RightPanelNormalWidth,
            _settings.RightPanelCompareWidth,
            bodyWidth);
    }

    /// <summary>
    /// Remember the user-dragged (or programmatically measured) right-panel width for a mode.
    /// Stale narrow Compare widths from older builds are not re-persisted as "user choice"
    /// when the live measurement is still below the usable floor — the next open re-expands.
    /// </summary>
    public void RememberRightPanelWidth(double width, bool compareMode)
    {
        if (width is <= 0 or double.NaN or double.PositiveInfinity)
            return;

        if (compareMode)
        {
            // Don't lock in a too-narrow width; leave null so the next enter uses the share formula.
            if (width < LayoutDefaults.RightPanelCompareMin)
                return;
            _settings.RightPanelCompareWidth = LayoutDefaults.ClampCompare(width);
        }
        else
        {
            _settings.RightPanelNormalWidth = LayoutDefaults.ClampNormal(width);
        }

        PersistSettings();
    }

    [RelayCommand]
    private void SelectLeftTab(string? tab)
    {
        if (string.IsNullOrWhiteSpace(tab))
            return;

        LeftSidebarTab = tab!;
        IsTokensTab = tab == "Tokens";
        IsLibraryTab = tab == "Library";
        IsHistoryTab = tab == "History";

        if (IsLibraryTab)
            RefreshLibrary();
        if (IsHistoryTab)
            RefreshHistory();
    }

    [RelayCommand]
    private void CycleTheme()
    {
        // Cycle preference label (not Avalonia's current effective variant) so System is stable.
        ThemeLabel = ThemeLabel switch
        {
            "System" => "Light",
            "Light" => "Dark",
            _ => "System",
        };

        ApplyThemeToApplication();
        _logger.LogInformation("Theme set to {Theme}", ThemeLabel);
        PersistSettings();
    }

    /// <summary>
    /// Re-applies the current theme preference to the running application.
    /// Called from the view on open so Avalonia has finished initializing.
    /// Uses <see cref="ThemeLabel"/> (source of truth after load/cycle), not a re-read of disk.
    /// </summary>
    public void ReapplyThemeFromSettings()
    {
        ApplyThemeToApplication();
    }

    [RelayCommand]
    private void ToggleOptions() => OptionsExpanded = !OptionsExpanded;

    [RelayCommand]
    private void CopyGeneratedCode()
    {
        if (string.IsNullOrEmpty(GeneratedCode))
        {
            StatusText = "Nothing to copy";
            return;
        }

        CopyTextRequested?.Invoke(GeneratedCode);
        StatusText = "Generated code copied to clipboard";
        _logger.LogInformation("Code generation copied ({Lang}, {Op})",
            SelectedCodeLanguage?.Id, SelectedCodegenOperation?.Id);
    }

    [RelayCommand]
    private void CopyPattern()
    {
        if (string.IsNullOrEmpty(Pattern)) return;
        CopyTextRequested?.Invoke(Pattern);
        StatusText = "Pattern copied to clipboard";
    }

    [RelayCommand]
    private void CopyMatchValue(MatchItemViewModel? match)
    {
        if (match is null) return;
        CopyTextRequested?.Invoke(match.Value);
        StatusText = "Match copied to clipboard";
    }

    [RelayCommand]
    private void SelectMatchInSubject(MatchItemViewModel? match)
    {
        if (match is null) return;
        SelectSubjectRangeRequested?.Invoke(match.Start, match.Length);
    }

    [RelayCommand]
    private void SaveToLibrary()
    {
        var name = string.IsNullOrWhiteSpace(LibraryName)
            ? (Pattern.Length <= 32 ? Pattern : Pattern[..29] + "…")
            : LibraryName.Trim();

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(Pattern))
        {
            StatusText = "Nothing to save — enter a pattern first";
            return;
        }

        var entry = new LibraryEntry
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Untitled pattern" : name,
            Description = LibraryDescription?.Trim() ?? string.Empty,
            Pattern = Pattern,
            Subject = Subject,
            Replacement = Replacement,
            FlavorId = SelectedFlavor?.Id ?? "dotnet",
            IgnoreCase = IgnoreCase,
            Multiline = Multiline,
            Singleline = Singleline,
            ExplicitCapture = ExplicitCapture,
            IgnorePatternWhitespace = IgnorePatternWhitespace,
            Category = LibraryCategory?.Trim() ?? string.Empty,
            Tags = LibraryTags?.Trim() ?? string.Empty,
            IsFavorite = LibraryFavorite,
        };

        _libraryStore.Save(entry);
        LibraryName = string.Empty;
        LibraryDescription = string.Empty;
        LibraryCategory = string.Empty;
        LibraryTags = string.Empty;
        LibraryFavorite = false;
        RefreshLibrary();
        StatusText = $"Saved to library: {entry.Name}";
        _logger.LogInformation("Library save: {Name}", entry.Name);
    }

    [RelayCommand]
    private void LoadLibraryItem(LibraryItemViewModel? item)
    {
        if (item is null) return;
        var e = item.Entry;
        Pattern = e.Pattern;
        Subject = e.Subject;
        Replacement = e.Replacement ?? string.Empty;
        IgnoreCase = e.IgnoreCase;
        Multiline = e.Multiline;
        Singleline = e.Singleline;
        ExplicitCapture = e.ExplicitCapture;
        IgnorePatternWhitespace = e.IgnorePatternWhitespace;

        if (!string.IsNullOrEmpty(e.FlavorId))
        {
            var flavor = Flavors.FirstOrDefault(f => f.Id == e.FlavorId);
            if (flavor is not null)
                SelectedFlavor = flavor;
        }

        SelectRightTab("Test");
        StatusText = $"Loaded library item: {item.Name}";
        _logger.LogInformation("Library load: {Id}", e.Id);
    }

    [RelayCommand]
    private void DeleteLibraryItem(LibraryItemViewModel? item)
    {
        if (item is null) return;
        if (item.IsBuiltIn)
        {
            StatusText = "Built-in patterns cannot be deleted";
            return;
        }

        if (_libraryStore.Delete(item.Id))
        {
            RefreshLibrary();
            StatusText = $"Deleted: {item.Name}";
        }
    }

    [RelayCommand]
    private void ToggleLibraryFavorite(LibraryItemViewModel? item)
    {
        if (item is null) return;
        var e = item.Entry;
        e.IsFavorite = !e.IsFavorite;
        _libraryStore.Save(e);
        RefreshLibrary();
        StatusText = e.IsFavorite ? $"Favorited: {e.Name}" : $"Unfavorited: {e.Name}";
    }

    [RelayCommand]
    private void LoadHistoryItem(HistoryItemViewModel? item)
    {
        if (item is null) return;
        var e = item.Entry;
        Pattern = e.Pattern;
        if (!string.IsNullOrEmpty(e.Subject))
            Subject = e.Subject;
        if (!string.IsNullOrEmpty(e.Replacement))
            Replacement = e.Replacement;

        if (!string.IsNullOrEmpty(e.FlavorId))
        {
            var flavor = Flavors.FirstOrDefault(f => f.Id == e.FlavorId);
            if (flavor is not null)
                SelectedFlavor = flavor;
        }

        SelectRightTab("Test");
        StatusText = "Restored from history";
    }

    [RelayCommand]
    private void ClearHistory()
    {
        _historyStore.Clear();
        RefreshHistory();
        StatusText = "History cleared";
    }

    [RelayCommand]
    private async Task BrowseGrepFolderAsync()
    {
        if (PickFolderRequested is null)
        {
            StatusText = "Folder picker unavailable";
            return;
        }

        var path = await PickFolderRequested();
        if (!string.IsNullOrWhiteSpace(path))
        {
            GrepRootPath = path;
            StatusText = $"GREP folder: {path}";
        }
    }

    [RelayCommand]
    private async Task RunGrepSearchAsync()
    {
        var engine = ResolveEngine();
        if (engine is null)
        {
            HasError = true;
            ErrorText = "No engine available for the selected flavor.";
            return;
        }

        if (string.IsNullOrWhiteSpace(GrepRootPath) || !Directory.Exists(GrepRootPath))
        {
            HasError = true;
            ErrorText = "Select an existing folder to search.";
            GrepSummary = "Folder required";
            return;
        }

        _grepCts?.Cancel();
        _grepCts = new CancellationTokenSource();
        var token = _grepCts.Token;

        IsGrepRunning = true;
        GrepHits.Clear();
        SelectedGrepHit = null;
        GrepPreviewText = string.Empty;
        GrepProgressValue = 0;
        GrepProgressText = "Starting…";
        GrepSummary = "Searching…";
        GrepEmptyMessage = "Searching…";
        HasError = false;
        ErrorText = string.Empty;
        StatusText = "GREP search running…";

        var request = new GrepSearchRequest
        {
            RootPath = GrepRootPath,
            Pattern = Pattern,
            Options = BuildOptions(),
            Recursive = GrepRecursive,
            IncludeGlobs = GrepIncludeGlobs,
            ExcludeGlobs = GrepExcludeGlobs,
        };

        var progress = new Progress<GrepProgress>(p =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                GrepProgressValue = p.FilesScanned;
                GrepProgressText = p.CurrentFile is null
                    ? p.Phase
                    : $"{p.Phase}: {Path.GetFileName(p.CurrentFile)} ({p.FilesScanned} files, {p.MatchCount} hits)";
                GrepSummary = $"{p.MatchCount} hit(s) in {p.FilesMatched} file(s) · scanned {p.FilesScanned}";
            });
        });

        try
        {
            var result = await _grepService.SearchAsync(engine, request, progress, token)
                .ConfigureAwait(true);

            GrepHits.Clear();
            foreach (var hit in result.Matches)
                GrepHits.Add(new GrepHitViewModel(hit));

            if (!result.Success)
            {
                HasError = true;
                ErrorText = result.ErrorMessage ?? "GREP failed";
                GrepSummary = "Search failed";
                GrepEmptyMessage = result.ErrorMessage ?? "Search failed.";
                StatusText = "GREP search failed";
            }
            else
            {
                HasError = false;
                var cancelNote = result.Cancelled ? " (cancelled)" : string.Empty;
                GrepSummary =
                    $"{result.Matches.Count} hit(s) in {result.FilesMatched} file(s) · scanned {result.FilesScanned} · {result.Duration.TotalMilliseconds:F0} ms{cancelNote}";
                GrepEmptyMessage = result.Matches.Count == 0
                    ? "No matches in the selected folder with the current include/exclude filters."
                    : string.Empty;
                StatusMatches = $"Hits: {result.Matches.Count}";
                StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
                StatusText = result.Cancelled
                    ? $"GREP cancelled — {result.Matches.Count} hit(s) so far"
                    : $"GREP OK — {result.Matches.Count} hit(s) via {engine.DisplayName}";
                GrepProgressText = result.Cancelled ? "Cancelled" : "Done";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorText = ex.Message;
            StatusText = "GREP search error";
            _logger.LogError(ex, "GREP search UI error");
        }
        finally
        {
            IsGrepRunning = false;
        }
    }

    [RelayCommand]
    private void CancelGrep()
    {
        _grepCts?.Cancel();
        StatusText = "Cancelling GREP…";
        GrepProgressText = "Cancelling…";
    }

    [RelayCommand]
    private async Task RunGrepReplaceAsync()
    {
        var engine = ResolveEngine();
        if (engine is null)
        {
            HasError = true;
            ErrorText = "No engine available.";
            return;
        }

        if (string.IsNullOrWhiteSpace(GrepRootPath) || !Directory.Exists(GrepRootPath))
        {
            HasError = true;
            ErrorText = "Select an existing folder for replace.";
            return;
        }

        if (!GrepDryRun)
        {
            // Confirmation is handled by the view when possible; here we still require dry-run first ideally
            _logger.LogWarning("GREP replace writing files under {Root} (backup={Backup})",
                GrepRootPath, GrepCreateBackup);
        }

        _grepCts?.Cancel();
        _grepCts = new CancellationTokenSource();
        var token = _grepCts.Token;

        IsGrepRunning = true;
        GrepProgressText = GrepDryRun ? "Dry-run replace…" : "Replacing files…";
        GrepReplaceSummary = string.Empty;
        StatusText = GrepDryRun ? "GREP dry-run replace…" : "GREP replace writing files…";

        var request = new GrepReplaceRequest
        {
            RootPath = GrepRootPath,
            Pattern = Pattern,
            Replacement = Replacement,
            Options = BuildOptions(),
            Recursive = GrepRecursive,
            IncludeGlobs = GrepIncludeGlobs,
            ExcludeGlobs = GrepExcludeGlobs,
            DryRun = GrepDryRun,
            CreateBackup = GrepCreateBackup,
        };

        var progress = new Progress<GrepProgress>(p =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                GrepProgressValue = p.FilesScanned;
                GrepProgressText = p.CurrentFile is null
                    ? p.Phase
                    : $"{p.Phase}: {Path.GetFileName(p.CurrentFile)}";
            });
        });

        try
        {
            var result = await _grepService.ReplaceAsync(engine, request, progress, token)
                .ConfigureAwait(true);

            if (!result.Success)
            {
                HasError = true;
                ErrorText = result.ErrorMessage ?? "Replace failed";
                GrepReplaceSummary = "Replace failed";
                StatusText = "GREP replace failed";
            }
            else
            {
                HasError = false;
                var mode = result.DryRun ? "Dry-run" : "Applied";
                var cancel = result.Cancelled ? " (cancelled)" : string.Empty;
                GrepReplaceSummary =
                    $"{mode}: {result.TotalReplacements} replacement(s) in {result.FilesModified} file(s) · scanned {result.FilesScanned} · {result.Duration.TotalMilliseconds:F0} ms{cancel}";
                StatusMatches = $"Replacements: {result.TotalReplacements}";
                StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
                StatusText = GrepReplaceSummary;
                GrepProgressText = result.Cancelled ? "Cancelled" : "Done";

                // Refresh search results after a real write
                if (!result.DryRun && !result.Cancelled)
                    await RunGrepSearchAsync();
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorText = ex.Message;
            StatusText = "GREP replace error";
            _logger.LogError(ex, "GREP replace UI error");
        }
        finally
        {
            IsGrepRunning = false;
        }
    }

    [RelayCommand]
    private void OpenGrepHitInSubject(GrepHitViewModel? hit)
    {
        if (hit is null) return;
        // Load the match line into subject for interactive testing
        Subject = hit.LineText;
        SelectRightTab("Test");
        StatusText = $"Opened line from {hit.FileName}:{hit.LineNumber} in Test";
    }

    public void PersistWindowBounds(double width, double height, int x, int y)
    {
        _settings.WindowWidth = width;
        _settings.WindowHeight = height;
        _settings.WindowX = x;
        _settings.WindowY = y;
        PersistSettings();
    }

    /// <summary>
    /// Persist current in-memory panel widths (and other settings) — used when the window closes
    /// after a final capture of the live column width.
    /// </summary>
    public void PersistRightPanelWidthsToStore() => PersistSettings();

    private async Task LoadGrepPreviewAsync(GrepHitViewModel hit)
    {
        try
        {
            GrepPreviewPath = $"{hit.Hit.FilePath}:{hit.LineNumber}";
            if (!File.Exists(hit.Hit.FilePath))
            {
                GrepPreviewText = "(file missing)";
                GrepPreviewHighlights = Array.Empty<HighlightSpan>();
                GrepPreviewChanged?.Invoke();
                return;
            }

            var text = await File.ReadAllTextAsync(hit.Hit.FilePath).ConfigureAwait(true);
            // Cap preview size for UI responsiveness
            const int maxPreview = 200_000;
            if (text.Length > maxPreview)
                text = text[..maxPreview] + "\n… [preview truncated]";

            GrepPreviewText = text;

            var engine = ResolveEngine();
            if (engine is not null)
            {
                var matchResult = engine.Match(Pattern, text, BuildOptions());
                GrepPreviewHighlights = matchResult.Success
                    ? MatchHighlightBuilder.Build(matchResult, includeGroups: true)
                    : Array.Empty<HighlightSpan>();
            }
            else
            {
                GrepPreviewHighlights = Array.Empty<HighlightSpan>();
            }

            GrepPreviewChanged?.Invoke();
        }
        catch (Exception ex)
        {
            GrepPreviewText = $"// Failed to load preview: {ex.Message}";
            GrepPreviewHighlights = Array.Empty<HighlightSpan>();
            GrepPreviewChanged?.Invoke();
        }
    }

    private void ScheduleLiveUpdate()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        // Unit tests (no Avalonia Application) must not touch Dispatcher.UIThread —
        // doing so races headless Avalonia init and fails CI with thread-affinity errors.
        if (Application.Current is null)
        {
            if (token.IsCancellationRequested) return;
            RunLiveUpdateCore(token);
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceMs, token);
                if (token.IsCancellationRequested) return;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested) return;
                    RunLiveUpdateCore(token);
                });
            }
            catch (TaskCanceledException)
            {
                // expected
            }
        }, token);
    }

    private void RunLiveUpdateCore(CancellationToken token)
    {
        if (token.IsCancellationRequested) return;
        RefreshAnalysis();
        RefreshGeneratedCode();
        if (IsGrepTab)
            return; // don't spam live match while grepping UI
        if (IsCompareTab)
        {
            RunCompareCore(live: true);
            return;
        }
        RunTestCore(live: true);
        if (IsReplaceTab)
            RunReplaceCore(live: true);
        if (IsSplitTab)
            RunSplitCore(live: true);
    }

    private void RebuildTokenList()
    {
        TokenCategories.Clear();
        var flavor = SelectedFlavor;
        var engineId = flavor?.EngineId;
        var found = _tokenCatalog.Search(TokenSearch);
        foreach (var group in found.GroupBy(t => t.Category))
        {
            TokenCategories.Add(new TokenCategoryViewModel(
                group.Key,
                group.Select(t => new TokenItemViewModel(t, flavor, engineId))));
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

    private void RefreshLibrary()
    {
        LibraryItems.Clear();
        foreach (var e in _libraryStore.Search(LibrarySearch))
            LibraryItems.Add(new LibraryItemViewModel(e));
    }

    private void RefreshHistory()
    {
        HistoryItems.Clear();
        var q = (HistorySearch ?? string.Empty).Trim();
        var any = false;
        foreach (var e in _historyStore.GetRecent())
        {
            if (q.Length > 0 &&
                !ContainsIgnoreCase(e.Pattern, q) &&
                !ContainsIgnoreCase(e.Subject, q) &&
                !ContainsIgnoreCase(e.Replacement, q) &&
                !ContainsIgnoreCase(e.FlavorId, q))
            {
                continue;
            }

            HistoryItems.Add(new HistoryItemViewModel(e));
            any = true;
        }

        HistoryEmptyMessage = q.Length > 0 && !any
            ? "No history entries match your search."
            : "History is empty. Patterns appear here after you test them.";
    }

    private static bool ContainsIgnoreCase(string? source, string query) =>
        !string.IsNullOrEmpty(source) &&
        source.Contains(query, StringComparison.OrdinalIgnoreCase);

    private void RefreshGeneratedCode(bool force = false)
    {
        if (SelectedCodeLanguage is null || SelectedCodegenOperation is null)
        {
            if (force || GeneratedCode.Length > 0)
                GeneratedCode = string.Empty;
            return;
        }

        var lang = ParseLanguage(SelectedCodeLanguage.Id);
        var op = ParseOperation(SelectedCodegenOperation.Id);
        var engineId = SelectedFlavor?.EngineId ?? "dotnet";

        string code;
        try
        {
            code = _codeGeneration.Generate(
                lang, op, Pattern, Subject, Replacement, BuildOptions(), engineId);
        }
        catch (Exception ex)
        {
            code = $"// Code generation error: {ex.Message}";
            _logger.LogWarning(ex, "Code generation failed");
        }

        // Force PropertyChanged when the editor needs a re-sync (e.g. Generate tab shown)
        // even if the generated text is identical.
        if (force && string.Equals(code, GeneratedCode, StringComparison.Ordinal))
        {
            GeneratedCode = code + "\u200b"; // zero-width space pulse
            GeneratedCode = code;
        }
        else
        {
            GeneratedCode = code;
        }
    }

    private static CodeLanguage ParseLanguage(string id) => id switch
    {
        "csharp" => CodeLanguage.CSharp,
        "javascript" => CodeLanguage.JavaScript,
        "typescript" => CodeLanguage.TypeScript,
        "python" => CodeLanguage.Python,
        "php" => CodeLanguage.Php,
        "java" => CodeLanguage.Java,
        "go" => CodeLanguage.Go,
        "rust" => CodeLanguage.Rust,
        "ruby" => CodeLanguage.Ruby,
        "perl" => CodeLanguage.Perl,
        "kotlin" => CodeLanguage.Kotlin,
        "swift" => CodeLanguage.Swift,
        _ => CodeLanguage.CSharp,
    };

    private static CodegenOperation ParseOperation(string id) => id switch
    {
        "IsMatch" => CodegenOperation.IsMatch,
        "Match" => CodegenOperation.Match,
        "Matches" => CodegenOperation.Matches,
        "Replace" => CodegenOperation.Replace,
        "Split" => CodegenOperation.Split,
        _ => CodegenOperation.Matches,
    };

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
            EmptyMatchesMessage = "No engine available.";
            return;
        }

        IsRunning = true;
        UpdateStatusEngine(engine);
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
            EmptyMatchesMessage = "Pattern error — fix the regex to see matches.";
            IsRunning = false;
            if (!live)
                _logger.LogWarning("Match failed on {Engine}: {Error}", engine.Id, result.ErrorMessage);
            return;
        }

        HasError = false;
        ErrorText = string.Empty;
        Matches.Clear();
        for (var i = 0; i < result.Matches.Count; i++)
        {
            Matches.Add(new MatchItemViewModel(
                i,
                result.Matches[i],
                text => CopyTextRequested?.Invoke(text),
                (start, len) => SelectSubjectRangeRequested?.Invoke(start, len)));
        }

        CurrentHighlights = MatchHighlightBuilder.Build(result, includeGroups: true);
        if (IsTestTab)
            HighlightsChanged?.Invoke();

        StatusMatches = $"Matches: {result.Matches.Count}";
        StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
        StatusText = live
            ? $"Live — {result.Matches.Count} match(es) via {engine.DisplayName}"
            : $"Test OK — {result.Matches.Count} match(es)";
        EmptyMatchesMessage = result.Matches.Count == 0
            ? "No matches — try adjusting the pattern or subject."
            : string.Empty;

        RecordHistoryIfNeeded(live, result.Matches.Count > 0 || !live);
        IsRunning = false;

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
            ReplaceHighlights = Array.Empty<HighlightSpan>();
            return;
        }

        IsRunning = true;
        var result = engine.Replace(Pattern, Subject, Replacement, BuildOptions());
        if (!result.Success)
        {
            HasError = true;
            ErrorText = result.ErrorMessage ?? "Replace failed";
            ReplacePreview = string.Empty;
            ReplaceCount = 0;
            ReplaceHighlights = Array.Empty<HighlightSpan>();
            StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
            StatusText = live ? "Live replace — error" : "Replace failed";
            IsRunning = false;
            return;
        }

        if (IsReplaceTab)
        {
            HasError = false;
            ErrorText = string.Empty;
        }

        ReplacePreview = result.Result;
        ReplaceCount = result.ReplacementCount;
        ReplaceHighlights = ReplaceHighlightBuilder.Build(result);
        if (IsReplaceTab)
        {
            CurrentHighlights = ReplaceHighlights;
            HighlightsChanged?.Invoke();
        }

        StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
        StatusMatches = $"Replacements: {result.ReplacementCount}";
        StatusText = live
            ? $"Live replace — {result.ReplacementCount} replacement(s)"
            : $"Replace OK — {result.ReplacementCount} replacement(s)";
        IsRunning = false;

        if (!live)
            _logger.LogInformation("Replace: {Count} via {Engine}", result.ReplacementCount, engine.Id);
    }

    private void RunSplitCore(bool live = false)
    {
        var engine = ResolveEngine();
        if (engine is null)
        {
            SplitParts.Clear();
            SplitPartCount = 0;
            return;
        }

        IsRunning = true;
        var result = engine.Split(Pattern, Subject, BuildOptions(), RemoveEmptySplitEntries);
        SplitParts.Clear();

        if (!result.Success)
        {
            HasError = true;
            ErrorText = result.ErrorMessage ?? "Split failed";
            SplitPartCount = 0;
            StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
            StatusText = live ? "Live split — error" : "Split failed";
            IsRunning = false;
            return;
        }

        if (IsSplitTab)
        {
            HasError = false;
            ErrorText = string.Empty;
        }

        for (var i = 0; i < result.Parts.Count; i++)
            SplitParts.Add(new SplitPartViewModel(i, result.Parts[i]));

        SplitPartCount = result.Parts.Count;
        var delimHighlights = ReplaceHighlightBuilder.BuildSplitDelimiters(result);
        if (IsSplitTab)
        {
            CurrentHighlights = delimHighlights;
            HighlightsChanged?.Invoke();
        }

        StatusTime = $"Time: {result.Duration.TotalMilliseconds:F2} ms";
        StatusMatches = $"Parts: {result.Parts.Count}";
        StatusText = live
            ? $"Live split — {result.Parts.Count} part(s)"
            : $"Split OK — {result.Parts.Count} part(s)";
        IsRunning = false;

        if (!live)
            _logger.LogInformation("Split: {Count} parts via {Engine}", result.Parts.Count, engine.Id);
    }

    private void RecordHistoryIfNeeded(bool live, bool meaningful)
    {
        if (!meaningful) return;
        if (string.IsNullOrWhiteSpace(Pattern)) return;

        var flavorId = SelectedFlavor?.Id ?? "dotnet";
        if (live
            && Pattern == _lastHistoryPattern
            && Subject == _lastHistorySubject
            && flavorId == _lastHistoryFlavor)
            return;

        if (live && Pattern == _lastHistoryPattern && flavorId == _lastHistoryFlavor)
            return;

        _historyStore.Add(new HistoryEntry
        {
            Pattern = Pattern,
            Subject = Subject,
            Replacement = Replacement,
            FlavorId = flavorId,
        });

        _lastHistoryPattern = Pattern;
        _lastHistorySubject = Subject;
        _lastHistoryFlavor = flavorId;
        RefreshHistory();
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

        // Drop options the current flavor does not support (e.g. free-spacing on JS).
        if (SelectedFlavor is not null)
            o = SelectedFlavor.FilterOptions(o);

        return o;
    }

    private void UpdateOptionsEnabledState()
    {
        var flavor = SelectedFlavor;
        if (flavor is null)
        {
            IgnoreCaseEnabled = true;
            MultilineEnabled = true;
            SinglelineEnabled = true;
            ExplicitCaptureEnabled = true;
            IgnorePatternWhitespaceEnabled = true;
            return;
        }

        IgnoreCaseEnabled = flavor.SupportsOption(RegexOptionsEx.IgnoreCase);
        MultilineEnabled = flavor.SupportsOption(RegexOptionsEx.Multiline);
        SinglelineEnabled = flavor.SupportsOption(RegexOptionsEx.Singleline);
        ExplicitCaptureEnabled = flavor.SupportsOption(RegexOptionsEx.ExplicitCapture);
        IgnorePatternWhitespaceEnabled = flavor.SupportsOption(RegexOptionsEx.IgnorePatternWhitespace);

        // Clear toggles that are no longer applicable so UI state matches engine input.
        if (!IgnoreCaseEnabled) IgnoreCase = false;
        if (!MultilineEnabled) Multiline = false;
        if (!SinglelineEnabled) Singleline = false;
        if (!ExplicitCaptureEnabled) ExplicitCapture = false;
        if (!IgnorePatternWhitespaceEnabled) IgnorePatternWhitespace = false;
    }

    private void ApplyPreferredCodegenLanguage(FlavorDefinition flavor)
    {
        if (string.IsNullOrWhiteSpace(flavor.CodegenLanguageId))
            return;

        var match = CodeLanguages.FirstOrDefault(l =>
            string.Equals(l.Id, flavor.CodegenLanguageId, StringComparison.OrdinalIgnoreCase));
        if (match is not null && !ReferenceEquals(SelectedCodeLanguage, match))
            SelectedCodeLanguage = match;
    }

    private void UpdateWindowTitle()
    {
        WindowTitle = ModeLabel switch
        {
            "GREP" => "RegexCraft — GREP",
            "Replace" => "RegexCraft — Replace",
            "Split" => "RegexCraft — Split",
            "Generate" => "RegexCraft — Generate",
            "Compare" => "RegexCraft — Compare",
            _ => "RegexCraft",
        };
    }

    private void ApplyThemeFromSettings(string? theme)
    {
        ThemeLabel = theme switch
        {
            "Light" => "Light",
            "Dark" => "Dark",
            "System" => "System",
            _ => "System",
        };

        ApplyThemeToApplication();
    }

    private void ApplyThemeToApplication()
    {
        var app = Application.Current;
        if (app is null) return;

        app.RequestedThemeVariant = ThemeLabel switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    private void UpdateFlavorUiState()
    {
        var flavor = SelectedFlavor;
        if (flavor is null)
        {
            ShowFidelityBanner = false;
            FidelityBannerText = string.Empty;
            FlavorTooltip = "Select regex flavor";
            OptionsContextLabel = "Options apply to the current engine";
            StatusEngine = "—";
            return;
        }

        var engine = _flavorService.GetEngine(flavor.EngineId);
        var engineName = engine?.DisplayName ?? flavor.EngineId;
        StatusEngine = $"Flavor: {flavor.StatusLabel} | Engine: {engineName}";

        ShowFidelityBanner = flavor.ShowFidelityBanner && !string.IsNullOrWhiteSpace(flavor.FidelityNote);
        FidelityBannerText = flavor.FidelityNote ?? string.Empty;

        FlavorTooltip = string.IsNullOrWhiteSpace(flavor.Notes)
            ? $"{flavor.DisplayName} — testing: {flavor.Fidelity.DisplayName()} via {engineName}"
            : $"{flavor.DisplayName} — {flavor.Notes}";

        var unsupported = new List<string>();
        if (!flavor.SupportsOption(RegexOptionsEx.ExplicitCapture))
            unsupported.Add("Explicit capture");
        if (!flavor.SupportsOption(RegexOptionsEx.IgnorePatternWhitespace))
            unsupported.Add("Ignore whitespace");

        OptionsContextLabel = flavor.EngineId switch
        {
            "javascript" => unsupported.Count > 0
                ? $"Options: i/m/s for {engineName}. N/A: {string.Join(", ", unsupported)}"
                : $"Options apply to {engineName}",
            "pcre2" when flavor.IsOptionApproximate(RegexOptionsEx.ExplicitCapture) =>
                $"Options apply to {engineName} — Explicit capture is approximate",
            _ when flavor.Fidelity.IsApproximateOrWeaker() =>
                $"Options apply to approximate engine ({engineName}) for {flavor.DisplayName}",
            _ => $"Options apply to {engineName}",
        };
    }

    private void UpdateStatusEngine(IRegexEngine engine)
    {
        var flavor = SelectedFlavor;
        if (flavor is null)
        {
            StatusEngine = $"Engine: {engine.DisplayName}";
            return;
        }

        StatusEngine = $"Flavor: {flavor.StatusLabel} | Engine: {engine.DisplayName}";
    }

    private void PersistSettings()
    {
        if (_suppressSettingsSave) return;
        try
        {
            _settings.Theme = ThemeLabel;
            _settings.FlavorId = SelectedFlavor?.Id ?? "dotnet";
            _settings.LastGrepRoot = GrepRootPath ?? string.Empty;
            _settings.GrepIncludeGlobs = GrepIncludeGlobs ?? string.Empty;
            _settings.GrepExcludeGlobs = GrepExcludeGlobs ?? string.Empty;
            _settings.GrepRecursive = GrepRecursive;
            _settings.GrepCreateBackup = GrepCreateBackup;
            _settings.OptionsExpanded = OptionsExpanded;
            _settings.IgnoreCase = IgnoreCase;
            _settings.Multiline = Multiline;
            _settings.Singleline = Singleline;
            _settings.ExplicitCapture = ExplicitCapture;
            _settings.IgnorePatternWhitespace = IgnorePatternWhitespace;
            // RightPanelNormalWidth / RightPanelCompareWidth are updated via RememberRightPanelWidth.
            _settingsStore.Save(_settings);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Settings persist failed");
        }
    }

    private void InitializeCompareFlavorChoices()
    {
        CompareFlavorChoices.Clear();

        // Prefer a diverse default set: selected flavor + other full engines.
        var preferredIds = new List<string>();
        if (SelectedFlavor is not null)
            preferredIds.Add(SelectedFlavor.Id);

        foreach (var id in new[] { "dotnet", "pcre2", "javascript", "python" })
        {
            if (preferredIds.Count >= CompareMaxFlavors)
                break;
            if (preferredIds.Contains(id, StringComparer.OrdinalIgnoreCase))
                continue;
            if (Flavors.Any(f => string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase)))
                preferredIds.Add(id);
        }

        while (preferredIds.Count < CompareMinFlavors && preferredIds.Count < Flavors.Count)
        {
            var next = Flavors.FirstOrDefault(f =>
                !preferredIds.Contains(f.Id, StringComparer.OrdinalIgnoreCase));
            if (next is null) break;
            preferredIds.Add(next.Id);
        }

        var selectedSet = preferredIds
            .Take(CompareMaxFlavors)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var flavor in Flavors)
        {
            CompareFlavorChoices.Add(new CompareFlavorChoiceViewModel(
                flavor,
                selectedSet.Contains(flavor.Id)));
        }

        UpdateCompareSelectionHint();
    }

    private IReadOnlyList<string> GetSelectedCompareFlavorIds() =>
        CompareFlavorChoices
            .Where(c => c.IsSelected)
            .Select(c => c.Flavor.Id)
            .ToList();

    private void UpdateCompareSelectionHint()
    {
        var n = CompareFlavorChoices.Count(c => c.IsSelected);
        CompareSelectionHint = n switch
        {
            0 => "Select 2–4 flavors",
            1 => "Select at least one more flavor (2–4)",
            > CompareMaxFlavors => $"Too many selected ({n}) — max {CompareMaxFlavors}",
            _ => $"{n} flavor(s) selected",
        };
    }

    /// <summary>
    /// Called after a Compare flavor checkbox toggles (two-way bound IsSelected).
    /// Enforces the 2–4 selection cap and re-runs comparison when on the Compare tab.
    /// </summary>
    [RelayCommand]
    private void CompareFlavorSelectionChanged(CompareFlavorChoiceViewModel? choice)
    {
        if (choice is null) return;

        if (choice.IsSelected && CompareFlavorChoices.Count(c => c.IsSelected) > CompareMaxFlavors)
        {
            choice.IsSelected = false;
            StatusText = $"Compare supports at most {CompareMaxFlavors} flavors";
        }

        UpdateCompareSelectionHint();
        if (IsCompareTab)
            RunCompareCore(live: true);
    }

    [RelayCommand]
    private void RunCompare() => RunCompareCore(live: false);

    [RelayCommand]
    private void CopyCompareSummary()
    {
        if (string.IsNullOrWhiteSpace(CompareExportText))
        {
            StatusText = "Nothing to copy — run Compare first";
            return;
        }

        CopyTextRequested?.Invoke(CompareExportText);
        StatusText = "Compare summary copied to clipboard";
        _logger.LogInformation("Compare summary copied");
    }

    private void RunCompareCore(bool live)
    {
        var selectedIds = GetSelectedCompareFlavorIds();
        UpdateCompareSelectionHint();

        if (selectedIds.Count < CompareMinFlavors)
        {
            CompareCards.Clear();
            CompareDifferenceNotes.Clear();
            CompareExportText = string.Empty;
            CompareEmptyMessage = $"Select at least {CompareMinFlavors} flavors to compare.";
            CompareSummary = CompareSelectionHint;
            StatusText = live ? "Compare — select more flavors" : CompareEmptyMessage;
            StatusMatches = "Compare: —";
            StatusTime = "Time: —";
            return;
        }

        if (selectedIds.Count > CompareMaxFlavors)
        {
            CompareEmptyMessage = $"Select at most {CompareMaxFlavors} flavors.";
            CompareSummary = CompareEmptyMessage;
            StatusText = CompareEmptyMessage;
            return;
        }

        IsCompareRunning = true;
        try
        {
            // Build options from UI toggles without filtering by the main selected flavor
            // so Compare can show which options each flavor drops.
            var options = RegexOptionsEx.None;
            if (IgnoreCase) options |= RegexOptionsEx.IgnoreCase;
            if (Multiline) options |= RegexOptionsEx.Multiline;
            if (Singleline) options |= RegexOptionsEx.Singleline;
            if (ExplicitCapture) options |= RegexOptionsEx.ExplicitCapture;
            if (IgnorePatternWhitespace) options |= RegexOptionsEx.IgnorePatternWhitespace;

            var result = _compareService.Compare(new CompareRequest
            {
                Pattern = Pattern ?? string.Empty,
                Subject = Subject ?? string.Empty,
                Options = options,
                FlavorIds = selectedIds,
                MaxMatchesToShow = 5,
            });

            CompareCards.Clear();
            foreach (var f in result.Flavors)
                CompareCards.Add(new CompareCardViewModel(f));

            CompareDifferenceNotes.Clear();
            foreach (var d in result.CrossFlavorDifferences)
                CompareDifferenceNotes.Add(d);

            CompareExportText = result.SummaryText;
            CompareSummary = result.StatusLine;
            CompareEmptyMessage = result.Flavors.Count == 0
                ? "No comparison results."
                : string.Empty;

            StatusMatches = $"Compare: {result.Flavors.Count} flavors";
            StatusTime = $"Time: {result.TotalDuration.TotalMilliseconds:F2} ms";
            StatusText = live
                ? $"Live compare — {result.StatusLine}"
                : $"Compare OK — {result.StatusLine}";

            // Surface a soft error banner only when every selected flavor failed.
            if (result.Flavors.Count > 0 && result.Flavors.All(f => !f.IsValid))
            {
                HasError = true;
                ErrorText = "Pattern is invalid on all selected compare flavors.";
            }
            else if (IsCompareTab)
            {
                HasError = false;
                ErrorText = string.Empty;
            }

            if (!live)
            {
                _logger.LogInformation(
                    "Compare: {Count} flavors, {Diffs} notes in {Ms:F2}ms",
                    result.Flavors.Count,
                    result.CrossFlavorDifferences.Count,
                    result.TotalDuration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Compare failed");
            CompareCards.Clear();
            CompareDifferenceNotes.Clear();
            CompareEmptyMessage = "Compare failed: " + ex.Message;
            CompareSummary = "Compare failed";
            StatusText = CompareEmptyMessage;
        }
        finally
        {
            IsCompareRunning = false;
        }
    }

    private static string GetAppVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            var plus = info.IndexOf('+');
            return plus > 0 ? info[..plus] : info;
        }

        var version = asm.GetName().Version;
        return version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
