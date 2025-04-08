using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlPrecedingCommentAttribute : Attribute
{
    internal string Comment { get; }

    public TomlPrecedingCommentAttribute(string comment)
    {
        Comment = comment;
    }
}