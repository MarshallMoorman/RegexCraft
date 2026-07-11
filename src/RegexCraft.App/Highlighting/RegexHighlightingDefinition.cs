using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

namespace RegexCraft.App.Highlighting;

/// <summary>
/// Programmatic blue-themed regex syntax highlighting for AvaloniaEdit.
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
        // Professional blues / cool accents (aligned with theme family).
        Color group = dark ? Color.Parse("#5BB0F5") : Color.Parse("#0078D4");
        Color charClass = dark ? Color.Parse("#4FC3F7") : Color.Parse("#00A4EF");
        Color quant = dark ? Color.Parse("#7EB6FF") : Color.Parse("#106EBE");
        Color escape = dark ? Color.Parse("#9CDCFE") : Color.Parse("#0451A5");
        Color anchor = dark ? Color.Parse("#CE9178") : Color.Parse("#C72E0F");
        Color comment = dark ? Color.Parse("#6A9955") : Color.Parse("#008000");
        Color alt = dark ? Color.Parse("#DCDCAA") : Color.Parse("#795E26");

        HighlightingColor C(string name, Color fg, bool bold = false) => new()
        {
            Name = name,
            Foreground = new SimpleHighlightingBrush(fg),
            FontWeight = bold ? FontWeight.Bold : null,
        };

        var groupColor = C("Group", group, bold: true);
        var classColor = C("CharacterClass", charClass);
        var quantColor = C("Quantifier", quant, bold: true);
        var escapeColor = C("Escape", escape);
        var anchorColor = C("Anchor", anchor, bold: true);
        var altColor = C("Alternation", alt, bold: true);
        var commentColor = C("Comment", comment);

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
                },
            },
        });

        // Groups ( ... ) — span coloring for parentheses via rules below; content uses main rules recursively
        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[()]", RegexOptions.Compiled),
            Color = groupColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?[:=!<][^)]*\)|\(\?<[^>]+>", RegexOptions.Compiled),
            Color = groupColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\\[dDwWsSbBAZzG]|\\p\{[^}]*\}|\\P\{[^}]*\}|\\k<[^>]+>|\\.", RegexOptions.Compiled),
            Color = escapeColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[*+?]|\{\d+(?:,\d*)?\}", RegexOptions.Compiled),
            Color = quantColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"[\^$]|\\A|\\z|\\Z|\\G", RegexOptions.Compiled),
            Color = anchorColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\|", RegexOptions.Compiled),
            Color = altColor,
        });

        rules.Rules.Add(new HighlightingRule
        {
            Regex = new Regex(@"\(\?#[^)]*\)", RegexOptions.Compiled),
            Color = commentColor,
        });

        return rules;
    }
}
