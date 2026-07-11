namespace RegexCraft.Core.Tokens;

/// <summary>
/// A insertable regex token for the palette. Display is text-only (no icons).
/// </summary>
public sealed class RegexToken
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string InsertText { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public string? Example { get; init; }

    /// <summary>
    /// Optional caret offset relative to the start of <see cref="InsertText"/> after insertion.
    /// Null places the caret after the inserted text.
    /// </summary>
    public int? CaretOffsetInInsert { get; init; }

    /// <summary>
    /// Optional comma-separated engine ids that fully support this token.
    /// Null means supported by all current engines.
    /// </summary>
    public string? SupportedEngines { get; init; }

    /// <summary>Search haystack: label + insert + description + category.</summary>
    public string SearchText => $"{Label} {InsertText} {Description} {Category} {Example}";

    public bool IsSupportedBy(string? engineId)
    {
        if (string.IsNullOrWhiteSpace(SupportedEngines) || string.IsNullOrWhiteSpace(engineId))
            return true;

        return SupportedEngines
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(e => string.Equals(e, engineId, StringComparison.OrdinalIgnoreCase));
    }
}
