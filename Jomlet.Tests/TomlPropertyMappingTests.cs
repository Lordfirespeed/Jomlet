using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class TomlPropertyMappingTests
    {
        [Fact]
        public void DeserializationWorks()
        {
            var record = JomletMain.To<ComplexTestRecordWithAttributeMapping>(TestResources.ComplexTestRecordForAttributeMapping);
            Assert.Equal("Test", record.MyString);
            Assert.IsType<WidgetForThisComplexTestRecordWithAttributeMapping>(record.MyWidget);
            Assert.Equal(42, record.MyWidget.MyInt);
        }

        [Fact]
        public void SerializationWorks()
        {
            var testString = TestResources.ComplexTestRecordForAttributeMapping.Trim().Replace("\r\n", "\n");
            var record = JomletMain.To<ComplexTestRecordWithAttributeMapping>(testString);
            var serialized = JomletMain.TomlStringFrom(record).Trim();
            Assert.Equal(testString, serialized);
        }

        [Fact]
        public void UserDefinedTypesAsPropertiesWorks()
        {
            var testString = TestResources.UserDefinedTypePropertyTestInput.Trim().Replace("\r\n", "\n");
            var record = JomletMain.To<Derived>(testString);

            Assert.Equal("Whatever", record.Junk);
            Assert.Equal(42, record.A.IntA);
            Assert.Equal(33, record.B.IntB);
            Assert.Equal(45, record.C.IntC);
            Assert.Equal("Answer", record.A.StringA);
        }
    }
}