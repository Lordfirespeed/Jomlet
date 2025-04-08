namespace Jomlet.Exceptions;

public class JomlTableArrayAlreadyExistsAsNonArrayException : JomlExceptionWithLine
{
    private readonly string _arrayName;

    public JomlTableArrayAlreadyExistsAsNonArrayException(int lineNumber, string arrayName) : base(lineNumber)
    {
        _arrayName = arrayName;
    }

    public override string Message => $"{_arrayName} is defined as a table-array (double-bracketed section) on line {LineNumber} but it has previously been used as a non-array type.";
}