using RegexCraft.Core.Models;
using RegexCraft.Core.Tokens;

namespace RegexCraft.Core.Flavors;

/// <summary>
/// Describes a regex flavor, which engine implements testing, and fidelity metadata.
/// Designed so future YAML/JSON loading can populate the same shape.
/// </summary>
public sealed class FlavorDefinition
{
    /// <summary>All common <see cref="RegexOptionsEx"/> flags (default for full engines).</summary>
    public const RegexOptionsEx AllCommonOptions =
        RegexOptionsEx.IgnoreCase
        | RegexOptionsEx.Multiline
        | RegexOptionsEx.Singleline
        | RegexOptionsEx.ExplicitCapture
        | RegexOptionsEx.IgnorePatternWhitespace;

    /// <summary>JS-compatible options (i / m / s). No ExplicitCapture or free-spacing.</summary>
    public const RegexOptionsEx JavaScriptOptions =
        RegexOptionsEx.IgnoreCase
        | RegexOptionsEx.Multiline
        | RegexOptionsEx.Singleline;

    /// <summary>Stable flavor id (e.g. "dotnet", "javascript", "python").</summary>
    public required string Id { get; init; }

    /// <summary>Display name shown in the UI.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Engine id that implements testing for this flavor (maps to <c>IRegexEngine.Id</c>).</summary>
    public required string EngineId { get; init; }

    /// <summary>Optional short description.</summary>
    public string? Description { get; init; }

    /// <summary>Whether this flavor has a real testing path (not codegen-only).</summary>
    public bool SupportsFullTesting { get; init; } = true;

    /// <summary>How closely testing matches the real dialect.</summary>
    public TestingFidelity Fidelity { get; init; } = TestingFidelity.Full;

    /// <summary>
    /// Short banner shown when testing is not full fidelity, e.g.
    /// "Testing uses closest engine (PCRE2). Results may differ slightly from real PHP."
    /// </summary>
    public string? FidelityNote { get; init; }

    /// <summary>Longer notes about dialect differences (tooltips / docs).</summary>
    public string? Notes { get; init; }

    /// <summary>Sort order in the flavor dropdown (lower first).</summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Options the UI should treat as supported for this flavor.
    /// Unsupported options are disabled and not applied when building engine options.
    /// </summary>
    public RegexOptionsEx SupportedOptions { get; init; } = AllCommonOptions;

    /// <summary>
    /// Options that exist but map only approximately (still enabled; noted in tooltips).
    /// </summary>
    public RegexOptionsEx ApproximateOptions { get; init; } = RegexOptionsEx.None;

    /// <summary>
    /// Token ids that should be disabled/dimmed for this flavor even if the mapped engine
    /// can execute them (e.g. lookbehind on RE2-like Go/Rust).
    /// </summary>
    public IReadOnlyList<string> UnsupportedTokenIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Preferred codegen language id (see <c>CodeLanguage.Id()</c>), e.g. "python", "csharp".
    /// </summary>
    public string CodegenLanguageId { get; init; } = "csharp";

    /// <summary>
    /// Important known behavioral differences vs the real dialect and/or mapped engine.
    /// Used by docs, tests, and tooltips.
    /// </summary>
    public IReadOnlyList<string> KnownDifferences { get; init; } = Array.Empty<string>();

    /// <summary>True when a non-full fidelity banner should be shown.</summary>
    public bool ShowFidelityBanner =>
        Fidelity is not TestingFidelity.Full
        || !string.IsNullOrWhiteSpace(FidelityNote);

    /// <summary>Status-bar friendly label combining display name and fidelity.</summary>
    public string StatusLabel =>
        Fidelity == TestingFidelity.Full
            ? DisplayName
            : $"{DisplayName} ({Fidelity.DisplayName()})";

    /// <summary>Whether the given option flag is supported for this flavor.</summary>
    public bool SupportsOption(RegexOptionsEx option) =>
        option == RegexOptionsEx.None || (SupportedOptions & option) == option;

    /// <summary>Whether the option is supported only approximately.</summary>
    public bool IsOptionApproximate(RegexOptionsEx option) =>
        option != RegexOptionsEx.None && (ApproximateOptions & option) == option;

    /// <summary>
    /// Whether a palette token should be treated as available for this flavor.
    /// Checks flavor-level exclusions first, then the token's engine support list.
    /// </summary>
    public bool IsTokenSupported(RegexToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (UnsupportedTokenIds.Count > 0
            && UnsupportedTokenIds.Any(id =>
                string.Equals(id, token.Id, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return token.IsSupportedBy(EngineId);
    }

    /// <summary>
    /// Filters engine options to only those supported by this flavor.
    /// </summary>
    public RegexOptionsEx FilterOptions(RegexOptionsEx options) => options & SupportedOptions;
}
