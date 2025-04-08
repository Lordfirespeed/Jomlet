using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class JomlFieldAttribute : Attribute
{
    private readonly string _mapFrom;

    public JomlFieldAttribute(string mapFrom)
    {
        _mapFrom = mapFrom;
    }

    public string GetMappedString()
    {
        return _mapFrom;
    }
}