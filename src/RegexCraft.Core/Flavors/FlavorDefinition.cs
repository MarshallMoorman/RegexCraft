namespace RegexCraft.Core.Flavors;

/// <summary>
/// Describes a regex flavor, which engine implements testing, and fidelity metadata.
/// Designed so future YAML/JSON loading can populate the same shape.
/// </summary>
public sealed class FlavorDefinition
{
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

    /// <summary>True when a non-full fidelity banner should be shown.</summary>
    public bool ShowFidelityBanner =>
        Fidelity is not TestingFidelity.Full
        || !string.IsNullOrWhiteSpace(FidelityNote);

    /// <summary>Status-bar friendly label combining display name and fidelity.</summary>
    public string StatusLabel =>
        Fidelity == TestingFidelity.Full
            ? DisplayName
            : $"{DisplayName} ({Fidelity.DisplayName()})";
}
