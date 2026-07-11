using RegexCraft.Core.Settings;

namespace RegexCraft.Tests.Settings;

[TestFixture]
[Category("Library")]
public sealed class JsonSettingsStoreTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "regexcraft-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }
        catch
        {
            // ignore
        }
    }

    [Test]
    public void SaveAndLoad_RoundTrips()
    {
        var path = Path.Combine(_dir, "settings.json");
        var store = new JsonSettingsStore(path);
        store.Save(new AppSettings
        {
            Theme = "Dark",
            FlavorId = "pcre2",
            LastGrepRoot = "/tmp/proj",
            GrepRecursive = false,
            WindowWidth = 1400,
            WindowHeight = 900,
        });

        var loaded = new JsonSettingsStore(path).Load();
        Assert.That(loaded.Theme, Is.EqualTo("Dark"));
        Assert.That(loaded.FlavorId, Is.EqualTo("pcre2"));
        Assert.That(loaded.LastGrepRoot, Is.EqualTo("/tmp/proj"));
        Assert.That(loaded.GrepRecursive, Is.False);
        Assert.That(loaded.WindowWidth, Is.EqualTo(1400));
    }

    [TestCase("Light")]
    [TestCase("Dark")]
    [TestCase("System")]
    public void Theme_PersistsAcrossReload(string theme)
    {
        var path = Path.Combine(_dir, $"theme-{theme}.json");
        new JsonSettingsStore(path).Save(new AppSettings { Theme = theme });
        var loaded = new JsonSettingsStore(path).Load();
        Assert.That(loaded.Theme, Is.EqualTo(theme));
    }
}
