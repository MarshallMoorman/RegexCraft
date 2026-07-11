using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

namespace RegexCraft.App.Highlighting;

/// <summary>
/// Programmatic regex syntax highlighting for AvaloniaEdit.
/// Uses high-contrast professional palettes for light and dark themes so that
/// groups, named groups, escapes, quantifiers, classes, anchors, and comments
/// remain distinct and readable on either background.
/// </summary>
public sealed class RegexHighlightingDefinition : IHighlightingDefinition
{
    private readonly HighlightingRuleSet _main;

    public RegexHighlightingDefinition(RegexSyntaxPalette palette)
    {
        Name = "RegEx";
        _main = BuildRuleSet(palette);
        NamedHighlightingColors = _main.Rules
            .Select(r => r.Color)
            .Concat(_main.Spans.Select(s => s.SpanColor).Where(c => c is not null).Cast<HighlightingColor>())
            .Where(c => c is not null)
            .DistinctBy(c => c!.Name)
            .Cast<HighlightingColor>()
            .ToList();
    }

    public string Name { get; }
    public HighlightingRuleSet MainRuleSet => _main;
    public IEnumerable<HighlightingColor> NamedHighlightingColors { get; }
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    public HighlightingColor? GetNamedColor(string name) =>
        NamedHighlightingColors.FirstOrDefault(c => c.Name == name);

    public HighlightingRuleSet? GetNamedRuleSet(string name) =>
        name == _main.Name ? _main : null;

    public static IHighlightingDefinition Create(bool dark = false) =>
        new RegexHighlightingDefinition(RegexSyntaxPalette.ForTheme(dark));

    public static IHighlightingDefinition Create(RegexSyntaxPalette palette) =>
        new RegexHighlightingDefinition(palette);

    private static HighlightingRuleSet BuildRuleSet(RegexSyntaxPalette p)
    {
        HighlightingColor C(string name, Color fg, bool bold = false) => new()
        {
            Name = name,
            Foreground = new SimpleHighlightingBrush(fg),
            FontWeight = bold ? FontWeight.SemiBold : null,
        };

        var groupColor = C("Group", p.Group, bold: true);
        var namedColor = C("NamedGroup", p.NamedGroup, bold: true);
        var classColor = C("CharacterClass", p.CharacterClass, bold: true);
        var quantColor = C("Quantifier", p.Quantifier, bold: true);
        var escapeColor = C("Escape", p.Escape, bold: true);
        var anchorColor = C("Anchor", p.Anchor, bold: true);
        var altColor = C("Alternation", p.Alternation, bold: true);
        var commentColor = C("Comment", p.Comment);
        var lookColor = C("Lookaround", p.Lookaround, bold: true);
        var literalColor = C("Literal", p.Literal);

        var rules = new HighlightingRuleSet { Name = "Main" };

        // Default color for unmatched text (literals, @, etc.) — critical for light-mode readability.
        // AvaloniaEdit falls back to TextEditor.Foreground when no rule matches; we also paint
        // common literal punctuation via rules where helpful.

        // Character classes [ ... ]
        rules.Spans.Add(new HighlightingSpan
        {
            StartExpression = new Regex(@"\[", RegexOptions.Compiled),
            EndExpression = new Regex(@"\]", RegexOptions.Compiled),
            SpanColor = classColor,
            StartColor = classColor,
            EndColor = classColor,
            RuleSet = new HighlightingRuleSet
            {
                Rules =
                {
                    new HighlightingRule
                    {
                        Regex = new Regex(@"\\.", RegexOptions.Compiled),
                        Color = escapeColor,
                    },
                    new HighlightingRule
                    {
                        Regex = new Regex(@"\^", RegexOptions.Compiled),
                        Color = quantColor,
                    },
                    new HighlightingRule
                    {
                        Regex = new Regex(@"-", RegexOptions.Compiled),
                        Color = quantColor,
                    },
                },
            },
        });

        // Comments (?#...)
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?#[^)]*\)", RegexOptions.Compiled),
            Color = commentColor,
        });

        // Named groups (?<name> or (?'name'
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?<[^>]+>|\(\?'[^']+'", RegexOptions.Compiled),
            Color = namedColor,
        });

        // Named backreferences \k<name>
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\\k<[^>]+>|\\k'[^']+'", RegexOptions.Compiled),
            Color = namedColor,
        });

        // Lookarounds, non-capturing, atomic, conditionals, inline modifiers
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?(?::|=|!|<=|<!|>|\(|[imsxnJ\-]+(?::|\)))", RegexOptions.Compiled),
            Color = lookColor,
        });

        // Parentheses (capturing groups)
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[()]", RegexOptions.Compiled),
            Color = groupColor,
        });

        // Escapes, shorthands, Unicode properties, numeric backrefs
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(
                @"\\[dDwWsSbBAZzG]|\\p\{[^}]*\}|\\P\{[^}]*\}|\\[1-9]\d*|\\x[0-9A-Fa-f]{2}|\\u[0-9A-Fa-f]{4}|\\.",
                RegexOptions.Compiled),
            Color = escapeColor,
        });

        // Quantifiers
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[*+?](?:[?+])?|\{\d+(?:,\d*)?\}(?:[?+])?", RegexOptions.Compiled),
            Color = quantColor,
        });

        // Anchors
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[\^$]", RegexOptions.Compiled),
            Color = anchorColor,
        });

        // Alternation
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\|", RegexOptions.Compiled),
            Color = altColor,
        });

        // Dot metacharacter
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\.", RegexOptions.Compiled),
            Color = quantColor,
        });

        // Keep a named literal color available for tooling / future spans
        _ = literalColor;

        return rules;
    }
}

/// <summary>
/// Color palette for regex syntax highlighting. Built from theme resources when available.
/// </summary>
public sealed class RegexSyntaxPalette
{
    public required Color Group { get; init; }
    public required Color NamedGroup { get; init; }
    public required Color CharacterClass { get; init; }
    public required Color Quantifier { get; init; }
    public required Color Escape { get; init; }
    public required Color Anchor { get; init; }
    public required Color Comment { get; init; }
    public required Color Alternation { get; init; }
    public required Color Lookaround { get; init; }
    public required Color Literal { get; init; }

    public static RegexSyntaxPalette ForTheme(bool dark) => dark ? Dark : Light;

    /// <summary>High-contrast light palette (GitHub / VS Code Light+ inspired).</summary>
    public static RegexSyntaxPalette Light { get; } = new()
    {
        Group = Color.Parse("#0550AE"),
        NamedGroup = Color.Parse("#0A3069"),
        CharacterClass = Color.Parse("#116329"),
        Quantifier = Color.Parse("#CF222E"),
        Escape = Color.Parse("#0A3069"),
        Anchor = Color.Parse("#A40E26"),
        Comment = Color.Parse("#57606A"),
        Alternation = Color.Parse("#9A6700"),
        Lookaround = Color.Parse("#6639BA"),
        Literal = Color.Parse("#1A1F26"),
    };

    /// <summary>Vivid dark palette for near-black editor backgrounds.</summary>
    public static RegexSyntaxPalette Dark { get; } = new()
    {
        Group = Color.Parse("#79C0FF"),
        NamedGroup = Color.Parse("#A5D6FF"),
        CharacterClass = Color.Parse("#7EE787"),
        Quantifier = Color.Parse("#FFA657"),
        Escape = Color.Parse("#A5D6FF"),
        Anchor = Color.Parse("#FF7B72"),
        Comment = Color.Parse("#8B949E"),
        Alternation = Color.Parse("#E3B341"),
        Lookaround = Color.Parse("#D2A8FF"),
        Literal = Color.Parse("#E6EDF3"),
    };
}
