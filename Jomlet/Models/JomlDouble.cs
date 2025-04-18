﻿using System.Globalization;

namespace Jomlet.Models;

public class JomlDouble : JomlValue
{
    private double _value;

    public JomlDouble(double value)
    {
        _value = value;
    }

    internal static JomlDouble? Parse(string valueInToml)
    {
        var nullableDouble = JomlNumberUtils.GetDoubleValue(valueInToml);

        if (!nullableDouble.HasValue)
            return null;

        return new JomlDouble(nullableDouble.Value);
    }

    public bool HasDecimal => Value != (int) Value;
    public double Value => _value;

    public bool IsNaN => double.IsNaN(Value);
    public bool IsInfinity => double.IsInfinity(Value);

    public override string StringValue => this switch
    {
        {IsInfinity: true} => double.IsPositiveInfinity(Value) ? "inf" : "-inf",
        {IsNaN: true} => "nan",
        {HasDecimal: true} => Value.ToString(CultureInfo.InvariantCulture),
        _ => Value.ToString("F1", CultureInfo.InvariantCulture) //When no decimal, force a decimal point (.0) to force any consuming tools (including ourselves!) to re-parse as float.
    };

    public override string SerializedValue => StringValue;
}