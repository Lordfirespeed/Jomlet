namespace Jomlet.Exceptions;

public class JomlWhitespaceInKeyException : JomlExceptionWithLine
{
    public JomlWhitespaceInKeyException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => "Found whitespace in an unquoted TOML key at line " + LineNumber;
}