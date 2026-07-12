namespace RegexCraft.Core.Flavors;

/// <summary>
/// Shared token-id sets for flavor capability matrices.
/// Ids must match <see cref="Tokens.TokenCatalog"/> token ids.
/// </summary>
public static class FlavorTokenSets
{
    /// <summary>
    /// Constructs unsupported (or not portable) on RE2-style engines (Go regexp, Rust regex crate).
    /// </summary>
    public static IReadOnlyList<string> Re2Unsupported { get; } =
    [
        // Lookarounds — RE2 has none
        "pos-lookahead",
        "neg-lookahead",
        "pos-lookbehind",
        "neg-lookbehind",
        "lookahead-word",
        "lookbehind-ws",
        // Backreferences
        "ref1",
        "ref2",
        "ref3",
        "ref-named",
        "ref-named-quote",
        // Possessive / atomic / conditionals / balancing
        "possessive-plus",
        "possessive-star",
        "atomic",
        "branch-reset",
        "cond-named",
        "balancing",
        // .NET-only / PCRE-only
        "explicit-capture",
        "h-space",
        "v-space",
        "named-quote",
        "comment",
    ];

    /// <summary>
    /// Constructs not available (or not portable) in standard ECMAScript RegExp.
    /// </summary>
    public static IReadOnlyList<string> JavaScriptUnsupported { get; } =
    [
        "possessive-plus",
        "possessive-star",
        "atomic",
        "branch-reset",
        "cond-named",
        "balancing",
        "explicit-capture",
        "h-space",
        "v-space",
        "named-quote",
        "ref-named-quote",
        "comment",
        "start-abs",   // \A
        "end-abs",     // \z
        "end-Z",       // \Z
        "cont",        // \G
        "ignore-ws",   // (?x) free-spacing not in JS
    ];

    /// <summary>
    /// Constructs not available in standard Python <c>re</c> (approximate via .NET).
    /// </summary>
    public static IReadOnlyList<string> PythonReUnsupported { get; } =
    [
        "possessive-plus",
        "possessive-star",
        "atomic",
        "branch-reset",
        "cond-named",
        "balancing",
        "explicit-capture",
        "h-space",
        "v-space",
        "named-quote",
        "ref-named-quote",
    ];

    /// <summary>
    /// Constructs not available in java.util.regex (approximate via .NET).
    /// </summary>
    public static IReadOnlyList<string> JavaUnsupported { get; } =
    [
        "balancing",
        "explicit-capture",
        "branch-reset",
        "cond-named",
        "h-space",
        "v-space",
        "named-quote",
        "ref-named-quote",
        // Java has possessive + atomic, so leave those available (mapped engine is approximate).
    ];

    /// <summary>
    /// Constructs unique to .NET that other dialects lack.
    /// </summary>
    public static IReadOnlyList<string> DotNetOnly { get; } =
    [
        "balancing",
        "explicit-capture",
    ];
}
