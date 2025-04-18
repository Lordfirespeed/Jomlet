using System;
using System.Diagnostics.CodeAnalysis;
using Jomlet.Exceptions;
using Jomlet.Models;

namespace Jomlet;

//Api class, these are supposed to be exposed
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class JomletMain
{
    [Attributes.ExcludeFromCodeCoverage]
    public static void RegisterMapper<T>(JomlSerializationMethods.Serialize<T>? serializer, JomlSerializationMethods.Deserialize<T>? deserializer)
        => JomlSerializationMethods.Register(serializer, deserializer);

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static T To<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(string tomlString, JomlSerializerOptions? options = null)
#else
        public static T To<T>(string tomlString, JomlSerializerOptions? options = null)
#endif
    {
        return (T)To(typeof(T), tomlString, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static object To([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type what, string tomlString, JomlSerializerOptions? options = null)
#else
        public static object To(Type what, string tomlString, JomlSerializerOptions? options = null)
#endif
    {
        var parser = new JomlParser();
        var tomlDocument = parser.Parse(tomlString);

        return To(what, tomlDocument, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static T To<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(JomlValue value, JomlSerializerOptions? options = null)
#else
        public static T To<T>(JomlValue value, JomlSerializerOptions? options = null)
#endif
    {
        return (T)To(typeof(T), value, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static object To([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type what, JomlValue value, JomlSerializerOptions? options = null)
#else
        public static object To(Type what, JomlValue value, JomlSerializerOptions? options = null)
#endif
    {
        var deserializer = JomlSerializationMethods.GetDeserializer(what, options);

        return deserializer.Invoke(value);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any object that is being serialized must have been in the consuming code in order for this call to be occurring, so the dynamic code requirement is already satisfied.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static JomlValue? ValueFrom<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, JomlSerializerOptions? options = null)
#else
        public static JomlValue? ValueFrom<T>(T? t, JomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;

        return ValueFrom(t.GetType(), t, options);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static JomlValue? ValueFrom([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, JomlSerializerOptions? options = null)
#else
        public static JomlValue? ValueFrom(Type type, object? t, JomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;
        
        var serializer = JomlSerializationMethods.GetSerializer(type, options);

        var tomlValue = serializer.Invoke(t);

        return tomlValue!;
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static JomlDocument? DocumentFrom<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, JomlSerializerOptions? options = null)
#else
        public static JomlDocument? DocumentFrom<T>(T? t, JomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;

        return DocumentFrom(typeof(T), t, options);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static JomlDocument? DocumentFrom([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, JomlSerializerOptions? options = null)
#else
        public static JomlDocument? DocumentFrom(Type type, object? t, JomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;
        
        var val = ValueFrom(type, t, options);

        return val switch
        {
            JomlDocument doc => doc,
            JomlTable table => new JomlDocument(table),
            _ => throw new JomlPrimitiveToDocumentException(type)
        };
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static string? TomlStringFrom<[DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, JomlSerializerOptions? options = null) => DocumentFrom(t, options)?.SerializedValue;
    
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static string? TomlStringFrom([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, JomlSerializerOptions? options = null) => DocumentFrom(type, t, options)?.SerializedValue;

#else
        public static string? TomlStringFrom<T>(T? t, JomlSerializerOptions? options = null) => DocumentFrom(t, options)?.SerializedValue;

        public static string? TomlStringFrom(Type type, object? t, JomlSerializerOptions? options = null) => DocumentFrom(type, t, options)?.SerializedValue;
#endif
}