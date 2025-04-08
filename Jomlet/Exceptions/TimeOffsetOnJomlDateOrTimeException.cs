namespace Tomlet.Exceptions;

public class TimeOffsetOnJomlDateOrTimeException : JomlExceptionWithLine
{
    private readonly string _tzString;

    public TimeOffsetOnJomlDateOrTimeException(int lineNumber, string tzString) : base(lineNumber)
    {
        _tzString = tzString;
    }

    public override string Message => $"Found a time offset string {_tzString} in a partial datetime on line {LineNumber}. This is not allowed - either specify both the date and the time, or remove the offset specifier.";
}