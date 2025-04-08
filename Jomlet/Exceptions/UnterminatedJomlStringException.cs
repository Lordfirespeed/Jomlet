namespace Jomlet.Exceptions;

public class UnterminatedJomlStringException : JomlExceptionWithLine
{
    public UnterminatedJomlStringException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated JOML string on line {LineNumber}";
}