using System;
using Tomlet.Exceptions;
using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class ExceptionTests
{
    private JomlDocument GetDocument(string resource) => new JomlParser().Parse(resource);
    
    private static void AssertThrows<T>(Action what) where T: Exception
    {
        Assert.Throws<T>(() =>
        {
            try
            {
                what();
            }
            catch (Exception e)
            {
                var _ = e.Message; //Call this for coverage idc
                throw;
            }
        });
    }

    private static void AssertThrows<T>(Func<object> what) where T: Exception
    {
        Assert.Throws<T>(() =>
        {
            try
            {
                what();
            }
            catch (Exception e)
            {
                var _ = e.Message; //Call this for coverage idc
                throw;
            }
        });
    }

    [Fact]
    public void InvalidInlineTablesThrow() => 
        AssertThrows<InvalidJomlInlineTableException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadInlineTableExample));

    [Fact]
    public void InvalidEscapesThrow() =>
        AssertThrows<InvalidJomlEscapeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadEscapeExample));
    
    [Fact]
    public void InvalidNumbersThrow() => 
        AssertThrows<InvalidJomlNumberException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadNumberExample));
    
    [Fact]
    public void InvalidDatesThrow() =>
        AssertThrows<InvalidJomlDateTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDateExample));
    
    [Fact]
    public void TruncatedFilesThrow() =>
        AssertThrows<JomlEndOfFileException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTruncatedFileExample));

    [Fact]
    public void UndefinedTableArraysThrow() => 
        AssertThrows<MissingIntermediateInJomlTableArraySpecException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTableArrayWithMissingIntermediateExample));
    
    [Fact]
    public void MissingKeysThrow() =>
        AssertThrows<NoJomlKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlMissingKeyExample));
    
    [Fact]
    public void TimesWithOffsetsButNoDateThrow() =>
        AssertThrows<TimeOffsetOnJomlDateOrTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlLocalTimeWithOffsetExample));
    
    [Fact]
    public void IncorrectlyFormattedArraysThrow() =>
        AssertThrows<JomlArraySyntaxException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadArrayExample));
    
    [Fact]
    public void DateTimesWithNoSeparatorThrow() =>
        AssertThrows<JomlDateTimeMissingSeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlDateTimeWithNoSeparatorExample));
    
    [Fact]
    public void DatesWithUnnecessarySeparatorThrow() =>
        AssertThrows<JomlDateTimeUnnecessarySeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnnecessaryDateTimeSeparatorExample));
    
    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyInADocumentWhenItIsNotThrows() =>
        AssertThrows<JomlDottedKeyParserException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDottedKeyExample));

    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyWhenItIsNotThrows()
    {
        var doc = GetDocument(TestResources.ArrayOfEmptyStringTestInput);
        AssertThrows<TomlDottedKeyException>(() => doc.Put("Array.a", "foo"));
    }
    
    [Fact]
    public void BadEnumValueThrows() =>
        AssertThrows<TomlEnumParseException>(() => JomletMain.To<TomlTestClassWithEnum>(DeliberatelyIncorrectTestResources.TomlBadEnumExample));

    [Fact]
    public void ReDefiningASubTableAsASubTableArrayThrowsAnException() => 
        AssertThrows<JomlKeyRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.ReDefiningSubTableAsSubTableArrayTestInput));

    [Fact]
    public void RedefiningAKeyAsATableNameThrowsAnException() => 
        AssertThrows<JomlKeyRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.KeyRedefinitionViaTableTestInput));
    
    [Fact]
    public void DefiningATableArrayWithTheSameNameAsATableThrowsAnException() => 
        AssertThrows<JomlTableArrayAlreadyExistsAsNonArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.DefiningAsArrayWhenAlreadyTableTestInput));

    [Fact]
    public void ReDefiningAnArrayAsATableArrayThrowsAnException() => 
        AssertThrows<JomlNonTableArrayUsedAsTableArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.ReDefiningAnArrayAsATableArrayIsAnErrorTestInput));
    
    [Fact]
    public void InlineTablesWithNewlinesThrowAnException() => 
        AssertThrows<NewLineInJomlInlineTableException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlInlineTableWithNewlineExample));
    
    [Fact]
    public void DoubleDottedKeysThrowAnException() => 
        AssertThrows<JomlDoubleDottedKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlDoubleDottedKeyExample));
    
    [Fact]
    public void MissingTheCommaInAnInlineTableThrows() => 
        AssertThrows<JomlInlineTableSeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlInlineTableWithMissingSeparatorExample));

    [Fact]
    public void ConvertingAPrimitiveToADocumentThrows() =>
        AssertThrows<TomlPrimitiveToDocumentException>(() => JomletMain.DocumentFrom("hello"));
    
    [Fact]
    public void BadTomlStringThrows() =>
        AssertThrows<JomlStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadStringExample));

    [Fact]
    public void TripleQuotedKeysThrow() => 
        AssertThrows<JomlTripleQuotedKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleQuotedKeyExample));
    
    [Fact]
    public void WhitespaceInKeyThrows() => 
        AssertThrows<JomlWhitespaceInKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlWhitespaceInKeyExample));
    
    [Fact]
    public void MissingEqualsSignThrows() => 
        AssertThrows<JomlMissingEqualsException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlMissingEqualsExample));
    
    [Fact]
    public void TripleSingleQuoteInStringThrows() => 
        AssertThrows<TripleQuoteInJomlMultilineLiteralException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleSingleQuoteInStringExample));
    
    [Fact]
    public void TripleDoubleQuoteInStringThrows() => 
        AssertThrows<TripleQuoteInJomlMultilineSimpleStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleDoubleQuoteInStringExample));
    
    [Fact]
    public void UnterminatedKeyThrows() => 
        AssertThrows<UnterminatedJomlKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedQuotedKeyExample));
    
    [Fact]
    public void UnterminatedStringThrows() =>
        AssertThrows<UnterminatedJomlStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedStringExample));
    
    [Fact]
    public void UnterminatedTableArrayThrows() => 
        AssertThrows<UnterminatedJomlTableArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedTableArrayExample));
    
    [Fact]
    public void UnterminatedTableNameThrows() => 
        AssertThrows<UnterminatedJomlTableNameException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedTableExample));
    
    [Fact]
    public void AttemptingToModifyInlineTablesThrowsAnException() => 
        AssertThrows<JomlTableLockedException>(() => GetDocument(DeliberatelyIncorrectTestResources.InlineTableLockedTestInput));
    
    [Fact]
    public void ReDefiningATableThrowsAnException() => 
        AssertThrows<JomlTableRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.TableRedefinitionTestInput));
    
    [Fact]
    public void UnicodeControlCharsThrowAnException() => 
        AssertThrows<JomlUnescapedUnicodeControlCharException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlNullBytesExample));

    //These are all runtime mistakes on otherwise-valid TOML documents, so they aren't in the DeliberatelyIncorrectTestResources file.
    
    [Fact]
    public void UnInstantiableObjectsThrow() => 
        AssertThrows<TomlInstantiationException>(() => JomletMain.To<IConvertible>(""));

    [Fact]
    public void MultipleParameterizedConstructorsThrow() =>
        AssertThrows<TomlInstantiationException>(() => JomletMain.To<ClassWithMultipleParameterizedConstructors>(""));
    
    [Fact]
    public void AbstractClassDeserializationThrows() =>
        AssertThrows<TomlInstantiationException>(() => JomletMain.To<AbstractClass>(""));
    
    [Fact]
    public void MismatchingTypesInPrimitiveMappingThrows() => 
        AssertThrows<TomlTypeMismatchException>(() => JomletMain.To<float>(GetDocument("MyFloat = \"hello\"").GetValue("MyFloat")));

    [Fact]
    public void GettingAValueWhichDoesntExistThrows() =>
        AssertThrows<TomlNoSuchValueException>(() => GetDocument("MyString = \"hello\"").GetValue("MyFloat"));
    
    [Fact]
    public void MismatchingTypesInDeserializationThrow() => 
        AssertThrows<TomlPropertyTypeMismatchException>(() => JomletMain.To<SimplePropertyTestClass>("MyFloat = \"hello\""));

    [Fact]
    public void AskingATableForTheValueAssociatedWithAnInvalidKeyThrows() =>
        AssertThrows<InvalidTomlKeyException>(() => GetDocument("").GetBoolean("\"I am invalid'"));
    
    [Fact]
    public void SettingAnInlineCommentToIncludeANewlineThrows() => 
        AssertThrows<TomlNewlineInInlineCommentException>(() => JomlDocument.CreateEmpty().Comments.InlineComment = "hello\nworld");

    [Fact]
    public void BadKeysThrow()
    {
        var doc = GetDocument("");
        
        //A key with both quotes
        AssertThrows<InvalidTomlKeyException>(() => doc.GetLong("\"hello'"));
    }
}