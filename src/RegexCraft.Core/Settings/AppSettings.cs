namespace RegexCraft.Core.Settings;

/// <summary>Persisted UI / session preferences.</summary>
public sealed class AppSettings
{
    public string Theme { get; set; } = "System";
    public string FlavorId { get; set; } = "dotnet";
    public string LastGrepRoot { get; set; } = string.Empty;
    public string GrepIncludeGlobs { get; set; } = "*.cs;*.json;*.md;*.txt;*.xml;*.yml;*.yaml;*.html;*.js;*.ts;*.py";
    public string GrepExcludeGlobs { get; set; } =
        "bin/**;obj/**;.git/**;node_modules/**;*.dll;*.exe;*.pdb;*.png;*.jpg;*.jpeg;*.gif;*.ico;*.woff;*.woff2;*.ttf;*.zip;*.gz";
    public bool GrepRecursive { get; set; } = true;
    public bool GrepCreateBackup { get; set; } = true;
    public bool OptionsExpanded { get; set; } = true;
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
    public int? WindowX { get; set; }
    public int? WindowY { get; set; }

    /// <summary>
    /// Remembered right-panel width (px) for Test / Replace / Split / Generate / GREP.
    /// </summary>
    public double? RightPanelNormalWidth { get; set; }

    /// <summary>
    /// Remembered right-panel width (px) when Compare mode is active.
    /// </summary>
    public double? RightPanelCompareWidth { get; set; }

    public bool IgnoreCase { get; set; }
    public bool Multiline { get; set; }
    public bool Singleline { get; set; }
    public bool ExplicitCapture { get; set; }
    public bool IgnorePatternWhitespace { get; set; }

    /// <summary>
    /// Honor-system acknowledgment: user indicates business use and holds a commercial license.
    /// No keys or activation — local preference only.
    /// </summary>
    public bool BusinessLicenseAcknowledged { get; set; }
}
