namespace Tomlet.Exceptions;

public class JomlEndOfFileException : JomlExceptionWithLine
{
    public JomlEndOfFileException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found unexpected EOF on line {LineNumber} when parsing TOML file";
}