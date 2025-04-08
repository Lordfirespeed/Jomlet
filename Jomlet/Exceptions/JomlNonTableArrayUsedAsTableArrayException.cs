namespace Jomlet.Exceptions;

public class JomlNonTableArrayUsedAsTableArrayException : JomlExceptionWithLine
{
    private readonly string _arrayName;

    public JomlNonTableArrayUsedAsTableArrayException(int lineNumber, string arrayName) : base(lineNumber)
    {
        _arrayName = arrayName;
    }

    public override string Message => $"{_arrayName} is used as a table-array on line {LineNumber} when it has previously been defined as a static array. This is not allowed.";
}