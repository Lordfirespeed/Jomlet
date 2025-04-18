using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Jomlet.Exceptions;
using Jomlet.Extensions;

namespace Jomlet.Models;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class JomlTable : JomlValue, IEnumerable<KeyValuePair<string, JomlValue>>
{
    public readonly Dictionary<string, JomlValue> Entries = new();

    internal bool Locked;
    internal bool Defined;

    public bool ForceNoInline { get; set; }

    public override string StringValue => $"Table ({Entries.Count} entries)";

    public HashSet<string> Keys => new(Entries.Keys);

    public bool ShouldBeSerializedInline => !ForceNoInline && Entries.Count < 4
                                                           && Entries.All(e => !e.Key.Contains(" ")
                                                                               && e.Value.Comments.ThereAreNoComments
                                                                               && (e.Value is JomlArray arr ? arr.IsSimpleArray : e.Value is not JomlTable));

    public override string SerializedValue
    {
        get
        {
            if (!ShouldBeSerializedInline)
                throw new Exception("Cannot use SerializeValue to serialize non-inline tables. Use SerializeNonInlineTable(keyName).");

            var builder = new StringBuilder("{ ");

            builder.Append(string.Join(", ", Entries.Select(o => EscapeKeyIfNeeded(o.Key) + " = " + o.Value.SerializedValue).ToArray()));

            builder.Append(" }");

            return builder.ToString();
        }
    }

    public string SerializeNonInlineTable(string? keyName, bool includeHeader = true)
    {
        var builder = new StringBuilder();
        if (includeHeader)
        {
            builder.Append('[').Append(keyName).Append("]");

            //For non-inline tables, the inline comment goes on the header line.
            if (Comments.InlineComment != null)
                builder.Append(" # ").Append(Comments.InlineComment);

            builder.Append('\n');
        }

        //Three passes: Simple key-value pairs including inline arrays and tables, sub-tables, then sub-table-arrays.
        foreach (var (subKey, value) in Entries)
        {
            if (value is JomlTable { ShouldBeSerializedInline: false } or JomlArray { CanBeSerializedInline: false })
                continue;

            WriteValueToStringBuilder(keyName, subKey, builder);
        }

        foreach (var (subKey, value) in Entries)
        {
            if (value is not JomlTable { ShouldBeSerializedInline: false })
                continue;

            WriteValueToStringBuilder(keyName, subKey, builder);
        }

        foreach (var (subKey, value) in Entries)
        {
            if (value is not JomlArray { CanBeSerializedInline: false })
                continue;

            WriteValueToStringBuilder(keyName, subKey, builder);
        }

        return builder.ToString();
    }

    private void WriteValueToStringBuilder(string? keyName, string subKey, StringBuilder builder)
    {
        var value = GetValue(subKey);

        subKey = EscapeKeyIfNeeded(subKey);

        if (keyName != null)
            keyName = EscapeKeyIfNeeded(keyName);

        var fullSubKey = keyName == null ? subKey : $"{keyName}.{subKey}";

        var hadBlankLine = builder.Length < 2 || builder[builder.Length - 2] == '\n';

        //Handle any preceding comment - this will ALWAYS go before any sort of value
        if (value.Comments.PrecedingComment != null)
            builder.Append(value.Comments.FormatPrecedingComment())
                .Append('\n');

        switch (value)
        {
            case JomlArray { CanBeSerializedInline: false } subArray:
                if (!hadBlankLine)
                    builder.Append('\n');

                builder.Append(subArray.SerializeTableArray(fullSubKey)); //No need to append newline as SerializeTableArray always ensure it ends with 2
                return; //Return because we don't do newline or handle inline comment here.
            case JomlArray subArray:
                builder.Append(subKey).Append(" = ").Append(subArray.SerializedValue);
                break;
            case JomlTable { ShouldBeSerializedInline: true } subTable:
                builder.Append(subKey).Append(" = ").Append(subTable.SerializedValue);
                break;
            case JomlTable subTable:
                builder.Append(subTable.SerializeNonInlineTable(fullSubKey)).Append('\n');
                return; //Return because we don't handle inline comment here.
            default:
                builder.Append(subKey).Append(" = ").Append(value.SerializedValue);
                break;
        }

        //If we're here we did something resembling an inline value, even if that value is actually a multi-line array.

        //First off, handle the inline comment.
        if (value.Comments.InlineComment != null)
            builder.Append(" # ").Append(value.Comments.InlineComment);

        //Then append a newline
        builder.Append('\n');
    }

    private static string EscapeKeyIfNeeded(string key)
    {
        if (key.StartsWith("\"") && key.EndsWith("\"") && key.Count(c => c == '"') == 2)
            //Already double quoted
            return key;

        if (key.StartsWith("'") && key.EndsWith("'") && key.Count(c => c == '\'') == 2)
            //Already single quoted
            return key;

        if (IsValidKey(key))
            return key;
                    
        key = JomlUtils.EscapeStringValue(key);
        return JomlUtils.AddCorrectQuotes(key);
    }

    private static bool IsValidKey(string key)
    {
        foreach (var c in key)
        {
            //TODO Future: This check for period is perhaps not super valid but it was way more broken without it so I'm leaving it in for now.
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
            {
                return false;
            }
        }

        return true;
    }

    internal void ParserPutValue(string key, JomlValue value, int lineNumber)
    {
        if (Locked)
            throw new JomlTableLockedException(lineNumber, key);

        InternalPutValue(key, value, lineNumber, true);
    }

    public void PutValue(string key, JomlValue value, bool quote = false)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (quote)
            key = JomlUtils.AddCorrectQuotes(key);
        InternalPutValue(key, value, null, false);
    }

#if MODERN_DOTNET
    public void Put<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(string key, T t, bool quote = false)
#else
    public void Put<T>(string key, T t, bool quote = false)
#endif
    {
        JomlValue? tomlValue;
        tomlValue = t is not JomlValue tv ? JomletMain.ValueFrom(t) : tv;

        if (tomlValue == null)
            throw new ArgumentException("Value to insert into TOML table serialized to null.", nameof(t));
            
        PutValue(key, tomlValue, quote);
    }

    public string DeQuoteKey(string key)
    {
        var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
        return !wholeKeyIsQuoted ? key : key.Substring(1, key.Length - 2);
    }

    private void InternalPutValue(string key, JomlValue value, int? lineNumber, bool callParserForm)
    {
        key = key.Trim();
        JomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

        if (!string.IsNullOrEmpty(restOfKey))
        {
            if (!Entries.TryGetValue(DeQuoteKey(ourKeyName), out var existingValue))
            {
                //We don't have a sub-table with this name defined. That's fine, make one.
                var subtable = new JomlTable();
                if (callParserForm)
                    ParserPutValue(ourKeyName, subtable, lineNumber!.Value);
                else
                    PutValue(ourKeyName, subtable);

                //And tell it to handle the rest of the key.
                if (callParserForm)
                    subtable.ParserPutValue(restOfKey, value, lineNumber!.Value);
                else
                    subtable.PutValue(restOfKey, value);
                return;
            }

            //We have a key by this name already. Is it a table?
            if (existingValue is not JomlTable existingTable)
            {
                //No - throw an exception
                if (lineNumber.HasValue)
                    throw new JomlDottedKeyParserException(lineNumber.Value, ourKeyName);

                throw new JomlDottedKeyException(ourKeyName);
            }

            //Yes, get the sub-table to handle the rest of the key
            if (callParserForm)
                existingTable.ParserPutValue(restOfKey, value, lineNumber!.Value);
            else
                existingTable.PutValue(restOfKey, value);
            return;
        }

        //Non-dotted keys land here.
        key = DeQuoteKey(key);

        if (Entries.ContainsKey(key) && lineNumber.HasValue)
            throw new JomlKeyRedefinitionException(lineNumber.Value, key);

        Entries[key] = value;
    }

    public bool ContainsKey(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        JomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

        if (string.IsNullOrEmpty(restOfKey))
            //Non-dotted key
            return Entries.ContainsKey(DeQuoteKey(key));

        if (!Entries.TryGetValue(ourKeyName, out var existingKey))
            return false;

        if (existingKey is JomlTable table)
            return table.ContainsKey(restOfKey);

        throw new JomlContainsDottedKeyNonTableException(key);
    }

#if MODERN_DOTNET
        public bool TryGetValue(string key, [NotNullWhen(true)] out JomlValue? value)
#else
    public bool TryGetValue(string key, out JomlValue value)
#endif
    {
        if (ContainsKey(key))
            return (value = GetValue(key)) != null;

#if MODERN_DOTNET
            value = null;
#else
        value = null!; //Not null asserting because this is the failure case, callers are expected to check the return value, and this is only on old frameworks that don't support NotNullWhen
#endif
        return false;
    }

    /// <summary>
    /// Returns the raw instance of <see cref="JomlValue"/> associated with this key. You must cast to a sub-class and access its value
    /// yourself.
    /// Unlike all the specific getters, this Getter respects dotted keys and quotes. You must quote any keys which contain a dot if you want to access the key itself,
    /// not a sub-key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>An instance of <see cref="JomlValue"/> associated with this key.</returns>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public JomlValue GetValue(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        if (!ContainsKey(key))
            throw new JomlNoSuchValueException(key);

        JomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

        if (string.IsNullOrEmpty(restOfKey))
            //Non-dotted key
            return Entries[DeQuoteKey(key)];

        if (!Entries.TryGetValue(ourKeyName, out var existingKey))
            throw new JomlNoSuchValueException(key); //Should already be handled by ContainsKey test

        if (existingKey is JomlTable table)
            return table.GetValue(restOfKey);

        throw new Exception("Jomlet Internal bug - existing key is not a table in JomlTable GetValue, but we didn't throw in ContainsKey?");
    }

    /// <summary>
    /// Returns the string value associated with the provided key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The string value associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not a string.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public string GetString(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlString str)
            throw new JomlTypeMismatchException(typeof(JomlString), value.GetType(), typeof(string));

        return str.Value;
    }

    /// <summary>
    /// Returns the integer value associated with the provided key, downsized from a long.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The integer value associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not an integer type.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public int GetInteger(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlLong lng)
            throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(int));

        return (int)lng.Value;
    }

    /// <summary>
    /// Returns the long (64-bit int) value associated with the provided key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The long/64-bit integer value associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not an integer type.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public long GetLong(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlLong lng)
            throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(int));

        return lng.Value;
    }

    /// <summary>
    /// Returns the 32-bit floating-point value associated with the provided key, downsized from a double.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The float value associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not a floating-point type.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public float GetFloat(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlDouble dbl)
            throw new JomlTypeMismatchException(typeof(JomlDouble), value.GetType(), typeof(float));

        return (float)dbl.Value;
    }

    /// <summary>
    /// Returns the boolean value associated with the provided key
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The boolean value associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not a boolean.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public bool GetBoolean(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlBoolean b)
            throw new JomlTypeMismatchException(typeof(JomlBoolean), value.GetType(), typeof(bool));

        return b.Value;
    }

    /// <summary>
    /// Returns the TOML array associated with the provided key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The TOML array associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not an array.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public JomlArray GetArray(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlArray arr)
            throw new JomlTypeMismatchException(typeof(JomlArray), value.GetType(), typeof(JomlArray));

        return arr;
    }

    /// <summary>
    /// Returns the TOML table associated with the provided key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The TOML table associated with the key.</returns>
    /// <exception cref="JomlTypeMismatchException">If the value associated with this key is not a table.</exception>
    /// <exception cref="JomlNoSuchValueException">If the key is not present in the table.</exception>
    public JomlTable GetSubTable(string key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        var value = GetValue(JomlUtils.AddCorrectQuotes(key));

        if (value is not JomlTable tbl)
            throw new JomlTypeMismatchException(typeof(JomlTable), value.GetType(), typeof(JomlTable));

        return tbl;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, JomlValue>> GetEnumerator()
    {
        return Entries.GetEnumerator();
    }
}