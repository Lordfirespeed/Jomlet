namespace Jomlet.Exceptions;

public class JomlStringException :JomlExceptionWithLine 
{
    public JomlStringException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an invalid TOML string on line {LineNumber}";
}