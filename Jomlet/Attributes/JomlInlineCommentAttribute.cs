using System;

namespace Jomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JomlInlineCommentAttribute : Attribute
{
    internal string Comment { get; }

    public JomlInlineCommentAttribute(string comment)
    {
        Comment = comment;
    }
}