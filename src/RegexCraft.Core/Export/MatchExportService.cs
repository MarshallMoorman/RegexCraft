using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RegexCraft.Core.Models;

namespace RegexCraft.Core.Export;

/// <summary>Metadata included with exported match results.</summary>
public sealed class MatchExportContext
{
    public string Pattern { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string FlavorId { get; init; } = string.Empty;
    public string FlavorDisplayName { get; init; } = string.Empty;
    public string EngineId { get; init; } = string.Empty;
    public string EngineDisplayName { get; init; } = string.Empty;
    public bool IgnoreCase { get; init; }
    public bool Multiline { get; init; }
    public bool Singleline { get; init; }
    public bool ExplicitCapture { get; init; }
    public bool IgnorePatternWhitespace { get; init; }
    public DateTimeOffset ExportedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Formats match results as CSV or JSON for Export from Test / Matches &amp; Groups.
/// </summary>
public static class MatchExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// CSV columns: MatchIndex, Value, Index, Length, then Group{N}_Name/Success/Value/Index/Length
    /// for each group number that appears (excluding group 0 full match, which is already Value).
    /// </summary>
    public static string ToCsv(MatchCollectionResult? result, MatchExportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        result ??= MatchCollectionResult.Failed(context.EngineId, "No results");

        var matches = result.Success ? result.Matches : Array.Empty<MatchResult>();
        var groupNumbers = CollectGroupNumbers(matches);

        var sb = new StringBuilder();
        // Header
        sb.Append("MatchIndex,Value,Index,Length");
        foreach (var n in groupNumbers)
        {
            sb.Append(CultureInfo.InvariantCulture, $",Group{n}_Name,Group{n}_Success,Group{n}_Value,Group{n}_Index,Group{n}_Length");
        }
        sb.AppendLine();

        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            sb.Append(i.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(CsvEscape(m.Value));
            sb.Append(',');
            sb.Append(m.Index.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(m.Length.ToString(CultureInfo.InvariantCulture));

            foreach (var n in groupNumbers)
            {
                var g = m.Groups.FirstOrDefault(x => x.Number == n);
                sb.Append(',');
                sb.Append(CsvEscape(g?.Name ?? n.ToString(CultureInfo.InvariantCulture)));
                sb.Append(',');
                sb.Append(g is { Success: true } ? "true" : "false");
                sb.Append(',');
                sb.Append(CsvEscape(g?.Value ?? string.Empty));
                sb.Append(',');
                sb.Append((g?.Index ?? -1).ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append((g?.Length ?? 0).ToString(CultureInfo.InvariantCulture));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string ToJson(MatchCollectionResult? result, MatchExportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        result ??= MatchCollectionResult.Failed(context.EngineId, "No results");

        var doc = new MatchExportDocument
        {
            ExportedAt = context.ExportedAt,
            Pattern = context.Pattern,
            Subject = context.Subject,
            FlavorId = context.FlavorId,
            FlavorDisplayName = context.FlavorDisplayName,
            EngineId = string.IsNullOrEmpty(result.EngineId) ? context.EngineId : result.EngineId,
            EngineDisplayName = context.EngineDisplayName,
            Success = result.Success,
            ErrorMessage = result.ErrorMessage,
            DurationMs = result.Duration.TotalMilliseconds,
            Options = new MatchExportOptions
            {
                IgnoreCase = context.IgnoreCase,
                Multiline = context.Multiline,
                Singleline = context.Singleline,
                ExplicitCapture = context.ExplicitCapture,
                IgnorePatternWhitespace = context.IgnorePatternWhitespace,
            },
            Matches = result.Success
                ? result.Matches.Select((m, i) => new MatchExportItem
                {
                    MatchIndex = i,
                    Value = m.Value,
                    Index = m.Index,
                    Length = m.Length,
                    Groups = m.Groups.Select(g => new GroupExportItem
                    {
                        Number = g.Number,
                        Name = g.Name,
                        Success = g.Success,
                        Value = g.Value,
                        Index = g.Index,
                        Length = g.Length,
                    }).ToList(),
                }).ToList()
                : Array.Empty<MatchExportItem>(),
        };

        return JsonSerializer.Serialize(doc, JsonOptions);
    }

    public static string SuggestedFileName(string extension, DateTimeOffset? at = null)
    {
        var stamp = (at ?? DateTimeOffset.UtcNow).ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var ext = extension.TrimStart('.');
        return $"regexcraft-matches-{stamp}.{ext}";
    }

    private static List<int> CollectGroupNumbers(IReadOnlyList<MatchResult> matches)
    {
        var set = new SortedSet<int>();
        foreach (var m in matches)
        {
            foreach (var g in m.Groups)
            {
                if (g.Number > 0)
                    set.Add(g.Number);
            }
        }

        return set.ToList();
    }

    public static string CsvEscape(string? value)
    {
        value ??= string.Empty;
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        return value;
    }
}

internal sealed class MatchExportDocument
{
    public DateTimeOffset ExportedAt { get; init; }
    public string Pattern { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string FlavorId { get; init; } = string.Empty;
    public string FlavorDisplayName { get; init; } = string.Empty;
    public string EngineId { get; init; } = string.Empty;
    public string EngineDisplayName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public double DurationMs { get; init; }
    public MatchExportOptions Options { get; init; } = new();
    public IReadOnlyList<MatchExportItem> Matches { get; init; } = Array.Empty<MatchExportItem>();
}

internal sealed class MatchExportOptions
{
    public bool IgnoreCase { get; init; }
    public bool Multiline { get; init; }
    public bool Singleline { get; init; }
    public bool ExplicitCapture { get; init; }
    public bool IgnorePatternWhitespace { get; init; }
}

internal sealed class MatchExportItem
{
    public int MatchIndex { get; init; }
    public string Value { get; init; } = string.Empty;
    public int Index { get; init; }
    public int Length { get; init; }
    public IReadOnlyList<GroupExportItem> Groups { get; init; } = Array.Empty<GroupExportItem>();
}

internal sealed class GroupExportItem
{
    public int Number { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Value { get; init; } = string.Empty;
    public int Index { get; init; }
    public int Length { get; init; }
}
