using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using NUnit.Framework;

[assembly: AvaloniaTestApplication(typeof(RegexCraft.Tests.Headless.TestAppBuilder))]
// Avalonia headless is not safe under multi-worker NUnit parallelization (dispatcher ownership).
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]

namespace RegexCraft.Tests.Headless;

/// <summary>
/// Headless Avalonia host for UI and screenshot tests.
/// Skia + UseHeadlessDrawing=false enables CaptureRenderedFrame().
/// </summary>
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false,
            })
            .WithInterFont();
}
