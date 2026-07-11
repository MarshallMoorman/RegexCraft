using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Engines.DotNet;
using RegexCraft.Engines.JavaScript;
using RegexCraft.Engines.Pcre;

namespace RegexCraft.Engines;

/// <summary>
/// Creates the standard set of RegexCraft engines (.NET, PCRE2, JavaScript).
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
            new JavaScriptRegexEngine(loggerFactory.CreateLogger<JavaScriptRegexEngine>()),
        ];
    }
}
