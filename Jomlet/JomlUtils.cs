﻿using Jomlet.Exceptions;

namespace Jomlet;

internal static class JomlUtils
{
    public static string EscapeStringValue(string key)
    {
        var escaped = key.Replace(@"\", @"\\")
            .Replace("\n", @"\n")
            .Replace("\r", "");
            
        return escaped;
    }

    public static string AddCorrectQuotes(string key)
    {
        if (key.Contains("'") && key.Contains("\""))
            throw new InvalidJomlKeyException(key);

        if (key.Contains("\""))
            return $"'{key}'";

        return $"\"{key}\"";
    }
}