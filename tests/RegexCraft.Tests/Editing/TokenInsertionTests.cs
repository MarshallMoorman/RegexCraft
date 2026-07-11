using RegexCraft.Core.Editing;

namespace RegexCraft.Tests.Editing;

[TestFixture]
[Category("Editing")]
public sealed class TokenInsertionTests
{
    [Test]
    public void Insert_AtCaret_NoSelection()
    {
        var result = TokenInsertion.Insert("ab", caretOffset: 1, selectionLength: 0, insertText: "XY");
        Assert.That(result.NewText, Is.EqualTo("aXYb"));
        Assert.That(result.NewCaretOffset, Is.EqualTo(3));
    }

    [Test]
    public void Insert_ReplacesSelection()
    {
        var result = TokenInsertion.Insert("hello", caretOffset: 1, selectionLength: 3, insertText: "XX");
        Assert.That(result.NewText, Is.EqualTo("hXXo"));
        Assert.That(result.NewCaretOffset, Is.EqualTo(3));
    }

    [Test]
    public void Insert_WithCaretOffsetInInsert_PlacesCaretInside()
    {
        var result = TokenInsertion.Insert("", 0, 0, "()", caretOffsetInInsert: 1);
        Assert.That(result.NewText, Is.EqualTo("()"));
        Assert.That(result.NewCaretOffset, Is.EqualTo(1));
    }

    [Test]
    public void Insert_AtEnd()
    {
        var result = TokenInsertion.Insert("abc", 3, 0, "d");
        Assert.That(result.NewText, Is.EqualTo("abcd"));
        Assert.That(result.NewCaretOffset, Is.EqualTo(4));
    }

    [Test]
    public void Insert_ClampsInvalidOffsets()
    {
        var result = TokenInsertion.Insert("ab", 50, 0, "z");
        Assert.That(result.NewText, Is.EqualTo("abz"));
    }

    [Test]
    public void Insert_NullSource_TreatedAsEmpty()
    {
        var result = TokenInsertion.Insert(null!, 0, 0, "x");
        Assert.That(result.NewText, Is.EqualTo("x"));
    }
}
