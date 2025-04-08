using Jomlet.Models;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests
{
    public class TomlTableArrayTests
    {
        private JomlDocument GetDocument(string resource)
        {
            var parser = new JomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void SimpleTableArraysAreSupported()
        {
            var document = GetDocument(TestResources.SimpleTableArrayTestInput);

            Assert.Single(document.Entries.Keys, keyName => keyName == "products");

            var products = Assert.IsType<JomlArray>(document.GetValue("products"));
            Assert.Equal(3, products.Count);

            var product1 = Assert.IsType<JomlTable>(products[0]);
            var product2 = Assert.IsType<JomlTable>(products[1]);
            var product3 = Assert.IsType<JomlTable>(products[2]);
            
            Assert.Equal("Hammer", product1.GetString("name"));
            Assert.Equal(738594937, product1.GetInteger("sku"));
            
            Assert.Empty(product2.Entries);
            
            Assert.Equal("Nail", product3.GetString("name"));
            Assert.Equal(284758393, product3.GetInteger("sku"));
            Assert.Equal("gray", product3.GetString("color"));
        }

        [Fact]
        public void ComplexTableArraysAreSupported()
        {
            var document = GetDocument(TestResources.ComplexTableArrayTestInput);

            Assert.Single(document.Entries);
            
            Assert.NotNull(document.GetArray("fruits"));
            Assert.Equal(2, document.GetArray("fruits").Count);

            //Apple
            var firstFruit = Assert.IsType<JomlTable>(document.GetArray("fruits")[0]);
            Assert.Equal("apple", firstFruit.GetString("name"));

            var physical = Assert.IsType<JomlTable>(firstFruit.GetValue("physical"));
            var jam = Assert.IsType<JomlTable>(firstFruit.GetValue("jam"));
            var varieties = Assert.IsType<JomlArray>(firstFruit.GetValue("varieties"));
            
            Assert.Equal("red", physical.GetString("color"));
            Assert.Equal("round", physical.GetString("shape"));

            Assert.Equal("yellow", jam.GetString("color"));
            Assert.Equal("sticky", jam.GetString("feel"));

            Assert.Equal(2, varieties.Count);
            Assert.Equal("red delicious", Assert.IsType<JomlTable>(varieties[0]).GetString("name"));
            Assert.Equal("granny smith", Assert.IsType<JomlTable>(varieties[1]).GetString("name"));
            
            //Banana
            var secondFruit = Assert.IsType<JomlTable>(document.GetArray("fruits")[1]);
            Assert.Equal("banana", secondFruit.GetString("name"));

            physical = Assert.IsType<JomlTable>(secondFruit.GetValue("physical"));
            var newtonian = Assert.IsType<JomlTable>(physical.GetValue("newtonian"));
            varieties = Assert.IsType<JomlArray>(secondFruit.GetValue("varieties"));

            Assert.Equal("yellow", physical.GetString("color"));
            Assert.Equal(118, newtonian.GetInteger("weight"));

            Assert.Single(varieties, val => Assert.IsType<JomlTable>(val).GetString("name") == "plantain");
        }

        [Fact]
        public void TableArraySerializationWorks()
        {
            //In order for table-array serialization to trigger, at least one of the tables has to be complicated (>= 5 entries or a nested one)
            var aComplexObject = new {
                name = "a",
                value = new {
                    a = "b",
                    c = "d"
                }
            };

            var array = new[] {aComplexObject, aComplexObject};
            
            var documentRoot = new {
                array
            };
            
            var tomlString = JomletMain.TomlStringFrom(documentRoot).Trim().ReplaceLineEndings();

            var expectedResult = @"
[[array]]
name = ""a""
value = { a = ""b"", c = ""d"" }

[[array]]
name = ""a""
value = { a = ""b"", c = ""d"" }
".Trim().ReplaceLineEndings();
            
            Assert.Equal(expectedResult, tomlString);
        }

        [Fact]
        public void TablesWithNestedArraysHaveCorrectWhitespace()
        {
            var item = new TomlClassWithNestedArray()
            {
                Root = new()
                {
                    SomeValue = "Hello!",
                    Array = new []
                    {
                        new TomlClassWithNestedArray.ClassWithArray.ArrayItem
                        {
                            A = "A",
                            B = "B"
                        },
                        new TomlClassWithNestedArray.ClassWithArray.ArrayItem
                        {
                            A = "C",
                            B = "D"
                        }
                    }
                }
            };

            var str = JomletMain.TomlStringFrom(item);
            
            Assert.Equal(@"[Root]
SomeValue = ""Hello!""

[[Root.Array]]
A = ""A""
B = ""B""

[[Root.Array]]
A = ""C""
B = ""D""".ReplaceLineEndings(), str.Trim().ReplaceLineEndings());
        }
    }
}