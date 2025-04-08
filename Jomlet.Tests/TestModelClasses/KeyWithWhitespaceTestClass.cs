using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

public class KeyWithWhitespaceTestClass
{
    [JomlProperty("Key With Whitespace")]
    public string KeyWithWhitespace { get; set; }
}