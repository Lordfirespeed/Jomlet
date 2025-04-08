namespace Jomlet.Exceptions;

public class JomlTableRedefinitionException : JomlExceptionWithLine
{
    private readonly string _key;

    public JomlTableRedefinitionException(int lineNumber, string key) : base(lineNumber)
    {
        _key = key;
    }

    public override string Message => $"JOML document attempts to re-define table '{_key}' on line {LineNumber}";
}