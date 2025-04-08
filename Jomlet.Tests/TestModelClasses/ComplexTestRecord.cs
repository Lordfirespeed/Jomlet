﻿namespace Tomlet.Tests.TestModelClasses
{
    public record ComplexTestRecord
    {
        public string MyString { get; set; }
        public Widget MyWidget { get; set; }
    }

    public record Widget
    {
        public int MyInt { get; set; }
    }
}
