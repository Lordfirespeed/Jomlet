namespace Tomlet.Exceptions;

public class JomlDateTimeMissingSeparatorException : JomlExceptionWithLine
{
    public JomlDateTimeMissingSeparatorException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found a date-time on line {LineNumber} which is missing a separator (T, t, or a space) between the date and time.";
}