﻿using System;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests
{
    public class ObjectToStringTests
    {
        [Fact]
        public void SimpleObjectToTomlStringWorks()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var serializedForm = JomletMain.TomlStringFrom(testObject);

            Assert.Equal("MyString = \"Hello, world!\"\nMyFloat = 420.69000244140625\nMyBool = true\nMyDateTime = 1970-01-01T07:00:00", serializedForm.Trim());
        }

        [Fact]
        public void SerializingSimpleObjectAndDeserializingAgainGivesEquivalentObject()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };
            
            var serializedForm = JomletMain.TomlStringFrom(testObject);

            var deserializedAgain = JomletMain.To<SimplePrimitiveTestClass>(serializedForm);

            Assert.Equal(testObject, deserializedAgain);
        }

        [Fact]
        public void SerializingSimplePropertyClassToTomlStringWorks()
        {
            var testObject = new SimplePropertyTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var serializedForm = JomletMain.TomlStringFrom(testObject);

            Assert.Equal("MyString = \"Hello, world!\"\nMyFloat = 420.69000244140625\nMyBool = true\nMyDateTime = 1970-01-01T07:00:00", serializedForm.Trim());
        }

        [Fact]
        public void SerializingSimplePropertyClassAndDeserializingAgainGivesEquivalentObject()
        {
            var testObject = new SimplePropertyTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var serializedForm = JomletMain.TomlStringFrom(testObject);

            var deserializedAgain = JomletMain.To<SimplePropertyTestClass>(serializedForm);

            Assert.Equal(testObject, deserializedAgain);
        }

        [Fact]
        public void SerializingSimpleTestRecordToTomlStringWorks()
        {
            var testObject = new SimpleTestRecord("Hello, world!", 420.69f, true,
                new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc));
            var serializedForm = JomletMain.TomlStringFrom(testObject);
        
            Assert.Equal("MyString = \"Hello, world!\"\nMyFloat = 420.69000244140625\nMyBool = true\nMyDateTime = 1970-01-01T07:00:00", serializedForm.Trim());
        }

        [Fact]
        public void SerializingSimpleTestRecordAndDeserializingAgainGivesEquivalentObject()
        {
            var testObject = new SimpleTestRecord("Hello, world!", 420.69f, true,
                new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc));

            var serializedForm = JomletMain.TomlStringFrom(testObject);
        
            var deserializedAgain = JomletMain.To<SimpleTestRecord>(serializedForm);
        
            Assert.Equal(testObject, deserializedAgain);
        }

        [Fact]
        public void SerializingAnEmptyObjectGivesAnEmptyString()
        {
            var tomlString = JomletMain.TomlStringFrom(new {}).Trim();
            
            Assert.Equal(string.Empty, tomlString);
        }
        
        [Fact]
        public void AttemptingToDirectlySerializeNullReturnsNull()
        {
            //We need to use a type of T that actually has something to serialize
            Assert.Null(JomletMain.DocumentFrom(typeof(SimplePrimitiveTestClass), null!, null));
        }
    }
}