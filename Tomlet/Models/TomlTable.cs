﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tomlet.Exceptions;

namespace Tomlet.Models
{
    public class TomlTable : TomlValue
    {
        public readonly Dictionary<string, TomlValue> Entries = new();

        internal bool Locked;

        public override string StringValue => $"Table ({Entries.Count} entries)";

        public HashSet<string> Keys => new(Entries.Keys);

        public virtual bool ShouldBeSerializedInline => Entries.Count < 4 && Entries.All(e => e.Value is not TomlTable && e.Value is TomlArray {IsSimpleArray: true} && e.Value.SerializedValue.Contains("\n"));

        public override string SerializedValue
        {
            get
            {
                if (!ShouldBeSerializedInline)
                    throw new Exception("Cannot use SerializeValue to serialize non-inline tables. Use SerializeNonInlineTable(keyName).");

                var builder = new StringBuilder("{ ");

                builder.Append(string.Join(", ", Entries.Select(o => o.Key + " = " + o.Value.SerializedValue).ToArray()));

                builder.Append(" }");

                return builder.ToString();
            }
        }

        public string SerializeNonInlineTable(string? keyName, bool includeHeader = true)
        {
            var builder = new StringBuilder();
            if (includeHeader)
                builder.Append('[').Append(keyName).Append("]").Append('\n');

            //Three passes: Simple key-value pairs including inline arrays and tables, sub-tables, then sub-table-arrays.
            foreach (var (subKey, value) in Entries)
            {
                if (value is TomlTable {ShouldBeSerializedInline: false} or TomlArray {CanBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlTable {ShouldBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlArray {CanBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            return builder.ToString();
        }

        private void WriteValueToStringBuilder(string? keyName, string subKey, StringBuilder builder)
        {
            var value = GetValue(subKey);

            subKey = EscapeKeyIfNeeded(subKey);

            if(keyName != null)
                keyName = EscapeKeyIfNeeded(keyName);

            var fullSubKey = keyName == null ? subKey : $"{keyName}.{subKey}";

            switch (value)
            {
                case TomlArray {CanBeSerializedInline: false} subArray:
                    builder.Append(subArray.SerializeTableArray(fullSubKey)).Append('\n').Append('\n');
                    break;
                case TomlArray subArray:
                    builder.Append(subKey).Append(" = ").Append(subArray.SerializedValue).Append('\n');
                    break;
                case TomlTable {ShouldBeSerializedInline: true} subTable:
                    builder.Append(subKey).Append(" = ").Append(subTable.SerializedValue).Append('\n');
                    break;
                case TomlTable subTable:
                    builder.Append(subTable.SerializeNonInlineTable(fullSubKey)).Append('\n');
                    break;
                default:
                    builder.Append(subKey).Append(" = ").Append(value.SerializedValue).Append('\n');
                    break;
            }
        }

        private string EscapeKeyIfNeeded(string key) {

            if(key.Contains("\"") || key.Contains("'"))
                key = QuoteKey(key);

            return key.Replace(@"\", @"\\")
                    .Replace("\n", @"\n")
                    .Replace("\r", "");
        }

        internal void ParserPutValue(string key, TomlValue value, int lineNumber)
        {
            if (Locked)
                throw new TomlTableLockedException(lineNumber, key);

            InternalPutValue(key, value, lineNumber, true);
        }

        public void PutValue(string key, TomlValue value, bool quote = false)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            if(value == null)
                throw new ArgumentNullException("value");

            if (quote)
                key = QuoteKey(key);
            InternalPutValue(key, value, null, false);
        }

        public void Put<T>(string key, T t, bool quote = false) => PutValue(key, TomletMain.ValueFrom(t), quote);

        public string DequoteKey(string key)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            return !wholeKeyIsQuoted ? key : key.Substring(1, key.Length - 2);
        }

        public static string QuoteKey(string key)
        {
            if (key.Contains("'") && key.Contains("\""))
                throw new InvalidTomlKeyException(key);

            if (key.Contains("\""))
                return $"'{key}'";

            return $"\"{key}\"";
        }

        private void InternalPutValue(string key, TomlValue value, int? lineNumber, bool callParserForm)
        {
            key = key.Trim();
            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (!string.IsNullOrEmpty(restOfKey))
            {
                if (!Entries.TryGetValue(DequoteKey(ourKeyName), out var existingValue))
                {
                    //We don't have a sub-table with this name defined. That's fine, make one.
                    var subtable = new TomlTable();
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
                if (existingValue is not TomlTable existingTable)
                {
                    //No - throw an exception
                    if (lineNumber.HasValue)
                        throw new TomlDottedKeyParserException(lineNumber.Value, ourKeyName);

                    throw new TomlDottedKeyException(ourKeyName);
                }

                //Yes, get the sub-table to handle the rest of the key
                if (callParserForm)
                    existingTable.ParserPutValue(restOfKey, value, lineNumber!.Value);
                else
                    existingTable.PutValue(restOfKey, value);
                return;
            }

            //Non-dotted keys land here.
            key = DequoteKey(key);

            if (Entries.ContainsKey(key) && lineNumber.HasValue)
                throw new TomlKeyRedefinitionException(lineNumber.Value, key);

            Entries[key] = value;
        }

        public bool ContainsKey(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries.ContainsKey(DequoteKey(key));

            if (!Entries.TryGetValue(ourKeyName, out var existingKey))
                return false;

            if (existingKey is TomlTable table)
                return table.ContainsKey(restOfKey);

            throw new TomlContainsDottedKeyNonTableException(key);
        }

        /// <summary>
        /// Returns the raw instance of <see cref="TomlValue"/> associated with this key. You must cast to a sub-class and access its value
        /// yourself.
        /// Unlike all the specific getters, this Getter respects dotted keys and quotes. You must quote any keys which contain a dot if you want to access the key itself,
        /// not a sub-key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>An instance of <see cref="TomlValue"/> associated with this key.</returns>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlValue GetValue(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            if (!ContainsKey(key))
                throw new TomlNoSuchValueException(key);

            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries[DequoteKey(key)];

            if (!Entries.TryGetValue(ourKeyName, out var existingKey))
                throw new TomlNoSuchValueException(key); //Should already be handled by ContainsKey test

            if (existingKey is TomlTable table)
                return table.GetValue(restOfKey);

            throw new Exception("Tomlet Internal bug - existing key is not a table in TomlTable GetValue, but we didn't throw in ContainsKey?");
        }

        /// <summary>
        /// Returns the string value associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The string value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a string.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public string GetString(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlString str)
                throw new TomlTypeMismatchException(typeof(TomlString), value.GetType(), typeof(string));

            return str.Value;
        }

        /// <summary>
        /// Returns the integer value associated with the provided key, downsized from a long.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The integer value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not an integer type.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public int GetInteger(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlLong lng)
                throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int));

            return (int) lng.Value;
        }

        /// <summary>
        /// Returns the 32-bit floating-point value associated with the provided key, downsized from a double.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The float value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a floating-point type.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public float GetFloat(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlDouble dbl)
                throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(float));

            return (float) dbl.Value;
        }

        /// <summary>
        /// Returns the boolean value associated with the provided key
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The boolean value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a boolean.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public bool GetBoolean(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlBoolean b)
                throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType(), typeof(bool));

            return b.Value;
        }

        /// <summary>
        /// Returns the TOML array associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The TOML array associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not an array.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlArray GetArray(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlArray arr)
                throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), typeof(TomlArray));

            return arr;
        }

        /// <summary>
        /// Returns the TOML table associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The TOML table associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a table.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlTable GetSubTable(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(QuoteKey(key));

            if (value is not TomlTable tbl)
                throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), typeof(TomlTable));

            return tbl;
        }
    }
}