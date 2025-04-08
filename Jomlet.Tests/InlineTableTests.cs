using Tomlet.Exceptions;
using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class InlineTableTests
    {
        private JomlDocument GetDocument(string resource)
        {
            var parser = new JomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void InlineTablesAreSupported()
        {
            var document = GetDocument(TestResources.BasicInlineTableTestInput);
            
            Assert.Equal(4, document.Entries.Count);
            
            Assert.NotNull(document.GetSubTable("name"));
            Assert.NotNull(document.GetSubTable("empty"));
            Assert.NotNull(document.GetSubTable("point"));
            Assert.NotNull(document.GetSubTable("animal"));
        }

        [Fact]
        public void TablesWithKeysContainingWhitespaceDoNotSerializeInline()
        {
            //Serializing these inline means we have to duplicate all the key quoting shenanigans, it's easier just to not inline them
            var obj = new KeyWithWhitespaceTestClass() {KeyWithWhitespace = "hello"};
            var document = JomlDocument.CreateEmpty();
            document.Put("myTable", obj);

            var tomlString = document.SerializedValue;
            var doc = GetDocument(tomlString);
            var newObj = JomletMain.To<KeyWithWhitespaceTestClass>(doc.GetSubTable("myTable"));
            
            Assert.Equal(obj.KeyWithWhitespace, newObj.KeyWithWhitespace);
        }

        [Fact]
        public void MembersAnnotatedDoNotInlineAreNotInlined()
        {
            var obj = new ClassWithDoNotInlineMembers()
            {
                ShouldBeInlined =
                {
                    ["key"] = "value"
                },
                ShouldNotBeInlinedField =
                {
                    ["key"] = "value"
                },
                ShouldNotBeInlinedProp =
                {
                    ["key"] = "value"
                }
            };

            var tomlString = JomletMain.TomlStringFrom(obj).Trim();

            var expectedString = @"
ShouldBeInlined = { key = ""value"" }
[ShouldNotBeInlinedField]
key = ""value""

[ShouldNotBeInlinedProp]
key = ""value""
".Trim().ReplaceLineEndings("\n");
            
            Assert.Equal(expectedString, tomlString);
        }
    }
}