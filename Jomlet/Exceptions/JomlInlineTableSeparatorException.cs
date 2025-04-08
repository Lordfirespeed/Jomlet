namespace Jomlet.Exceptions;

public class JomlInlineTableSeparatorException : JomlExceptionWithLine
{
    private readonly char _found;

    public JomlInlineTableSeparatorException(int lineNumber, char found) : base(lineNumber)
    {
        _found = found;
    }

    public override string Message => $"Expected '}}' or ',' after key-value pair in JOML inline table, found '{_found}'";
}