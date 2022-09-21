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
using Serilog.Sinks.SystemConsole.Rendering;

using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Formatting;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Rendering;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;

#nullable enable

namespace Xenial.Identity.Infrastructure.Logging.MemoryConsole.Output;

internal class MessageTemplateOutputTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme theme;
    private readonly PropertyToken token;
    private readonly ThemedMessageTemplateRenderer renderer;

    public MessageTemplateOutputTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        this.token = token ?? throw new ArgumentNullException(nameof(token));

        bool isLiteral = false, isJson = false;

        if (token.Format != null)
        {
            for (var i = 0; i < token.Format.Length; ++i)
            {
                if (token.Format[i] == 'l')
                {
                    isLiteral = true;
                }
                else if (token.Format[i] == 'j')
                {
                    isJson = true;
                }
            }
        }

        var valueFormatter = isJson
            ? (ThemedValueFormatter)new ThemedJsonValueFormatter(theme, formatProvider)
            : new ThemedDisplayValueFormatter(theme, formatProvider);

        renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        if (token.Alignment is null || !theme.CanBuffer)
        {
            _ = renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
            return;
        }

        var buffer = new StringWriter();
        var invisible = renderer.Render(logEvent.MessageTemplate, logEvent.Properties, buffer);
        var value = buffer.ToString();
        Padding.Apply(output, value, token.Alignment.Value.Widen(invisible));
    }
}
