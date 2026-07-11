namespace RegexCraft.Core.Library;

/// <summary>A recently tested pattern snapshot for History.</summary>
public sealed class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Pattern { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
    public string FlavorId { get; set; } = "dotnet";
    public DateTimeOffset UsedUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Short display line for the history list.</summary>
    public string DisplayLabel
    {
        get
        {
            var p = Pattern.Length <= 48 ? Pattern : Pattern[..45] + "…";
            return string.IsNullOrWhiteSpace(p) ? "(empty pattern)" : p;
        }
    }
}
