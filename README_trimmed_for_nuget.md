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