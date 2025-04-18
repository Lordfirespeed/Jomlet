using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jomlet.Attributes;
using Jomlet.Extensions;
using Jomlet.Models;

namespace Jomlet;

internal static class JomlCompositeSerializer
{
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any field that is being serialized must have been used as a field in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    public static JomlSerializationMethods.Serialize<object> For([DynamicallyAccessedMembers(JomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, JomlSerializerOptions options)
#else
    public static JomlSerializationMethods.Serialize<object> For(Type type, JomlSerializerOptions options)
#endif
    {
        JomlSerializationMethods.Serialize<object> serializer;

        if (type.IsEnum)
        {
            var stringSerializer = JomlSerializationMethods.GetSerializer(typeof(string), options);
            serializer = o => stringSerializer.Invoke(Enum.GetName(type, o!) ?? throw new ArgumentException($"Jomlet: Cannot serialize {o} as an enum of type {type} because the enum type does not declare a name for that value"));
        }
        else
        {
            //Get all instance fields
            var memberFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!options.IgnoreNonPublicMembers) {
                memberFlags |= BindingFlags.NonPublic;
            }

            var fields = type.GetFields(memberFlags);
            var fieldAttribs = fields
                .ToDictionary(f => f, f => new {inline = GenericExtensions.GetCustomAttribute<JomlInlineCommentAttribute>(f), preceding = GenericExtensions.GetCustomAttribute<JomlPrecedingCommentAttribute>(f), field = GenericExtensions.GetCustomAttribute<JomlFieldAttribute>(f), noInline = GenericExtensions.GetCustomAttribute<JomlDoNotInlineObjectAttribute>(f)});
            var props = type.GetProperties(memberFlags)
                .ToArray();
            var propAttribs = props
                .ToDictionary(p => p, p => new {inline = GenericExtensions.GetCustomAttribute<JomlInlineCommentAttribute>(p), preceding = GenericExtensions.GetCustomAttribute<JomlPrecedingCommentAttribute>(p), prop = GenericExtensions.GetCustomAttribute<JomlPropertyAttribute>(p), noInline = GenericExtensions.GetCustomAttribute<JomlDoNotInlineObjectAttribute>(p)});

            var isForcedNoInline = GenericExtensions.GetCustomAttribute<JomlDoNotInlineObjectAttribute>(type) != null;

            //Ignore NonSerialized and CompilerGenerated fields.
            fields = fields.Where(f => !(f.IsNotSerialized || GenericExtensions.GetCustomAttribute<JomlNonSerializedAttribute>(f) != null)
                && GenericExtensions.GetCustomAttribute<CompilerGeneratedAttribute>(f) == null 
                && !f.Name.Contains('<')).ToArray();

            //Ignore TomlNonSerializedAttribute Decorated Properties
            props = props.Where(p => GenericExtensions.GetCustomAttribute<JomlNonSerializedAttribute>(p) == null).ToArray();

            if (fields.Length + props.Length == 0)
                return _ => new JomlTable();

            serializer = instance =>
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), "Object being serialized is null. TOML does not support null values.");

                var resultTable = new JomlTable {ForceNoInline = isForcedNoInline};

                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(instance);

                    if (fieldValue == null)
                        continue; //Skip nulls - TOML doesn't support them.

                    var tomlValue = JomlSerializationMethods.GetSerializer(field.FieldType, options).Invoke(fieldValue);
                    
                    if(tomlValue == null)
                        continue;
                    
                    var thisFieldAttribs = fieldAttribs[field];

                    if (resultTable.ContainsKey(field.Name))
                        //Do not overwrite fields if they have the same name as something already in the table
                        //This fixes serializing types which re-declare a field using the `new` keyword, overwriting a field of the same name
                        //in its supertype. 
                        continue;

                    tomlValue.Comments.InlineComment = thisFieldAttribs.inline?.Comment;
                    tomlValue.Comments.PrecedingComment = thisFieldAttribs.preceding?.Comment;
                    
                    if(thisFieldAttribs.noInline != null && tomlValue is JomlTable table)
                        table.ForceNoInline = true;

                    resultTable.PutValue(thisFieldAttribs.field?.GetMappedString() ?? field.Name, tomlValue);
                }

                foreach (var prop in props)
                {
                    if(prop.GetGetMethod(true) == null)
                        continue; //Skip properties without a getter
                    
                    if(prop.Name == "EqualityContract")
                        continue; //Skip record equality contract property. Wish there was a better way to do this.
                    
                    var propValue = prop.GetValue(instance, null);
                    
                    if(propValue == null)
                        continue;
                    
                    var tomlValue = JomlSerializationMethods.GetSerializer(prop.PropertyType, options).Invoke(propValue);

                    if (tomlValue == null) 
                        continue;
                    
                    var thisPropAttribs = propAttribs[prop];
                    
                    tomlValue.Comments.InlineComment = thisPropAttribs.inline?.Comment;
                    tomlValue.Comments.PrecedingComment = thisPropAttribs.preceding?.Comment;

                    if (thisPropAttribs.noInline != null && tomlValue is JomlTable table)
                        table.ForceNoInline = true;

                    resultTable.PutValue(thisPropAttribs.prop?.GetMappedString() ?? prop.Name, tomlValue);
                }

                return resultTable;
            };
        }

        //Cache composite deserializer.
        JomlSerializationMethods.Register(type, serializer, null);

        return serializer;
    }
}