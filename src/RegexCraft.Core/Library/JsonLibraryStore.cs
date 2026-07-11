using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RegexCraft.Core.Library;

/// <summary>
/// JSON-file backed library of saved patterns.
/// </summary>
public sealed class JsonLibraryStore : ILibraryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;
    private readonly ILogger<JsonLibraryStore> _logger;
    private readonly object _gate = new();
    private List<LibraryEntry> _entries = new();
    private bool _loaded;

    public JsonLibraryStore(string? filePath = null, ILogger<JsonLibraryStore>? logger = null)
    {
        _filePath = filePath ?? Path.Combine(AppDataPaths.GetDataDirectory(), "library.json");
        _logger = logger ?? NullLogger<JsonLibraryStore>.Instance;
    }

    public IReadOnlyList<LibraryEntry> GetAll()
    {
        EnsureLoaded();
        lock (_gate)
            return _entries.OrderByDescending(e => e.ModifiedUtc).ToList();
    }

    public IReadOnlyList<LibraryEntry> Search(string? query)
    {
        var all = GetAll();
        if (string.IsNullOrWhiteSpace(query))
            return all;

        return all.Where(e =>
                Contains(e.Name, query) ||
                Contains(e.Description, query) ||
                Contains(e.Pattern, query) ||
                Contains(e.Subject, query))
            .ToList();
    }

    public LibraryEntry? GetById(string id)
    {
        EnsureLoaded();
        lock (_gate)
            return _entries.FirstOrDefault(e => e.Id == id);
    }

    public LibraryEntry Save(LibraryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        EnsureLoaded();

        lock (_gate)
        {
            var existing = _entries.FirstOrDefault(e => e.Id == entry.Id);
            if (existing is null)
            {
                entry.CreatedUtc = DateTimeOffset.UtcNow;
                entry.ModifiedUtc = entry.CreatedUtc;
                if (string.IsNullOrWhiteSpace(entry.Id))
                    entry.Id = Guid.NewGuid().ToString("N");
                _entries.Add(entry);
                _logger.LogInformation("Library: saved new entry {Id} ({Name})", entry.Id, entry.Name);
            }
            else
            {
                existing.Name = entry.Name;
                existing.Description = entry.Description;
                existing.Pattern = entry.Pattern;
                existing.Subject = entry.Subject;
                existing.Replacement = entry.Replacement;
                existing.FlavorId = entry.FlavorId;
                existing.IgnoreCase = entry.IgnoreCase;
                existing.Multiline = entry.Multiline;
                existing.Singleline = entry.Singleline;
                existing.ExplicitCapture = entry.ExplicitCapture;
                existing.IgnorePatternWhitespace = entry.IgnorePatternWhitespace;
                existing.ModifiedUtc = DateTimeOffset.UtcNow;
                entry = existing;
                _logger.LogInformation("Library: updated entry {Id} ({Name})", entry.Id, entry.Name);
            }

            PersistUnlocked();
            return Clone(entry);
        }
    }

    public bool Delete(string id)
    {
        EnsureLoaded();
        lock (_gate)
        {
            var removed = _entries.RemoveAll(e => e.Id == id) > 0;
            if (removed)
            {
                PersistUnlocked();
                _logger.LogInformation("Library: deleted entry {Id}", id);
            }

            return removed;
        }
    }

    private void EnsureLoaded()
    {
        lock (_gate)
        {
            if (_loaded) return;
            LoadUnlocked();
            _loaded = true;
        }
    }

    private void LoadUnlocked()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _entries = new List<LibraryEntry>();
                return;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<LibraryEntry>>(json, JsonOptions);
            _entries = data ?? new List<LibraryEntry>();
            _logger.LogDebug("Library: loaded {Count} entries from {Path}", _entries.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Library: failed to load {Path}", _filePath);
            _entries = new List<LibraryEntry>();
        }
    }

    private void PersistUnlocked()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(_entries, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Library: failed to save {Path}", _filePath);
        }
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static LibraryEntry Clone(LibraryEntry e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Pattern = e.Pattern,
        Subject = e.Subject,
        Replacement = e.Replacement,
        FlavorId = e.FlavorId,
        IgnoreCase = e.IgnoreCase,
        Multiline = e.Multiline,
        Singleline = e.Singleline,
        ExplicitCapture = e.ExplicitCapture,
        IgnorePatternWhitespace = e.IgnorePatternWhitespace,
        CreatedUtc = e.CreatedUtc,
        ModifiedUtc = e.ModifiedUtc,
    };
}
