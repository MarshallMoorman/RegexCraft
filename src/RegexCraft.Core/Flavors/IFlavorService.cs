using RegexCraft.Core.Engines;

namespace RegexCraft.Core.Flavors;

/// <summary>
/// Resolves flavors to engines and enumerates available flavors.
/// Adding a third flavor should only require a new definition + optional engine class.
/// </summary>
public interface IFlavorService
{
    IReadOnlyList<FlavorDefinition> GetFlavors();
    FlavorDefinition? GetFlavor(string flavorId);
    IRegexEngine? GetEngineForFlavor(string flavorId);
    IRegexEngine? GetEngine(string engineId);
    IReadOnlyList<IRegexEngine> GetEngines();
}
