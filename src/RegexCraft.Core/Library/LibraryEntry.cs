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
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;
}
