namespace Jomlet.Exceptions;

public class NewLineInJomlInlineTableException : JomlExceptionWithLine
{
    public NewLineInJomlInlineTableException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => "Found a new-line character within a JOML inline table. This is not allowed.";
}