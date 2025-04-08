using System;

namespace Jomlet.Exceptions;

public class JomlEnumParseException : JomlException
{
    private string _valueName;
    private Type _enumType;

    public JomlEnumParseException(string valueName, Type enumType)
    {
        _valueName = valueName;
        _enumType = enumType;
    }

    public override string Message => $"Could not find enum value by name \"{_valueName}\" in enum class {_enumType} while deserializing.";
}