using System;

namespace Tomlet.Exceptions;

public abstract class JomlExceptionWithLine : JomlException
{
    protected int LineNumber;

    protected JomlExceptionWithLine(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    protected JomlExceptionWithLine(int lineNumber, Exception cause) : base(cause)
    {
        LineNumber = lineNumber;
    }
}