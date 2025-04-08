namespace Jomlet.Exceptions;

public class JomlUnescapedUnicodeControlCharException : JomlExceptionWithLine
{
    private readonly int _theChar;

    public JomlUnescapedUnicodeControlCharException(int lineNumber, int theChar) : base(lineNumber)
    {
        _theChar = theChar;
    }

    public override string Message => $"Found an unescaped unicode control character U+{_theChar:0000} on line {LineNumber}. Control character other than tab (U+0009) are not allowed in JOML unless they are escaped.";
}