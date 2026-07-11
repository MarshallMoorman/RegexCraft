using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Core.Grep;

/// <summary>
/// File/folder GREP using <see cref="IRegexEngine"/>. Async, cancellable, progress-aware.
/// </summary>
public sealed class GrepService : IGrepService
{
    private static readonly Encoding[] EncodingsToTry =
    [
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
        Encoding.UTF8,
        Encoding.Default,
    ];

    private readonly ILogger<GrepService> _logger;

    public GrepService(ILogger<GrepService>? logger = null)
    {
        _logger = logger ?? NullLogger<GrepService>.Instance;
    }

    public async Task<GrepSearchResult> SearchAsync(
        IRegexEngine engine,
        GrepSearchRequest request,
        IProgress<GrepProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(request);

        var sw = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(request.RootPath) || !Directory.Exists(request.RootPath))
        {
            return new GrepSearchResult
            {
                Success = false,
                ErrorMessage = string.IsNullOrWhiteSpace(request.RootPath)
                    ? "Select a folder to search."
                    : $"Folder not found: {request.RootPath}",
                Duration = sw.Elapsed,
            };
        }

        if (string.IsNullOrEmpty(request.Pattern))
        {
            return new GrepSearchResult
            {
                Success = false,
                ErrorMessage = "Pattern is empty.",
                Duration = sw.Elapsed,
            };
        }

        // Validate pattern once against an empty subject
        var probe = engine.Match(request.Pattern, string.Empty, request.Options);
        if (!probe.Success)
        {
            return new GrepSearchResult
            {
                Success = false,
                ErrorMessage = probe.ErrorMessage ?? "Invalid pattern",
                Duration = sw.Elapsed,
            };
        }

        var hits = new List<GrepMatchHit>();
        var filesMatched = 0;
        var filesScanned = 0;
        var cancelled = false;

        try
        {
            var files = EnumerateFiles(
                request.RootPath,
                request.Recursive,
                request.IncludeGlobs,
                request.ExcludeGlobs,
                request.MaxFileSizeBytes,
                request.MaxFiles,
                out var skipped).ToList();

            _logger.LogInformation(
                "GREP search start: root={Root}, files={Count}, pattern length={Len}, engine={Engine}",
                request.RootPath, files.Count, request.Pattern.Length, engine.Id);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                filesScanned++;

                progress?.Report(new GrepProgress
                {
                    FilesScanned = filesScanned,
                    FilesMatched = filesMatched,
                    MatchCount = hits.Count,
                    CurrentFile = file,
                    Phase = "Searching",
                });

                string? text;
                try
                {
                    text = await ReadTextAsync(file, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "GREP skip unreadable file {File}", file);
                    skipped++;
                    continue;
                }

                if (text is null)
                {
                    skipped++;
                    continue;
                }

                MatchCollectionResult result;
                try
                {
                    result = await Task.Run(
                        () => engine.Match(request.Pattern, text, request.Options),
                        cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GREP match failed on {File}", file);
                    continue;
                }

                if (!result.Success || result.Matches.Count == 0)
                    continue;

                filesMatched++;
                var lineMap = BuildLineMap(text);

                foreach (var m in result.Matches)
                {
                    if (hits.Count >= request.MaxMatches)
                        break;

                    var (lineNumber, lineStart, lineText) = LocateLine(text, lineMap, m.Index);
                    var startInLine = Math.Max(0, m.Index - lineStart);
                    hits.Add(new GrepMatchHit
                    {
                        FilePath = file,
                        LineNumber = lineNumber,
                        LineText = lineText,
                        LineStartOffset = lineStart,
                        MatchStartInLine = startInLine,
                        MatchLength = m.Length,
                        MatchValue = m.Value,
                    });
                }

                if (hits.Count >= request.MaxMatches)
                {
                    _logger.LogInformation("GREP hit max matches limit {Max}", request.MaxMatches);
                    break;
                }
            }

            progress?.Report(new GrepProgress
            {
                FilesScanned = filesScanned,
                FilesMatched = filesMatched,
                MatchCount = hits.Count,
                Phase = "Done",
                IsComplete = true,
            });

            sw.Stop();
            _logger.LogInformation(
                "GREP search done: scanned={Scanned}, matched files={Files}, hits={Hits}, ms={Ms:F1}",
                filesScanned, filesMatched, hits.Count, sw.Elapsed.TotalMilliseconds);

            return new GrepSearchResult
            {
                Success = true,
                Matches = hits,
                FilesScanned = filesScanned,
                FilesMatched = filesMatched,
                FilesSkipped = skipped,
                Duration = sw.Elapsed,
            };
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            sw.Stop();
            _logger.LogInformation("GREP search cancelled after {Scanned} files", filesScanned);
            return new GrepSearchResult
            {
                Success = true,
                Cancelled = true,
                Matches = hits,
                FilesScanned = filesScanned,
                FilesMatched = filesMatched,
                Duration = sw.Elapsed,
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "GREP search failed");
            return new GrepSearchResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Matches = hits,
                FilesScanned = filesScanned,
                FilesMatched = filesMatched,
                Duration = sw.Elapsed,
                Cancelled = cancelled,
            };
        }
    }

    public async Task<GrepReplaceResult> ReplaceAsync(
        IRegexEngine engine,
        GrepReplaceRequest request,
        IProgress<GrepProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(request);

        var sw = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(request.RootPath) || !Directory.Exists(request.RootPath))
        {
            return new GrepReplaceResult
            {
                Success = false,
                DryRun = request.DryRun,
                ErrorMessage = string.IsNullOrWhiteSpace(request.RootPath)
                    ? "Select a folder for replace."
                    : $"Folder not found: {request.RootPath}",
                Duration = sw.Elapsed,
            };
        }

        if (string.IsNullOrEmpty(request.Pattern))
        {
            return new GrepReplaceResult
            {
                Success = false,
                DryRun = request.DryRun,
                ErrorMessage = "Pattern is empty.",
                Duration = sw.Elapsed,
            };
        }

        if (!engine.SupportsReplace)
        {
            return new GrepReplaceResult
            {
                Success = false,
                DryRun = request.DryRun,
                ErrorMessage = $"Engine {engine.DisplayName} does not support Replace.",
                Duration = sw.Elapsed,
            };
        }

        var probe = engine.Replace(request.Pattern, "probe", request.Replacement ?? string.Empty, request.Options);
        if (!probe.Success)
        {
            return new GrepReplaceResult
            {
                Success = false,
                DryRun = request.DryRun,
                ErrorMessage = probe.ErrorMessage ?? "Invalid pattern or replacement",
                Duration = sw.Elapsed,
            };
        }

        var fileResults = new List<GrepFileReplaceResult>();
        var filesScanned = 0;
        var filesModified = 0;
        var totalReplacements = 0;

        try
        {
            var files = EnumerateFiles(
                request.RootPath,
                request.Recursive,
                request.IncludeGlobs,
                request.ExcludeGlobs,
                request.MaxFileSizeBytes,
                maxFiles: 50_000,
                out _).ToList();

            _logger.LogInformation(
                "GREP replace start: root={Root}, dryRun={Dry}, backup={Backup}, files={Count}, engine={Engine}",
                request.RootPath, request.DryRun, request.CreateBackup, files.Count, engine.Id);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                filesScanned++;

                progress?.Report(new GrepProgress
                {
                    FilesScanned = filesScanned,
                    FilesMatched = filesModified,
                    MatchCount = totalReplacements,
                    CurrentFile = file,
                    Phase = request.DryRun ? "Dry-run replace" : "Replacing",
                });

                string? text;
                try
                {
                    text = await ReadTextAsync(file, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    fileResults.Add(new GrepFileReplaceResult
                    {
                        FilePath = file,
                        ErrorMessage = ex.Message,
                    });
                    continue;
                }

                if (text is null)
                    continue;

                ReplaceResult result;
                try
                {
                    result = await Task.Run(
                        () => engine.Replace(
                            request.Pattern,
                            text,
                            request.Replacement ?? string.Empty,
                            request.Options),
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    fileResults.Add(new GrepFileReplaceResult
                    {
                        FilePath = file,
                        ErrorMessage = ex.Message,
                    });
                    continue;
                }

                if (!result.Success)
                {
                    fileResults.Add(new GrepFileReplaceResult
                    {
                        FilePath = file,
                        ErrorMessage = result.ErrorMessage,
                    });
                    continue;
                }

                if (result.ReplacementCount == 0 || string.Equals(result.Result, text, StringComparison.Ordinal))
                    continue;

                filesModified++;
                totalReplacements += result.ReplacementCount;

                var written = false;
                var backedUp = false;
                string? writeError = null;

                if (!request.DryRun)
                {
                    try
                    {
                        if (request.CreateBackup)
                        {
                            var bak = file + ".bak";
                            File.Copy(file, bak, overwrite: true);
                            backedUp = true;
                        }

                        await File.WriteAllTextAsync(file, result.Result, new UTF8Encoding(false), cancellationToken)
                            .ConfigureAwait(false);
                        written = true;
                    }
                    catch (Exception ex)
                    {
                        writeError = ex.Message;
                        _logger.LogError(ex, "GREP replace write failed for {File}", file);
                    }
                }

                fileResults.Add(new GrepFileReplaceResult
                {
                    FilePath = file,
                    ReplacementCount = result.ReplacementCount,
                    Written = written,
                    BackedUp = backedUp,
                    ErrorMessage = writeError,
                    PreviewBefore = Truncate(text, 400),
                    PreviewAfter = Truncate(result.Result, 400),
                });
            }

            progress?.Report(new GrepProgress
            {
                FilesScanned = filesScanned,
                FilesMatched = filesModified,
                MatchCount = totalReplacements,
                Phase = "Done",
                IsComplete = true,
            });

            sw.Stop();
            _logger.LogInformation(
                "GREP replace done: scanned={Scanned}, modified={Mod}, replacements={Rep}, dry={Dry}, ms={Ms:F1}",
                filesScanned, filesModified, totalReplacements, request.DryRun, sw.Elapsed.TotalMilliseconds);

            return new GrepReplaceResult
            {
                Success = true,
                DryRun = request.DryRun,
                Files = fileResults,
                FilesScanned = filesScanned,
                FilesModified = filesModified,
                TotalReplacements = totalReplacements,
                Duration = sw.Elapsed,
            };
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            _logger.LogInformation("GREP replace cancelled");
            return new GrepReplaceResult
            {
                Success = true,
                Cancelled = true,
                DryRun = request.DryRun,
                Files = fileResults,
                FilesScanned = filesScanned,
                FilesModified = filesModified,
                TotalReplacements = totalReplacements,
                Duration = sw.Elapsed,
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "GREP replace failed");
            return new GrepReplaceResult
            {
                Success = false,
                DryRun = request.DryRun,
                ErrorMessage = ex.Message,
                Files = fileResults,
                FilesScanned = filesScanned,
                FilesModified = filesModified,
                TotalReplacements = totalReplacements,
                Duration = sw.Elapsed,
            };
        }
    }

    public IEnumerable<string> EnumerateFiles(
        string rootPath,
        bool recursive,
        string includeGlobs,
        string excludeGlobs,
        long maxFileSizeBytes,
        int maxFiles,
        out int skipped)
    {
        skipped = 0;
        var skipCount = 0;
        var includes = FileGlobMatcher.ParseList(includeGlobs);
        var excludes = FileGlobMatcher.ParseList(excludeGlobs);
        if (includes.Count == 0)
            includes = ["*"];

        var root = Path.GetFullPath(rootPath);
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var results = new List<string>();

        IEnumerable<string> raw;
        try
        {
            raw = Directory.EnumerateFiles(root, "*", option);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enumerate {Root}", root);
            return Array.Empty<string>();
        }

        foreach (var file in raw)
        {
            if (results.Count >= maxFiles)
                break;

            string relative;
            try
            {
                relative = Path.GetRelativePath(root, file);
            }
            catch
            {
                relative = file;
            }

            if (!FileGlobMatcher.IsIncluded(relative, includes, excludes))
            {
                skipCount++;
                continue;
            }

            try
            {
                var info = new FileInfo(file);
                if (!info.Exists)
                {
                    skipCount++;
                    continue;
                }

                if (info.Length > maxFileSizeBytes)
                {
                    skipCount++;
                    continue;
                }

                // Skip likely-binary by extension already in defaults; also skip zero-length is fine to include
                results.Add(file);
            }
            catch
            {
                skipCount++;
            }
        }

        skipped = skipCount;
        return results;
    }

    private static async Task<string?> ReadTextAsync(string path, CancellationToken ct)
    {
        // Prefer UTF-8; fall back if invalid
        try
        {
            var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
            if (bytes.Length == 0)
                return string.Empty;

            // Heuristic: high ratio of NUL → binary
            var sample = Math.Min(bytes.Length, 512);
            var nuls = 0;
            for (var i = 0; i < sample; i++)
            {
                if (bytes[i] == 0) nuls++;
            }

            if (nuls > sample / 20)
                return null;

            foreach (var enc in EncodingsToTry)
            {
                try
                {
                    return enc.GetString(bytes);
                }
                catch (DecoderFallbackException)
                {
                    // try next
                }
            }

            return Encoding.UTF8.GetString(bytes);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Returns list of line start offsets (0-based) for each line.</summary>
    private static List<int> BuildLineMap(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }

        return starts;
    }

    private static (int lineNumber, int lineStart, string lineText) LocateLine(
        string text, List<int> lineStarts, int absoluteIndex)
    {
        absoluteIndex = Math.Clamp(absoluteIndex, 0, Math.Max(0, text.Length - 1));
        // Binary search last start <= absoluteIndex
        var lo = 0;
        var hi = lineStarts.Count - 1;
        while (lo <= hi)
        {
            var mid = (lo + hi) / 2;
            if (lineStarts[mid] <= absoluteIndex)
                lo = mid + 1;
            else
                hi = mid - 1;
        }

        var lineIndex = Math.Max(0, hi);
        var start = lineStarts[lineIndex];
        var end = lineIndex + 1 < lineStarts.Count ? lineStarts[lineIndex + 1] : text.Length;
        // Trim trailing CR/LF from display
        var lineEnd = end;
        while (lineEnd > start && (text[lineEnd - 1] == '\n' || text[lineEnd - 1] == '\r'))
            lineEnd--;
        var lineText = text[start..lineEnd];
        return (lineIndex + 1, start, lineText);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
