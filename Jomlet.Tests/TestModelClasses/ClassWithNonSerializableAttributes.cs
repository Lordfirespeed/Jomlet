using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses
{
    public class ClassWithNonSerializableAttributes
    {
        public string SerializedString { get; set; }

        public int SerializedInt { get; set; }

        [TomlNonSerialized]
        public string NonSerializedProperty { get; set; }

        private string _SerializedField = "Serialized Field";

        [TomlNonSerialized]
        private string _NonSerializedField = "Non-Serialized private field";

    }
}
