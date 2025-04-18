using Jomlet.Models;
using Xunit;

namespace Jomlet.Tests;

public class CommentDeserializationTests
{
    private JomlDocument GetDocument(string resource)
    {
        var parser = new JomlParser();
        return parser.Parse(resource);
    }
    
    [Fact]
    public void CommentsCanBeReadFromTheFileCorrectly()
    {
        var doc = GetDocument(TestResources.CommentTestInput);

        var firstValue = doc.GetValue("key1");
        Assert.Null(firstValue.Comments.PrecedingComment);
        Assert.Null(firstValue.Comments.InlineComment);

        var secondValue = doc.GetValue("key2");
        Assert.Equal("This is a full-line comment", secondValue.Comments.PrecedingComment);
        Assert.Equal("This is a comment at the end of a line", secondValue.Comments.InlineComment);
    }
}