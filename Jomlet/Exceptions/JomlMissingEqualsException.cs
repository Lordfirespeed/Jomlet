namespace Jomlet.Exceptions;

public class JomlMissingEqualsException : JomlExceptionWithLine
{
    private readonly char _found;
    public JomlMissingEqualsException(int lineNumber, char found) : base(lineNumber)
    {
        _found = found;
    }
        
    public override string Message => $"Expecting an equals sign ('=') on line {LineNumber}, but found '{_found}'";
}