namespace Jomlet.Models;

public class JomlLong : JomlValue
{
    private long _value;

    public JomlLong(long value)
    {
        _value = value;
    }
        
    internal static JomlLong? Parse(string valueInToml)
    {
        var nullableDouble = JomlNumberUtils.GetLongValue(valueInToml);

        if (!nullableDouble.HasValue)
            return null;

        return new JomlLong(nullableDouble.Value);
    }

    public long Value => _value;
    public override string StringValue => Value.ToString();
        
    public override string SerializedValue => StringValue;
}