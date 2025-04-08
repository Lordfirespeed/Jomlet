using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jomlet.Attributes;
using Jomlet.Exceptions;
using Jomlet.Extensions;
using Jomlet.Models;

namespace Jomlet;

internal static class JomlCompositeDeserializer
{
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any field that is being deserialized to must have been used as a field in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    public static JomlSerializationMethods.Deserialize<object> For([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, JomlSerializerOptions options)
#else
    public static JomlSerializationMethods.Deserialize<object> For(Type type, JomlSerializerOptions options)
#endif
    {
        JomlSerializationMethods.Deserialize<object> deserializer;
        if (type.IsEnum)
        {
            var stringDeserializer = JomlSerializationMethods.GetDeserializer(typeof(string), options);
            deserializer = value =>
            {
                var enumName = (string)stringDeserializer.Invoke(value);

                try
                {
                    return Enum.Parse(type, enumName, true);
                }
                catch (Exception)
                {
                    if(options.IgnoreInvalidEnumValues)
                        return Enum.GetValues(type).GetValue(0)!;
                    
                    throw new JomlEnumParseException(enumName, type);
                }
            };
        }
        else
        {
            //Get all instance fields
            var memberFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!options.IgnoreNonPublicMembers) {
                memberFlags |= BindingFlags.NonPublic;
            }

            var fields = type.GetFields(memberFlags);

            //Ignore NonSerialized and CompilerGenerated fields.
            var fieldsDict = fields
                .Where(f => !f.IsNotSerialized && GenericExtensions.GetCustomAttribute<CompilerGeneratedAttribute>(f) == null)
                .Select(f => new KeyValuePair<FieldInfo, JomlFieldAttribute?>(f, GenericExtensions.GetCustomAttribute<JomlFieldAttribute>(f)))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            var props = type.GetProperties(memberFlags);

            //Ignore TomlNonSerializedAttribute Decorated Properties
            var propsDict = props
                .Where(p => p.GetSetMethod(true) != null && GenericExtensions.GetCustomAttribute<TomlNonSerializedAttribute>(p) == null)
                .Select(p => new KeyValuePair<PropertyInfo, JomlPropertyAttribute?>(p, GenericExtensions.GetCustomAttribute<JomlPropertyAttribute>(p)))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            if (fieldsDict.Count + propsDict.Count == 0)
                return value => CreateInstance(type, value, options, out _);

            deserializer = value =>
            {
                if (value is not JomlTable table)
                    throw new JomlTypeMismatchException(typeof(JomlTable), value.GetType(), type);

                var instance = CreateInstance(type, value, options, out var assignedMembers);

                foreach (var (field, attribute) in fieldsDict)
                {
                    var name = attribute?.GetMappedString() ?? field.Name;
                    if (!options.OverrideConstructorValues && assignedMembers.Contains(name))
                        continue;
                        
                    if (!table.TryGetValue(name, out var entry))
                        continue; //TODO: Do we want to make this configurable? As in, throw exception if data is missing?

                    object fieldValue;
                    try
                    {
                        fieldValue = JomlSerializationMethods.GetDeserializer(field.FieldType, options).Invoke(entry);
                    }
                    catch (JomlTypeMismatchException e)
                    {
                        throw new JomlFieldTypeMismatchException(type, field, e);
                    }

                    field.SetValue(instance, fieldValue);
                }

                foreach (var (prop, attribute) in propsDict)
                {
                    var name = attribute?.GetMappedString() ?? prop.Name;
                    if (!options.OverrideConstructorValues && assignedMembers.Contains(name))
                        continue;
                        
                    if (!table.TryGetValue(name, out var entry))
                        continue; //TODO: As above, configurable?

                    object propValue;

                    try
                    {
                        propValue = JomlSerializationMethods.GetDeserializer(prop.PropertyType, options).Invoke(entry);
                    }
                    catch (JomlTypeMismatchException e)
                    {
                        throw new JomlPropertyTypeMismatchException(type, prop, e);
                    }

                    prop.SetValue(instance, propValue, null);
                }

                return instance;
            };
        }

        //Cache composite deserializer.
        JomlSerializationMethods.Register(type, null, deserializer);

        return deserializer;
    }

#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any constructor parameter must have been used somewhere in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    private static object CreateInstance([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, JomlValue jomlValue, JomlSerializerOptions options, out HashSet<string> assignedMembers)
#else
    private static object CreateInstance(Type type, JomlValue jomlValue, JomlSerializerOptions options, out HashSet<string> assignedMembers)
#endif
    {
        if (jomlValue is not JomlTable table)
            throw new JomlTypeMismatchException(typeof(JomlTable), jomlValue.GetType(), type);
        
        if (!type.TryGetBestMatchConstructor(out var constructor))
        {
            throw new JomlInstantiationException();
        }

        var parameters = constructor!.GetParameters();
        if (parameters.Length == 0)
        {
            assignedMembers = new HashSet<string>();
            return constructor.Invoke(null);
        }

        assignedMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var arguments = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            object argument;
            
            if (!table.TryGetValue(parameter.Name!.ToPascalCase(), out var entry))
                continue;

            try
            {
                argument = JomlSerializationMethods.GetDeserializer(parameter.ParameterType, options).Invoke(entry);
            }
            catch (JomlTypeMismatchException e)
            {
                throw new JomlParameterTypeMismatchException(parameter.ParameterType, parameter, e);
            }

            arguments[i] = argument;
            assignedMembers.Add(parameter.Name!);
        }

        return constructor.Invoke(arguments);
    }
}