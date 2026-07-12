using RegexCraft.Core.Flavors;
using RegexCraft.Core.Tokens;

namespace RegexCraft.App.ViewModels;

public sealed class TokenItemViewModel
{
    public TokenItemViewModel(RegexToken token, FlavorDefinition? flavor = null, string? currentEngineId = null)
    {
        Token = token;
        Label = token.Label;
        InsertText = token.InsertText;
        Category = token.Category;

        var engineId = flavor?.EngineId ?? currentEngineId;
        IsSupported = flavor is not null
            ? flavor.IsTokenSupported(token)
            : token.IsSupportedBy(engineId);

        var supportNote = IsSupported
            ? string.Empty
            : "\n⚠ Limited or unsupported for the current flavor.";
        Tooltip = string.IsNullOrEmpty(token.Example)
            ? $"{token.Description}\nInserts: {token.InsertText}{supportNote}"
            : $"{token.Description}\nExample: {token.Example}\nInserts: {token.InsertText}{supportNote}";
        DisplayLine = $"{token.Label}    {token.InsertText}";
        Opacity = IsSupported ? 1.0 : 0.45;
    }

    public RegexToken Token { get; }
    public string Label { get; }
    public string InsertText { get; }
    public string Category { get; }
    public string Tooltip { get; }
    public string DisplayLine { get; }
    public bool IsSupported { get; }
    public double Opacity { get; }
}

public sealed class TokenCategoryViewModel
{
    public TokenCategoryViewModel(string name, IEnumerable<TokenItemViewModel> tokens)
    {
        Name = name;
        Tokens = tokens.ToList();
    }

    public string Name { get; }
    public IReadOnlyList<TokenItemViewModel> Tokens { get; }
}
