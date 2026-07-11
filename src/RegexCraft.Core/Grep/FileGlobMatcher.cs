namespace RegexCraft.Core.Grep;

/// <summary>
/// Simple glob matcher for GREP include/exclude patterns.
/// Supports <c>*</c>, <c>?</c>, <c>**</c>, path separators, and semicolon/comma-separated lists.
/// </summary>
public static class FileGlobMatcher
{
    public static IReadOnlyList<string> ParseList(string? list)
    {
        if (string.IsNullOrWhiteSpace(list))
            return Array.Empty<string>();

        return list
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .ToList();
    }

    /// <summary>
    /// Returns true if <paramref name="relativePath"/> matches any include (or include is empty/all)
    /// and does not match any exclude.
    /// </summary>
    public static bool IsIncluded(
        string relativePath,
        IReadOnlyList<string> includes,
        IReadOnlyList<string> excludes)
    {
        var norm = Normalize(relativePath);
        if (excludes.Count > 0 && excludes.Any(g => Matches(norm, g)))
            return false;

        if (includes.Count == 0)
            return true;

        // Treat bare "*" / "**" as match-all
        if (includes.All(IsMatchAll))
            return true;

        return includes.Any(g => Matches(norm, g));
    }

    public static bool Matches(string relativePath, string glob)
    {
        if (string.IsNullOrWhiteSpace(glob))
            return false;

        var path = Normalize(relativePath);
        var pattern = Normalize(glob.Trim());

        if (IsMatchAll(pattern))
            return true;

        // Patterns without a slash also match the file name only
        if (!pattern.Contains('/') && !pattern.Contains("**", StringComparison.Ordinal))
        {
            var fileName = Path.GetFileName(path);
            return MatchSegment(fileName, pattern);
        }

        return MatchPath(path, pattern);
    }

    private static bool IsMatchAll(string pattern) =>
        pattern is "*" or "**" or "**/*" or "*.*";

    private static string Normalize(string path) =>
        path.Replace('\\', '/').TrimStart('/');

    private static bool MatchPath(string path, string pattern)
    {
        // Split on ** for multi-segment wildcard
        if (pattern.Contains("**", StringComparison.Ordinal))
            return MatchWithDoubleStar(path, pattern);

        var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var patParts = pattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length != patParts.Length)
            return false;

        for (var i = 0; i < pathParts.Length; i++)
        {
            if (!MatchSegment(pathParts[i], patParts[i]))
                return false;
        }

        return true;
    }

    private static bool MatchWithDoubleStar(string path, string pattern)
    {
        // Recursive: convert simple ** patterns via segment walk
        var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var patParts = pattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return MatchPartsRecursive(pathParts, 0, patParts, 0);
    }

    private static bool MatchPartsRecursive(string[] path, int pi, string[] pat, int gi)
    {
        while (gi < pat.Length && pi <= path.Length)
        {
            if (pat[gi] == "**")
            {
                gi++;
                if (gi >= pat.Length)
                    return true; // trailing ** matches rest
                // Consume zero or more path segments
                for (var skip = pi; skip <= path.Length; skip++)
                {
                    if (MatchPartsRecursive(path, skip, pat, gi))
                        return true;
                }

                return false;
            }

            if (pi >= path.Length)
                return false;

            if (!MatchSegment(path[pi], pat[gi]))
                return false;

            pi++;
            gi++;
        }

        return pi == path.Length && gi == pat.Length;
    }

    private static bool MatchSegment(string text, string pattern)
    {
        // Convert glob segment to regex
        var rx = "^" + GlobSegmentToRegex(pattern) + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(
            text,
            rx,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

    private static string GlobSegmentToRegex(string pattern)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in pattern)
        {
            switch (c)
            {
                case '*':
                    sb.Append(".*");
                    break;
                case '?':
                    sb.Append('.');
                    break;
                case '.':
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '+':
                case '^':
                case '$':
                case '|':
                case '\\':
                    sb.Append('\\').Append(c);
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}
