using Spectre.Console;
using System.Globalization;
using System.IO;
using System;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;
using TextMateSharp.Registry;

namespace Xenial.Identity.Infrastructure;

public static class StringExtensions
{
    internal static string SubstringAtIndexes(this string str, int startIndex, int endIndex)
        => str.Substring(startIndex, endIndex - startIndex);
}
