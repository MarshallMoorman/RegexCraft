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
        Assert.That(saved.IsBuiltIn, Is.False);

        var reloaded = new JsonLibraryStore(path);
        var all = reloaded.GetAll();
        // Built-ins are always merged in
        Assert.That(all.Count, Is.GreaterThanOrEqualTo(1 + BuiltInLibrary.GetDefaults().Count));
        var user = all.First(e => e.Name == "Email" && !e.IsBuiltIn);
        Assert.That(user.Pattern, Is.EqualTo(@"\w+@\w+"));
        Assert.That(user.IgnoreCase, Is.True);
    }

    [Test]
    public void Library_SearchAndDelete()
    {
        var path = Path.Combine(_dir, "library2.json");
        var store = new JsonLibraryStore(path);
        store.Save(new LibraryEntry { Name = "Digits", Pattern = @"\d+" });
        store.Save(new LibraryEntry { Name = "Words", Pattern = @"\w+" });

        Assert.That(store.Search("digit").Count(e => e.Name == "Digits"), Is.EqualTo(1));
        var id = store.GetAll().First(e => e.Name == "Digits").Id;
        Assert.That(store.Delete(id), Is.True);
        Assert.That(store.GetAll().Any(e => e.Name == "Digits"), Is.False);
        Assert.That(store.GetAll().Any(e => e.Name == "Words"), Is.True);
    }

    [Test]
    public void Library_SeedsBuiltInsOnEmptyFile()
    {
        var path = Path.Combine(_dir, "library-empty.json");
        var store = new JsonLibraryStore(path);
        var all = store.GetAll();
        var builtins = BuiltInLibrary.GetDefaults();
        Assert.That(all.Count, Is.GreaterThanOrEqualTo(builtins.Count));
        Assert.That(all.Count(e => e.IsBuiltIn), Is.EqualTo(builtins.Count));
        Assert.That(all.Any(e => e.Name.Contains("Email", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(all.Any(e => e.Name.Contains("UUID", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void Library_CannotDeleteBuiltIn()
    {
        var path = Path.Combine(_dir, "library-builtin-del.json");
        var store = new JsonLibraryStore(path);
        var builtin = store.GetAll().First(e => e.IsBuiltIn);
        Assert.That(store.Delete(builtin.Id), Is.False);
        Assert.That(store.GetById(builtin.Id), Is.Not.Null);
    }

    [Test]
    public void Library_BuiltInFavoritePreservedOnReload()
    {
        var path = Path.Combine(_dir, "library-fav-builtin.json");
        var store = new JsonLibraryStore(path);
        var builtin = store.GetAll().First(e => e.IsBuiltIn);
        builtin.IsFavorite = true;
        store.Save(builtin);

        var reloaded = new JsonLibraryStore(path);
        var again = reloaded.GetById(builtin.Id);
        Assert.That(again, Is.Not.Null);
        Assert.That(again!.IsFavorite, Is.True);
        Assert.That(again.IsBuiltIn, Is.True);
        Assert.That(again.Pattern, Is.EqualTo(builtin.Pattern));
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
        Assert.That(store.Search("inbox").Count(e => e.Name == "Starred"), Is.EqualTo(1));
        // "Email" may also match built-in email patterns
        Assert.That(store.Search("Email").Any(e => e.Name == "Starred"), Is.True);
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
