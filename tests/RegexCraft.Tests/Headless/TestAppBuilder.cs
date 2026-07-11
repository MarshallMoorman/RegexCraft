using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;

[assembly: AvaloniaTestApplication(typeof(RegexCraft.Tests.Headless.TestAppBuilder))]

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
