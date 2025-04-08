using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TomlFieldAttribute : Attribute
{
    private readonly string _mapFrom;

    public TomlFieldAttribute(string mapFrom)
    {
        _mapFrom = mapFrom;
    }

    public string GetMappedString()
    {
        return _mapFrom;
    }
}