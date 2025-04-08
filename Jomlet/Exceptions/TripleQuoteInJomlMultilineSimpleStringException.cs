namespace Tomlet.Exceptions;

public class TripleQuoteInJomlMultilineSimpleStringException : JomlExceptionWithLine
{
    public TripleQuoteInJomlMultilineSimpleStringException(int lineNumber) : base(lineNumber)
    {
    }
        
    public override string Message => $"Found a triple-double-quote (\"\"\") inside a multiline simple string on line {LineNumber}. This is not allowed.";
}