﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Jomlet.Exceptions;
using Jomlet.Extensions;
using Jomlet.Models;

namespace Jomlet;

public static class JomlSerializationMethods
{
#if MODERN_DOTNET
    internal const DynamicallyAccessedMemberTypes MainDeserializerAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.NonPublicProperties;
#endif
        
    private static MethodInfo _stringKeyedDictionaryMethod = typeof(JomlSerializationMethods).GetMethod(nameof(StringKeyedDictionaryDeserializerFor), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static MethodInfo _primitiveKeyedDictionaryMethod = typeof(JomlSerializationMethods).GetMethod(nameof(PrimitiveKeyedDictionaryDeserializerFor), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static MethodInfo _genericDictionarySerializerMethod = typeof(JomlSerializationMethods).GetMethod(nameof(GenericDictionarySerializer), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static MethodInfo _genericNullableSerializerMethod = typeof(JomlSerializationMethods).GetMethod(nameof(GenericNullableSerializer), BindingFlags.Static | BindingFlags.NonPublic)!;

    public delegate T Deserialize<out T>(JomlValue value);
    public delegate T ComplexDeserialize<out T>(JomlValue value, JomlSerializerOptions options);
    public delegate JomlValue? Serialize<in T>(T? t);
    public delegate JomlValue? ComplexSerialize<in T>(T? t, JomlSerializerOptions options);

    private static readonly Dictionary<Type, Delegate> Deserializers = new();
    private static readonly Dictionary<Type, Delegate> Serializers = new();


    [Attributes.ExcludeFromCodeCoverage]
    static JomlSerializationMethods()
    {
        //Register built-in serializers

        //String
        Register(s => new JomlString(s!), value => (value as JomlString)?.Value ?? value.StringValue);

        //Bool
        Register(JomlBoolean.ValueOf, value => (value as JomlBoolean)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlBoolean), value.GetType(), typeof(bool)));

        //Byte
        Register(i => new JomlLong(i), value => (byte)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(byte))));

        //SByte
        Register(i => new JomlLong(i), value => (sbyte)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(sbyte))));

        //UShort
        Register(i => new JomlLong(i), value => (ushort)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(ushort))));

        //Short
        Register(i => new JomlLong(i), value => (short)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(short))));

        //UInt
        Register(i => new JomlLong(i), value => (uint)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(uint))));

        //Int
        Register(i => new JomlLong(i), value => (int)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(int))));

        //ULong
        Register(l => new JomlLong((long)l), value => (ulong)((value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(ulong))));

        //Long
        Register(l => new JomlLong(l), value => (value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLong), value.GetType(), typeof(long)));

        //Double
        Register(d => new JomlDouble(d), value => (value as JomlDouble)?.Value ?? (value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlDouble), value.GetType(), typeof(double)));

        //Float
        Register(f => new JomlDouble(f), value => (float)((value as JomlDouble)?.Value ?? (value as JomlLong)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlDouble), value.GetType(), typeof(float))));

        //LocalDate(Time)
        Register(dt => dt.TimeOfDay == TimeSpan.Zero ? new JomlLocalDate(dt) : new JomlLocalDateTime(dt), value => (value as IJomlValueWithDateTime)?.Value ?? throw new JomlTypeMismatchException(typeof(IJomlValueWithDateTime), value.GetType(), typeof(DateTime)));

        //OffsetDateTime
        Register(odt => new JomlOffsetDateTime(odt), value => (value as JomlOffsetDateTime)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlOffsetDateTime), value.GetType(), typeof(DateTimeOffset)));

        //LocalTime
        Register(lt => new JomlLocalTime(lt), value => (value as JomlLocalTime)?.Value ?? throw new JomlTypeMismatchException(typeof(JomlLocalTime), value.GetType(), typeof(TimeSpan)));
    }

    /// <summary>
    /// Returns the default (reflection-based) serializer for the given type. Can be useful if you're implementing your own custom serializer but want to use the default behavior (e.g. to extend it or to use it as a fallback). 
    /// </summary>
    /// <param name="type">The type to get the default serializer for</param>
    /// <param name="options">The options to use for the serializer</param>
    /// <returns>The default reflection-based serializer for the given type.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is a primitive type.</exception>
#if MODERN_DOTNET
    public static Serialize<object> GetDefaultSerializerForType([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type type, JomlSerializerOptions? options = null)
#else
    public static Serialize<object> GetDefaultSerializerForType(Type type, JomlSerializerOptions? options = null)
#endif
    {
        options ??= JomlSerializerOptions.Default;
        if(type.IsPrimitive)
            throw new ArgumentException("Can't use reflection-based serializer for primitive types.", nameof(type));
            
        return JomlCompositeSerializer.For(type, options);
    }
        
    /// <summary>
    /// Returns the default (reflection-based) deserializer for the given type. Can be useful if you're implementing your own custom deserializer but want to use the default behavior (e.g. to extend it or to use it as a fallback).
    /// </summary>
    /// <param name="type">The type to get the default deserializer for</param>
    /// <param name="options">The options to use for the deserializer</param>
    /// <returns>The default reflection-based deserializer for the given type.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is a primitive type.</exception>
#if MODERN_DOTNET
    public static Deserialize<object> GetDefaultDeserializerForType([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type type, JomlSerializerOptions? options = null)
#else
    public static Deserialize<object> GetDefaultDeserializerForType(Type type, JomlSerializerOptions? options = null)
#endif
    {
        options ??= JomlSerializerOptions.Default;
        if(type.IsPrimitive)
            throw new ArgumentException("Can't use reflection-based deserializer for primitive types.", nameof(type));
            
        return JomlCompositeDeserializer.For(type, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    internal static Serialize<object> GetSerializer([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type t, JomlSerializerOptions? options)
#else
        internal static Serialize<object> GetSerializer(Type t, JomlSerializerOptions? options)
#endif
    {
        options ??= JomlSerializerOptions.Default;
            
        if (Serializers.TryGetValue(t, out var value))
            return (Serialize<object>)value;

        //First check, lists and arrays get serialized as enumerables.
        if (t.IsArray || t is { Namespace: "System.Collections.Generic", Name: "List`1" })
        {
            var arrSerializer = GenericEnumerableSerializer();
            Serializers[t] = arrSerializer;
            return arrSerializer;
        }

        //Check for dicts and nullables
        if (t.IsGenericType)
        {
            if (t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return DictionarySerializerFor(t, options);
            }

            if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return NullableSerializerFor(t, options);
            }
        }
            
        //Now we've got dicts out of the way we can check if we're dealing with something that's IEnumerable and if so serialize as an array. We do this only *after* checking for dictionaries, because we don't want to serialize dictionaries as enumerables (i.e. table-arrays)
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            var enumerableSerializer = GenericEnumerableSerializer();
            Serializers[t] = enumerableSerializer;
            return enumerableSerializer;
        }

        return JomlCompositeSerializer.For(t, options);
    }

#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2060", Justification = "Any provided list, enumerable, nullable, or array type's underlying generic arguments must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
    [UnconditionalSuppressMessage("AOT", "IL2062", Justification = "Any provided list, enumerable, nullable, or array type's underlying generic arguments must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any provided list, enumerable, nullable, or array type's underlying generic arguments must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
#if NET7_0_OR_GREATER        
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    internal static Deserialize<object> GetDeserializer([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type t, JomlSerializerOptions? options)
#else
        internal static Deserialize<object> GetDeserializer(Type t, JomlSerializerOptions? options)
#endif
    {
        options ??= JomlSerializerOptions.Default;
            
        if (Deserializers.TryGetValue(t, out var value))
            return (Deserialize<object>)value;

        //We allow deserializing to IEnumerable fields/props, by setting them an array. We DO NOT do anything for classes that implement IEnumerable, though, because that would mess with deserializing lists, dictionaries, etc.
        if (t.IsArray || t.IsInterface && typeof(IEnumerable).IsAssignableFrom(t)) 
        {
            var elementType = t.IsInterface ? t.GetGenericArguments()[0] : t.GetElementType()!;
            var arrayDeserializer = ArrayDeserializerFor(elementType, options);
            Deserializers[t] = arrayDeserializer;
            return arrayDeserializer;
        }

        if (t.Namespace == "System.Collections.Generic" && t.Name == "List`1")
        {
            var listDeserializer = ListDeserializerFor(t.GetGenericArguments()[0], options);
            Deserializers[t] = listDeserializer;
            return listDeserializer;
        }
            
        if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments() is {Length: 1})
        {
            var nullableDeserializer = NullableDeserializerFor(t, options);
            Deserializers[t] = nullableDeserializer;
            return nullableDeserializer;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>) && t.GetGenericArguments() is { Length: 2 } genericArgs)
        {
            if (genericArgs[0] == typeof(string))
            {
                return (Deserialize<object>)_stringKeyedDictionaryMethod.MakeGenericMethod(genericArgs[1]).Invoke(null, new object[]{options})!;
            }

            if (genericArgs[0].IsIntegerType() || genericArgs[0] == typeof(bool) || genericArgs[0] == typeof(char))
            {
                // float primitives not supported due to decimal point causing issues
                return (Deserialize<object>)_primitiveKeyedDictionaryMethod.MakeGenericMethod(genericArgs).Invoke(null, new object[]{options})!;
            }
        }

        return JomlCompositeDeserializer.For(t, options);
    }

    private static Serialize<object?> GenericEnumerableSerializer() =>
        o =>
        {
            if (o is not IEnumerable arr)
                throw new Exception("How did ArraySerializer end up getting a non-array?");

            var ret = new JomlArray();
            foreach (var entry in arr)
            {
                ret.Add(entry);
            }

            return ret;
        };

#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2060", Justification = "The dictType's underlying generic arguments must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Serialize<object> DictionarySerializerFor([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type dictType, JomlSerializerOptions options)
#else
        private static Serialize<object> DictionarySerializerFor(Type dictType, JomlSerializerOptions options)
#endif
    {
        var serializer = _genericDictionarySerializerMethod.MakeGenericMethod(dictType.GetGenericArguments());

        var del = Delegate.CreateDelegate(typeof(ComplexSerialize<>).MakeGenericType(dictType), serializer);
        var ret = (Serialize<object>)(dict => (JomlValue?)del.DynamicInvoke(dict, options));
        Serializers[dictType] = ret;

        return ret;
    }
        
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2060", Justification = "The nullableType's underlying T must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Serialize<object> NullableSerializerFor([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type nullableType, JomlSerializerOptions options)
#else
        private static Serialize<object> NullableSerializerFor(Type nullableType, JomlSerializerOptions options)
#endif
    {
        var serializer = _genericNullableSerializerMethod.MakeGenericMethod(nullableType.GetGenericArguments());
                    
        var del = Delegate.CreateDelegate(typeof(ComplexSerialize<>).MakeGenericType(nullableType), serializer);
        var ret = (Serialize<object>)(dict => (JomlValue?)del.DynamicInvoke(dict, options));
        Serializers[nullableType] = ret;
                    
        return ret;
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Deserialize<object> ArrayDeserializerFor([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type elementType, JomlSerializerOptions options) =>
#else
        private static Deserialize<object> ArrayDeserializerFor(Type elementType, JomlSerializerOptions options) =>
#endif
        value =>
        {
            if (value is not JomlArray tomlArray)
                throw new JomlTypeMismatchException(typeof(JomlArray), value.GetType(), elementType.MakeArrayType());

            var ret = Array.CreateInstance(elementType, tomlArray.Count);
            var deserializer = GetDeserializer(elementType, options);
            for (var index = 0; index < tomlArray.ArrayValues.Count; index++)
            {
                var arrayValue = tomlArray.ArrayValues[index];
                ret.SetValue(deserializer(arrayValue), index);
            }

            return ret;
        };

#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "The element type must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
    private static Deserialize<object> ListDeserializerFor([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type elementType, JomlSerializerOptions options)
#else
        private static Deserialize<object> ListDeserializerFor(Type elementType, JomlSerializerOptions options)
#endif
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var relevantAddMethod = listType.GetMethod("Add")!;

        return value =>
        {
            if (value is not JomlArray tomlArray)
                throw new JomlTypeMismatchException(typeof(JomlArray), value.GetType(), listType);

            var ret = Activator.CreateInstance(listType)!;
            var deserializer = GetDeserializer(elementType, options);

            foreach (var arrayValue in tomlArray.ArrayValues)
            {
                relevantAddMethod.Invoke(ret, new[] { deserializer(arrayValue) });
            }

            return ret;
        };
    }
        
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2062", Justification = "The nullableType's underlying T must have been used somewhere in the consuming code in order for this method to be called, so the dynamic code requirement is already satisfied.")]
#if NET7_0_OR_GREATER        
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Deserialize<object> NullableDeserializerFor([DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] Type nullableType, JomlSerializerOptions options)
#else 
        private static Deserialize<object> NullableDeserializerFor(Type nullableType, JomlSerializerOptions options)
#endif
    {
        var elementType = nullableType.GetGenericArguments()[0];
        var elementDeserializer = GetDeserializer(elementType, options);
            
        return value =>
        {
            //If we're deserializing, we know the value is not null
            var element = elementDeserializer(value);
            return Activator.CreateInstance(nullableType, element)!;
        };
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Deserialize<Dictionary<string, T>> StringKeyedDictionaryDeserializerFor<[DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] T>(JomlSerializerOptions options)
#else
        private static Deserialize<Dictionary<string, T>> StringKeyedDictionaryDeserializerFor<T>(JomlSerializerOptions options)
#endif
    {
        var deserializer = GetDeserializer(typeof(T), options);

        return value =>
        {
            if (value is not JomlTable table)
                throw new JomlTypeMismatchException(typeof(JomlTable), value.GetType(), typeof(Dictionary<string, T>));

            return table.Entries.ToDictionary(entry => entry.Key, entry => (T)deserializer(entry.Value));
        };
    }

    // unmanaged + IConvertible is the closest I can get to expressing "primitives only"
#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static Deserialize<Dictionary<TKey, TValue>> PrimitiveKeyedDictionaryDeserializerFor<TKey, [DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] TValue>(JomlSerializerOptions options) where TKey : unmanaged, IConvertible
#else
        private static Deserialize<Dictionary<TKey, TValue>> PrimitiveKeyedDictionaryDeserializerFor<TKey, TValue>(JomlSerializerOptions options) where TKey : unmanaged, IConvertible
#endif
    {
        var valueDeserializer = GetDeserializer(typeof(TValue), options);
        var type = typeof(TKey);
        return value =>
        {
            if (value is not JomlTable table)
                throw new JomlTypeMismatchException(typeof(JomlTable), value.GetType(), typeof(Dictionary<TKey, TValue>));

            return table.Entries.ToDictionary(
                entry =>
                {
                    if (!type.IsEnum)
                    {
                        return (TKey)(entry.Key as IConvertible).ToType(typeof(TKey), CultureInfo.InvariantCulture);
                    }

                    try
                    {
                        return (TKey)Enum.Parse(type, entry.Key, true);
                    }
                    catch (ArgumentException)
                    {
                        if (options.IgnoreInvalidEnumValues)
                            return (TKey)Enum.GetValues(type).GetValue(0)!;

                        throw new JomlEnumParseException(entry.Key, typeof(TKey));
                    }
                },
                entry => (TValue)valueDeserializer(entry.Value)
            );
        };
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static JomlValue? GenericNullableSerializer<[DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] T>(T? nullable, JomlSerializerOptions options) where T : struct
#else
        private static JomlValue? GenericNullableSerializer<T>(T? nullable, JomlSerializerOptions options) where T : struct
#endif
    {
        var elementSerializer = GetSerializer(typeof(T), options);
            
        if (nullable.HasValue)
            return elementSerializer(nullable.Value);

        return null;
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    private static JomlValue GenericDictionarySerializer<TKey, [DynamicallyAccessedMembers(MainDeserializerAccessedMemberTypes)] TValue>(Dictionary<TKey, TValue> dict, JomlSerializerOptions options) where TKey : notnull
#else
        private static JomlValue GenericDictionarySerializer<TKey, TValue>(Dictionary<TKey, TValue> dict, JomlSerializerOptions options) where TKey : notnull
#endif
    {
        var valueSerializer = GetSerializer(typeof(TValue), options);

        var ret = new JomlTable();
        foreach (var entry in dict)
        {
            var keyAsString = entry.Key.ToString();
                
            if(keyAsString == null)
                continue;

            var value = valueSerializer(entry.Value);
                
            if(value == null)
                continue;
                
            ret.PutValue(keyAsString, value, true);
        }

        return ret;
    }

    internal static void Register<T>(Serialize<T>? serializer, Deserialize<T>? deserializer)
    {
        if (serializer != null)
        {
            RegisterSerializer(serializer);

            RegisterDictionarySerializer(serializer);
        }

        if (deserializer != null)
        {
            RegisterDeserializer(deserializer);
            RegisterDictionaryDeserializer(deserializer);
        }
    }

    internal static void Register(Type t, Serialize<object>? serializer, Deserialize<object>? deserializer)
    {
        if (serializer != null)
            RegisterSerializer(serializer);

        if (deserializer != null)
            RegisterDeserializer(deserializer);
    }

    private static void RegisterDeserializer<T>(Deserialize<T> deserializer)
    {
        object BoxedDeserializer(JomlValue value) => deserializer.Invoke(value) ?? throw new Exception($"TOML Deserializer returned null for type {nameof(T)}");
        Deserializers[typeof(T)] = (Deserialize<object>)BoxedDeserializer;
    }

    private static void RegisterSerializer<T>(Serialize<T> serializer)
    {
        JomlValue? ObjectAcceptingSerializer(object value) => serializer.Invoke((T)value);
        Serializers[typeof(T)] = (Serialize<object>)ObjectAcceptingSerializer!;
    }

    private static void RegisterDictionarySerializer<T>(Serialize<T> serializer)
    {
        RegisterSerializer<Dictionary<string, T>>(dict =>
        {
            var table = new JomlTable();

            if (dict == null)
                return table;

            var keys = dict.Keys.ToList();
            var values = dict.Values.Select(serializer.Invoke).ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                var tomlValue = values[i];
                if (tomlValue == null)
                    //Skip null values
                    continue;
                    
                table.PutValue(keys[i], tomlValue, true);
            }

            return table;
        });
    }

    private static void RegisterDictionaryDeserializer<T>(Deserialize<T> deserializer)
    {
        RegisterDeserializer(value =>
        {
            if (value is not JomlTable table)
                throw new JomlTypeMismatchException(typeof(JomlTable), value.GetType(), typeof(Dictionary<string, T>));

            return table.Entries
                .Select(kvp => new KeyValuePair<string, T>(kvp.Key, deserializer.Invoke(kvp.Value)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        });
    }
}