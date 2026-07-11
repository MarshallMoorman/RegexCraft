namespace RegexCraft.Core.Editing;

/// <summary>
/// Pure logic for inserting token text into an editor buffer.
/// </summary>
public static class TokenInsertion
{
    public readonly record struct Result(string NewText, int NewCaretOffset);

    /// <summary>
    /// Inserts <paramref name="insertText"/> at <paramref name="caretOffset"/>,
    /// replacing the selection if <paramref name="selectionLength"/> &gt; 0.
    /// </summary>
    /// <param name="caretOffsetInInsert">
    /// Optional caret position relative to the start of the inserted text.
    /// When null, caret is placed after the inserted text.
    /// </param>
    public static Result Insert(
        string source,
        int caretOffset,
        int selectionLength,
        string insertText,
        int? caretOffsetInInsert = null)
    {
        source ??= string.Empty;
        insertText ??= string.Empty;

        if (caretOffset < 0)
            caretOffset = 0;
        if (caretOffset > source.Length)
            caretOffset = source.Length;
        if (selectionLength < 0)
            selectionLength = 0;
        if (caretOffset + selectionLength > source.Length)
            selectionLength = source.Length - caretOffset;

        var before = source[..caretOffset];
        var after = source[(caretOffset + selectionLength)..];
        var newText = before + insertText + after;

        var relative = caretOffsetInInsert ?? insertText.Length;
        if (relative < 0)
            relative = 0;
        if (relative > insertText.Length)
            relative = insertText.Length;

        var newCaret = caretOffset + relative;
        return new Result(newText, newCaret);
    }
}
