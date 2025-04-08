using Jomlet.Exceptions;
using Jomlet.Models;
using Xunit;

namespace Jomlet.Tests
{
    public class NullTests
    {
        private JomlDocument GetDocument(string resource)
        {
            var parser = new JomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void BasicNullFunctionsAsIntended()
        {
            var document = GetDocument(TestResources.BasicNullTestInput);

            Assert.Single(document.Entries);
            
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("null1", key1)
            );

            Assert.Collection(document.Entries.Values,
                entry => Assert.Null(Assert.IsType<JomlNull>(entry).Value)
            );
        }
    }
}