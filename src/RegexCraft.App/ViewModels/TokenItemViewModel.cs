using RegexCraft.Core.Tokens;

namespace RegexCraft.App.ViewModels;

public sealed class TokenItemViewModel
{
    public TokenItemViewModel(RegexToken token, string? currentEngineId = null)
    {
        Token = token;
        Label = token.Label;
        InsertText = token.InsertText;
        Category = token.Category;
        IsSupported = token.IsSupportedBy(currentEngineId);
        var supportNote = IsSupported
            ? string.Empty
            : "\n⚠ Limited / engine-specific support for the current flavor.";
        Tooltip = string.IsNullOrEmpty(token.Example)
            ? $"{token.Description}\nInserts: {token.InsertText}{supportNote}"
            : $"{token.Description}\nExample: {token.Example}\nInserts: {token.InsertText}{supportNote}";
        DisplayLine = $"{token.Label}    {token.InsertText}";
        Opacity = IsSupported ? 1.0 : 0.55;
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
