namespace Tomlet.Exceptions;

public class UnterminatedJomlKeyException : JomlExceptionWithLine
{
    public UnterminatedJomlKeyException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated quoted key on line {LineNumber}";
}