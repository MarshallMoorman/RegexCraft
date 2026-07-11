using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
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
    private MainWindowViewModel? _vm;
    private bool _syncingPattern;
    private bool _syncingSubject;
    private bool _syncingReplace;

    public MainWindow()
    {
        InitializeComponent();
        Title = "RegexCraft";
        Opened += OnOpened;
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Title = "RegexCraft";
        ConfigurePatternEditor();
        ConfigureSubjectEditor();
        ConfigureReplaceEditor();
        ApplyThemeBrushes();
        ActualThemeVariantChanged += (_, _) =>
        {
            ApplyThemeBrushes();
            ApplyRegexHighlighting();
            RefreshSubjectHighlights();
            RefreshReplaceHighlights();
        };

        // Expand analysis tree items by default when the tree loads
        ExpandAnalysisTree();
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
            _vm.PropertyChanged -= OnVmPropertyChanged;
        }

        _vm = DataContext as MainWindowViewModel;
        if (_vm is null)
            return;

        Title = "RegexCraft";
        _vm.InsertTokenRequested += OnInsertTokenRequested;
        _vm.HighlightsChanged += OnHighlightsChanged;
        _vm.SelectPatternRangeRequested += OnSelectPatternRange;
        _vm.SelectSubjectRangeRequested += OnSelectSubjectRange;
        _vm.CopyTextRequested += OnCopyText;
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
        PatternEditor.TextArea.TextView.CurrentLineBackground =
            TryBrush("AccentBlueSoftBrush") ?? new SolidColorBrush(Color.Parse("#E5F3FF"));
        PatternEditor.TextArea.TextView.CurrentLineBorder =
            new Pen(TryBrush("PrimaryBlueBrush") ?? Brushes.DodgerBlue, 1);

        ApplyRegexHighlighting();
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
    }

    private void ApplyRegexHighlighting()
    {
        var dark = ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        PatternEditor.SyntaxHighlighting = RegexHighlightingDefinition.Create(dark);
    }

    private void ApplyThemeBrushes()
    {
        var match = TryBrush("MatchHighlightBrush") ?? new SolidColorBrush(Color.Parse("#FFF3B0"));
        var g0 = TryBrush("GroupHighlight0Brush") ?? new SolidColorBrush(Color.Parse("#A5D6FF"));
        var g1 = TryBrush("GroupHighlight1Brush") ?? new SolidColorBrush(Color.Parse("#B4F0C8"));
        var g2 = TryBrush("GroupHighlight2Brush") ?? new SolidColorBrush(Color.Parse("#FFC9B8"));
        var g3 = TryBrush("GroupHighlight3Brush") ?? new SolidColorBrush(Color.Parse("#E0C3FC"));

        _subjectHighlighter.SetBrushes(match, g0, g1, g2, g3);
        _replaceHighlighter.SetBrushes(match, g0, g1, g2, g3);

        var bg = TryBrush("BackgroundSecondaryBrush");
        var fg = TryBrush("TextPrimaryBrush");
        if (bg is not null)
        {
            PatternEditor.Background = bg;
            SubjectEditor.Background = bg;
            if (ReplacePreviewEditor is not null)
                ReplacePreviewEditor.Background = bg;
            if (GenerateCodeEditor is not null)
                GenerateCodeEditor.Background = bg;
        }

        if (fg is not null)
        {
            PatternEditor.Foreground = fg;
            SubjectEditor.Foreground = fg;
            if (ReplacePreviewEditor is not null)
                ReplacePreviewEditor.Foreground = fg;
            if (GenerateCodeEditor is not null)
                GenerateCodeEditor.Foreground = fg;
        }
    }

    private IBrush? TryBrush(string key)
    {
        if (Application.Current?.TryGetResource(key, ActualThemeVariant, out var res) == true
            && res is IBrush brush)
            return brush;
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
