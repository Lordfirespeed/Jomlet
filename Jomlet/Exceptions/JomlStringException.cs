namespace Jomlet.Exceptions;

public class JomlStringException :JomlExceptionWithLine 
{
    public JomlStringException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an invalid JOML string on line {LineNumber}";
}