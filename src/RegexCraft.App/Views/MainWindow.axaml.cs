using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using RegexCraft.App.Behaviors;
using RegexCraft.App.Highlighting;
using RegexCraft.App.ViewModels;

namespace RegexCraft.App.Views;

public partial class MainWindow : Window
{
    private readonly MatchHighlightTransformer _subjectHighlighter = new();
    private readonly MatchHighlightTransformer _replaceHighlighter = new();
    private readonly MatchHighlightTransformer _grepHighlighter = new();
    private MainWindowViewModel? _vm;
    private bool _syncingPattern;
    private bool _syncingSubject;
    private bool _syncingReplace;
    private bool _boundsApplied;

    public MainWindow()
    {
        InitializeComponent();
        Title = "RegexCraft";
        Opened += OnOpened;
        Closing += OnClosing;
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Title = _vm?.WindowTitle ?? "RegexCraft";
        ConfigurePatternEditor();
        ConfigureSubjectEditor();
        ConfigureReplaceEditor();
        ConfigureGrepPreviewEditor();
        ApplyThemeBrushes();
        ApplySavedBounds();
        ActualThemeVariantChanged += (_, _) =>
        {
            ApplyThemeBrushes();
            ApplyRegexHighlighting();
            RefreshSubjectHighlights();
            RefreshReplaceHighlights();
            RefreshGrepHighlights();
        };

        ExpandAnalysisTree();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_vm is null) return;
        try
        {
            var pos = Position;
            _vm.PersistWindowBounds(Width, Height, pos.X, pos.Y);
        }
        catch
        {
            // ignore bound persistence failures
        }
    }

    private void ApplySavedBounds()
    {
        if (_boundsApplied || _vm is null) return;
        _boundsApplied = true;
        var s = _vm.LoadedSettings;
        if (s.WindowWidth is > 400 and < 10000)
            Width = s.WindowWidth.Value;
        if (s.WindowHeight is > 300 and < 10000)
            Height = s.WindowHeight.Value;
        if (s.WindowX is not null && s.WindowY is not null)
        {
            try
            {
                Position = new PixelPoint(s.WindowX.Value, s.WindowY.Value);
            }
            catch
            {
                // multi-monitor edge cases
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_vm is null) return;

        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        if (ctrl && e.Key == Key.Enter)
        {
            if (_vm.IsReplaceTab)
                _vm.RunReplaceCommand.Execute(null);
            else if (_vm.IsSplitTab)
                _vm.RunSplitCommand.Execute(null);
            else if (_vm.IsGrepTab)
                _ = _vm.RunGrepSearchCommand.ExecuteAsync(null);
            else
                _vm.RunTestCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (ctrl && e.Key == Key.D1)
        {
            _vm.SelectRightTabCommand.Execute("Test");
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.D2)
        {
            _vm.SelectRightTabCommand.Execute("Replace");
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.D3)
        {
            _vm.SelectRightTabCommand.Execute("Split");
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.D4)
        {
            _vm.SelectRightTabCommand.Execute("Generate");
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.D5)
        {
            _vm.SelectRightTabCommand.Execute("Grep");
            e.Handled = true;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm is not null)
        {
            _vm.InsertTokenRequested -= OnInsertTokenRequested;
            _vm.HighlightsChanged -= OnHighlightsChanged;
            _vm.SelectPatternRangeRequested -= OnSelectPatternRange;
            _vm.SelectSubjectRangeRequested -= OnSelectSubjectRange;
            _vm.CopyTextRequested -= OnCopyText;
            _vm.GrepPreviewChanged -= OnGrepPreviewChanged;
            _vm.PickFolderRequested -= OnPickFolderRequested;
            _vm.PropertyChanged -= OnVmPropertyChanged;
        }

        _vm = DataContext as MainWindowViewModel;
        if (_vm is null)
            return;

        Title = _vm.WindowTitle;
        _vm.InsertTokenRequested += OnInsertTokenRequested;
        _vm.HighlightsChanged += OnHighlightsChanged;
        _vm.SelectPatternRangeRequested += OnSelectPatternRange;
        _vm.SelectSubjectRangeRequested += OnSelectSubjectRange;
        _vm.CopyTextRequested += OnCopyText;
        _vm.GrepPreviewChanged += OnGrepPreviewChanged;
        _vm.PickFolderRequested += OnPickFolderRequested;
        _vm.PropertyChanged += OnVmPropertyChanged;

        if (PatternEditor.Document is not null && PatternEditor.Document.Text != _vm.Pattern)
        {
            _syncingPattern = true;
            PatternEditor.Document.Text = _vm.Pattern ?? string.Empty;
            _syncingPattern = false;
        }

        if (SubjectEditor.Document is not null && SubjectEditor.Document.Text != _vm.Subject)
        {
            _syncingSubject = true;
            SubjectEditor.Document.Text = _vm.Subject ?? string.Empty;
            _syncingSubject = false;
        }

        if (ReplacePreviewEditor is not null
            && ReplacePreviewEditor.Document is not null
            && ReplacePreviewEditor.Document.Text != _vm.ReplacePreview)
        {
            _syncingReplace = true;
            ReplacePreviewEditor.Document.Text = _vm.ReplacePreview ?? string.Empty;
            _syncingReplace = false;
        }

        RefreshSubjectHighlights();
        RefreshReplaceHighlights();
        ExpandAnalysisTree();
    }

    private void ConfigurePatternEditor()
    {
        PatternEditor.Document ??= new TextDocument();
        PatternEditor.Options.EnableHyperlinks = false;
        PatternEditor.Options.EnableEmailHyperlinks = false;
        PatternEditor.Options.AllowScrollBelowDocument = false;
        PatternEditor.Options.HighlightCurrentLine = true;

        ApplyRegexHighlighting();
        ApplyEditorTheme(PatternEditor, patternEditor: true);
        EditorBinding.Attach(PatternEditor);

        PatternEditor.TextChanged += (_, _) =>
        {
            if (_syncingPattern || _vm is null)
                return;
            _syncingPattern = true;
            try
            {
                _vm.Pattern = PatternEditor.Document?.Text ?? string.Empty;
                _vm.PatternCaretOffset = PatternEditor.CaretOffset;
                _vm.PatternSelectionLength = PatternEditor.SelectionLength;
            }
            finally
            {
                _syncingPattern = false;
            }
        };

        PatternEditor.TextArea.Caret.PositionChanged += (_, _) =>
        {
            if (_vm is null || _syncingPattern)
                return;
            _vm.PatternCaretOffset = PatternEditor.CaretOffset;
            _vm.PatternSelectionLength = PatternEditor.SelectionLength;
        };
    }

    private void ConfigureSubjectEditor()
    {
        SubjectEditor.Document ??= new TextDocument();
        SubjectEditor.Options.EnableHyperlinks = false;
        SubjectEditor.Options.EnableEmailHyperlinks = false;
        SubjectEditor.TextArea.TextView.LineTransformers.Add(_subjectHighlighter);
        ApplyEditorTheme(SubjectEditor);

        SubjectEditor.TextChanged += (_, _) =>
        {
            if (_syncingSubject || _vm is null)
                return;
            _syncingSubject = true;
            try
            {
                _vm.Subject = SubjectEditor.Document?.Text ?? string.Empty;
            }
            finally
            {
                _syncingSubject = false;
            }
        };
    }

    private void ConfigureReplaceEditor()
    {
        if (ReplacePreviewEditor is null)
            return;

        ReplacePreviewEditor.Document ??= new TextDocument();
        ReplacePreviewEditor.Options.EnableHyperlinks = false;
        ReplacePreviewEditor.Options.EnableEmailHyperlinks = false;
        ReplacePreviewEditor.IsReadOnly = true;
        ReplacePreviewEditor.TextArea.TextView.LineTransformers.Add(_replaceHighlighter);
        ApplyEditorTheme(ReplacePreviewEditor);
    }

    private void ConfigureGrepPreviewEditor()
    {
        if (GrepPreviewEditor is null)
            return;

        GrepPreviewEditor.Document ??= new TextDocument();
        GrepPreviewEditor.Options.EnableHyperlinks = false;
        GrepPreviewEditor.Options.EnableEmailHyperlinks = false;
        GrepPreviewEditor.IsReadOnly = true;
        GrepPreviewEditor.TextArea.TextView.LineTransformers.Add(_grepHighlighter);
        ApplyEditorTheme(GrepPreviewEditor, showLineNumbers: true);
    }

    private void ApplyRegexHighlighting()
    {
        var dark = ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        var palette = BuildSyntaxPalette(dark);
        PatternEditor.SyntaxHighlighting = RegexHighlightingDefinition.Create(palette);
    }

    private RegexSyntaxPalette BuildSyntaxPalette(bool dark)
    {
        var fallback = RegexSyntaxPalette.ForTheme(dark);
        return new RegexSyntaxPalette
        {
            Group = TryColor("SyntaxGroup") ?? fallback.Group,
            NamedGroup = TryColor("SyntaxNamedGroup") ?? fallback.NamedGroup,
            CharacterClass = TryColor("SyntaxClass") ?? fallback.CharacterClass,
            Quantifier = TryColor("SyntaxQuantifier") ?? fallback.Quantifier,
            Escape = TryColor("SyntaxEscape") ?? fallback.Escape,
            Anchor = TryColor("SyntaxAnchor") ?? fallback.Anchor,
            Comment = TryColor("SyntaxComment") ?? fallback.Comment,
            Alternation = TryColor("SyntaxAlternation") ?? fallback.Alternation,
            Lookaround = TryColor("SyntaxLookaround") ?? fallback.Lookaround,
            Literal = TryColor("SyntaxLiteral") ?? fallback.Literal,
        };
    }

    private void ApplyThemeBrushes()
    {
        var match = TryBrush("MatchHighlightBrush") ?? new SolidColorBrush(Color.Parse("#FFF2A8"));
        var g0 = TryBrush("GroupHighlight0Brush") ?? new SolidColorBrush(Color.Parse("#B3D9FF"));
        var g1 = TryBrush("GroupHighlight1Brush") ?? new SolidColorBrush(Color.Parse("#B6F0C8"));
        var g2 = TryBrush("GroupHighlight2Brush") ?? new SolidColorBrush(Color.Parse("#FFC9B8"));
        var g3 = TryBrush("GroupHighlight3Brush") ?? new SolidColorBrush(Color.Parse("#E0C3FC"));

        _subjectHighlighter.SetBrushes(match, g0, g1, g2, g3);
        _replaceHighlighter.SetBrushes(match, g0, g1, g2, g3);
        _grepHighlighter.SetBrushes(match, g0, g1, g2, g3);

        ApplyEditorTheme(PatternEditor, patternEditor: true);
        ApplyEditorTheme(SubjectEditor);
        if (ReplacePreviewEditor is not null)
            ApplyEditorTheme(ReplacePreviewEditor);
        if (GenerateCodeEditor is not null)
            ApplyEditorTheme(GenerateCodeEditor, showLineNumbers: true);
        if (GrepPreviewEditor is not null)
            ApplyEditorTheme(GrepPreviewEditor, showLineNumbers: true);
    }

    /// <summary>
    /// Applies editor background, foreground, selection, caret, line numbers, and
    /// current-line highlight from theme resources so light mode is never low-contrast.
    /// </summary>
    private void ApplyEditorTheme(TextEditor editor, bool patternEditor = false, bool showLineNumbers = false)
    {
        var bg = TryBrush("EditorBackgroundBrush")
                 ?? TryBrush("BackgroundSecondaryBrush")
                 ?? new SolidColorBrush(Colors.White);
        var fg = TryBrush("EditorForegroundBrush")
                 ?? TryBrush("TextPrimaryBrush")
                 ?? new SolidColorBrush(Color.Parse("#1A1F26"));
        var lineNum = TryBrush("EditorLineNumberBrush")
                      ?? TryBrush("TextMutedBrush")
                      ?? new SolidColorBrush(Color.Parse("#6B7683"));
        var currentLine = TryBrush("EditorCurrentLineBrush")
                          ?? TryBrush("AccentBlueSoftBrush")
                          ?? new SolidColorBrush(Color.Parse("#EEF5FC"));
        var selection = TryBrush("EditorSelectionBrush")
                        ?? new SolidColorBrush(Color.Parse("#B3D7F5"));
        var selectionFg = TryBrush("EditorSelectionForegroundBrush") ?? fg;
        var caret = TryBrush("EditorCaretBrush") ?? fg;
        var border = TryBrush("PrimaryBlueBrush") ?? Brushes.DodgerBlue;

        editor.Background = bg;
        editor.Foreground = fg;
        editor.LineNumbersForeground = lineNum;

        var area = editor.TextArea;
        area.Background = bg;
        area.Foreground = fg;
        area.SelectionBrush = selection;
        area.SelectionForeground = selectionFg;

        if (area.Caret is not null)
            area.Caret.CaretBrush = caret;

        if (patternEditor || showLineNumbers || editor.ShowLineNumbers)
        {
            area.TextView.CurrentLineBackground = currentLine;
            area.TextView.CurrentLineBorder = new Pen(border, 1);
        }

        if (patternEditor)
        {
            editor.Options.HighlightCurrentLine = true;
        }

        area.TextView.Redraw();
    }

    private IBrush? TryBrush(string key)
    {
        if (Application.Current?.TryGetResource(key, ActualThemeVariant, out var res) == true
            && res is IBrush brush)
            return brush;
        return null;
    }

    private Color? TryColor(string key)
    {
        if (Application.Current?.TryGetResource(key, ActualThemeVariant, out var res) == true)
        {
            if (res is Color c)
                return c;
            if (res is SolidColorBrush scb)
                return scb.Color;
        }

        return null;
    }

    private void OnInsertTokenRequested(string insertText, int? caretInInsert)
    {
        EditorBinding.InsertText(PatternEditor, insertText, caretInInsert);
        if (_vm is not null)
        {
            _syncingPattern = true;
            _vm.Pattern = PatternEditor.Document.Text;
            _vm.PatternCaretOffset = PatternEditor.CaretOffset;
            _vm.PatternSelectionLength = 0;
            _syncingPattern = false;
        }

        PatternEditor.Focus();
        PatternEditor.TextArea.Focus();
    }

    private void OnHighlightsChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            RefreshSubjectHighlights();
            RefreshReplaceHighlights();
        });
    }

    private void OnGrepPreviewChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_vm is null || GrepPreviewEditor?.Document is null)
                return;

            if (GrepPreviewEditor.Document.Text != _vm.GrepPreviewText)
                GrepPreviewEditor.Document.Text = _vm.GrepPreviewText ?? string.Empty;
            RefreshGrepHighlights();

            // Scroll to first match if any
            var first = _vm.GrepPreviewHighlights.FirstOrDefault();
            if (first is not null
                && first.Range.Start >= 0
                && first.Range.Start < GrepPreviewEditor.Document.TextLength)
            {
                try
                {
                    GrepPreviewEditor.ScrollToLine(
                        GrepPreviewEditor.Document.GetLineByOffset(first.Range.Start).LineNumber);
                }
                catch
                {
                    // ignore
                }
            }
        });
    }

    private async Task<string?> OnPickFolderRequested()
    {
        try
        {
            var top = TopLevel.GetTopLevel(this);
            if (top is null)
                return null;

            var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder to GREP",
                AllowMultiple = false,
            });

            var folder = folders.FirstOrDefault();
            if (folder is null)
                return null;

            // Prefer local path
            return folder.TryGetLocalPath() ?? folder.Path.LocalPath;
        }
        catch
        {
            return null;
        }
    }

    private void OnSelectPatternRange(int start, int length)
    {
        if (PatternEditor.Document is null) return;
        var len = PatternEditor.Document.TextLength;
        start = Math.Clamp(start, 0, len);
        length = Math.Clamp(length, 0, len - start);
        PatternEditor.Select(start, length);
        PatternEditor.TextArea.Caret.Offset = start + length;
        PatternEditor.TextArea.Focus();
    }

    private void OnSelectSubjectRange(int start, int length)
    {
        if (SubjectEditor.Document is null) return;
        var len = SubjectEditor.Document.TextLength;
        start = Math.Clamp(start, 0, len);
        length = Math.Clamp(length, 0, len - start);
        SubjectEditor.Select(start, length);
        SubjectEditor.TextArea.Focus();
    }

    private async void OnCopyText(string text)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
                await clipboard.SetTextAsync(text);
        }
        catch
        {
            // Clipboard may be unavailable in headless/tests
        }
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_vm is null)
            return;

        if (e.PropertyName == nameof(MainWindowViewModel.WindowTitle))
            Title = _vm.WindowTitle;

        if (e.PropertyName == nameof(MainWindowViewModel.Pattern) && !_syncingPattern)
        {
            if (PatternEditor.Document is not null && PatternEditor.Document.Text != _vm.Pattern)
            {
                _syncingPattern = true;
                var caret = Math.Min(PatternEditor.CaretOffset, _vm.Pattern?.Length ?? 0);
                PatternEditor.Document.Text = _vm.Pattern ?? string.Empty;
                PatternEditor.CaretOffset = Math.Clamp(caret, 0, PatternEditor.Document.TextLength);
                _syncingPattern = false;
            }

            ExpandAnalysisTree();
        }

        if (e.PropertyName == nameof(MainWindowViewModel.Subject) && !_syncingSubject)
        {
            if (SubjectEditor.Document is not null && SubjectEditor.Document.Text != _vm.Subject)
            {
                _syncingSubject = true;
                SubjectEditor.Document.Text = _vm.Subject ?? string.Empty;
                _syncingSubject = false;
            }
        }

        if (e.PropertyName == nameof(MainWindowViewModel.ReplacePreview) && !_syncingReplace)
        {
            if (ReplacePreviewEditor?.Document is not null
                && ReplacePreviewEditor.Document.Text != _vm.ReplacePreview)
            {
                _syncingReplace = true;
                ReplacePreviewEditor.Document.Text = _vm.ReplacePreview ?? string.Empty;
                _syncingReplace = false;
                RefreshReplaceHighlights();
            }
        }

        if (e.PropertyName == nameof(MainWindowViewModel.GeneratedCode)
            && GenerateCodeEditor?.Document is not null)
        {
            GenerateCodeEditor.Document.Text = _vm.GeneratedCode ?? string.Empty;
        }

        if (e.PropertyName is nameof(MainWindowViewModel.AnalysisNodes)
            or nameof(MainWindowViewModel.AnalysisRoot))
        {
            ExpandAnalysisTree();
        }
    }

    private void RefreshSubjectHighlights()
    {
        if (_vm is null)
            return;
        _subjectHighlighter.SetSpans(_vm.CurrentHighlights);
        SubjectEditor.TextArea.TextView.Redraw();
    }

    private void RefreshReplaceHighlights()
    {
        if (_vm is null || ReplacePreviewEditor is null)
            return;
        _replaceHighlighter.SetSpans(_vm.ReplaceHighlights);
        ReplacePreviewEditor.TextArea.TextView.Redraw();
    }

    private void RefreshGrepHighlights()
    {
        if (_vm is null || GrepPreviewEditor is null)
            return;
        _grepHighlighter.SetSpans(_vm.GrepPreviewHighlights);
        GrepPreviewEditor.TextArea.TextView.Redraw();
    }

    private void ExpandAnalysisTree()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (AnalysisTree is null) return;
            ExpandTreeViewItems(AnalysisTree);
        }, DispatcherPriority.Background);
    }

    private static void ExpandTreeViewItems(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.GetRealizedContainers())
        {
            if (item is TreeViewItem tvi)
            {
                tvi.IsExpanded = true;
                ExpandTreeViewItems(tvi);
            }
        }
    }
}
