using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JomlNonSerializedAttribute : Attribute
{
}