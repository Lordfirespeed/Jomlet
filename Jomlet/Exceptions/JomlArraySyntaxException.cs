namespace Jomlet.Exceptions;

public class JomlArraySyntaxException : JomlExceptionWithLine
{
    private readonly char _charFound;

    public JomlArraySyntaxException(int lineNumber, char charFound) : base(lineNumber)
    {
        _charFound = charFound;
    }

    public override string Message => $"Expecting ',' or ']' after value in array on line {LineNumber}, found '{_charFound}'";
}