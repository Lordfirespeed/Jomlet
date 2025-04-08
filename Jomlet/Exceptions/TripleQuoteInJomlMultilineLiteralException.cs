namespace Tomlet.Exceptions;

public class TripleQuoteInJomlMultilineLiteralException : JomlExceptionWithLine
{
    public TripleQuoteInJomlMultilineLiteralException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found a triple-single-quote (''') inside a multiline string literal on line {LineNumber}. This is not allowed.";
}