using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class TomlDoNotInlineObjectAttribute : Attribute
{
    
}