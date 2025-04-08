namespace Tomlet.Exceptions;

public class JomlKeyRedefinitionException : JomlExceptionWithLine
{
    private readonly string _key;

    public JomlKeyRedefinitionException(int lineNumber, string key) : base(lineNumber)
    {
        _key = key;
    }

    public override string Message => $"TOML document attempts to re-define key '{_key}' on line {LineNumber}";
}