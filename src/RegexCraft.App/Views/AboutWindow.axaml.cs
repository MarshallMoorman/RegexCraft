using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RegexCraft.Core.Commercial;
using RegexCraft.Core.Settings;

namespace RegexCraft.App.Views;

public partial class AboutWindow : Window
{
    public const string WebsiteUrl = CommercialLinks.WebsiteUrl;
    public const string GitHubUrl = CommercialLinks.DistRepoUrl;
    public const string PricingUrl = CommercialLinks.PricingUrl;
    public const string EulaUrl = CommercialLinks.EulaUrl;
    public const string BuyLicenseUrl = CommercialLinks.BuyLicenseUrl;
    public const string DownloadUrl = CommercialLinks.DownloadUrl;

    private readonly ISettingsStore? _settingsStore;
    private AppSettings? _settings;
    private bool _suppressAckSave;

    public AboutWindow()
        : this(null)
    {
    }

    public AboutWindow(ISettingsStore? settingsStore)
    {
        _settingsStore = settingsStore;
        InitializeComponent();
        VersionTextBlock.Text = $"Version {GetAppVersion()}";
        LicenseSummaryText.Text = CommercialLinks.LicenseSummary;
        Title = "About RegexCraft";
        LoadBusinessAck();
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

    private void LoadBusinessAck()
    {
        if (_settingsStore is null)
            return;

        try
        {
            _settings = _settingsStore.Load();
            _suppressAckSave = true;
            BusinessAckCheckBox.IsChecked = _settings.BusinessLicenseAcknowledged;
            _suppressAckSave = false;
        }
        catch
        {
            _suppressAckSave = false;
        }
    }

    private void BusinessAck_OnCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressAckSave || _settingsStore is null)
            return;

        try
        {
            _settings ??= _settingsStore.Load();
            _settings.BusinessLicenseAcknowledged = BusinessAckCheckBox.IsChecked == true;
            _settingsStore.Save(_settings);
        }
        catch
        {
            // Local honor-system flag only — ignore I/O failures
        }
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void Website_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(WebsiteUrl);

    private void Pricing_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(PricingUrl);

    private void Downloads_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(DownloadUrl);

    private void BuyLicense_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(BuyLicenseUrl);

    private void Eula_OnClick(object? sender, RoutedEventArgs e) => OpenUrl(EulaUrl);

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
