using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Engines;

/// <summary>
/// Creates the standard set of Phase 0 engines.
/// </summary>
public static class EngineFactory
{
    public static IReadOnlyList<IRegexEngine> CreateDefaultEngines(ILoggerFactory? loggerFactory = null)
    {
        loggerFactory ??= NullLoggerFactory.Instance;

        return
        [
            new DotNetRegexEngine(loggerFactory.CreateLogger<DotNetRegexEngine>()),
            new PcreRegexEngine(loggerFactory.CreateLogger<PcreRegexEngine>()),
        ];
    }
}
