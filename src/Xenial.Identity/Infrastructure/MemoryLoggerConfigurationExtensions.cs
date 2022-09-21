using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;
using Serilog;
using Serilog.Formatting;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Output;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole;

#nullable enable

namespace Xenial.Identity.Infrastructure;

/// <summary>
/// Adds the WriteTo.Memory() extension method to <see cref="LoggerConfiguration"/>.
/// </summary>
public static class MemoryLoggerConfigurationExtensions
{
    private static readonly object defaultSyncRoot = new object();
    private const string defaultConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Writes log events to Memory.
    /// </summary>
    /// <param name="sinkConfiguration">Logger sink configuration.</param>
    /// <param name="restrictedToMinimumLevel">The minimum level for
    /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.
    /// The default is <code>"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"</code>.</param>
    /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the console output. If you specify this, you
    /// will have the ability to lock on this object, and guarantee that the console sink will not be about to output anything while
    /// the lock is held.</param>
    /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
    /// <param name="levelSwitch">A switch allowing the pass-through minimum level
    /// to be changed at runtime.</param>
    /// <param name="standardErrorFromLevel">Specifies the level at which events will be written to standard error.</param>
    /// <param name="theme">The theme to apply to the styled output. If not specified,
    /// uses <see cref="SystemConsoleTheme.Literate"/>.</param>
    /// <param name="applyThemeToRedirectedOutput">Applies the selected or default theme even when output redirection is detected.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="sinkConfiguration"/> is <code>null</code></exception>
    /// <exception cref="ArgumentNullException">When <paramref name="outputTemplate"/> is <code>null</code></exception>
    public static LoggerConfiguration Memory(
        this LoggerSinkConfiguration sinkConfiguration,
        out InMemorySink inMemorySink,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = defaultConsoleOutputTemplate,
        IFormatProvider? formatProvider = null,
        LoggingLevelSwitch? levelSwitch = null,
        Logging.MemoryConsole.Themes.ConsoleTheme? theme = null,
        bool applyThemeToRedirectedOutput = false,
        object? syncRoot = null)
    {
        _ = sinkConfiguration ?? throw new ArgumentNullException(nameof(sinkConfiguration));
        _ = outputTemplate ?? throw new ArgumentNullException(nameof(outputTemplate));

        var appliedTheme = !applyThemeToRedirectedOutput && (System.Console.IsOutputRedirected || System.Console.IsErrorRedirected) ?
            Logging.MemoryConsole.Themes.ConsoleTheme.None :
            theme ?? SystemConsoleThemes.Literate;

        syncRoot ??= defaultSyncRoot;

        var formatter = new OutputTemplateRenderer(appliedTheme, outputTemplate, formatProvider);

        inMemorySink = new Logging.MemoryConsole.InMemorySink(appliedTheme, formatter, syncRoot);

        return sinkConfiguration.Sink(inMemorySink, restrictedToMinimumLevel, levelSwitch);
    }

    /// <summary>
    /// Writes log events to memory.
    /// </summary>
    /// <param name="sinkConfiguration">Logger sink configuration.</param>
    /// <param name="formatter">Controls the rendering of log events into text, for example to log JSON. To
    /// control plain text formatting, use the overload that accepts an output template.</param>
    /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the console output. If you specify this, you
    /// will have the ability to lock on this object, and guarantee that the console sink will not be about to output anything while
    /// the lock is held.</param>
    /// <param name="restrictedToMinimumLevel">The minimum level for
    /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
    /// <param name="levelSwitch">A switch allowing the pass-through minimum level
    /// to be changed at runtime.</param>
    /// <param name="standardErrorFromLevel">Specifies the level at which events will be written to standard error.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="sinkConfiguration"/> is <code>null</code></exception>
    /// <exception cref="ArgumentNullException">When <paramref name="formatter"/> is <code>null</code></exception>
    public static LoggerConfiguration Memory(
        this LoggerSinkConfiguration sinkConfiguration,
        out InMemorySink inMemorySink,
        ITextFormatter formatter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch? levelSwitch = null,
        object? syncRoot = null)
    {
        _ = sinkConfiguration ?? throw new ArgumentNullException(nameof(sinkConfiguration));
        _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

        syncRoot ??= defaultSyncRoot;
        inMemorySink = new InMemorySink(Logging.MemoryConsole.Themes.ConsoleTheme.None, formatter, syncRoot);
        return sinkConfiguration.Sink(inMemorySink, restrictedToMinimumLevel, levelSwitch);
    }
}
