namespace Tomlet.Exceptions;

public class JomlDateTimeUnnecessarySeparatorException : JomlExceptionWithLine
{
    public JomlDateTimeUnnecessarySeparatorException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unnecessary date-time separator (T, t, or a space) in a date or time on line {LineNumber}";
}