using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JomlPrecedingCommentAttribute : Attribute
{
    internal string Comment { get; }

    public JomlPrecedingCommentAttribute(string comment)
    {
        Comment = comment;
    }
}