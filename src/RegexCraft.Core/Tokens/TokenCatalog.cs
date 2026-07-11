namespace RegexCraft.Core.Tokens;

/// <summary>
/// Built-in text-only token palette. Engine-agnostic common constructs.
/// </summary>
public sealed class TokenCatalog : ITokenCatalog
{
    private readonly IReadOnlyList<RegexToken> _tokens;

    public TokenCatalog()
    {
        _tokens = BuildTokens();
    }

    public IReadOnlyList<RegexToken> GetAllTokens() => _tokens;

    public IReadOnlyList<string> GetCategories() =>
        _tokens.Select(t => t.Category).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>
    /// Multi-word search: every term must match label, insert, description, category, or example.
    /// Terms are space-separated; matching is case-insensitive substring (fuzzy-enough for palette use).
    /// </summary>
    public IReadOnlyList<RegexToken> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _tokens;

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return _tokens
            .Where(t => terms.All(term =>
                t.SearchText.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static IReadOnlyList<RegexToken> BuildTokens()
    {
        var list = new List<RegexToken>();

        void Add(string category, string id, string label, string insert, string description,
            string? example = null, int? caret = null, string? engines = null) =>
            list.Add(new RegexToken
            {
                Id = id,
                Label = label,
                InsertText = insert,
                Category = category,
                Description = description,
                Example = example,
                CaretOffsetInInsert = caret,
                SupportedEngines = engines,
            });

        // Literals / wildcards
        Add("Literals", "any", "Any character", ".", "Matches any character (except newline unless Singleline).", "a.c → abc");
        Add("Literals", "literal-escape", "Escape meta", "\\", "Escapes the following metacharacter.", "\\. → literal dot", 1);
        Add("Literals", "or", "Alternation", "|", "Matches either the left or right expression.", "cat|dog");
        Add("Literals", "comment", "Comment group", "(?#)", "Inline comment (not matched).", "(?#todo)", 3);

        // Character classes
        Add("Character Classes", "digit", "Digit", "\\d", "Matches a digit [0-9].", "\\d+ → 42");
        Add("Character Classes", "non-digit", "Non-digit", "\\D", "Matches a non-digit.");
        Add("Character Classes", "word", "Word char", "\\w", "Matches a word character [A-Za-z0-9_].", "\\w+ → hello");
        Add("Character Classes", "non-word", "Non-word", "\\W", "Matches a non-word character.");
        Add("Character Classes", "whitespace", "Whitespace", "\\s", "Matches whitespace.", "\\s+");
        Add("Character Classes", "non-ws", "Non-whitespace", "\\S", "Matches non-whitespace.");
        Add("Character Classes", "class", "Character class", "[]", "Matches one character from the set.", "[aeiou]", 1);
        Add("Character Classes", "neg-class", "Negated class", "[^]", "Matches one character not in the set.", "[^0-9]", 2);
        Add("Character Classes", "range", "Range", "a-z", "Character range inside a class.", "[a-z]");
        Add("Character Classes", "hex", "Hex digit class", "[0-9A-Fa-f]", "Matches a hexadecimal digit.", "[0-9A-Fa-f]+");
        Add("Character Classes", "alpha", "Letters a–zA–Z", "[a-zA-Z]", "ASCII letters only.", "[a-zA-Z]+");
        Add("Character Classes", "alnum", "Alphanumeric", "[a-zA-Z0-9]", "ASCII letters and digits.");
        Add("Character Classes", "h-space", "Horizontal space", "\\h", "Horizontal whitespace (PCRE).", null, null, "pcre2");
        Add("Character Classes", "v-space", "Vertical space", "\\v", "Vertical whitespace (PCRE).", null, null, "pcre2");

        // Quantifiers
        Add("Quantifiers", "zero-or-more", "Zero or more", "*", "Matches the previous atom 0+ times (greedy).", "a*");
        Add("Quantifiers", "one-or-more", "One or more", "+", "Matches the previous atom 1+ times (greedy).", "a+");
        Add("Quantifiers", "optional", "Optional", "?", "Matches the previous atom 0 or 1 time.", "colou?r");
        Add("Quantifiers", "exact", "Exactly n", "{n}", "Matches exactly n times.", "a{3}", 1);
        Add("Quantifiers", "at-least", "At least n", "{n,}", "Matches n or more times.", "a{2,}", 1);
        Add("Quantifiers", "between", "Between n and m", "{n,m}", "Matches between n and m times.", "a{2,4}", 1);
        Add("Quantifiers", "lazy-star", "Lazy *", "*?", "Non-greedy zero or more.", "a*?");
        Add("Quantifiers", "lazy-plus", "Lazy +", "+?", "Non-greedy one or more.", "a+?");
        Add("Quantifiers", "lazy-opt", "Lazy ?", "??", "Non-greedy optional.");
        Add("Quantifiers", "lazy-between", "Lazy {n,m}", "{n,m}?", "Non-greedy between n and m.", "a{2,4}?", 1);
        Add("Quantifiers", "possessive-plus", "Possessive +", "++", "Possessive one or more (no backtrack).", "a++", null, "pcre2");
        Add("Quantifiers", "possessive-star", "Possessive *", "*+", "Possessive zero or more (PCRE).", "a*+", null, "pcre2");

        // Groups
        Add("Groups", "capture", "Capturing group", "()", "Creates a numbered capturing group.", "(abc)", 1);
        Add("Groups", "named", "Named group", "(?<name>)", "Named capturing group.", "(?<id>\\d+)", 7);
        Add("Groups", "named-quote", "Named group (quotes)", "(?'name')", "Alternate named group syntax.", "(?'id'\\d+)", 7);
        Add("Groups", "non-capture", "Non-capturing", "(?:)", "Groups without capturing.", "(?:ab)+", 3);
        Add("Groups", "atomic", "Atomic group", "(?>)", "Atomic (possessive) group.", "(?>a+)", 3);
        Add("Groups", "branch-reset", "Conditional", "(?(1))", "Conditional if group 1 matched.", null, 4);
        Add("Groups", "cond-named", "Conditional (named)", "(?(name))", "Conditional if named group matched.", null, 7);
        Add("Groups", "balancing", "Balancing group", "(?<open-close>)", "Balancing group (.NET).", null, 8, "dotnet");

        // Lookarounds
        Add("Lookarounds", "pos-lookahead", "Positive lookahead", "(?=)", "Asserts that the pattern matches ahead.", "\\d(?=px)", 3);
        Add("Lookarounds", "neg-lookahead", "Negative lookahead", "(?!)", "Asserts that the pattern does not match ahead.", "\\d(?!px)", 3);
        Add("Lookarounds", "pos-lookbehind", "Positive lookbehind", "(?<=)", "Asserts that the pattern matches behind.", "(?<=\\$)\\d+", 4);
        Add("Lookarounds", "neg-lookbehind", "Negative lookbehind", "(?<!)", "Asserts that the pattern does not match behind.", "(?<!\\$)\\d+", 4);
        Add("Lookarounds", "lookahead-word", "Lookahead word", "(?=\\w)", "Assert next char is a word character.", null, 4);
        Add("Lookarounds", "lookbehind-ws", "Lookbehind space", "(?<=\\s)", "Assert previous char is whitespace.", null, 5);

        // Anchors
        Add("Anchors", "start", "Start of string", "^", "Matches the start of the string (or line with Multiline).", "^Hello");
        Add("Anchors", "end", "End of string", "$", "Matches the end of the string (or line with Multiline).", "world$");
        Add("Anchors", "word-boundary", "Word boundary", "\\b", "Matches a word boundary.", "\\bcat\\b");
        Add("Anchors", "non-boundary", "Non-boundary", "\\B", "Matches a non-word-boundary position.");
        Add("Anchors", "start-abs", "Absolute start", "\\A", "Start of string only (not per-line).");
        Add("Anchors", "end-abs", "Absolute end", "\\z", "End of string only.");
        Add("Anchors", "end-Z", "End before newline", "\\Z", "End of string or before final newline.");
        Add("Anchors", "cont", "Previous match end", "\\G", "Start at end of previous match.");

        // Unicode
        Add("Unicode", "letter", "Unicode letter", "\\p{L}", "Any Unicode letter category.", "\\p{L}+");
        Add("Unicode", "number", "Unicode number", "\\p{N}", "Any Unicode number category.");
        Add("Unicode", "punct", "Unicode punctuation", "\\p{P}", "Any Unicode punctuation.");
        Add("Unicode", "not-letter", "Not letter", "\\P{L}", "Not a Unicode letter.");
        Add("Unicode", "lower", "Lowercase letter", "\\p{Ll}", "Unicode lowercase letter.");
        Add("Unicode", "upper", "Uppercase letter", "\\p{Lu}", "Unicode uppercase letter.");
        Add("Unicode", "mark", "Mark / diacritic", "\\p{M}", "Unicode combining mark.");
        Add("Unicode", "separator", "Separator", "\\p{Z}", "Unicode separator (spaces).");
        Add("Unicode", "symbol", "Symbol", "\\p{S}", "Unicode symbol category.");
        Add("Unicode", "other", "Other / control", "\\p{C}", "Unicode other (control, format, …).");
        Add("Unicode", "decimal", "Decimal digit", "\\p{Nd}", "Unicode decimal digits.");
        Add("Unicode", "currency", "Currency symbol", "\\p{Sc}", "Currency symbols ($, €, …).");

        // Mode modifiers
        Add("Mode Modifiers", "ignore-case", "Ignore case (inline)", "(?i)", "Enables ignore-case for following pattern.", "(?i)hello");
        Add("Mode Modifiers", "multiline", "Multiline (inline)", "(?m)", "Enables multiline mode.");
        Add("Mode Modifiers", "singleline", "Singleline (inline)", "(?s)", "Dot matches newline.");
        Add("Mode Modifiers", "ignore-ws", "Ignore whitespace", "(?x)", "Ignores unescaped whitespace and # comments.");
        Add("Mode Modifiers", "explicit-capture", "Explicit capture", "(?n)", "Only named groups capture (.NET).", null, null, "dotnet");
        Add("Mode Modifiers", "scoped-i", "Scoped ignore-case", "(?i:)", "Ignore-case for the group only.", "(?i:abc)", 4);
        Add("Mode Modifiers", "off-i", "Turn off ignore-case", "(?-i)", "Disables ignore-case for following pattern.");

        // Backreferences
        Add("Backreferences", "ref1", "Backref group 1", "\\1", "Matches the same text as group 1.", "(\\w+)\\s+\\1");
        Add("Backreferences", "ref2", "Backref group 2", "\\2", "Matches the same text as group 2.");
        Add("Backreferences", "ref3", "Backref group 3", "\\3", "Matches the same text as group 3.");
        Add("Backreferences", "ref-named", "Named backref", "\\k<name>", "Matches the same text as a named group.", "(?<w>\\w+)\\s+\\k<w>", 3);
        Add("Backreferences", "ref-named-quote", "Named backref (quotes)", "\\k'name'", "Alternate named backreference syntax.", null, 3);

        // Common patterns
        Add("Common", "email-simple", "Email (simple)", @"[\w.+-]+@[\w.-]+\.\w+", "Simple email-shaped match.");
        Add("Common", "url-http", "HTTP(S) URL", @"https?://[^\s]+", "Matches http or https URLs.");
        Add("Common", "ipv4", "IPv4 address", @"\b(?:\d{1,3}\.){3}\d{1,3}\b", "Rough IPv4 pattern.");
        Add("Common", "iso-date", "ISO date", @"\d{4}-\d{2}-\d{2}", "YYYY-MM-DD date.");
        Add("Common", "hex-color", "Hex color", @"#(?:[0-9A-Fa-f]{3}){1,2}\b", "CSS hex color #RGB or #RRGGBB.");
        Add("Common", "uuid", "UUID", @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", "UUID / GUID shape.");
        Add("Common", "integer", "Integer", @"-?\d+", "Optional sign and digits.");
        Add("Common", "decimal", "Decimal number", @"-?\d+(?:\.\d+)?", "Integer or decimal number.");
        Add("Common", "quoted", "Double-quoted string", "\"(?:\\\\.|[^\"\\\\])*\"", "Double-quoted string with escapes.");
        Add("Common", "slug", "URL slug", @"[a-z0-9]+(?:-[a-z0-9]+)*", "Lowercase slug with hyphens.");

        return list;
    }
}
