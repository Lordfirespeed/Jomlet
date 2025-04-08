namespace Jomlet.Tests.TestModelClasses;

public class ClassWithParameterlessConstructor : ClassWithMultipleParameterizedConstructors
{
    public ClassWithParameterlessConstructor() : base(string.Empty, int.MinValue)
    {
        
    }
}