namespace RegexCraft.Core.Codegen;

/// <summary>Supported code generation target languages.</summary>
public enum CodeLanguage
{
    CSharp,
    JavaScript,
    Python,
    Php,
    Java,
    Go,
    Rust,
}

public static class CodeLanguageExtensions
{
    public static string DisplayName(this CodeLanguage lang) => lang switch
    {
        CodeLanguage.CSharp => "C#",
        CodeLanguage.JavaScript => "JavaScript",
        CodeLanguage.Python => "Python",
        CodeLanguage.Php => "PHP",
        CodeLanguage.Java => "Java",
        CodeLanguage.Go => "Go",
        CodeLanguage.Rust => "Rust",
        _ => lang.ToString(),
    };

    public static string Id(this CodeLanguage lang) => lang switch
    {
        CodeLanguage.CSharp => "csharp",
        CodeLanguage.JavaScript => "javascript",
        CodeLanguage.Python => "python",
        CodeLanguage.Php => "php",
        CodeLanguage.Java => "java",
        CodeLanguage.Go => "go",
        CodeLanguage.Rust => "rust",
        _ => lang.ToString().ToLowerInvariant(),
    };
}
