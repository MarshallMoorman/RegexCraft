using Avalonia;
using Avalonia.Controls;
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
    private MainWindowViewModel? _vm;
    private bool _syncingPattern;
    private bool _syncingSubject;

    public MainWindow()
    {
        InitializeComponent();
        Opened += OnOpened;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        ConfigurePatternEditor();
        ConfigureSubjectEditor();
        ApplyThemeBrushes();
        ActualThemeVariantChanged += (_, _) =>
        {
            ApplyThemeBrushes();
            ApplyRegexHighlighting();
            RefreshSubjectHighlights();
        };
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm is not null)
        {
            _vm.InsertTokenRequested -= OnInsertTokenRequested;
            _vm.HighlightsChanged -= OnHighlightsChanged;
            _vm.PropertyChanged -= OnVmPropertyChanged;
        }

        _vm = DataContext as MainWindowViewModel;
        if (_vm is null)
            return;

        _vm.InsertTokenRequested += OnInsertTokenRequested;
        _vm.HighlightsChanged += OnHighlightsChanged;
        _vm.PropertyChanged += OnVmPropertyChanged;

        // Initial text
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

        RefreshSubjectHighlights();
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

    private void ApplyRegexHighlighting()
    {
        var dark = ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        PatternEditor.SyntaxHighlighting = RegexHighlightingDefinition.Create(dark);
    }

    private void ApplyThemeBrushes()
    {
        _subjectHighlighter.SetBrushes(
            TryBrush("MatchHighlightBrush") ?? new SolidColorBrush(Color.Parse("#FFF3B0")),
            TryBrush("GroupHighlight0Brush") ?? new SolidColorBrush(Color.Parse("#A5D6FF")),
            TryBrush("GroupHighlight1Brush") ?? new SolidColorBrush(Color.Parse("#B4F0C8")),
            TryBrush("GroupHighlight2Brush") ?? new SolidColorBrush(Color.Parse("#FFC9B8")),
            TryBrush("GroupHighlight3Brush") ?? new SolidColorBrush(Color.Parse("#E0C3FC")));

        // Editor surfaces from theme
        var bg = TryBrush("BackgroundSecondaryBrush");
        var fg = TryBrush("TextPrimaryBrush");
        if (bg is not null)
        {
            PatternEditor.Background = bg;
            SubjectEditor.Background = bg;
        }
        if (fg is not null)
        {
            PatternEditor.Foreground = fg;
            SubjectEditor.Foreground = fg;
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
        // Prefer real editor caret over VM snapshot
        EditorBinding.InsertText(PatternEditor, insertText, caretInInsert);
        if (_vm is not null)
        {
            _syncingPattern = true;
            _vm.Pattern = PatternEditor.Document.Text;
            _vm.PatternCaretOffset = PatternEditor.CaretOffset;
            _vm.PatternSelectionLength = 0;
            _syncingPattern = false;
        }
    }

    private void OnHighlightsChanged()
    {
        Dispatcher.UIThread.Post(RefreshSubjectHighlights);
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
    }

    private void RefreshSubjectHighlights()
    {
        if (_vm is null)
            return;
        _subjectHighlighter.SetSpans(_vm.CurrentHighlights);
        SubjectEditor.TextArea.TextView.Redraw();
    }
}
