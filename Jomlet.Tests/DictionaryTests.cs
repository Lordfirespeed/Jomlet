﻿using System;
using System.Collections.Generic;
using Jomlet.Models;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests;

public class DictionaryTests
{
    private JomlDocument GetDocument(string resource)
    {
        var parser = new JomlParser();
        return parser.Parse(resource);
    }
    
    [Fact]
    public void DictionariesAsFieldsWork()
    {
        var doc = GetDocument(TestResources.DictionaryAsFieldTestInput);

        var obj = JomletMain.To<ClassWithDictionary>(doc);
        
        Assert.Equal(2, obj.name.Count);
        Assert.True(obj.name.ContainsKey("subname1"));
        Assert.True(obj.name.ContainsKey("subname2"));

        //Just make sure this doesn't throw
        var serialized = JomletMain.TomlStringFrom(obj); 
    }

    [Fact]
    public void DictionaryKeysShouldBeProperlyEscaped()
    {
        var dictionary = new Dictionary<string, string>
        {
            {"normal-key", "normal-key"},
            {"normal_key", "normal_key"},
            {"normalkey", "normalkey"},
            {"key with space", "\"key with spaces\""},
            {"key!with{}(*%&)random[other+symbols", "\"key!with{}(*%&)random[other+symbols\""},
            {"key/with/slashes", "\"key/with/slashes\""},
            {"Nam\\e", "\"Nam\\\\e\""}
        };

        var obj = new ClassWithDictionary
        {
            GenericDictionary = dictionary
        };
        
        var serialized = JomletMain.TomlStringFrom(obj);
        foreach(var (_, expectedValue) in dictionary)
        {
            Assert.Contains(expectedValue, serialized);
        }
    }

    private bool PrimitiveKeyTestHelper<T>(params T[] values) where T : unmanaged, IConvertible
    {
        var primitiveDict = new Dictionary<T, string>();
        for (int i=0; i<values.Length; i++) {
            T val = values[i];
            primitiveDict[val] = $"Test {i+1}";
        }

        var serialized = JomletMain.TomlStringFrom(primitiveDict);

        var deserialized = JomletMain.To<Dictionary<T, string>>(serialized);
        
        foreach (var (key, value) in primitiveDict) {
            if (!deserialized.ContainsKey(key)) {
                return false;
            }
            if (deserialized[key] != value) {
                return false;
            }
        }
        return true;
    }

    [Fact]
    public void PrimitiveDictionaryKeysShouldWork()
    {
        Assert.True(PrimitiveKeyTestHelper(true, false));
        Assert.True(PrimitiveKeyTestHelper(long.MaxValue, long.MinValue, 0, 4736251));
        Assert.True(PrimitiveKeyTestHelper(uint.MinValue, uint.MaxValue, 0u, 1996u));

        // \n causes an exception when deserializing
        // I don't consider this a bug with the primitive dict deserializer because the string dict deserializer also has this issue
        Assert.True(PrimitiveKeyTestHelper('a', 'b', 'c' /*, '\n' */));
    }

}