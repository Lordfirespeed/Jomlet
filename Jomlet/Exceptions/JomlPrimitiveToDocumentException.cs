using System;

namespace Jomlet.Exceptions;

public class JomlPrimitiveToDocumentException : JomlException
{
    private Type primitiveType;
        
    public JomlPrimitiveToDocumentException(Type primitiveType)
    {
        this.primitiveType = primitiveType;
    }

    public override string Message => $"Tried to create a TOML document from a primitive value of type {primitiveType.Name}. Documents can only be created from objects.";
}