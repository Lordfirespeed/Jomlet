using System;

namespace Jomlet.Models;

public class JomlLocalTime : JomlValue
{
    private readonly TimeSpan _value;

    public JomlLocalTime(TimeSpan value)
    {
        _value = value;
    }
        
    public TimeSpan Value => _value;
        
    public override string StringValue => Value.ToString();

    public static JomlLocalTime? Parse(string input)
    {
        if (!TimeSpan.TryParse(input, out var dt))
            return null;

        return new JomlLocalTime(dt);
    }
        
    public override string SerializedValue => StringValue;
}