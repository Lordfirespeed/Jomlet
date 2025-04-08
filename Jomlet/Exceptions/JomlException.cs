using System;

namespace Tomlet.Exceptions;

public abstract class JomlException : Exception
{
    protected JomlException()
    {
    }

    protected JomlException(Exception cause) : base("", cause)
    {
    }
}