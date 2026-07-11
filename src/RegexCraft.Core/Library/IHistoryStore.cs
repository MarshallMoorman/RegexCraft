namespace RegexCraft.Core.Library;

public interface IHistoryStore
{
    IReadOnlyList<HistoryEntry> GetRecent(int? limit = null);
    void Add(HistoryEntry entry);
    void Clear();
}
