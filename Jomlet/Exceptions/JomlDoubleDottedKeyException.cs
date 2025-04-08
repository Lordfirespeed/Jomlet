namespace Tomlet.Exceptions;

public class JomlDoubleDottedKeyException : JomlExceptionWithLine
{
    public JomlDoubleDottedKeyException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => "Found two consecutive dots, or a leading dot, in a key on line " + LineNumber;
}