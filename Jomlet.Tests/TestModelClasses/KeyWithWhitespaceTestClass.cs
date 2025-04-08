using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

public class KeyWithWhitespaceTestClass
{
    [TomlProperty("Key With Whitespace")]
    public string KeyWithWhitespace { get; set; }
}