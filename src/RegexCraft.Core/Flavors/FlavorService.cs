using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;

namespace RegexCraft.Core.Flavors;

/// <summary>
/// In-memory flavor registry. Phase 0 hard-codes .NET and PCRE2 flavors.
/// </summary>
public sealed class FlavorService : IFlavorService
{
    private readonly IReadOnlyList<FlavorDefinition> _flavors;
    private readonly Dictionary<string, IRegexEngine> _enginesById;
    private readonly ILogger<FlavorService> _logger;

    public FlavorService(IEnumerable<IRegexEngine> engines, ILogger<FlavorService>? logger = null)
    {
        _logger = logger ?? NullLogger<FlavorService>.Instance;
        _enginesById = engines.ToDictionary(e => e.Id, StringComparer.OrdinalIgnoreCase);

        _flavors =
        [
            new FlavorDefinition
            {
                Id = "dotnet",
                DisplayName = ".NET",
                EngineId = "dotnet",
                Description = "System.Text.RegularExpressions (.NET)",
                SupportsFullTesting = true,
            },
            new FlavorDefinition
            {
                Id = "pcre2",
                DisplayName = "PCRE2",
                EngineId = "pcre2",
                Description = "Perl Compatible Regular Expressions (PCRE2 via PCRE.NET)",
                SupportsFullTesting = true,
            },
        ];

        _logger.LogDebug(
            "FlavorService initialized with {EngineCount} engines and {FlavorCount} flavors",
            _enginesById.Count,
            _flavors.Count);
    }

    public IReadOnlyList<FlavorDefinition> GetFlavors() => _flavors;

    public FlavorDefinition? GetFlavor(string flavorId) =>
        _flavors.FirstOrDefault(f => string.Equals(f.Id, flavorId, StringComparison.OrdinalIgnoreCase));

    public IRegexEngine? GetEngineForFlavor(string flavorId)
    {
        var flavor = GetFlavor(flavorId);
        if (flavor is null)
        {
            _logger.LogWarning("Unknown flavor id: {FlavorId}", flavorId);
            return null;
        }

        return GetEngine(flavor.EngineId);
    }

    public IRegexEngine? GetEngine(string engineId)
    {
        if (_enginesById.TryGetValue(engineId, out var engine))
            return engine;

        _logger.LogWarning("Unknown engine id: {EngineId}", engineId);
        return null;
    }

    public IReadOnlyList<IRegexEngine> GetEngines() => _enginesById.Values.ToList();
}
