﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jomlet.Exceptions;
using Jomlet.Models;
using Jomlet.Tests.TestDataGenerators;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests
{
    public class ClassDeserializationTests
    {
        [Fact]
        public void DictionaryDeserializationWorks()
        {
            var dict = JomletMain.To<Dictionary<string, string>>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal(4, dict.Count);
            Assert.Equal("Hello, world!", dict["MyString"]);
            Assert.Equal("690.42", dict["MyFloat"]);
            Assert.Equal("true", dict["MyBool"]);
            Assert.Equal("1970-01-01T07:00:00", dict["MyDateTime"]);
        }
        
        [Fact]
        public void SimpleCompositeDeserializationWorks()
        {
            var type = JomletMain.To<SimplePrimitiveTestClass>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void SimplePropertyDeserializationWorks()
        {
            var type = JomletMain.To<SimplePropertyTestClass>(TestResources.SimplePrimitiveDeserializationTestInput);

            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void SimpleRecordDeserializationWorks()
        {
            var type = JomletMain.To<SimpleTestRecord>(TestResources.SimplePrimitiveDeserializationTestInput);

            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void ClassWithParameterlessConstructorDeserializationWorks()
        {
            var type = JomletMain.To<ClassWithParameterlessConstructor>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal("Hello, world!", type.MyString);
        }
        
        [Fact]
        public void AnArrayOfEmptyStringsCanBeDeserialized()
        {
            var wrapper = JomletMain.To<StringArrayWrapper>(TestResources.ArrayOfEmptyStringTestInput);
            var array = wrapper.Array;
            
            Assert.Equal(5, array.Length);
            Assert.All(array, s => Assert.Equal(string.Empty, s));
        }
        
        [Theory]
        [ClassData(typeof(EnumerableDeserializerDataGenerator))]
        public void EnumerableCanBeDeserialized(Type wrapperType, string input, int expectedCount, string expectedValue)
        {
            dynamic wrapper = JomletMain.To(wrapperType, input);
            var array = (IEnumerable<string>)wrapper.Array;

            // ReSharper disable once PossibleMultipleEnumeration
            Assert.Equal(expectedCount, array.Count());
            // ReSharper disable once PossibleMultipleEnumeration
            Assert.All(array, s => Assert.Equal(expectedValue, s));
        }

        [Fact]
        public void AttemptingToDeserializeADocumentWithAnIncorrectlyTypedFieldThrows()
        {
            var document = JomlDocument.CreateEmpty();
            document.Put("MyFloat", "Not a float");

            var ex = Assert.Throws<JomlFieldTypeMismatchException>(() => JomletMain.To<SimplePrimitiveTestClass>(document));

            var msg = $"While deserializing an object of type {typeof(SimplePrimitiveTestClass).FullName}, found field MyFloat expecting a type of Double, but value in JOML was of type String";
            Assert.Equal(msg, ex.Message);
        }

        [Fact]
        public void ShouldOverrideDefaultConstructorsValues()
        {
            var options = new JomlSerializerOptions { OverrideConstructorValues = true };
            var type = JomletMain.To<ClassWithValuesSetOnConstructor>(TestResources.SimplePrimitiveDeserializationTestInput, options);
            
            Assert.Equal("Hello, world!", type.MyString);
        }
        
        [Fact]
        public void ShouldNotOverrideDefaultConstructorsValues()
        {
            var options = new JomlSerializerOptions { OverrideConstructorValues = false };
            var type = JomletMain.To<ClassWithValuesSetOnConstructor>(TestResources.SimplePrimitiveDeserializationTestInput, options);
            
            Assert.Equal("Modified on constructor!", type.MyString);
        }
    }
}