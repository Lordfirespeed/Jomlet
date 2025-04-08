namespace Tomlet.Exceptions;

public class UnterminatedJomlStringException : JomlExceptionWithLine
{
    public UnterminatedJomlStringException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated TOML string on line {LineNumber}";
}