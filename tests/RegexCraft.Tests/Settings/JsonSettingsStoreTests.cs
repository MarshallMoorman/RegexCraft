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
            RightPanelNormalWidth = 380,
            RightPanelCompareWidth = 540,
        });

        var loaded = new JsonSettingsStore(path).Load();
        Assert.That(loaded.Theme, Is.EqualTo("Dark"));
        Assert.That(loaded.FlavorId, Is.EqualTo("pcre2"));
        Assert.That(loaded.LastGrepRoot, Is.EqualTo("/tmp/proj"));
        Assert.That(loaded.GrepRecursive, Is.False);
        Assert.That(loaded.WindowWidth, Is.EqualTo(1400));
        Assert.That(loaded.RightPanelNormalWidth, Is.EqualTo(380));
        Assert.That(loaded.RightPanelCompareWidth, Is.EqualTo(540));
    }

    [Test]
    public void RightPanelWidths_RoundTripIndependently()
    {
        var path = Path.Combine(_dir, "panel-widths.json");
        new JsonSettingsStore(path).Save(new AppSettings
        {
            RightPanelNormalWidth = 360,
            RightPanelCompareWidth = 560,
        });
        var loaded = new JsonSettingsStore(path).Load();
        Assert.That(loaded.RightPanelNormalWidth, Is.EqualTo(360));
        Assert.That(loaded.RightPanelCompareWidth, Is.EqualTo(560));
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
