using RegexCraft.Core.Library;

namespace RegexCraft.App.ViewModels;

public sealed class LibraryItemViewModel
{
    public LibraryItemViewModel(LibraryEntry entry)
    {
        Entry = entry;
        Name = string.IsNullOrWhiteSpace(entry.Name) ? "(unnamed)" : entry.Name;
        PatternPreview = Truncate(entry.Pattern, 40);
        Description = entry.Description;
        Category = entry.Category ?? string.Empty;
        Tags = entry.Tags ?? string.Empty;
        IsFavorite = entry.IsFavorite;
        FavoriteLabel = entry.IsFavorite ? "★" : "☆";
        var bits = new List<string>();
        if (entry.IsFavorite) bits.Add("★ Favorite");
        if (!string.IsNullOrWhiteSpace(entry.Category)) bits.Add(entry.Category.Trim());
        if (!string.IsNullOrWhiteSpace(entry.Description))
            bits.Add(Truncate(entry.Description, 40));
        else
            bits.Add(PatternPreview);
        Subtitle = string.Join(" · ", bits);
    }

    public LibraryEntry Entry { get; }
    public string Name { get; }
    public string PatternPreview { get; }
    public string Description { get; }
    public string Category { get; }
    public string Tags { get; }
    public bool IsFavorite { get; }
    public string FavoriteLabel { get; }
    public string Subtitle { get; }
    public string Id => Entry.Id;

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) ? string.Empty : s.Length <= max ? s : s[..max] + "…";
}

/// <summary>One GREP hit shown in the results list.</summary>
public sealed class GrepHitViewModel
{
    public GrepHitViewModel(Core.Grep.GrepMatchHit hit)
    {
        Hit = hit;
        FileName = Path.GetFileName(hit.FilePath);
        RelativeOrPath = hit.FilePath;
        LineNumber = hit.LineNumber;
        LineText = hit.LineText;
        MatchValue = hit.MatchValue;
        Summary = $"{FileName}:{LineNumber}";
        Detail = Truncate(hit.LineText.TrimEnd(), 120);
    }

    public Core.Grep.GrepMatchHit Hit { get; }
    public string FileName { get; }
    public string RelativeOrPath { get; }
    public int LineNumber { get; }
    public string LineText { get; }
    public string MatchValue { get; }
    public string Summary { get; }
    public string Detail { get; }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

public sealed class HistoryItemViewModel
{
    public HistoryItemViewModel(HistoryEntry entry)
    {
        Entry = entry;
        Label = entry.DisplayLabel;
        When = entry.UsedUtc.ToLocalTime().ToString("g");
        Flavor = entry.FlavorId;
    }

    public HistoryEntry Entry { get; }
    public string Label { get; }
    public string When { get; }
    public string Flavor { get; }
    public string Id => Entry.Id;
}

public sealed class SplitPartViewModel
{
    public SplitPartViewModel(int index, string value)
    {
        Index = index;
        Value = value;
        Display = string.IsNullOrEmpty(value) ? "(empty)" : value;
        IsEmpty = value.Length == 0;
    }

    public int Index { get; }
    public string Value { get; }
    public string Display { get; }
    public bool IsEmpty { get; }
    public string Summary => $"[{Index}] {Truncate(Display, 60)}";

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

public sealed class CodeLanguageItem
{
    public CodeLanguageItem(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string Id { get; }
    public string DisplayName { get; }
}

public sealed class CodegenOperationItem
{
    public CodegenOperationItem(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string Id { get; }
    public string DisplayName { get; }
}
