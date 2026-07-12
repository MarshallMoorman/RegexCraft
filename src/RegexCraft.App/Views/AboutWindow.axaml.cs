using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RegexCraft.App.Views;

public partial class AboutWindow : Window
{
    public const string WebsiteUrl = "https://regexcraft.com";
    public const string GitHubUrl = "https://github.com/marshallmoorman/RegexCraft";

    public AboutWindow()
    {
        InitializeComponent();
        VersionTextBlock.Text = $"Version {GetAppVersion()}";
        Title = "About RegexCraft";
    }

    public static string GetAppVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            var plus = info.IndexOf('+');
            return plus > 0 ? info[..plus] : info;
        }

        var version = asm.GetName().Version;
        return version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void Website_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(WebsiteUrl);

    private void GitHub_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(GitHubUrl);

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch
        {
            // Browser launch may fail in headless/CI — ignore
        }
    }
}
