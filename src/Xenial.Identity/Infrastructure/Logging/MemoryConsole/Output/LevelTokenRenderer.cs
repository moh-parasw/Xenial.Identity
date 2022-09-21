// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Events;
using Serilog.Parsing;

using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Rendering;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;

#nullable enable

namespace Xenial.Identity.Infrastructure.Logging.MemoryConsole.Output;

internal class LevelTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme theme;
    private readonly PropertyToken levelToken;
    private static readonly Dictionary<LogEventLevel, ConsoleThemeStyle> levels = new()
    {
        { LogEventLevel.Verbose, ConsoleThemeStyle.LevelVerbose },
        { LogEventLevel.Debug, ConsoleThemeStyle.LevelDebug },
        { LogEventLevel.Information, ConsoleThemeStyle.LevelInformation },
        { LogEventLevel.Warning, ConsoleThemeStyle.LevelWarning },
        { LogEventLevel.Error, ConsoleThemeStyle.LevelError },
        { LogEventLevel.Fatal, ConsoleThemeStyle.LevelFatal },
    };

    public LevelTokenRenderer(ConsoleTheme theme, PropertyToken levelToken)
    {
        this.theme = theme;
        this.levelToken = levelToken;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, levelToken.Format);
        if (!levels.TryGetValue(logEvent.Level, out var levelStyle))
        {
            levelStyle = ConsoleThemeStyle.Invalid;
        }

        var _ = 0;
        using (theme.Apply(output, levelStyle, ref _))
        {
            Padding.Apply(output, moniker, levelToken.Alignment);
        }
    }
}
