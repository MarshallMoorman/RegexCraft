using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RegexCraft.Core.Library;

/// <summary>
/// JSON-file backed recent pattern history (newest first, capped).
/// </summary>
public sealed class JsonHistoryStore : IHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;
    private readonly int _maxEntries;
    private readonly ILogger<JsonHistoryStore> _logger;
    private readonly object _gate = new();
    private List<HistoryEntry> _entries = new();
    private bool _loaded;

    public JsonHistoryStore(string? filePath = null, int maxEntries = 40, ILogger<JsonHistoryStore>? logger = null)
    {
        _filePath = filePath ?? Path.Combine(AppDataPaths.GetDataDirectory(), "history.json");
        _maxEntries = Math.Clamp(maxEntries, 5, 200);
        _logger = logger ?? NullLogger<JsonHistoryStore>.Instance;
    }

    public IReadOnlyList<HistoryEntry> GetRecent(int? limit = null)
    {
        EnsureLoaded();
        lock (_gate)
        {
            var take = limit ?? _maxEntries;
            return _entries.Take(take).Select(Clone).ToList();
        }
    }

    public void Add(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (string.IsNullOrWhiteSpace(entry.Pattern) && string.IsNullOrWhiteSpace(entry.Subject))
            return;

        EnsureLoaded();
        lock (_gate)
        {
            // Deduplicate consecutive identical pattern+flavor
            if (_entries.Count > 0)
            {
                var head = _entries[0];
                if (string.Equals(head.Pattern, entry.Pattern, StringComparison.Ordinal)
                    && string.Equals(head.FlavorId, entry.FlavorId, StringComparison.Ordinal)
                    && string.Equals(head.Subject, entry.Subject, StringComparison.Ordinal))
                {
                    head.UsedUtc = DateTimeOffset.UtcNow;
                    head.Replacement = entry.Replacement;
                    PersistUnlocked();
                    return;
                }
            }

            // Also bump existing identical pattern to top
            var existing = _entries.FirstOrDefault(e =>
                string.Equals(e.Pattern, entry.Pattern, StringComparison.Ordinal)
                && string.Equals(e.FlavorId, entry.FlavorId, StringComparison.Ordinal));
            if (existing is not null)
            {
                _entries.Remove(existing);
                existing.Subject = entry.Subject;
                existing.Replacement = entry.Replacement;
                existing.UsedUtc = DateTimeOffset.UtcNow;
                _entries.Insert(0, existing);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(entry.Id))
                    entry.Id = Guid.NewGuid().ToString("N");
                entry.UsedUtc = DateTimeOffset.UtcNow;
                _entries.Insert(0, entry);
            }

            while (_entries.Count > _maxEntries)
                _entries.RemoveAt(_entries.Count - 1);

            PersistUnlocked();
            _logger.LogDebug("History: recorded pattern ({Len} chars)", entry.Pattern?.Length ?? 0);
        }
    }

    public void Clear()
    {
        EnsureLoaded();
        lock (_gate)
        {
            _entries.Clear();
            PersistUnlocked();
            _logger.LogInformation("History: cleared");
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
                _entries = new List<HistoryEntry>();
                return;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<HistoryEntry>>(json, JsonOptions);
            _entries = data ?? new List<HistoryEntry>();
            _logger.LogDebug("History: loaded {Count} entries from {Path}", _entries.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "History: failed to load {Path}", _filePath);
            _entries = new List<HistoryEntry>();
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
            _logger.LogError(ex, "History: failed to save {Path}", _filePath);
        }
    }

    private static HistoryEntry Clone(HistoryEntry e) => new()
    {
        Id = e.Id,
        Pattern = e.Pattern,
        Subject = e.Subject,
        Replacement = e.Replacement,
        FlavorId = e.FlavorId,
        UsedUtc = e.UsedUtc,
    };
}
