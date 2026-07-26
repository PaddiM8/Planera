namespace Planera.Api.Utility;

public static class StringExtensions
{
    public static string Truncate(this string value,  int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }
}