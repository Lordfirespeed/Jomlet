using System;

namespace Tomlet.Models;

public class JomlOffsetDateTime : TomlValue
{
    private readonly DateTimeOffset _value;

    public JomlOffsetDateTime(DateTimeOffset value)
    {
        _value = value;
    }
        
    public DateTimeOffset Value => _value;
        
    public override string StringValue => Value.ToString("O");

    public static JomlOffsetDateTime? Parse(string input)
    {
        if (!DateTimeOffset.TryParse(input, out var dt))
            return null;

        return new JomlOffsetDateTime(dt);
    }
        
    public override string SerializedValue => StringValue;
}