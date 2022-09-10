using Spectre.Console;

namespace Xenial.Identity.Infrastructure;

/// <summary>
/// A console capable of writing ANSI escape sequences.
/// </summary>
public static partial class LogConsole
{
    /// <summary>
    /// Writes the specified markup to the console.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public static void Markup(string value)
        => AnsiConsole.Markup(value);

    /// <summary>
    /// Writes the specified markup to the console.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write.</param>
    public static void Markup(string format, params object[] args)
        => AnsiConsole.Markup(format, args);

    /// <summary>
    /// Writes the specified markup to the console.
    /// <para/>
    /// All interpolation holes which contain a string are automatically escaped so you must not call <see cref="StringExtensions.EscapeMarkup"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// string input = args[0];
    /// string output = Process(input);
    /// AnsiConsole.MarkupInterpolated($"[blue]{input}[/] -> [green]{output}[/]");
    /// </code>
    /// </example>
    /// <param name="value">The interpolated string value to write.</param>
    public static void MarkupInterpolated(FormattableString value)
        => AnsiConsole.MarkupInterpolated(value);

    /// <summary>
    /// Writes the specified markup to the console.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write.</param>
    public static void Markup(IFormatProvider provider, string format, params object[] args)
        => AnsiConsole.Markup(provider, format, args);

    /// <summary>
    /// Writes the specified markup to the console.
    /// <para/>
    /// All interpolation holes which contain a string are automatically escaped so you must not call <see cref="StringExtensions.EscapeMarkup"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// string input = args[0];
    /// string output = Process(input);
    /// AnsiConsole.MarkupInterpolated(CultureInfo.InvariantCulture, $"[blue]{input}[/] -> [green]{output}[/]");
    /// </code>
    /// </example>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="value">The interpolated string value to write.</param>
    public static void MarkupInterpolated(IFormatProvider provider, FormattableString value)
        => AnsiConsole.MarkupInterpolated(provider, value);

    /// <summary>
    /// Writes the specified markup, followed by the current line terminator, to the console.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public static void MarkupLine(string value)
        => AnsiConsole.MarkupLine(value);

    /// <summary>
    /// Writes the specified markup, followed by the current line terminator, to the console.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write.</param>
    public static void MarkupLine(string format, params object[] args)
        => AnsiConsole.MarkupLine(format, args);

    /// <summary>
    /// Writes the specified markup, followed by the current line terminator, to the console.
    /// <para/>
    /// All interpolation holes which contain a string are automatically escaped so you must not call <see cref="StringExtensions.EscapeMarkup"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// string input = args[0];
    /// string output = Process(input);
    /// AnsiConsole.MarkupLineInterpolated($"[blue]{input}[/] -> [green]{output}[/]");
    /// </code>
    /// </example>
    /// <param name="value">The interpolated string value to write.</param>
    public static void MarkupLineInterpolated(FormattableString value)
        => AnsiConsole.MarkupLineInterpolated(value);

    /// <summary>
    /// Writes the specified markup, followed by the current line terminator, to the console.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write.</param>
    public static void MarkupLine(IFormatProvider provider, string format, params object[] args)
        => AnsiConsole.MarkupLine(provider, format, args);

    /// <summary>
    /// Writes the specified markup, followed by the current line terminator, to the console.
    /// <para/>
    /// All interpolation holes which contain a string are automatically escaped so you must not call <see cref="StringExtensions.EscapeMarkup"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// string input = args[0];
    /// string output = Process(input);
    /// AnsiConsole.MarkupLineInterpolated(CultureInfo.InvariantCulture, $"[blue]{input}[/] -> [green]{output}[/]");
    /// </code>
    /// </example>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="value">The interpolated string value to write.</param>
    public static void MarkupLineInterpolated(IFormatProvider provider, FormattableString value)
        => AnsiConsole.MarkupLineInterpolated(provider, value);
}
