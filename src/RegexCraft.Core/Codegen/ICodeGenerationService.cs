using RegexCraft.Core.Models;

namespace RegexCraft.Core.Codegen;

public interface ICodeGenerationService
{
    IReadOnlyList<CodeLanguage> SupportedLanguages { get; }

    string Generate(
        CodeLanguage language,
        CodegenOperation operation,
        string pattern,
        string subject,
        string? replacement,
        RegexOptionsEx options,
        string engineId);
}
