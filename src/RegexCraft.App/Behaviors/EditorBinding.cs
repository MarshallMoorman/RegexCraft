using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace RegexCraft.App.Behaviors;

/// <summary>
/// Attached properties to two-way bind AvaloniaEdit <see cref="TextEditor"/> text and report caret.
/// </summary>
public static class EditorBinding
{
    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, string?>("Text", typeof(EditorBinding),
            defaultValue: string.Empty, inherits: false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly AttachedProperty<int> CaretOffsetProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, int>("CaretOffset", typeof(EditorBinding));

    public static readonly AttachedProperty<int> SelectionLengthProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, int>("SelectionLength", typeof(EditorBinding));

    private static readonly AttachedProperty<bool> IsUpdatingProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, bool>("IsUpdating", typeof(EditorBinding));

    static EditorBinding()
    {
        TextProperty.Changed.AddClassHandler<TextEditor>(OnTextChanged);
    }

    public static string? GetText(TextEditor editor) => editor.GetValue(TextProperty);
    public static void SetText(TextEditor editor, string? value) => editor.SetValue(TextProperty, value);

    public static int GetCaretOffset(TextEditor editor) => editor.GetValue(CaretOffsetProperty);
    public static void SetCaretOffset(TextEditor editor, int value) => editor.SetValue(CaretOffsetProperty, value);

    public static int GetSelectionLength(TextEditor editor) => editor.GetValue(SelectionLengthProperty);
    public static void SetSelectionLength(TextEditor editor, int value) => editor.SetValue(SelectionLengthProperty, value);

    public static void Attach(TextEditor editor)
    {
        editor.Document ??= new TextDocument();
        editor.TextChanged += (_, _) =>
        {
            if (editor.GetValue(IsUpdatingProperty))
                return;
            editor.SetValue(IsUpdatingProperty, true);
            try
            {
                SetText(editor, editor.Document.Text);
                SetCaretOffset(editor, editor.CaretOffset);
                SetSelectionLength(editor, editor.SelectionLength);
            }
            finally
            {
                editor.SetValue(IsUpdatingProperty, false);
            }
        };

        editor.TextArea.Caret.PositionChanged += (_, _) =>
        {
            if (editor.GetValue(IsUpdatingProperty))
                return;
            SetCaretOffset(editor, editor.CaretOffset);
            SetSelectionLength(editor, editor.SelectionLength);
        };

        editor.TextArea.SelectionChanged += (_, _) =>
        {
            if (editor.GetValue(IsUpdatingProperty))
                return;
            SetCaretOffset(editor, editor.CaretOffset);
            SetSelectionLength(editor, editor.SelectionLength);
        };
    }

    private static void OnTextChanged(TextEditor editor, AvaloniaPropertyChangedEventArgs e)
    {
        if (editor.GetValue(IsUpdatingProperty))
            return;

        var newText = e.NewValue as string ?? string.Empty;
        editor.Document ??= new TextDocument();
        if (editor.Document.Text == newText)
            return;

        editor.SetValue(IsUpdatingProperty, true);
        try
        {
            var caret = Math.Min(editor.CaretOffset, newText.Length);
            editor.Document.Text = newText;
            editor.CaretOffset = Math.Clamp(caret, 0, newText.Length);
        }
        finally
        {
            editor.SetValue(IsUpdatingProperty, false);
        }
    }

    /// <summary>
    /// Inserts text at the current caret (or replaces selection) and updates bound properties.
    /// </summary>
    public static void InsertText(TextEditor editor, string insertText, int? caretOffsetInInsert = null)
    {
        editor.Document ??= new TextDocument();
        var offset = editor.SelectionLength > 0 ? editor.SelectionStart : editor.CaretOffset;
        var length = editor.SelectionLength;
        editor.Document.Replace(offset, length, insertText);

        var relative = caretOffsetInInsert ?? insertText.Length;
        relative = Math.Clamp(relative, 0, insertText.Length);
        editor.CaretOffset = offset + relative;
        editor.TextArea.Focus();

        SetText(editor, editor.Document.Text);
        SetCaretOffset(editor, editor.CaretOffset);
        SetSelectionLength(editor, 0);
    }
}
