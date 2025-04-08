using System.Diagnostics.CodeAnalysis;
using Jomlet.Attributes;

namespace Jomlet.Tests.TestModelClasses;

[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ExampleMailboxConfigClass
{
    [JomlInlineComment("The name of the mailbox")]
    public string mailbox;
    [JomlInlineComment("Your username for the mailbox")]
    public string username;
    [JomlInlineComment("The password you use to access the mailbox")]
    public string password;

    [JomlPrecedingComment("The rules for the mailbox follow")]
    public Rule[] rules { get; set; }

    public class Rule
    {
        public string address;
        public string[] blocked;
        public string[] allowed;
    }
}