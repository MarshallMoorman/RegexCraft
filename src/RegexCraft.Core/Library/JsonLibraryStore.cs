using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RegexCraft.Core.Library;

/// <summary>
/// JSON-file backed library of saved patterns, with built-in defaults merged in.
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
            return _entries
                .OrderByDescending(e => e.IsFavorite)
                .ThenBy(e => e.IsBuiltIn ? 0 : 1) // built-ins after favorites, before plain user entries when unfavorited sort ties
                .ThenBy(e => e.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(e => e.ModifiedUtc)
                .Select(Clone)
                .ToList();
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
                Contains(e.Subject, query) ||
                Contains(e.Category, query) ||
                Contains(e.Tags, query) ||
                (e.IsBuiltIn && Contains("built-in", query)))
            .ToList();
    }

    public LibraryEntry? GetById(string id)
    {
        EnsureLoaded();
        lock (_gate)
        {
            var e = _entries.FirstOrDefault(x => x.Id == id);
            return e is null ? null : Clone(e);
        }
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
                // Never create new built-ins via Save — user entries only
                entry.IsBuiltIn = false;
                entry.CreatedUtc = DateTimeOffset.UtcNow;
                entry.ModifiedUtc = entry.CreatedUtc;
                if (string.IsNullOrWhiteSpace(entry.Id) || BuiltInLibrary.IsBuiltInId(entry.Id))
                    entry.Id = Guid.NewGuid().ToString("N");
                _entries.Add(entry);
                _logger.LogInformation("Library: saved new entry {Id} ({Name})", entry.Id, entry.Name);
            }
            else if (existing.IsBuiltIn || BuiltInLibrary.IsBuiltInId(existing.Id))
            {
                // Built-ins: only favorite (and display metadata) may change
                existing.IsFavorite = entry.IsFavorite;
                existing.ModifiedUtc = DateTimeOffset.UtcNow;
                entry = existing;
                _logger.LogInformation("Library: updated built-in favorite {Id} ({Name})", entry.Id, entry.Name);
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
                existing.Category = entry.Category;
                existing.Tags = entry.Tags;
                existing.IsFavorite = entry.IsFavorite;
                existing.IsBuiltIn = false;
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
            var existing = _entries.FirstOrDefault(e => e.Id == id);
            if (existing is null)
                return false;

            if (existing.IsBuiltIn || BuiltInLibrary.IsBuiltInId(existing.Id))
            {
                _logger.LogInformation("Library: refused delete of built-in {Id}", id);
                return false;
            }

            var removed = _entries.Remove(existing);
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
            MergeBuiltInsUnlocked();
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

    /// <summary>
    /// Ensures every built-in id exists. Refreshes pattern body for built-ins
    /// while preserving user favorite flags.
    /// </summary>
    private void MergeBuiltInsUnlocked()
    {
        var defaults = BuiltInLibrary.GetDefaults();
        var changed = false;

        foreach (var builtin in defaults)
        {
            var existing = _entries.FirstOrDefault(e => e.Id == builtin.Id);
            if (existing is null)
            {
                _entries.Add(Clone(builtin));
                changed = true;
            }
            else
            {
                // Refresh shipped content; keep favorite preference
                var fav = existing.IsFavorite;
                existing.Name = builtin.Name;
                existing.Description = builtin.Description;
                existing.Pattern = builtin.Pattern;
                existing.Subject = builtin.Subject;
                existing.Replacement = builtin.Replacement;
                existing.FlavorId = builtin.FlavorId;
                existing.Category = builtin.Category;
                existing.Tags = builtin.Tags;
                existing.IsBuiltIn = true;
                existing.IsFavorite = fav;
                // leave ModifiedUtc if only favorite differed
            }
        }

        // Mark any leftover entries with builtin- prefix as built-in
        foreach (var e in _entries.Where(e => BuiltInLibrary.IsBuiltInId(e.Id)))
            e.IsBuiltIn = true;

        if (changed || !File.Exists(_filePath))
        {
            PersistUnlocked();
            _logger.LogInformation(
                "Library: merged {BuiltInCount} built-in patterns (total {Total})",
                defaults.Count,
                _entries.Count);
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
        Category = e.Category,
        Tags = e.Tags,
        IsFavorite = e.IsFavorite,
        IsBuiltIn = e.IsBuiltIn,
        CreatedUtc = e.CreatedUtc,
        ModifiedUtc = e.ModifiedUtc,
    };
}
