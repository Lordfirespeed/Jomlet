# Jomlet
## A JOML library for .NET

[![NuGet](https://img.shields.io/nuget/v/Lordfirespeed.Jomlet)](https://www.nuget.org/packages/Lordfirespeed.Jomlet/)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Lordfirespeed/Jomlet/dotnet.yml)
![Toml Version](https://img.shields.io/badge/TOML%20Version-1.0.0-blue)
[![Coverage Status](https://coveralls.io/repos/github/Lordfirespeed/Jomlet/badge.svg?branch=main)](https://coveralls.io/github/Lordfirespeed/Jomlet?branch=main)

Jomlet is a zero-dependency library for the [JOML](https://lordfirespeed.dev/joml) configuration file format.

The entire [TOML 1.0.0 specification](https://toml.io/en/v1.0.0) *was* implemented by [Sam](https://github.com/SamboyCoding/Tomlet), then Joe
screwed it up by adding support for `null` literals which are expressly forbidden by the TOML specification.

Jomlet does not preserve layout, ordering, or whitespace around entries in a document. When serialized, documents are ordered in such a way as to maximise human readability
and predictability. This means:
- Within a table (including the top-level document), simple key-value pairs (including inline arrays and inline tables) are first, followed by sub-tables, 
followed by table-arrays.
- Floating-point values are always serialized with a decimal part, even if that decimal part is zero. This is in the hope that any future parser therefore
correctly identifies the value as a decimal.
- Comments are sorted into "preceding" and "inline" and assigned to a specific value, or marked as trailing on the document. Preceding comments will be on the line(s)
immediately prior to the value, with no blank line separating them, and inline comments follow the value after a single space. Trailing comments are put at the end of the
document and are separated from the last key by one blank line.
- Tables are serialized inline if, and only if, they are not marked as forced no-inline (via an attribute or property, see below for details), they contain fewer than four
entries, all of their entries can also be serialized inline (nested inline tables are not permitted), and none of their entries contain comments.
- Arrays are serialized all on one line if they are made up purely of primitives with no comments, over multiple lines if they contain any inline tables, arrays, or comments
on individual entries, and as table-arrays if they contain only tables and one or more table cannot be serialized inline using the above rules. If an array contains mixed
primitive values and tables that cannot be serialized inline, an exception is thrown when serializing the array.

## A word on dotted keys

The TOML specification allows for dotted keys (e.g. `a.b.c = "d"`, which creates a table `a`, containing a table `b`, containing the key `c` with the string value `d`), 
as well as quoted dotted keys (e.g. `a."b.c" = "d"`, which creates a table `a`, containing the key `b.c`, with the string value `d`).

Jomlet correctly handles both of these cases, but there is room for ambiguity in calls to `JomlTable#GetValue` and its sibling methods.

For ease of internal use, `GetValue` and `ContainsKey` will interpret keys as-above, with key names containing dots requiring the key name to be quoted. So a call to `ContainsKey("a.b")` will look for a table `a` containing a key `b`.
Note that, if you mistakenly do this, and there is a value mapped to the key `a` which is NOT a table, a `JomlContainsDottedKeyNonTableException` will be thrown.

However, for a more convenient API, calls to specific typed variants of `GetValue` (`GetString`, `GetInteger`, `GetDouble`, etc.) will assume keys are supposed to be quoted. That is, a call to
`GetString("a.b")` will look for a single key `a.b`, not a table `a` containing key `b`.

## Usage

### Serialize a runtime object to JOML

```c#
var myClass = new MyClass("hello world", 1, 3);
JomlDocument jomlDoc = JomletMain.DocumentFrom(myClass); //JOML document representation. Can be serialized using the SerializedValue property.
string jomlString = JomletMain.JomlStringFrom(myClass); //Formatted JOML string. Equivalent to TomletMain.DocumentFrom(myClass).SerializedValue
```

### Deserialize JOML to a runtime object

```c#
string myString = GetJomlStringSomehow(); //Web request, read file, etc.
var myClass = JomletMain.To<MyClass>(myString); //Requires a public, zero-argument constructor on MyClass.
Console.WriteLine(myClass.configurationFileVersion); //Or whatever properties you define.
```

### Disable table inlining

By default, Jomlet tries to inline simple tables to reduce the document size. If you don't want this behavior,
either set `JomlTable#ForceNoInline` (if manually building a Joml doc), or use the 
`JomlDoNotInlineObjectAttribute` on a class to force all instances of that class to be serialized as a full table.

### Change what name Jomlet uses to de/serialize a field

Given that you had the above setup and were serializing a class using `JomletMain.JomlStringFrom(myClass)`, you could override JOML key names like so:

```c#
class MyClass {
    [JomlProperty("name")] //Tell Jomlet to use "name" as the key, instead of "Username", when serializing and deserializing this type.
    public string Username { get; set; }
    [JomlProperty("password")] //Tell Jomlet to use the lowercase "password" rather than "Password"
    public string Password { get; set; }
}
```

### Comments

Comments are parsed and stored alongside their corresponding values, where possible. Every instance of `JomlValue`
has a `Comments` property, which contains both the "inline" and "preceding" comments. Preceding comments are
the comments that appear before the value (and therefore can span multiple lines), and inline comments are
the comments that appear on the same line as the value (and thus must be a single line).

Any preceding comment which is not associated with a value (i.e. it is placed after the last value) will be
stored in the `TrailingComment` property of the JOML document itself, and will be re-serialized from there.

If you're using Jomlet's reflective serialization feature (i.e. `JomletMain.____From`), you can use the `JomlInlineComment` and `JomlPrecedingComment`
attributes on fields or properties to specify the respective comments.

### Parse a JOML File

`JomlParser.ParseFile` is a utility method to parse an on-disk joml file. This just uses File.ReadAllText, so I/O errors will be thrown up to your calling code.

```c#
JomlDocument document = JomlParser.ParseFile(@"C:\MyFile.joml");

//You can then convert this document to a runtime object, if you so desire.
var myClass = JomletMain.To<MyClass>(document);
```

### Parse Arbitrary JOML input
Useful for parsing e.g. the response of a web request.
```c#
JomlParser parser = new JomlParser();
JomlDocument document = parser.Parse(myJomlString);
```

### Creating your own mappers.

By default, serialization and deserialization are reflection-based. Both fields and properties are supported, and properties can be remapped (i.e. told not to
use their name, but an alternative key) by using the `JomlProperty` attribute. Any fields or properties marked with attributes `[JomlNonSerialized]` or `[NonSerialized]` are skipped over(ignored),
both when serializing and deserializing. Deserializing requires a parameterless constructor to instantiate the object. 

This approach should work for most model classes, but should something more complex be used, such as storing a colour as an integer/hex string, or if you have a more compact/proprietary
method of serializing your classes, then you can override this default using code such as this:

```c#
// Example: UnityEngine.Color stored as an integer in JOML. There is no differentiation between 32-bit and 64-bit integers, so we use JomlLong.
JomletMain.RegisterMapper<Color>(
        //Serializer (joml value from class). Return null if you don't want to serialize this value.
        color => new JomlLong(color.a << 24 | color.r << 16 | color.g << 8 | color.b),
        //Deserializler (class from joml value)
        jomlValue => {
            if(!(jomlValue is JomlLong jomlLong)) 
                throw new JomlTypeMismatchException(typeof(JomlLong), jomlValue.GetType(), typeof(Color))); //Expected type, actual type, context (type being deserialized)
            
            int a = jomlLong.Value >> 24 & 0xFF;
            int r = jomlLong.Value >> 16 & 0xFF;
            int g = jomlLong.Value >> 8 & 0xFF;
            int b = jomlLong.Value & 0xFF;
            
            return new Color(r, g, b, a); 
        }
);
```

Calls to `JomletMain.RegisterMapper` can specify either the serializer or deserializer as `null`, in which case the default handler (usually reflection-based, unless
you're overriding the behavior for primitive values, IEnumerables, or arrays) will be used.

### Manually retrieving data from a JomlDocument

```c#
JomlDocument document; // See above for how to obtain one.
int someInt = document.GetInteger("myInt");
string someString = document.GetString("myString");

// JomlArray implements IEnumerable<JomlValue>, so you can iterate over it or use LINQ.
foreach(var value in document.GetArray("myArray")) {
    Console.WriteLine(value.StringValue);
}

//It also has an index operator, so you can do this
Console.WriteLine(document.GetArray("myArray")[0]);

List<string> strings = document.GetArray("stringArray").Select(v => (JomlString) v).ToList();

//Retrieving sub-tables. JomlDocument is just a special JomlTable, so you can 
//call GetSubTable on the resulting JomlTable too.
string subTableString = document.GetSubTable("myTable").GetString("aString");

//Date-Time objects. There's no API for these (yet)
var dateValue = document.GetValue("myDateTime");
if(dateValue is JomlOffsetDateTime jomlODT) {
    DateTimeOffset date = jomlODT.Value;
    Console.WriteLine(date); //27/05/1979 00:32:00 -07:00
} else if(dateValue is JomlLocalDateTime jomlLDT) {
    DateTime date = jomlLDT.Value;
    Console.WriteLine(date.ToUniversalTime()); //27/05/1979 07:32:00
} else if(dateValue is JomlLocalDate jomlLD) {
    DateTime date = jomlLD.Value;
    Console.WriteLine(date.ToUniversalTime()); //27/05/1979 00:00:00
} else if(dateValue is JomlLocalTimejomlLT) {
    TimeSpan time = jomlLT.Value;
    Console.WriteLine(time); //07:32:00.9999990
} 
```

### Handling Errors

Both `ParseFile` and `Parse` can throw multiple exceptions (currently around 40), but they are all an instance of `JomlException`.

Most of these are specific to various errors as specified in the specification, but there is also `JomlInternalException` which is thrown if there is an exception in Jomlet itself. 
If it is thrown, it will have the unhandled exception as a cause, and the exception message will state the line number which caused the exception. I'd appreciate it
if you could report these to me.

The full list of exceptions follows, in alphabetical order:

| Exception Class                                    |                                                                                                                             Probable Cause                                                                                                                             |                                                                                                                                Resolution |
|:---------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|------------------------------------------------------------------------------------------------------------------------------------------:|
| `InvalidJomlDateTimeException`                     |                                                                     A generic date/time error is present in the file. The string which Jomlet tried to parse as a date is provided in the message.                                                                     |                                                                                          Fix the string to be a valid RFC 3339 date/time. |
| `InvalidJomlEscapeException`                       |                                                                         A string escape such as `\n` was found in a double-quoted string, but is reserved according to the JOML specification.                                                                         |                                         Remove the escape sequence, escape the backslash, or use \uXXXX to specify the unicode character. |
| `InvalidJomlInlineTableException`                  |                                                    A key-value pair was found in an inline table which either doesn't have a key, or is missing an equals sign (=). The exception cause will have more information                                                     |                                                                                                 Fix the syntax error in the inline table. |
| `InvalidJomlKeyException`                          |                                                                Never thrown by the parser. Only thrown during calls to `JomlTable.GetXXXX` when the key provided contains both single and double quotes                                                                |                                                                                         Pass a valid JOML key into the `GetXXXX` function |
| `InvalidJomlNumberException`                       |                                                                  A number (float or integer) literal was found in the JOML document, but isn't valid. The exception message contains the line number.                                                                  |                                                             Quote the value in the file and interpret it as a string, or remove the value |
| `MissingIntermediateInJomlTableArraySpecException` |                                                                  A Table-Array was found in the file which is a child element of an as-yet undefined value. The exception message details this value.                                                                  |                                                                   Fix the file to be in a valid order according to the JOML specification |
| `NewLineInJomlInlineTableException`                |                                                                 A new-line character (`\n`) was found between the key-value pairs of an inline table. The exception message provides the line number.                                                                  |                                                                                                             Remove the new-line character |
| `NoJomlKeyException`                               |                                                            A key-value pair was found in the JOML document which doesn't have a key before the equals sign. The exception message provides the line number.                                                            |                                                                              If you really want an empty-string as a key, quote it (`""`) |
| `TimeOffsetOnJomlDateOrTimeException`              |                                                                    A time offset (such as `-07:00`, or `Z`) was found on a Local Date or Local Time object in the JOML document. This is not valid.                                                                    |                                                                               Remove the time offset, or provide a full date-time string. |
| `JomlArraySyntaxException`                         |                                    The parser was expecting a comma (`,`) or closing bracket (`]`) but found something else, while parsing an inline array. The exception message provides the line number and offending character.                                    |                                                                                                                     Fix the array syntax. |
| `JomlContainsDottedKeyNonTableException`           |                               A call was made to `JomlTable.ContainsKey` or `JomlTable.GetXXXX` with a dotted key, which implied that a key should be a table, when it in fact wasn't. The exception message provides the offending key.                               |                                                                                                Fix the call to `ContainsKey` or `GetXXXX` |
| `JomlDateTimeMissingSeparatorException`            |                                      RFC 3339 date/time literals must contain a `T`, `t`, or space between the date and time strings. This literal did not have such a separator. The exception message provides the line number.                                      |                                                                                 Insert an appropriate separator between the date and time |
| `JomlDateTimeUnnecessarySeparatorException`        |                                                          A date/time separator (`T`, `t`, or a space) was found in a Local Date or Local Time string, where it has nothing to separate. This is not allowed.                                                           |                                                                                                                     Remove the separator. |
| `JomlDottedKeyException`                           |                     An attempt was made to programmatically insert a value into a `JomlTable` using a dotted key, which implied that an intermediary key was a table, when it is in fact not. The exception message provides the intermediary key.                     |                                                                                  Fix the code which inserted the value into the JomlTable |
| `JomlDottedKeyParserException`                     |                                                        Similar to the above, except the dotted key was in the JOML document being parsed. The exception message provides the line number and intermediary key.                                                         |                                                                          Check the JOML document to ensure you are using the correct key. |
| `JomlDoubleDottedKeyException`                     |                                                                    A key was encountered in the JOML file which contained two consecutive periods. The exception message provides the line number.                                                                     |                                                                                                     Correct the key in the JOML document. |
| `JomlEnumParseException`                           |                                                              An attempt was made to deserialize an enum type, and the value present in the JOML document could not be resolved to any entry in the enum.                                                               |                                                   Check that the enum contains all the possible values, and check the document for typos. |
| `JomlEndOfFileException`                           |                                                                                               The end of a file was reached while attempting to parse a key/value pair.                                                                                                |                                                                                       Restore the truncated data from the end of the file |
| `JomlFieldTypeMismatchException`                   |     While deserializing an object, the mapper found a field for which the type did not match the type of data in the document. The exception message provides the type and field being deserialized, the type of the field, and the type of data in the document.      |                                            Ensure the field declaration in your model class matches the type of the data in the document. | 
| `JomlInlineTableSeparatorException`                |                         The parser was expecting a comma (`,`) or closing brace (`}`) after a key-value pair, while parsing an inline table, but found something else. The exception message provides the line number and offending character.                         |                                                                                                 Fix the syntax error in the inline table. |
| `JomlInstantiationException`                       |                                           While deserializing an object, the mapper found a type which has no public parameterless constructor. The exception message provides the type for which a constructor is missing.                                            |                                                                           Ensure all model types have a public parameterless constructor. | 
| `JomlInternalException`                            |                                                                                                             Detailed in the paragraph preceding this table                                                                                                             |                                                                                                Report the issue on the GitHub repository. |
| `JomlInvalidValueException`                        |                      While parsing a key-value pair, the parser read the first character of a value, which does not appear to indicate the start of any valid value type. The exception message provides the line number and offending character.                      |                                                                        Correct the value. Is it supposed to be a (quoted) string literal? |
| `JomlKeyRedefinitionException`                     |                                                                                                The Parser found that a value is present twice within the JOML document.                                                                                                |                                                                                                          Remove the duplicate assignment. |
| `JomlMissingEqualsException`                       |                                                                         The Parser found a value before it found an equals sign, after a key. The exception message provides the line number.                                                                          |                                                  Insert the equals sign, or, if the key is supposed to contain whitespace, quote the key. | 
| `JomlMissingNewlineException`                      |       The Parser found the beginning of another key-value pair on the same line as one it has previously parsed. All key-value pairs must be on their own line. The exception message provides the line number and first character of the second key-value pair.       |                                                                                   Insert a newline character between the key-value pairs. |
| `JomlNonTableArrayUsedAsTableArrayException`       |                                            The Parser found a table-array declaration (`[[TableArrayName]]`) which is re-using the name of a previous array. The exception message provides the line number and array name.                                            |                                                                      Remove the conflicting array declaration, or rename the table-array. |
| `JomlNoSuchValueException`                         |                                                                                 Thrown when a call to `JomlTable.GetXXXX` is made with a key which wasn't present in the JOML document                                                                                 |                                                     Check the call to `GetXXXX`, or check if the key is present first using `ContainsKey` |
| `JomlPrimitiveToDocumentException`                 |                                      Thrown when a call to `JomletMain.DocumentFrom` is made with a primitive value. Primitive values cannot be made into documents (what do you put as the key name?), so this is not supported.                                      |                                          Use `JomletMain.ValueFrom` if you just want to convert a primitive to its equivalent JOML value. |
| `JomlPropertyTypeMismatchException`                | While deserializing an object, the mapper found a property for which the type did not match the type of data in the document. The exception message provides the type and property being deserialized, the type of the property, and the type of data in the document. |                                         Ensure the property declaration in your model class matches the type of the data in the document. |
| `JomlStringException`                              |                                 The parser found a string which starts with two of the same quote (single or double) and then has a non-quote, non-whitespace, non-comment character. The exception message provides the line number.                                  |                                                                                                               Correct the string literal. |
| `JomlTableArrayAlreadyExistsAsNonArrayException`   |                                                                 A Table-Array declaration was found which overwrites a non-array value. The exception message provides the line number and array name.                                                                 |                                                                                   Remove the conflicting value, or rename the table-array |
| `JomlTableLockedException`                         |                 The Parser found an attempt to insert or modify the value of a key contained within a table which was declared inline. Inline tables are immutable. The exception message provides the line number and key being updated or inserted.                  |                                                                                   Remove the assignment, or declare the table non-inline. |
| `JomlTableRedefinitionException`                   |                                                          The Parser found an attempt to re-declare an already declared table name. The exception message provides the line number and conflicting table name.                                                          |                                                                                               Remove or rename the duplicated definition. |
| `JomlTripleQuotedKeyException`                     |                                                                   The Parser found a triple-quoted (`"""`) key in the document. This is not allowed. The exception message provides the line number.                                                                   |                                                                                                      Fix the syntax of the JOML document. |
| `JomlTypeMismatchException`                        |                                                                   Thrown when a call is made to `GetXXXX` but the type of `XXXX` does not match the type of the literal read from the JOML document.                                                                   |                                                                                              Check the expected type in the JOML document |
| `JomlUnescapedUnicodeControlCharException`         |                               Thrown when the parser encounters an unescaped unicode control char (other than newline or tab) within a comment, key, or string. The exception message provides the line number and offending character.                                |                                                                                   Remove or escape (if possible) the offending character. |
| `JomlWhitespaceInKeyException`                     |                                              An unquoted key was found in the JOML document which appears to contain whitespace (e.g `my key = 17`). This is not allowed. The exception message provides the line number.                                              |                                                                                                   Quote the key or remove the whitespace. |
| `TripleQuoteInJomlMultilineLiteralException`       |                                                         A triple single quote (`'''`) was found within a JOML multiline literal (triple-single-quoted string). The exception message provides the line number.                                                         | Remove the triple-quote or use a double-quoted string with appropriate escape sequences. You cannot escape within a triple-quoted string. |
| `TripleQuoteInJomlMultilineSimpleStringException`  |                                                      A triple double quote (`"""`) was found within a JOML multiline simple string (triple-double-quoted string). The exception message provides the line number.                                                      |                                                                                                          Escape one of the double quotes. |
| `UnterminatedJomlKeyException`                     |                                                                           The Parser found an unterminated quoted key in the JOML document. The exception message provides the line number.                                                                            |                                                                                                                 Terminate the quoted key. |
| `UnterminatedJomlStringException`                  |                                                             The Parser finished parsing a string literal and didn't find the appropriate closing quote(s). The exception message provides the line number.                                                             |                                                                                        Ensure the string literal is terminated correctly. |
| `UnterminatedJomlTableArrayException`              |                                                        The Parser finished parsing a Table-Array declaration and didn't find the closing double-bracket (`]]`). The exception message provides the line number.                                                        |                                                                           Ensure the Table-Array declaration is appropriately terminated. |
| `UnterminatedJomlTableNameException`               |                                                               The Parser finished parsing a Table declaration and didn't find the closing bracket (`]`). The exception message provides the line number.                                                               |                                                                                 Ensure the table declaration is terminated appropriately. |
