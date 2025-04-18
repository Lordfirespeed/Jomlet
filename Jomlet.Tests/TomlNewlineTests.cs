﻿using System.Collections.Generic;
using Xunit;

namespace Jomlet.Tests
{
    public class TomlNewlineTests
    {
        [Fact]
        public void TomlKeysCanHaveNewlines()
        {
            var key = "Hello\nWorld";
            var model = new Dictionary<string, string>
            {
                {key, "Test Value"}
            };

            var tomlDoc = JomletMain.TomlStringFrom(model);
            
            Assert.Equal("\"Hello\\nWorld\" = \"Test Value\"", tomlDoc.Trim());

            var backToDict = JomletMain.To<Dictionary<string, string>>(tomlDoc);

            Assert.Equal(model[key], backToDict[key]);
        }

        [Fact]
        public void TomlStringValuesCanHaveNewlines()
        {
            var key = "Test";
            var model = new Dictionary<string, string>
            {
                {key, "Test\nValue"}
            };

            var tomlDoc = JomletMain.TomlStringFrom(model);
            
            Assert.Equal("Test = \"Test\\nValue\"", tomlDoc.Trim());

            var backToDict = JomletMain.To<Dictionary<string, string>>(tomlDoc);

            Assert.Equal(model[key], backToDict[key]);
        }
    }
}