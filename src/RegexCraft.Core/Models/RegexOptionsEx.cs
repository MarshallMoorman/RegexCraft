namespace RegexCraft.Core.Models;

/// <summary>
/// Cross-engine regex options. Mapped to each engine's native option flags.
/// </summary>
[Flags]
public enum RegexOptionsEx
{
    None = 0,
    IgnoreCase = 1 << 0,
    Multiline = 1 << 1,
    Singleline = 1 << 2,
    ExplicitCapture = 1 << 3,
    IgnorePatternWhitespace = 1 << 4,
}
