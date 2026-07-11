namespace RegexCraft.Core.Highlighting;

/// <summary>Half-open character range [Start, Start+Length).</summary>
public readonly record struct TextRange(int Start, int Length)
{
    public int End => Start + Length;

    public bool Contains(int offset) => offset >= Start && offset < End;

    public bool Overlaps(int lineStart, int lineEnd) =>
        Start < lineEnd && End > lineStart;
}
