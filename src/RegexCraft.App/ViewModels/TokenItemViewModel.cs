using RegexCraft.Core.Tokens;

namespace RegexCraft.App.ViewModels;

public sealed class TokenItemViewModel
{
    public TokenItemViewModel(RegexToken token)
    {
        Token = token;
        Label = token.Label;
        InsertText = token.InsertText;
        Category = token.Category;
        Tooltip = string.IsNullOrEmpty(token.Example)
            ? $"{token.Description}\nInserts: {token.InsertText}"
            : $"{token.Description}\nExample: {token.Example}\nInserts: {token.InsertText}";
        DisplayLine = $"{token.Label}    {token.InsertText}";
    }

    public RegexToken Token { get; }
    public string Label { get; }
    public string InsertText { get; }
    public string Category { get; }
    public string Tooltip { get; }
    public string DisplayLine { get; }
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
