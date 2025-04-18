﻿namespace Jomlet.Exceptions;

public class JomlInstantiationException : JomlException
{
    public override string Message =>
        "Deserialization of types without a parameterless constructor or a singular parameterized constructor is not supported.";
}