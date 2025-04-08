namespace Jomlet.Exceptions;

public class InvalidJomlKeyException : JomlException
{
    private readonly string _key;

    public InvalidJomlKeyException(string key)
    {
        _key = key;
    }

    public override string Message => $"The string |{_key}| (between the two bars) contains at least one of both a double quote and a single quote, so it cannot be used for a TOML key.";
}