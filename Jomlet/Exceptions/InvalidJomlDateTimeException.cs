namespace Jomlet.Exceptions;

public class InvalidJomlDateTimeException : JomlExceptionWithLine
{
    private readonly string _inputString;

    public InvalidJomlDateTimeException(int lineNumber, string inputString) : base(lineNumber)
    {
        _inputString = inputString;
    }

    public override string Message => $"Found an invalid TOML date/time string '{_inputString}' on line {LineNumber}";
}