﻿using System;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests
{
    public class ObjectToTomlDocTests
    {
        
        [Fact]
        public void SimpleObjectToTomlDocWorks()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var tomlDoc = JomletMain.DocumentFrom(testObject);
            
            Assert.Equal(4, tomlDoc.Entries.Count);
            Assert.True(tomlDoc.GetBoolean("MyBool"));
            Assert.True(Math.Abs(tomlDoc.GetFloat("MyFloat") - 420.69) < 0.01);
            Assert.Equal("Hello, world!", tomlDoc.GetString("MyString"));
            Assert.Equal("1970-01-01T07:00:00", tomlDoc.GetValue("MyDateTime").StringValue);
        }

        [Fact]
        public void SerializationRecptsOverridingFieldsUsingNewKeyword()
        {
            var testObject = new ClassWhichOverwritesFieldUsingNewKeyword
            {
                OverwrittenField = "this should be overwritten, not 'default value' as set in superclass.",
                NotOverwrittenSubclassField = "this is a non-overwritten field, defined only in the subclass.",
                NotOverwrittenSuperclassField = "this is a non-overwritten field, defined only in the superclass."
            };

            var expectedResult = @"OverwrittenField = ""this should be overwritten, not 'default value' as set in superclass.""
NotOverwrittenSubclassField = ""this is a non-overwritten field, defined only in the subclass.""
NotOverwrittenSuperclassField = ""this is a non-overwritten field, defined only in the superclass.""
";

            var tomlDoc = JomletMain.DocumentFrom(testObject);

            Assert.Equal(expectedResult.ReplaceLineEndings(), tomlDoc.SerializedValue.ReplaceLineEndings());
        }

        [Fact]
        public void SimplePropertyClassToTomlDocWorks()
        {
            var testObject = new SimplePropertyTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var tomlDoc = JomletMain.DocumentFrom(testObject);

            Assert.Equal(4, tomlDoc.Entries.Count);
            Assert.True(tomlDoc.GetBoolean("MyBool"));
            Assert.True(Math.Abs(tomlDoc.GetFloat("MyFloat") - 420.69) < 0.01);
            Assert.Equal("Hello, world!", tomlDoc.GetString("MyString"));
            Assert.Equal("1970-01-01T07:00:00", tomlDoc.GetValue("MyDateTime").StringValue);
        }
        
        [Fact]
        public void SimpleTestRecordToTomlDocWorks()
        {
            var testObject = new SimpleTestRecord("Hello, world!", 420.69f, true,
                new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc));

            var tomlDoc = JomletMain.DocumentFrom(testObject);
        
            Assert.Equal(4, tomlDoc.Entries.Count);
            Assert.True(tomlDoc.GetBoolean("MyBool"));
            Assert.True(Math.Abs(tomlDoc.GetFloat("MyFloat") - 420.69) < 0.01);
            Assert.Equal("Hello, world!", tomlDoc.GetString("MyString"));
            Assert.Equal("1970-01-01T07:00:00", tomlDoc.GetValue("MyDateTime").StringValue);
        }
    }
}