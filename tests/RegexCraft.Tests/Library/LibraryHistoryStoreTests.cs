using RegexCraft.Core.Library;

namespace RegexCraft.Tests.Library;

[TestFixture]
[Category("Library")]
public sealed class LibraryHistoryStoreTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "regexcraft-lib-" + Guid.NewGuid().ToString("N"));
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
    public void Library_SaveLoadPersist()
    {
        var path = Path.Combine(_dir, "library.json");
        var store = new JsonLibraryStore(path);
        var saved = store.Save(new LibraryEntry
        {
            Name = "Email",
            Description = "Simple email",
            Pattern = @"\w+@\w+",
            Subject = "a@b",
            FlavorId = "dotnet",
            IgnoreCase = true,
        });

        Assert.That(saved.Id, Is.Not.Empty);

        var reloaded = new JsonLibraryStore(path);
        var all = reloaded.GetAll();
        Assert.That(all, Has.Count.EqualTo(1));
        Assert.That(all[0].Name, Is.EqualTo("Email"));
        Assert.That(all[0].Pattern, Is.EqualTo(@"\w+@\w+"));
        Assert.That(all[0].IgnoreCase, Is.True);
    }

    [Test]
    public void Library_SearchAndDelete()
    {
        var path = Path.Combine(_dir, "library2.json");
        var store = new JsonLibraryStore(path);
        store.Save(new LibraryEntry { Name = "Digits", Pattern = @"\d+" });
        store.Save(new LibraryEntry { Name = "Words", Pattern = @"\w+" });

        Assert.That(store.Search("digit"), Has.Count.EqualTo(1));
        var id = store.GetAll().First(e => e.Name == "Digits").Id;
        Assert.That(store.Delete(id), Is.True);
        Assert.That(store.GetAll(), Has.Count.EqualTo(1));
        Assert.That(store.GetAll()[0].Name, Is.EqualTo("Words"));
    }

    [Test]
    public void Library_FavoritesAndTags_SearchAndSort()
    {
        var path = Path.Combine(_dir, "library-fav.json");
        var store = new JsonLibraryStore(path);
        store.Save(new LibraryEntry
        {
            Name = "Plain",
            Pattern = "a",
            Category = "Misc",
            Tags = "x",
            IsFavorite = false,
        });
        store.Save(new LibraryEntry
        {
            Name = "Starred",
            Pattern = "b",
            Category = "Email",
            Tags = "inbox,mail",
            IsFavorite = true,
        });

        var all = store.GetAll();
        Assert.That(all[0].Name, Is.EqualTo("Starred"));
        Assert.That(store.Search("Email"), Has.Count.EqualTo(1));
        Assert.That(store.Search("inbox"), Has.Count.EqualTo(1));
    }

    [Test]
    public void History_AddDedupAndCap()
    {
        var path = Path.Combine(_dir, "history.json");
        var store = new JsonHistoryStore(path, maxEntries: 5);

        store.Add(new HistoryEntry { Pattern = "a", Subject = "1", FlavorId = "dotnet" });
        store.Add(new HistoryEntry { Pattern = "a", Subject = "1", FlavorId = "dotnet" });
        Assert.That(store.GetRecent(), Has.Count.EqualTo(1));

        for (var i = 0; i < 10; i++)
            store.Add(new HistoryEntry { Pattern = $"p{i}", FlavorId = "dotnet" });

        Assert.That(store.GetRecent(), Has.Count.EqualTo(5));
    }

    [Test]
    public void History_PersistsAcrossInstances()
    {
        var path = Path.Combine(_dir, "history2.json");
        var store = new JsonHistoryStore(path);
        store.Add(new HistoryEntry { Pattern = "persist-me", Subject = "s", FlavorId = "pcre2" });

        var reloaded = new JsonHistoryStore(path);
        var recent = reloaded.GetRecent();
        Assert.That(recent, Has.Count.EqualTo(1));
        Assert.That(recent[0].Pattern, Is.EqualTo("persist-me"));
        Assert.That(recent[0].FlavorId, Is.EqualTo("pcre2"));
    }

    [Test]
    public void History_Clear()
    {
        var path = Path.Combine(_dir, "history3.json");
        var store = new JsonHistoryStore(path);
        store.Add(new HistoryEntry { Pattern = "x" });
        store.Clear();
        Assert.That(store.GetRecent(), Is.Empty);
    }
}
