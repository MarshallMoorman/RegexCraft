using RegexCraft.Core.Editing;

namespace RegexCraft.Tests.Editing;

[TestFixture]
[Category("Tokens")]
[Category("Editing")]
public sealed class TokenInsertionExpandedTests
{
    [Test]
    public void Insert_AtCaret_InsertsText()
    {
        var r = TokenInsertion.Insert("ab", caretOffset: 1, selectionLength: 0, insertText: @"\d");
        Assert.That(r.NewText, Is.EqualTo(@"a\db"));
        Assert.That(r.NewCaretOffset, Is.EqualTo(3));
    }

    [Test]
    public void Insert_ReplacesSelection()
    {
        var r = TokenInsertion.Insert("hello", caretOffset: 1, selectionLength: 3, insertText: "XX");
        Assert.That(r.NewText, Is.EqualTo("hXXo"));
    }

    [Test]
    public void Insert_WithCaretInInsert_PositionsInside()
    {
        var r = TokenInsertion.Insert("", caretOffset: 0, selectionLength: 0, insertText: "()", caretOffsetInInsert: 1);
        Assert.That(r.NewText, Is.EqualTo("()"));
        Assert.That(r.NewCaretOffset, Is.EqualTo(1));
    }

    [Test]
    public void Insert_AtEnd()
    {
        var r = TokenInsertion.Insert("abc", caretOffset: 3, selectionLength: 0, insertText: "+");
        Assert.That(r.NewText, Is.EqualTo("abc+"));
        Assert.That(r.NewCaretOffset, Is.EqualTo(4));
    }

    [Test]
    public void Insert_AtStart()
    {
        var r = TokenInsertion.Insert("abc", caretOffset: 0, selectionLength: 0, insertText: "^");
        Assert.That(r.NewText, Is.EqualTo("^abc"));
        Assert.That(r.NewCaretOffset, Is.EqualTo(1));
    }

    [Test]
    public void Insert_ClampsOutOfRangeCaret()
    {
        var r = TokenInsertion.Insert("ab", caretOffset: 99, selectionLength: 0, insertText: "x");
        Assert.That(r.NewText, Is.EqualTo("abx"));
    }

    [Test]
    public void Insert_EmptyInsert_IsNoOpText()
    {
        var r = TokenInsertion.Insert("ab", caretOffset: 1, selectionLength: 0, insertText: "");
        Assert.That(r.NewText, Is.EqualTo("ab"));
    }
}
