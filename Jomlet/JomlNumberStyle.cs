using System.Globalization;

namespace Jomlet;

internal static class JomlNumberStyle
{
    internal static NumberStyles FloatingPoint = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
    internal static NumberStyles Integer = NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
}