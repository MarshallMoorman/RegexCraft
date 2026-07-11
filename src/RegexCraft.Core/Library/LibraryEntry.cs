namespace RegexCraft.Core.Library;

/// <summary>A user-saved regex pattern with optional metadata.</summary>
public sealed class LibraryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
    public string FlavorId { get; set; } = "dotnet";
    public bool IgnoreCase { get; set; }
    public bool Multiline { get; set; }
    public bool Singleline { get; set; }
    public bool ExplicitCapture { get; set; }
    public bool IgnorePatternWhitespace { get; set; }
    /// <summary>Optional category / folder label (e.g. "Email", "Validation").</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>Comma/semicolon-separated tags for filtering.</summary>
    public string Tags { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;
}
