﻿namespace Jomlet;

public class JomlSerializerOptions
{
    public static JomlSerializerOptions Default = new();
    
    /// <summary>
    /// When set to false (default) the deserializer will skip assigning fields that have constructor params of the same name.
    /// </summary>
    public bool OverrideConstructorValues { get; set; } = false;
    
    /// <summary>
    /// When set to true, the deserializer will ignore non-public members. When set to false, only members marked [NonSerialized] will be ignored.
    /// </summary>
    public bool IgnoreNonPublicMembers { get; set; } = false;
    
    /// <summary>
    /// When set to true, the deserializer will ignore invalid enum values (and they will be implicitly left at their default value). When set to false, an exception will be thrown if the enum value is not found.
    /// </summary>
    public bool IgnoreInvalidEnumValues { get; set; } = false;
}