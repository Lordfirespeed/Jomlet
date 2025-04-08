using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

public class TableA
{
    [JomlProperty("IntA")]
    public int IntA { get; set; }
    [JomlProperty("StringA")]
    public string StringA { get; set; }
}

public class TableB
{
    [JomlProperty("IntB")]
    public int IntB { get; set; }
    
    [JomlProperty("StringB")]
    public string StringB { get; set; }
}

public abstract class Base
{
    [JomlProperty("A")]
    public TableA A { get; set; }
    
    [JomlProperty("B")]
    public TableB B { get; set; }
    public string Junk;
}

public class TableC
{
    [JomlProperty("IntC")]
    public int IntC { get; set; }
    [JomlProperty("StringC")]
    public string StringC { get; set; }
}

public class Derived : Base
{
    [JomlProperty("C")]
    public TableC C { get; set; }
}