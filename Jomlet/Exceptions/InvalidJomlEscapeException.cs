namespace Jomlet.Exceptions;

public class InvalidJomlEscapeException : JomlExceptionWithLine
{
    private readonly string _escapeSequence;

    public InvalidJomlEscapeException(int lineNumber, string escapeSequence) : base(lineNumber)
    {
        _escapeSequence = escapeSequence;
    }

    public override string Message => $"Found an invalid escape sequence '\\{_escapeSequence}' on line {LineNumber}";
}