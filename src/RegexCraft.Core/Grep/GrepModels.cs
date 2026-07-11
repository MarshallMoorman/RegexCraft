namespace RegexCraft.Core.Grep;

/// <summary>A single match hit within a file line.</summary>
public sealed class GrepMatchHit
{
    public required string FilePath { get; init; }
    public required int LineNumber { get; init; }
    public required string LineText { get; init; }
    public required int LineStartOffset { get; init; }
    public required int MatchStartInLine { get; init; }
    public required int MatchLength { get; init; }
    public required string MatchValue { get; init; }

    /// <summary>Absolute character offset of the match within the full file text.</summary>
    public int AbsoluteStart => LineStartOffset + MatchStartInLine;

    public string DisplayPath => FilePath;
    public string Summary =>
        $"{Path.GetFileName(FilePath)}:{LineNumber}: {Truncate(LineText.TrimEnd(), 100)}";

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

/// <summary>Progress reported during GREP search or replace.</summary>
public sealed class GrepProgress
{
    public int FilesScanned { get; init; }
    public int FilesMatched { get; init; }
    public int MatchCount { get; init; }
    public string? CurrentFile { get; init; }
    public string Phase { get; init; } = "Scanning";
    public bool IsComplete { get; init; }
}

/// <summary>Options for a GREP search across files/folders.</summary>
public sealed class GrepSearchRequest
{
    public required string RootPath { get; init; }
    public required string Pattern { get; init; }
    public Models.RegexOptionsEx Options { get; init; }
    public bool Recursive { get; init; } = true;
    /// <summary>Semicolon/comma-separated include globs, e.g. <c>*.cs;*.json</c>. Empty = all files.</summary>
    public string IncludeGlobs { get; init; } = "*";
    /// <summary>Semicolon/comma-separated exclude globs, e.g. <c>bin/**;obj/**;*.min.js</c>.</summary>
    public string ExcludeGlobs { get; init; } = "bin/**;obj/**;.git/**;node_modules/**;*.dll;*.exe;*.pdb;*.png;*.jpg;*.jpeg;*.gif;*.ico;*.woff;*.woff2;*.ttf;*.zip;*.gz";
    public long MaxFileSizeBytes { get; init; } = 2 * 1024 * 1024;
    public int MaxMatches { get; init; } = 10_000;
    public int MaxFiles { get; init; } = 50_000;
}

/// <summary>Result of a GREP search.</summary>
public sealed class GrepSearchResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool Cancelled { get; init; }
    public IReadOnlyList<GrepMatchHit> Matches { get; init; } = Array.Empty<GrepMatchHit>();
    public int FilesScanned { get; init; }
    public int FilesMatched { get; init; }
    public int FilesSkipped { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>Options for GREP replace-across-files.</summary>
public sealed class GrepReplaceRequest
{
    public required string RootPath { get; init; }
    public required string Pattern { get; init; }
    public required string Replacement { get; init; }
    public Models.RegexOptionsEx Options { get; init; }
    public bool Recursive { get; init; } = true;
    public string IncludeGlobs { get; init; } = "*";
    public string ExcludeGlobs { get; init; } = "bin/**;obj/**;.git/**;node_modules/**;*.dll;*.exe;*.pdb";
    public long MaxFileSizeBytes { get; init; } = 2 * 1024 * 1024;
    /// <summary>When true, compute changes without writing files.</summary>
    public bool DryRun { get; init; } = true;
    /// <summary>When true and not dry-run, write <c>.bak</c> beside each modified file.</summary>
    public bool CreateBackup { get; init; } = true;
}

/// <summary>Per-file replace outcome.</summary>
public sealed class GrepFileReplaceResult
{
    public required string FilePath { get; init; }
    public int ReplacementCount { get; init; }
    public bool Written { get; init; }
    public bool BackedUp { get; init; }
    public string? ErrorMessage { get; init; }
    public string? PreviewBefore { get; init; }
    public string? PreviewAfter { get; init; }
}

/// <summary>Result of a GREP replace operation.</summary>
public sealed class GrepReplaceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool Cancelled { get; init; }
    public bool DryRun { get; init; }
    public IReadOnlyList<GrepFileReplaceResult> Files { get; init; } = Array.Empty<GrepFileReplaceResult>();
    public int FilesScanned { get; init; }
    public int FilesModified { get; init; }
    public int TotalReplacements { get; init; }
    public TimeSpan Duration { get; init; }
}
