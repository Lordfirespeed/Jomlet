namespace Jomlet.Exceptions;

public class JomlNoSuchValueException : JomlException
{
    private readonly string _key;

    public JomlNoSuchValueException(string key)
    {
        _key = key;
    }

    public override string Message => $"Attempted to get the value for key {_key} but no value is associated with that key";
}