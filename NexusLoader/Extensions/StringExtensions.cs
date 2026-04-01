using System.Text.RegularExpressions;

namespace NexusLoader.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        
        string processed = str.Trim().ToLowerInvariant();
        
        return Regex.Replace(processed, @"\s+", "_");
    }
}