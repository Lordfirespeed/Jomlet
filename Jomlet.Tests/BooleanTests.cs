﻿using Jomlet.Models;
using Xunit;

namespace Jomlet.Tests
{
    public class BooleanTests
    {
        private JomlDocument GetDocument(string resource)
        {
            var parser = new JomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void BooleansWorkAsIntended()
        {
            var document = GetDocument(TestResources.BooleanTestInput);

            Assert.Equal(2, document.Entries.Count);

            Assert.Collection(document.Entries.Keys,
                key => Assert.Equal("bool1", key),
                key => Assert.Equal("bool2", key)
            );
            
            Assert.Collection(document.Entries.Values,
                value => Assert.True(Assert.IsType<JomlBoolean>(value).Value),
                value => Assert.False(Assert.IsType<JomlBoolean>(value).Value)
            );
        }
    }
}