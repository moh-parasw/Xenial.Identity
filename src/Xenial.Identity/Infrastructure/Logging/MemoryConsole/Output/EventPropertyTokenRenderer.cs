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

internal class EventPropertyTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme theme;
    private readonly PropertyToken token;
    private readonly IFormatProvider? formatProvider;

    public EventPropertyTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        this.theme = theme;
        this.token = token;
        this.formatProvider = formatProvider;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        // If a property is missing, don't render anything (message templates render the raw token here).
        if (!logEvent.Properties.TryGetValue(token.PropertyName, out var propertyValue))
        {
            Padding.Apply(output, string.Empty, token.Alignment);
            return;
        }

        var _ = 0;
        using (theme.Apply(output, ConsoleThemeStyle.SecondaryText, ref _))
        {
            var writer = token.Alignment.HasValue ? new StringWriter() : output;

            // If the value is a scalar string, support some additional formats: 'u' for uppercase
            // and 'w' for lowercase.
            if (propertyValue is ScalarValue sv && sv.Value is string literalString)
            {
                var cased = Casing.Format(literalString, token.Format);
                writer.Write(cased);
            }
            else
            {
                propertyValue.Render(writer, token.Format, formatProvider);
            }

            if (token.Alignment.HasValue)
            {
                var str = writer.ToString()!;
                Padding.Apply(output, str, token.Alignment);
            }
        }
    }
}
