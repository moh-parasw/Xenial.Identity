namespace Xenial.Identity.Infrastructure;

public static class StringExtensions
{
    internal static string SubstringAtIndexes(this string str, int startIndex, int endIndex)
        => str[startIndex..endIndex];
}
