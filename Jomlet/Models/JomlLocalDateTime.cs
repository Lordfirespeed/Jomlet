
using System;
using System.Xml;

namespace Tomlet.Models;

public class JomlLocalDateTime : JomlValue, IJomlValueWithDateTime
{
    private readonly DateTime _value;

    public JomlLocalDateTime(DateTime value)
    {
        _value = value;
    }
        
    public DateTime Value => _value;
        
    public override string StringValue => XmlConvert.ToString(Value, XmlDateTimeSerializationMode.Unspecified); //XmlConvert specifies RFC 3339

    public static JomlLocalDateTime? Parse(string input)
    {
        if (!DateTime.TryParse(input, out var dt))
            return null;

        return new JomlLocalDateTime(dt);
    }
        
    public override string SerializedValue => StringValue;
}