namespace RegexCraft.Core.Library;

/// <summary>
/// Curated built-in library patterns shipped with RegexCraft.
/// Entries use stable ids so user favorites can be preserved across upgrades.
/// </summary>
public static class BuiltInLibrary
{
    public const string IdPrefix = "builtin-";

    public static IReadOnlyList<LibraryEntry> GetDefaults() =>
    [
        Entry(
            "email",
            "Email address",
            "Practical email-shaped pattern (not full RFC 5322).",
            @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
            "Contact support@regexcraft.com or hello.world+tag@example.org today.",
            "Validation",
            "email,contact",
            "Recommended: all flavors. Portable character classes only."),
        Entry(
            "url",
            "HTTP(S) URL",
            "Matches http and https URLs with optional path/query.",
            @"https?://[^\s/$.?#].[^\s]*",
            "Visit https://regexcraft.com/docs?ref=lib and http://example.org/a/b.",
            "Web",
            "url,http,uri",
            "Recommended: all flavors. Does not validate every URL edge case."),
        Entry(
            "ipv4",
            "IPv4 address",
            "Dotted-decimal IPv4 (0–255 per octet).",
            @"\b(?:(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b",
            "Server 192.168.0.1 responded; ignore 999.1.1.1 and 10.0.0.42.",
            "Network",
            "ip,ipv4,network",
            "Recommended: all flavors (RE2-safe — no lookaround/backrefs)."),
        Entry(
            "ipv6",
            "IPv6 address (simplified)",
            "Common full and compressed IPv6 forms (not every RFC edge case).",
            @"(?:[0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4}|(?:[0-9A-Fa-f]{1,4}:){1,7}:|(?:[0-9A-Fa-f]{1,4}:){1,6}:[0-9A-Fa-f]{1,4}",
            "Address 2001:0db8:85a3:0000:0000:8a2e:0370:7334 and fe80::1.",
            "Network",
            "ip,ipv6,network",
            "Recommended: all flavors. Prefer application-level validators for production IPv6."),
        Entry(
            "phone-us",
            "US phone number",
            "US numbers with optional country code and common separators.",
            @"(?:\+?1[-.\s]?)?\(?[2-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}",
            "Call (415) 555-2671 or +1-800-555-0199 for support.",
            "Validation",
            "phone,us",
            "Recommended: all flavors (portable)."),
        Entry(
            "phone-intl",
            "International phone (E.164-ish)",
            "Leading + and 7–15 digits (loose E.164 shape).",
            @"\+[1-9]\d{6,14}",
            "Dial +442071838750 or +14155552671.",
            "Validation",
            "phone,international,e164",
            "Recommended: all flavors (portable)."),
        Entry(
            "date-iso",
            "Date ISO (YYYY-MM-DD)",
            "ISO 8601 calendar date.",
            @"\b\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])\b",
            "Shipped on 2026-07-11; not 2026-13-40.",
            "Dates",
            "date,iso",
            "Recommended: all flavors (RE2-safe)."),
        Entry(
            "date-us",
            "Date US (MM/DD/YYYY)",
            "Common US slash-separated date.",
            @"\b(?:0?[1-9]|1[0-2])/(?:0?[1-9]|[12]\d|3[01])/(?:19|20)\d{2}\b",
            "Due 07/11/2026 or 7/4/2026.",
            "Dates",
            "date,us",
            "Recommended: all flavors (portable)."),
        Entry(
            "date-eu",
            "Date EU (DD/MM/YYYY)",
            "Common European slash-separated date.",
            @"\b(?:0?[1-9]|[12]\d|3[01])/(?:0?[1-9]|1[0-2])/(?:19|20)\d{2}\b",
            "Due 11/07/2026 or 4/7/2026.",
            "Dates",
            "date,eu",
            "Recommended: all flavors (portable)."),
        Entry(
            "time-24h",
            "Time 24-hour (HH:MM)",
            "24-hour clock with optional seconds.",
            @"\b(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?\b",
            "Meeting at 09:30 and 23:59:59.",
            "Dates",
            "time",
            "Recommended: all flavors (portable)."),
        Entry(
            "hex-color",
            "Hex color (#RGB / #RRGGBB)",
            "CSS hex colors with optional alpha (#RRGGBBAA).",
            @"#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})\b",
            "Theme uses #1A73E8, #fff, and #112233AA.",
            "Web",
            "color,css,hex",
            "Recommended: all flavors (portable)."),
        Entry(
            "uuid",
            "UUID / GUID",
            "RFC 4122 UUID shape (any version).",
            @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
            "id=550e8400-e29b-41d4-a716-446655440000",
            "Identifiers",
            "uuid,guid",
            "Recommended: all flavors (RE2-safe)."),
        Entry(
            "credit-card",
            "Credit card (basic)",
            "13–19 digit sequences with optional spaces/dashes (Luhn not checked).",
            @"\b(?:\d[ -]*?){13,19}\b",
            "Card 4111 1111 1111 1111 or 5500-0000-0000-0004.",
            "Validation",
            "payment,card",
            "Recommended: all flavors. Always validate with Luhn in application code."),
        Entry(
            "strong-password",
            "Strong password (8+ mixed)",
            "At least 8 chars with upper, lower, digit, and special.",
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            "GoodPass1! vs weak",
            "Validation",
            "password,security",
            "Recommended: .NET, PCRE2, JS, PHP, Python, Java. Not RE2-safe (lookahead). Multiline off."),
        Entry(
            "html-tag",
            "HTML/XML tag",
            "Opening, closing, or self-closing tags (simplified).",
            @"</?[A-Za-z][\w:-]*(?:\s+[\w:-]+(?:=(?:""[^""]*""|'[^']*'|[^\s>'""=]+))?)*\s*/?>",
            @"<div class=""box"">hi</div> <br/> <img src='a.png' />",
            "Markup",
            "html,xml",
            "Recommended: all full/high engines. Not a full HTML parser."),
        Entry(
            "whitespace-normalize",
            "Whitespace runs",
            "One or more whitespace characters (for normalize/replace).",
            @"\s+",
            "Too   many\t\tspaces\nand newlines.",
            "Text",
            "whitespace,normalize",
            "Recommended: all flavors. Replace with a single space to normalize."),
        Entry(
            "log-level",
            "Log level line",
            "Common log levels at line start.",
            @"(?m)^\s*(?:TRACE|DEBUG|INFO|WARN(?:ING)?|ERROR|FATAL)\b",
            "INFO Started\nDEBUG detail\nERROR boom\nwarning not matched",
            "Logs",
            "log,level",
            "Recommended: .NET, PCRE2, PHP, Python, Java. Enable Multiline (or use (?m))."),
        Entry(
            "iso-datetime",
            "ISO date-time",
            "ISO 8601 date with time and optional Z/offset.",
            @"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:?\d{2})?\b",
            "Event at 2026-07-11T14:30:00Z and 2026-07-11T09:00:00-05:00.",
            "Dates",
            "datetime,iso",
            "Recommended: all flavors (RE2-safe)."),
        Entry(
            "slug",
            "URL slug",
            "Lowercase alphanumeric words joined by hyphens.",
            @"\b[a-z0-9]+(?:-[a-z0-9]+)+\b",
            "Posts: my-cool-post and another-slug vs My_Post.",
            "Web",
            "slug,url",
            "Recommended: all flavors (RE2-safe)."),
        Entry(
            "semver",
            "Semantic version",
            "Major.minor.patch with optional pre-release.",
            @"\b\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?\b",
            "RegexCraft 0.9.0 and 1.0.0-rc.1",
            "Identifiers",
            "version,semver",
            "Recommended: all flavors (RE2-safe)."),
    ];

    private static LibraryEntry Entry(
        string key,
        string name,
        string description,
        string pattern,
        string subject,
        string category,
        string tags,
        string? engineNotes = null)
    {
        var desc = engineNotes is null
            ? description
            : $"{description} {engineNotes}";

        return new LibraryEntry
        {
            Id = IdPrefix + key,
            Name = name,
            Description = desc,
            Pattern = pattern,
            Subject = subject,
            Replacement = string.Empty,
            FlavorId = "dotnet",
            Category = category,
            Tags = tags,
            IsFavorite = false,
            IsBuiltIn = true,
            CreatedUtc = DateTimeOffset.UnixEpoch,
            ModifiedUtc = DateTimeOffset.UnixEpoch,
        };
    }

    public static bool IsBuiltInId(string? id) =>
        !string.IsNullOrEmpty(id) && id.StartsWith(IdPrefix, StringComparison.Ordinal);
}
