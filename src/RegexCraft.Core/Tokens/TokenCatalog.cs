namespace RegexCraft.Core.Tokens;

/// <summary>
/// Built-in text-only token palette. Engine-agnostic common constructs for Phase 1.
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
            string? example = null, int? caret = null) =>
            list.Add(new RegexToken
            {
                Id = id,
                Label = label,
                InsertText = insert,
                Category = category,
                Description = description,
                Example = example,
                CaretOffsetInInsert = caret,
            });

        // Literals / wildcards
        Add("Literals", "any", "Any character", ".", "Matches any character (except newline unless Singleline).", "a.c → abc");
        Add("Literals", "literal-escape", "Escape meta", "\\", "Escapes the following metacharacter.", "\\. → literal dot", 1);
        Add("Literals", "or", "Alternation", "|", "Matches either the left or right expression.", "cat|dog");

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

        // Groups
        Add("Groups", "capture", "Capturing group", "()", "Creates a numbered capturing group.", "(abc)", 1);
        Add("Groups", "named", "Named group", "(?<name>)", "Named capturing group.", "(?<id>\\d+)", 7);
        Add("Groups", "non-capture", "Non-capturing", "(?:)", "Groups without capturing.", "(?:ab)+", 3);
        Add("Groups", "atomic", "Atomic group", "(?>)", "Atomic (possessive) group — engine-dependent.", "(?>a+)", 3);
        Add("Groups", "branch-reset", "Conditional", "(?(1))", "Conditional pattern if group 1 matched (limited support).", null, 4);

        // Lookarounds
        Add("Lookarounds", "pos-lookahead", "Positive lookahead", "(?=)", "Asserts that the pattern matches ahead.", "\\d(?=px)", 3);
        Add("Lookarounds", "neg-lookahead", "Negative lookahead", "(?!)", "Asserts that the pattern does not match ahead.", "\\d(?!px)", 3);
        Add("Lookarounds", "pos-lookbehind", "Positive lookbehind", "(?<=)", "Asserts that the pattern matches behind.", "(?<=\\$)\\d+", 4);
        Add("Lookarounds", "neg-lookbehind", "Negative lookbehind", "(?<!)", "Asserts that the pattern does not match behind.", "(?<!\\$)\\d+", 4);

        // Anchors
        Add("Anchors", "start", "Start of string", "^", "Matches the start of the string (or line with Multiline).", "^Hello");
        Add("Anchors", "end", "End of string", "$", "Matches the end of the string (or line with Multiline).", "world$");
        Add("Anchors", "word-boundary", "Word boundary", "\\b", "Matches a word boundary.", "\\bcat\\b");
        Add("Anchors", "non-boundary", "Non-boundary", "\\B", "Matches a non-word-boundary position.");
        Add("Anchors", "start-abs", "Absolute start", "\\A", "Start of string only (not per-line).");
        Add("Anchors", "end-abs", "Absolute end", "\\z", "End of string only.");

        // Unicode
        Add("Unicode", "letter", "Unicode letter", "\\p{L}", "Any Unicode letter category.", "\\p{L}+");
        Add("Unicode", "number", "Unicode number", "\\p{N}", "Any Unicode number category.");
        Add("Unicode", "punct", "Unicode punctuation", "\\p{P}", "Any Unicode punctuation.");
        Add("Unicode", "not-letter", "Not letter", "\\P{L}", "Not a Unicode letter.");

        // Mode modifiers
        Add("Mode Modifiers", "ignore-case", "Ignore case (inline)", "(?i)", "Enables ignore-case for following pattern.", "(?i)hello");
        Add("Mode Modifiers", "multiline", "Multiline (inline)", "(?m)", "Enables multiline mode.");
        Add("Mode Modifiers", "singleline", "Singleline (inline)", "(?s)", "Dot matches newline.");
        Add("Mode Modifiers", "ignore-ws", "Ignore whitespace", "(?x)", "Ignores unescaped whitespace and # comments.");

        // Backreferences
        Add("Backreferences", "ref1", "Backref group 1", "\\1", "Matches the same text as group 1.", "(\\w+)\\s+\\1");
        Add("Backreferences", "ref-named", "Named backref", "\\k<name>", "Matches the same text as a named group.", "(?<w>\\w+)\\s+\\k<w>", 3);

        return list;
    }
}
