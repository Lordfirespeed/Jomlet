using Jomlet.Tests.TestModelClasses;
using Xunit;

namespace Jomlet.Tests;

public class NullableTests
{
    [Fact]
    public void SerializingNullablesSkipsThemIfTheyDontHaveAValue()
    {
        var withValue = new ClassWithNullableValueType() {MyShort = 123};
        var withoutValue = new ClassWithNullableValueType() {MyShort = null};
        
        var withValueToml = JomletMain.TomlStringFrom(withValue).Trim();
        var withoutValueToml = JomletMain.TomlStringFrom(withoutValue).Trim();
        
        Assert.Equal("MyShort = 123", withValueToml);
        Assert.Equal("", withoutValueToml);
    }
    
    [Fact]
    public void DeserializingNullablesWorks()
    {
        var withValueToml = "MyShort = 123";
        var withoutValueToml = "";
        
        var withValue = JomletMain.To<ClassWithNullableValueType>(withValueToml);
        var withoutValue = JomletMain.To<ClassWithNullableValueType>(withoutValueToml);
        
        Assert.Equal((short) 123, withValue.MyShort);
        Assert.Null(withoutValue.MyShort);
    }
}