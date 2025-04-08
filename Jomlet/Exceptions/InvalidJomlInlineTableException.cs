namespace Jomlet.Exceptions;

public class InvalidJomlInlineTableException : JomlExceptionWithLine
{
    public InvalidJomlInlineTableException(int lineNumber, JomlException cause) : base(lineNumber, cause)
    {
    }

    public override string Message => $"Found an invalid inline JOML table on line {LineNumber}. See further down for cause.";
}