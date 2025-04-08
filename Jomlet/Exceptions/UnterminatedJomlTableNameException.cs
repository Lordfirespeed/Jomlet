namespace Jomlet.Exceptions;

public class UnterminatedJomlTableNameException : JomlExceptionWithLine
{
    public UnterminatedJomlTableNameException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated table name on line {LineNumber}";
}