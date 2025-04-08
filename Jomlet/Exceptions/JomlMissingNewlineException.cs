namespace Jomlet.Exceptions;

public class JomlMissingNewlineException : JomlExceptionWithLine
{
    private readonly char _found;

    public JomlMissingNewlineException(int lineNumber, char found) : base(lineNumber)
    {
        _found = found;
    }

    public override string Message => $"Expecting a newline character at the end of a statement on line {LineNumber}, but found an unexpected '{_found}'";
}