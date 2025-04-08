namespace Jomlet.Exceptions;

public class JomlInvalidValueException : JomlExceptionWithLine
{
    private readonly char _found;

    public JomlInvalidValueException(int lineNumber, char found) : base(lineNumber)
    {
        _found = found;
    }

    public override string Message => $"Expected the start of a number, string literal, boolean, array, or table on line {LineNumber}, found '{_found}'";
}