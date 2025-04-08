using System;
using Jomlet.Models;

namespace Jomlet.Exceptions;

public class JomlTypeMismatchException : JomlException
{
    protected readonly string ExpectedTypeName;
    protected readonly string ActualTypeName;
    protected internal readonly Type ExpectedType;
    protected internal readonly Type ActualType;
    private readonly Type _context;

    public JomlTypeMismatchException(Type expected, Type actual, Type context)
    {
        ExpectedTypeName = typeof(JomlValue).IsAssignableFrom(expected) ? expected.Name.Replace("Joml", "") : expected.Name;
        ActualTypeName = typeof(JomlValue).IsAssignableFrom(actual) ? actual.Name.Replace("Joml", "") : actual.Name;
        ExpectedType = expected;
        ActualType = actual;
        _context = context;
    }

    public override string Message => $"While trying to convert to type {_context}, a JOML value of type {ExpectedTypeName} was required but a value of type {ActualTypeName} was found";
}