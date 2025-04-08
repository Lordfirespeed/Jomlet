using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

[JomlDoNotInlineObject]
public class TomlClassWithNestedArray
{
    public ClassWithArray Root;
    
    [JomlDoNotInlineObject]
    public class ClassWithArray
    {
        public string SomeValue;

        public ArrayItem[] Array;
        
        [JomlDoNotInlineObject]
        public class ArrayItem
        {
            public string A;
            public string B;
        }
    }
}