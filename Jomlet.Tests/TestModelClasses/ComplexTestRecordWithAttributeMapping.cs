using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses
{
    public record ComplexTestRecordWithAttributeMapping
    {
        [JomlProperty("string")]
        public string MyString { get; init; }
        public WidgetForThisComplexTestRecordWithAttributeMapping MyWidget { get; init; }
    }

    public record WidgetForThisComplexTestRecordWithAttributeMapping
    {
        [JomlProperty("my_int")]
        public int MyInt { get; init; }
    }
}
