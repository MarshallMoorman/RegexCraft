namespace RegexCraft.Core.Library;

public interface ILibraryStore
{
    IReadOnlyList<LibraryEntry> GetAll();
    IReadOnlyList<LibraryEntry> Search(string? query);
    LibraryEntry? GetById(string id);
    LibraryEntry Save(LibraryEntry entry);
    bool Delete(string id);
}
