using System.Text;

namespace Jomlet.Extensions;

internal static class StringExtensions
{
    internal static string ToPascalCase(this string str)
    {
        var sb = new StringBuilder(str.Length);

        if (str.Length > 0)
        {
            sb.Append(char.ToUpper(str[0]));
        }

        for (var i = 1; i < str.Length; i++)
        {
            sb.Append(char.IsWhiteSpace(str[i - 1]) ? char.ToUpper(str[i]) : str[i]);
        }

        return sb.ToString();
    }
}