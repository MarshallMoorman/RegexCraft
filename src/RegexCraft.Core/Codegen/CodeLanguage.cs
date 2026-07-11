namespace RegexCraft.Core.Codegen;

/// <summary>Supported code generation target languages.</summary>
public enum CodeLanguage
{
    CSharp,
    JavaScript,
    TypeScript,
    Python,
    Php,
    Java,
    Go,
    Rust,
    Ruby,
    Perl,
    Kotlin,
    Swift,
}

public static class CodeLanguageExtensions
{
    public static string DisplayName(this CodeLanguage lang) => lang switch
    {
        CodeLanguage.CSharp => "C#",
        CodeLanguage.JavaScript => "JavaScript",
        CodeLanguage.TypeScript => "TypeScript",
        CodeLanguage.Python => "Python",
        CodeLanguage.Php => "PHP",
        CodeLanguage.Java => "Java",
        CodeLanguage.Go => "Go",
        CodeLanguage.Rust => "Rust",
        CodeLanguage.Ruby => "Ruby",
        CodeLanguage.Perl => "Perl",
        CodeLanguage.Kotlin => "Kotlin",
        CodeLanguage.Swift => "Swift",
        _ => lang.ToString(),
    };

    public static string Id(this CodeLanguage lang) => lang switch
    {
        CodeLanguage.CSharp => "csharp",
        CodeLanguage.JavaScript => "javascript",
        CodeLanguage.TypeScript => "typescript",
        CodeLanguage.Python => "python",
        CodeLanguage.Php => "php",
        CodeLanguage.Java => "java",
        CodeLanguage.Go => "go",
        CodeLanguage.Rust => "rust",
        CodeLanguage.Ruby => "ruby",
        CodeLanguage.Perl => "perl",
        CodeLanguage.Kotlin => "kotlin",
        CodeLanguage.Swift => "swift",
        _ => lang.ToString().ToLowerInvariant(),
    };
}
