﻿using System.Globalization;

using Spectre.Console;

using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace Xenial.Identity.Infrastructure;

public class ConsoleHelper
{
    public static void PrintSource(string code, string lang = "cs")
    {
        using var sr = new StringReader(code);
        PrintSource(sr, lang);
    }

    private static Lazy<(IGrammar grammar, Theme theme)> TextMateContextCSharp { get; } = new Lazy<(IGrammar grammer, Theme theme)>(() =>
    {
        var options = new RegistryOptions(ThemeName.DarkPlus);

        var registry = new Registry(options);

        var theme = registry.GetTheme();

        var grammar = registry.LoadGrammar(options.GetScopeByExtension(".cs"));
        return (grammar, theme);
    });

    private static Lazy<(IGrammar grammar, Theme theme)> TextMateContextSql { get; } = new Lazy<(IGrammar grammer, Theme theme)>(() =>
    {
        var options = new RegistryOptions(ThemeName.DarkPlus);

        var registry = new Registry(options);

        var theme = registry.GetTheme();

        //This throws NRE in Textmate 1.0.41. So use the scope name direct
        //var scope = options.GetScopeByExtension(".sql");
        //var grammar = registry.LoadGrammar(scope);
        var grammar = registry.LoadGrammar("source.sql");
        return (grammar, theme);
    });

    private static Lazy<(IGrammar grammar, Theme theme)> TextMateContextXml { get; } = new Lazy<(IGrammar grammer, Theme theme)>(() =>
    {
        var options = new RegistryOptions(ThemeName.DarkPlus);

        var registry = new Registry(options);

        var theme = registry.GetTheme();

        //This throws NRE in Textmate 1.0.41. So use the scope name direct
        //var scope = options.GetScopeByExtension(".xml");
        //var grammar = registry.LoadGrammar(scope);
        var grammar = registry.LoadGrammar("text.xml");
        return (grammar, theme);
    });

    private static void PrintSource(StringReader sr, string lang = "cs")
    {
        var (grammar, theme) = lang switch
        {
            "cs" => TextMateContextCSharp.Value,
            "xml" => TextMateContextXml.Value,
            "sql" => TextMateContextSql.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, $"Could not find grammer for language '{lang}'.")
        };

        IStateStack ruleStack = null;

        var line = sr.ReadLine();

        while (line is not null)
        {
            var l = line;
            var result = grammar.TokenizeLine(l, ruleStack, TimeSpan.MaxValue);

            ruleStack = result.RuleStack;

            foreach (var token in result.Tokens)
            {
                var startIndex = token.StartIndex > line.Length ?
                    line.Length : token.StartIndex;
                var endIndex = token.EndIndex > line.Length ?
                    line.Length : token.EndIndex;

                var foreground = -1;
                var background = -1;
                var fontStyle = -1;

                foreach (var themeRule in theme.Match(token.Scopes))
                {
                    if (foreground == -1 && themeRule.foreground > 0)
                    {
                        foreground = themeRule.foreground;
                    }

                    if (background == -1 && themeRule.background > 0)
                    {
                        background = themeRule.background;
                    }

                    if (fontStyle == -1 && themeRule.fontStyle > 0)
                    {
                        fontStyle = themeRule.fontStyle;
                    }
                }

                WriteToken(line.SubstringAtIndexes(startIndex, endIndex), foreground, background, fontStyle, theme);
            }

            AnsiConsole.WriteLine();
            line = sr.ReadLine();
        }
    }

    private static void WriteToken(string text, int foreground, int background, int fontStyle, Theme theme)
    {
        if (foreground == -1)
        {
#pragma warning disable Spectre1000 // Use AnsiConsole instead of System.Console
            Console.Write(text);
#pragma warning restore Spectre1000
            return;
        }

        var decoration = GetDecoration(fontStyle);

        var backgroundColor = GetColor(background, theme);
        var foregroundColor = GetColor(foreground, theme);

        var style = new Style(foregroundColor, backgroundColor, decoration);
        var markup = new Markup(text
            .Replace("[", "[[", StringComparison.OrdinalIgnoreCase)
            .Replace("]", "]]", StringComparison.OrdinalIgnoreCase),
            style
        );

        AnsiConsole.Write(markup);
    }

    private static Color GetColor(int colorId, Theme theme) => colorId == -1 ? Color.Default : HexToColor(theme.GetColor(colorId));

    private static Decoration GetDecoration(int fontStyle)
    {
        var result = Decoration.None;

        if (fontStyle == FontStyle.NotSet)
        {
            return result;
        }

        if ((fontStyle & FontStyle.Italic) != 0)
        {
            result |= Decoration.Italic;
        }

        if ((fontStyle & FontStyle.Underline) != 0)
        {
            result |= Decoration.Underline;
        }

        if ((fontStyle & FontStyle.Bold) != 0)
        {
            result |= Decoration.Bold;
        }

        return result;
    }

    private static Color HexToColor(string hexString)
    {
        //replace # occurences
        if (hexString.IndexOf('#', StringComparison.OrdinalIgnoreCase) != -1)
        {
            hexString = hexString.Replace("#", "", StringComparison.OrdinalIgnoreCase);
        }

#pragma warning disable CA1305 // Specify IFormatProvider
        var r = byte.Parse(hexString[..2], NumberStyles.AllowHexSpecifier);
        var g = byte.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
        var b = byte.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
#pragma warning restore CA1305 // Specify IFormatProvider

        return new Color(r, g, b);
    }
}
