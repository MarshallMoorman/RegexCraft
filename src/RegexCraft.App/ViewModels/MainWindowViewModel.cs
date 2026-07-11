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
using RegexCraft.Core.Editing;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Highlighting;
using RegexCraft.Core.Library;
using RegexCraft.Core.Models;
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
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _debounceCts;
    private const int DebounceMs = 200;
    private string? _lastHistoryPattern;
    private string? _lastHistorySubject;
    private string? _lastHistoryFlavor;

    public MainWindowViewModel(
        IFlavorService flavorService,
        ITokenCatalog tokenCatalog,
        IRegexAnalysisService analysisService,
        ICodeGenerationService codeGeneration,
        ILibraryStore libraryStore,
        IHistoryStore historyStore,
        ILogger<MainWindowViewModel> logger)
    {
        _flavorService = flavorService;
        _tokenCatalog = tokenCatalog;
        _analysisService = analysisService;
        _codeGeneration = codeGeneration;
        _libraryStore = libraryStore;
        _historyStore = historyStore;
        _logger = logger;

        Flavors = new ObservableCollection<FlavorDefinition>(_flavorService.GetFlavors());
        SelectedFlavor = Flavors.FirstOrDefault();

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

        VersionText = $"v{GetAppVersion()}";
        RebuildTokenList();
        RefreshLibrary();
        RefreshHistory();
        RefreshAnalysis();
        RunTestCore(live: false);
        RefreshGeneratedCode();

        _logger.LogInformation("MainWindowViewModel initialized (Phase 2)");
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
            NullLogger<MainWindowViewModel>.Instance)
    {
    }

    /// <summary>Raised when a token should be inserted into the pattern editor.</summary>
    public event Action<string, int?>? InsertTokenRequested;

    /// <summary>Raised when match / replace / split highlights should be applied.</summary>
    public event Action? HighlightsChanged;

    /// <summary>Raised to select a range in the pattern editor (analysis tree click).</summary>
    public event Action<int, int>? SelectPatternRangeRequested;

    /// <summary>Raised to select a range in the subject editor (match/group click).</summary>
    public event Action<int, int>? SelectSubjectRangeRequested;

    /// <summary>Raised when text should be copied to the clipboard (view handles platform clipboard).</summary>
    public event Action<string>? CopyTextRequested;

    public ObservableCollection<FlavorDefinition> Flavors { get; }
    public ObservableCollection<TokenCategoryViewModel> TokenCategories { get; } = new();
    public ObservableCollection<MatchItemViewModel> Matches { get; } = new();
    public ObservableCollection<AnalysisNode> AnalysisNodes { get; } = new();
    public ObservableCollection<LibraryItemViewModel> LibraryItems { get; } = new();
    public ObservableCollection<HistoryItemViewModel> HistoryItems { get; } = new();
    public ObservableCollection<SplitPartViewModel> SplitParts { get; } = new();
    public ObservableCollection<CodeLanguageItem> CodeLanguages { get; } = new();
    public ObservableCollection<CodegenOperationItem> CodegenOperations { get; } = new();
    public string VersionText { get; }

    public IReadOnlyList<HighlightSpan> CurrentHighlights { get; private set; } = Array.Empty<HighlightSpan>();
    public IReadOnlyList<HighlightSpan> ReplaceHighlights { get; private set; } = Array.Empty<HighlightSpan>();

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
    [ObservableProperty] private bool _isSplitTab;
    [ObservableProperty] private bool _isGenerateTab;
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
    [ObservableProperty] private string _librarySearch = string.Empty;
    [ObservableProperty] private string _libraryName = string.Empty;
    [ObservableProperty] private string _libraryDescription = string.Empty;
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
    [ObservableProperty] private string _optionsContextLabel = "Options apply to the current engine";

    partial void OnPatternChanged(string value)
    {
        ScheduleLiveUpdate();
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
    }

    partial void OnMultilineChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnSinglelineChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnExplicitCaptureChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnIgnorePatternWhitespaceChanged(bool value)
    {
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnRemoveEmptySplitEntriesChanged(bool value) => ScheduleLiveUpdate();

    partial void OnSelectedFlavorChanged(FlavorDefinition? value)
    {
        if (value is null) return;
        _logger.LogInformation("Flavor selected: {FlavorId}", value.Id);
        StatusEngine = $"Flavor: {value.DisplayName} | Engine: {value.EngineId}";
        OptionsContextLabel = $"Options apply to {value.DisplayName} ({value.EngineId})";
        RebuildTokenList();
        ScheduleLiveUpdate();
        RefreshGeneratedCode();
    }

    partial void OnTokenSearchChanged(string value) => RebuildTokenList();
    partial void OnLibrarySearchChanged(string value) => RefreshLibrary();
    partial void OnSelectedCodeLanguageChanged(CodeLanguageItem? value) => RefreshGeneratedCode();
    partial void OnSelectedCodegenOperationChanged(CodegenOperationItem? value) => RefreshGeneratedCode();

    partial void OnSelectedAnalysisNodeChanged(AnalysisNode? value)
    {
        if (value is null || !value.HasRange) return;
        SelectPatternRangeRequested?.Invoke(value.StartIndex, value.Length);
    }

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

        RightPanelTab = tab!;
        IsTestTab = tab == "Test";
        IsReplaceTab = tab == "Replace";
        IsSplitTab = tab == "Split";
        IsGenerateTab = tab == "Generate";

        IsMatchMode = IsTestTab;
        IsReplaceMode = IsReplaceTab;
        IsSplitMode = IsSplitTab;
        IsGenerateMode = IsGenerateTab;
        ModeLabel = tab switch
        {
            "Replace" => "Replace",
            "Split" => "Split",
            "Generate" => "Generate",
            _ => "Match",
        };

        if (IsReplaceTab)
            RunReplaceCore(live: false);
        else if (IsSplitTab)
            RunSplitCore(live: false);
        else if (IsGenerateTab)
            RefreshGeneratedCode();
        else if (IsTestTab)
            RunTestCore(live: false);
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
        };

        _libraryStore.Save(entry);
        LibraryName = string.Empty;
        LibraryDescription = string.Empty;
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
        if (_libraryStore.Delete(item.Id))
        {
            RefreshLibrary();
            StatusText = $"Deleted: {item.Name}";
        }
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
                    if (IsReplaceTab)
                        RunReplaceCore(live: true);
                    if (IsSplitTab)
                        RunSplitCore(live: true);
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
        var engineId = SelectedFlavor?.EngineId;
        var found = _tokenCatalog.Search(TokenSearch);
        foreach (var group in found.GroupBy(t => t.Category))
        {
            TokenCategories.Add(new TokenCategoryViewModel(
                group.Key,
                group.Select(t => new TokenItemViewModel(t, engineId))));
        }
    }

    private void RefreshAnalysis()
    {
        AnalysisRoot = _analysisService.Analyze(Pattern);
        AnalysisNodes.Clear();
        if (AnalysisRoot is null)
            return;

        // Always show full tree under root for richer navigation
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
        foreach (var e in _historyStore.GetRecent())
            HistoryItems.Add(new HistoryItemViewModel(e));
    }

    private void RefreshGeneratedCode()
    {
        if (SelectedCodeLanguage is null || SelectedCodegenOperation is null)
        {
            GeneratedCode = string.Empty;
            return;
        }

        var lang = ParseLanguage(SelectedCodeLanguage.Id);
        var op = ParseOperation(SelectedCodegenOperation.Id);
        var engineId = SelectedFlavor?.EngineId ?? "dotnet";

        try
        {
            GeneratedCode = _codeGeneration.Generate(
                lang, op, Pattern, Subject, Replacement, BuildOptions(), engineId);
        }
        catch (Exception ex)
        {
            GeneratedCode = $"// Code generation error: {ex.Message}";
            _logger.LogWarning(ex, "Code generation failed");
        }
    }

    private static CodeLanguage ParseLanguage(string id) => id switch
    {
        "csharp" => CodeLanguage.CSharp,
        "javascript" => CodeLanguage.JavaScript,
        "python" => CodeLanguage.Python,
        "php" => CodeLanguage.Php,
        "java" => CodeLanguage.Java,
        "go" => CodeLanguage.Go,
        "rust" => CodeLanguage.Rust,
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

        // Avoid flooding history on every keystroke of live mode — only when pattern stabilizes differently
        var flavorId = SelectedFlavor?.Id ?? "dotnet";
        if (live
            && Pattern == _lastHistoryPattern
            && Subject == _lastHistorySubject
            && flavorId == _lastHistoryFlavor)
            return;

        // For live, only record after a successful full run when pattern changed
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
        return o;
    }

    private static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "0.3.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
