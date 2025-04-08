namespace Jomlet.Exceptions;

public class UnterminatedJomlTableArrayException : JomlExceptionWithLine
{
    public UnterminatedJomlTableArrayException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated table-array (expecting two ]s to close it) on line {LineNumber}";
}