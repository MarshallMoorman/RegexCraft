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
    public bool IgnoreCase { get; set; }
    public bool Multiline { get; set; }
    public bool Singleline { get; set; }
    public bool ExplicitCapture { get; set; }
    public bool IgnorePatternWhitespace { get; set; }
}
