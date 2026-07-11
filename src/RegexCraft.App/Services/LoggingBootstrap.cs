using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace RegexCraft.App.Services;

/// <summary>
/// Configures Serilog from appsettings.json with 7-day rolling file defaults.
/// </summary>
public static class LoggingBootstrap
{
    public static ILoggerFactory CreateLoggerFactory(out IConfiguration configuration)
    {
        var basePath = AppContext.BaseDirectory;
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "Production";

        configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Ensure logs directory exists relative to working directory (and base directory).
        EnsureLogsDirectory();

        var serilogLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "RegexCraft")
            .Enrich.WithProperty("Version", GetAppVersion())
            .CreateLogger();

        Log.Logger = serilogLogger;

        return new SerilogLoggerFactory(serilogLogger, dispose: true);
    }

    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }

    private static void EnsureLogsDirectory()
    {
        try
        {
            Directory.CreateDirectory("logs");
            var baseLogs = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(baseLogs);
        }
        catch
        {
            // Best-effort; Serilog will surface write failures if needed.
        }
    }

    private static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "0.1.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
