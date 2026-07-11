namespace RegexCraft.Core.Codegen;

/// <summary>Kind of generated code snippet.</summary>
public enum CodegenOperation
{
    IsMatch,
    Match,
    Matches,
    Replace,
    Split,
}

public static class CodegenOperationExtensions
{
    public static string DisplayName(this CodegenOperation op) => op switch
    {
        CodegenOperation.IsMatch => "IsMatch",
        CodegenOperation.Match => "Match (first)",
        CodegenOperation.Matches => "Matches (all)",
        CodegenOperation.Replace => "Replace",
        CodegenOperation.Split => "Split",
        _ => op.ToString(),
    };
}
