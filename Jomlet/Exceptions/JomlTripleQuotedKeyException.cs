namespace Jomlet.Exceptions;

public class JomlTripleQuotedKeyException : JomlExceptionWithLine
{
    public JomlTripleQuotedKeyException(int lineNumber) : base(lineNumber)
    {
    }
        
    public override string Message => $"Found a triple-quoted key on line {LineNumber}. This is not allowed.";
}