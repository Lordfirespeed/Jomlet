namespace Tomlet.Exceptions;

public class JomlDottedKeyException : JomlException
{
    private readonly string _key;

    public JomlDottedKeyException(string key)
    {
        _key = key;
    }

    public override string Message => $"Tried to redefine key {_key} as a table (by way of a dotted key) when it's already defined as not being a table.";
}