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


#nullable enable

namespace Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;

internal struct StyleReset : IDisposable
{
    private readonly ConsoleTheme theme;
    private readonly TextWriter output;

    public StyleReset(ConsoleTheme theme, TextWriter output)
    {
        this.theme = theme;
        this.output = output;
    }

    public void Dispose() => theme.Reset(output);
}
