using System.Collections.Generic;
using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

public class ClassWithDoNotInlineMembers
{
    [JomlDoNotInlineObject] public Dictionary<string, string> ShouldNotBeInlinedField = new();

    [JomlDoNotInlineObject] public Dictionary<string, string> ShouldNotBeInlinedProp { get; set; } = new();

    public Dictionary<string, string> ShouldBeInlined = new();
}