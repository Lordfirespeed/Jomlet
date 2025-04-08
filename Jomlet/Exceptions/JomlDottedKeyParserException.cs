namespace Jomlet.Exceptions;

public class JomlDottedKeyParserException : JomlExceptionWithLine
{
    private readonly string _key;

    public JomlDottedKeyParserException(int lineNumber, string key) : base(lineNumber)
    {
        _key = key;
    }

    public override string Message => $"Tried to redefine key {_key} as a table (by way of a dotted key on line {LineNumber}) when it's already defined as not being a table.";
}