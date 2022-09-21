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

internal class TimestampTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme theme;
    private readonly PropertyToken token;
    private readonly IFormatProvider? formatProvider;

    public TimestampTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        this.theme = theme;
        this.token = token;
        this.formatProvider = formatProvider;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        // We need access to ScalarValue.Render() to avoid this alloc; just ensures
        // that custom format providers are supported properly.
        var sv = new ScalarValue(logEvent.Timestamp);

        var _ = 0;
        using (theme.Apply(output, ConsoleThemeStyle.SecondaryText, ref _))
        {
            if (token.Alignment is null)
            {
                sv.Render(output, token.Format, formatProvider);
            }
            else
            {
                var buffer = new StringWriter();
                sv.Render(buffer, token.Format, formatProvider);
                var str = buffer.ToString();
                Padding.Apply(output, str, token.Alignment);
            }
        }
    }
}
