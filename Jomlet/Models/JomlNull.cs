namespace Jomlet.Models;

public class JomlNull : JomlValue
{
    public static JomlNull Instance = new();
    private const string _Value = "null";
    public string Value => _Value;
    public override string StringValue => Value;
    public override string SerializedValue => StringValue;
}