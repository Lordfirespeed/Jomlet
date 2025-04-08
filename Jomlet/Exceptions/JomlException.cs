using System;

namespace Jomlet.Exceptions;

public abstract class JomlException : Exception
{
    protected JomlException()
    {
    }

    protected JomlException(Exception cause) : base("", cause)
    {
    }
}