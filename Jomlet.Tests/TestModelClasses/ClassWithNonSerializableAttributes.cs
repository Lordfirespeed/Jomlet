using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses
{
    public class ClassWithNonSerializableAttributes
    {
        public string SerializedString { get; set; }

        public int SerializedInt { get; set; }

        [JomlNonSerialized]
        public string NonSerializedProperty { get; set; }

        private string _SerializedField = "Serialized Field";

        [JomlNonSerialized]
        private string _NonSerializedField = "Non-Serialized private field";

    }
}
