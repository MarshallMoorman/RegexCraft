using RegexCraft.Core.Engines;

namespace RegexCraft.Core.Grep;

/// <summary>
/// Searches and replaces across files/folders using an <see cref="IRegexEngine"/>.
/// </summary>
public interface IGrepService
{
    Task<GrepSearchResult> SearchAsync(
        IRegexEngine engine,
        GrepSearchRequest request,
        IProgress<GrepProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<GrepReplaceResult> ReplaceAsync(
        IRegexEngine engine,
        GrepReplaceRequest request,
        IProgress<GrepProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerate candidate files under a root for the given include/exclude globs.</summary>
    IEnumerable<string> EnumerateFiles(
        string rootPath,
        bool recursive,
        string includeGlobs,
        string excludeGlobs,
        long maxFileSizeBytes,
        int maxFiles,
        out int skipped);
}
