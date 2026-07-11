namespace RegexCraft.Core.Library;

/// <summary>Resolves the per-user data directory for RegexCraft persistence.</summary>
public static class AppDataPaths
{
    public static string GetDataDirectory()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".regexcraft");
        else
            root = Path.Combine(root, "RegexCraft");

        Directory.CreateDirectory(root);
        return root;
    }
}
