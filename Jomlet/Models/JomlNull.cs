namespace Jomlet.Models;

public class JomlNull : JomlValue
{
    public static JomlNull Instance = new();
    private const string NullString = "null";
    public object? Value => null;
    public override string StringValue => NullString;
    public override string SerializedValue => StringValue;
}