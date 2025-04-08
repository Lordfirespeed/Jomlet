namespace Jomlet.Exceptions;

public class InvalidJomlNumberException : JomlExceptionWithLine
{
    private readonly string _input;

    public InvalidJomlNumberException(int lineNumber, string input) : base(lineNumber)
    {
        _input = input;
    }

    public override string Message => $"While reading input line {LineNumber}, found an invalid number literal '{_input}'";
}