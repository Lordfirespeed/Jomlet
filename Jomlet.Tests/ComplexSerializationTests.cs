using System;
using Tomlet.Tests.TestDataGenerators;
using Tomlet.Tests.TestModelClasses;
using Xunit;
using Xunit.Abstractions;

namespace Tomlet.Tests
{
    public class ComplexSerializationTests
    {
        
        private readonly ITestOutputHelper _testOutputHelper;

        public ComplexSerializationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void ComplexSerializationWorks()
        {
            var testClass = new ComplexTestClass
            {
                TestString = "Hello world, how are you?",
                ClassOnes =
                {
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Hello"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "World"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "How"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Are"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "You"},
                },
                SubClass2 = new ComplexTestClass.SubClassTwo {
                    SubKeyOne = "Hello world, how are you?",
                    SubKeyTwo = DateTimeOffset.Now,
                    SubKeyThree = 17,
                    SubKeyFour = 2.34f,
                }
            };

            var tomlString = JomletMain.TomlStringFrom(testClass);
            
            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);
            
            var deserializedAgain = JomletMain.To<ComplexTestClass>(tomlString);
            
            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void SerializingNullFieldsExcludesThem()
        {
            var testClass = new ComplexTestClass
            {
                TestString = null,
                ClassOnes =
                {
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Hello"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "World"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "How"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Are"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "You"},
                },
                SubClass2 = null
            };

            var tomlString = JomletMain.TomlStringFrom(testClass);
            
            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var doc = new JomlParser().Parse(tomlString);
            
            Assert.False(doc.ContainsKey("SC2"));
            Assert.False(doc.ContainsKey("TestString"));
            
            var deserializedAgain = JomletMain.To<ComplexTestClass>(tomlString);
            
            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void SerializingNullPropertiesExcludesThem()
        {
            var testClass = new SimplePropertyTestClass
            {
                MyString = null,
                MyBool = true,
            };

            var tomlString = JomletMain.TomlStringFrom(testClass);

            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var doc = new JomlParser().Parse(tomlString);

            Assert.False(doc.ContainsKey("MyString"));

            var deserializedAgain = JomletMain.To<SimplePropertyTestClass>(tomlString);

            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void ComplexRecordSerializationWorks()
        {
            var testRecord = new ComplexTestRecord
            {
                MyString = "Test",
                MyWidget = new Widget
                {
                    MyInt = 1,
                },
            };

            var tomlString = JomletMain.TomlStringFrom(testRecord);

            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var deserializedAgain = JomletMain.To<ComplexTestRecord>(tomlString);

            Assert.Equal(testRecord, deserializedAgain);
        }

        [Fact]
        public void DeserializingArraysWorks()
        {
            //Test string taken from a query on discord, that's where the unusual email addresses come from
            var deserialized = JomletMain.To<ExampleMailboxConfigClass>(TestResources.ExampleMailboxConfigurationTestInput);
            
            Assert.Equal("whatev@gmail.com", deserialized.mailbox);
            Assert.Equal("no", deserialized.username);
            Assert.Equal("secret", deserialized.password);
            
            Assert.Equal(2, deserialized.rules.Length);
            
            Assert.Equal(2, deserialized.rules[0].allowed.Length);
            Assert.Equal("yeet@gmail.com", deserialized.rules[0].address);
            
            Assert.Equal("urmum@gmail.com", deserialized.rules[1].address);
        }

        [Fact]
        public void SerializationChoosesInlineTablesForSimpleObjects()
        {
            var anObject = new {value = 1, str = "hello"};

            var anObjectWithTheObject = new {obj = anObject};

            var tomlString = JomletMain.TomlStringFrom(anObjectWithTheObject);
            
            Assert.Equal("obj = { value = 1, str = \"hello\" }", tomlString.Trim());
            
        }
        
        [Theory]
        [ClassData(typeof(EnumerableSerializerDataGenerator))]
        public void EnumerableShouldSerialize(StringEnumerableWrapper inputWrapper, string expectedOutput)
        {
            var tomlString = JomletMain.TomlStringFrom(inputWrapper).Trim();
            Assert.Equal(tomlString, expectedOutput);
        }
    }
}