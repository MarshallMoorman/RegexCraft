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
        Subtitle = string.IsNullOrWhiteSpace(entry.Description)
            ? PatternPreview
            : Truncate(entry.Description, 48);
    }

    public LibraryEntry Entry { get; }
    public string Name { get; }
    public string PatternPreview { get; }
    public string Description { get; }
    public string Subtitle { get; }
    public string Id => Entry.Id;

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) ? string.Empty : s.Length <= max ? s : s[..max] + "…";
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
