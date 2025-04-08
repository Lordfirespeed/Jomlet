using System;
using System.Xml;

namespace Tomlet.Models;

public class JomlLocalDate : TomlValue, ITomlValueWithDateTime
{
    private readonly DateTime _value;

    public JomlLocalDate(DateTime value)
    {
        _value = value;
    }
        
    public DateTime Value => _value;
        
    public override string StringValue => XmlConvert.ToString(Value, XmlDateTimeSerializationMode.Unspecified); //XmlConvert specifies RFC 3339

    public static JomlLocalDate? Parse(string input)
    {
        if (!DateTime.TryParse(input, out var dt))
            return null;

        return new JomlLocalDate(dt);
    }
        
    public override string SerializedValue => StringValue;
}