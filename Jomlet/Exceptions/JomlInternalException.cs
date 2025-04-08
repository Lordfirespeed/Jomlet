using System;

namespace Jomlet.Exceptions;

public class JomlInternalException : JomlExceptionWithLine
{
    public JomlInternalException(int lineNumber, Exception cause) : base(lineNumber, cause)
    {
    }

    public override string Message => $"An internal exception occured while parsing line {LineNumber} of the JOML document";
}