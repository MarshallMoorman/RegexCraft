namespace RegexCraft.Core.Flavors;

/// <summary>
/// Describes a regex flavor and which engine implements it.
/// Designed so future YAML/JSON loading can populate the same shape.
/// </summary>
public sealed class FlavorDefinition
{
    /// <summary>Stable flavor id (e.g. "dotnet", "pcre2").</summary>
    public required string Id { get; init; }

    /// <summary>Display name shown in the UI.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Engine id that implements this flavor (maps to <c>IRegexEngine.Id</c>).</summary>
    public required string EngineId { get; init; }

    /// <summary>Optional short description.</summary>
    public string? Description { get; init; }

    /// <summary>Whether this flavor is available for full testing.</summary>
    public bool SupportsFullTesting { get; init; } = true;
}
