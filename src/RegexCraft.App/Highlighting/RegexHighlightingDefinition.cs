using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

namespace RegexCraft.App.Highlighting;

/// <summary>
/// Programmatic blue-themed regex syntax highlighting for AvaloniaEdit.
/// Groups, named groups, escapes, quantifiers, classes, and anchors are clearly distinct.
/// </summary>
public sealed class RegexHighlightingDefinition : IHighlightingDefinition
{
    private readonly HighlightingRuleSet _main;

    public RegexHighlightingDefinition(bool dark)
    {
        Name = "RegEx";
        _main = BuildRuleSet(dark);
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
        new RegexHighlightingDefinition(dark);

    private static HighlightingRuleSet BuildRuleSet(bool dark)
    {
        // Higher-contrast professional palette for both themes.
        Color group = dark ? Color.Parse("#6CB6FF") : Color.Parse("#0550AE");
        Color named = dark ? Color.Parse("#79C0FF") : Color.Parse("#0969DA");
        Color charClass = dark ? Color.Parse("#56D4DD") : Color.Parse("#0550AE");
        Color quant = dark ? Color.Parse("#FFA657") : Color.Parse("#CF222E");
        Color escape = dark ? Color.Parse("#A5D6FF") : Color.Parse("#0A3069");
        Color anchor = dark ? Color.Parse("#FF7B72") : Color.Parse("#A40E26");
        Color comment = dark ? Color.Parse("#8B949E") : Color.Parse("#6E7781");
        Color alt = dark ? Color.Parse("#E3B341") : Color.Parse("#9A6700");
        Color look = dark ? Color.Parse("#D2A8FF") : Color.Parse("#8250DF");

        HighlightingColor C(string name, Color fg, bool bold = false) => new()
        {
            Name = name,
            Foreground = new SimpleHighlightingBrush(fg),
            FontWeight = bold ? FontWeight.Bold : null,
        };

        var groupColor = C("Group", group, bold: true);
        var namedColor = C("NamedGroup", named, bold: true);
        var classColor = C("CharacterClass", charClass);
        var quantColor = C("Quantifier", quant, bold: true);
        var escapeColor = C("Escape", escape, bold: true);
        var anchorColor = C("Anchor", anchor, bold: true);
        var altColor = C("Alternation", alt, bold: true);
        var commentColor = C("Comment", comment);
        var lookColor = C("Lookaround", look, bold: true);

        var rules = new HighlightingRuleSet { Name = "Main" };

        // Character classes [ ... ]
        rules.Spans.Add(new HighlightingSpan
        {
            StartExpression = new Regex(@"\[", RegexOptions.Compiled),
            EndExpression = new Regex(@"\]", RegexOptions.Compiled),
            SpanColor = classColor,
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

        // Lookarounds and non-capturing / atomic
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?(?::|=|!|<=|<!|>)", RegexOptions.Compiled),
            Color = lookColor,
        });

        // Parentheses
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[()]", RegexOptions.Compiled),
            Color = groupColor,
        });

        // Escapes and classes
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(
                @"\\[dDwWsSbBAZzG]|\\p\{[^}]*\}|\\P\{[^}]*\}|\\k<[^>]+>|\\[1-9]\d*|\\.",
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

        return rules;
    }
}
