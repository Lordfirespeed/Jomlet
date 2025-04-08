using System;
using System.Reflection;

namespace Jomlet.Exceptions;

public class JomlParameterTypeMismatchException : JomlTypeMismatchException
{
    private readonly Type _typeBeingInstantiated;
    private readonly ParameterInfo _paramBeingDeserialized;

    public JomlParameterTypeMismatchException(Type typeBeingInstantiated, ParameterInfo paramBeingDeserialized, JomlTypeMismatchException cause) : base(cause.ExpectedType, cause.ActualType, paramBeingDeserialized.ParameterType)
    {
        _typeBeingInstantiated = typeBeingInstantiated;
        _paramBeingDeserialized = paramBeingDeserialized;
    }

    public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found parameter {_paramBeingDeserialized.Name} expecting a type of {ExpectedTypeName}, but value in TOML was of type {ActualTypeName}";
}