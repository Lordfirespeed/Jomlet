namespace Tomlet.Models;

public class JomlBoolean : TomlValue
{
    public static JomlBoolean True => new(true);
    public static JomlBoolean False => new(false);
        
    private bool _value;
    private JomlBoolean(bool value)
    {
        _value = value;
    }

    public static JomlBoolean ValueOf(bool b) => b ? True : False;

    public bool Value => _value;

    public override string StringValue => Value ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant();
        
    public override string SerializedValue => StringValue;
}