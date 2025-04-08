using System;
using System.Reflection;

namespace Jomlet.Exceptions;

public class JomlFieldTypeMismatchException : JomlTypeMismatchException
{
    private readonly Type _typeBeingInstantiated;
    private readonly FieldInfo _fieldBeingDeserialized;

    public JomlFieldTypeMismatchException(Type typeBeingInstantiated, FieldInfo fieldBeingDeserialized, JomlTypeMismatchException cause) : base(cause.ExpectedType, cause.ActualType, fieldBeingDeserialized.FieldType)
    {
        _typeBeingInstantiated = typeBeingInstantiated;
        _fieldBeingDeserialized = fieldBeingDeserialized;
    }

    public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found field {_fieldBeingDeserialized.Name} expecting a type of {ExpectedTypeName}, but value in JOML was of type {ActualTypeName}";
}