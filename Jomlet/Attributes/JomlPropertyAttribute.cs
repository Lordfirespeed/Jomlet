using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class JomlPropertyAttribute : Attribute
{
    private readonly string _mapFrom;

    public JomlPropertyAttribute(string mapFrom)
    {
        _mapFrom = mapFrom;
    }

    public string GetMappedString()
    {
        return _mapFrom;
    }
}