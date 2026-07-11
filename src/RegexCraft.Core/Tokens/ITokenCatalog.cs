namespace RegexCraft.Core.Tokens;

public interface ITokenCatalog
{
    IReadOnlyList<RegexToken> GetAllTokens();
    IReadOnlyList<string> GetCategories();
    IReadOnlyList<RegexToken> Search(string? query);
}
