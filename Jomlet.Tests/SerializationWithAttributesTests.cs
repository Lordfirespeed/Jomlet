using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jomlet.Attributes;
using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests;

public class SerializationWithAttributesTests
{

    [Fact]
    public void NonSerializableTomlAttributeDoesNotSerialize()
    {
        var testClass = new ClassWithNonSerializableAttributes()
        {
            NonSerializedProperty = "Don't Serialize Me",
            SerializedInt= 1,
            SerializedString = "Serialized Me"
        };

        var tomlString = JomletMain.TomlStringFrom(testClass);

        //_testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

        var doc = new JomlParser().Parse(tomlString);

        Assert.False(doc.ContainsKey("NonSerializedProperty"));
        Assert.False(doc.ContainsKey("_NonSerializedField"));
        Assert.True(doc.ContainsKey("SerializedInt"));
        Assert.True(doc.ContainsKey("SerializedString"));
        Assert.True(doc.ContainsKey("_SerializedField"));

        //Deserialize and check again        
        var deserializedAgain = JomletMain.To<ClassWithNonSerializableAttributes>(tomlString);

        Assert.Equal<ClassWithNonSerializableAttributes>(testClass, deserializedAgain, new NonSerializedClassComparer());
    }

    private class NonSerializedClassComparer : IEqualityComparer<ClassWithNonSerializableAttributes> 
    {
        public bool Equals(ClassWithNonSerializableAttributes x, ClassWithNonSerializableAttributes y)
        {
            var a = GetProperties(x);
            var b = GetProperties(y);
            bool equal = false;
            if (a.Count == b.Count) // Require equal count.
            {
                equal = true;
                foreach (var pair in b)
                {
                    string value;
                    if (a.TryGetValue(pair.Key, out value))
                    {
                        // Require value be equal.
                        if (value != pair.Value)
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        // Require key be present.
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }

        private Dictionary<string, string> GetProperties(ClassWithNonSerializableAttributes instance)
        {
            var type = instance.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldsD = fields.Where(f => !(f.IsNotSerialized || f.GetCustomAttribute<JomlNonSerializedAttribute>() != null)
                && f.GetCustomAttribute<CompilerGeneratedAttribute>() == null
                && !f.Name.Contains('<')).ToDictionary(f=>f.Name, f=>f.GetValue(instance).ToString());

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<JomlNonSerializedAttribute>() == null)
                .ToDictionary(p => p.Name, p => p.GetValue(instance).ToString());

            return props.Concat(fieldsD)
               .GroupBy(kv => kv.Key)
               .ToDictionary(g => g.Key, g => g.First().Value);
        }


        public int GetHashCode([DisallowNull] ClassWithNonSerializableAttributes obj)
        {
            return obj.GetHashCode();
        }
    }

}