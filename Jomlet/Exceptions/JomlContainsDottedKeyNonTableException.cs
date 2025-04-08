namespace Tomlet.Exceptions;

public class JomlContainsDottedKeyNonTableException : JomlException
{
    internal readonly string Key;

    public JomlContainsDottedKeyNonTableException(string key)
    {
        Key = key;
    }

    public override string Message => $"A call was made on a TOML table which attempted to access a sub-key of {Key}, but the value it refers to is not a table";
}