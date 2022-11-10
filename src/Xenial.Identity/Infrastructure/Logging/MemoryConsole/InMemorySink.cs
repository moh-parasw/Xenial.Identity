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

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;

namespace Xenial.Identity.Infrastructure.Logging.MemoryConsole;

#nullable enable

public class InMemorySink : IEnumerable<string>, ILogEventSink
{
    private readonly ConsoleTheme theme;
    private readonly ITextFormatter formatter;
    private readonly object syncRoot;
    private const int defaultWriteBufferCapacity = 256;

    public InMemorySink(
        ConsoleTheme theme,
        ITextFormatter formatter,
        object syncRoot)
    {
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        this.formatter = formatter;
        this.syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
    }

    [DebuggerStepThrough]
    private TextWriter SelectOutputStream()
        => CreateOutputWriter(stream);

    private static readonly MemoryStream stream = new();

    [DebuggerStepThrough]
    private static TextWriter CreateOutputWriter(Stream outputStream) => TextWriter.Synchronized(outputStream == Stream.Null ?
            StreamWriter.Null :
            new StreamWriter(
                stream: outputStream,
                leaveOpen: true)
            {
                AutoFlush = true
            });

    private readonly FixedSizedQueue<string> queue = new() { Limit = 250 };

    public event EventHandler<InMemoryLogEventArgs>? Emitted;

    [DebuggerStepThrough]
    public void Emit(LogEvent logEvent)
    {
        var output = SelectOutputStream();

        // ANSI escape codes can be pre-rendered into a buffer; however, if we're on Windows and
        // using its console coloring APIs, the color switches would happen during the off-screen
        // buffered write here and have no effect when the line is actually written out.
        if (theme.CanBuffer)
        {
            var buffer = new StringWriter(new StringBuilder(defaultWriteBufferCapacity));
            formatter.Format(logEvent, buffer);
            var formattedLogEventText = buffer.ToString();
            lock (syncRoot)
            {
                output.Write(formattedLogEventText);
                queue.Enqueue(formattedLogEventText);
                Emitted?.Invoke(this, new InMemoryLogEventArgs(formattedLogEventText));
                output.Flush();
            }
        }
        else
        {
            lock (syncRoot)
            {
                formatter.Format(logEvent, output);
                var sr = new StreamReader(stream);
                var myStr = sr.ReadToEnd();
                queue.Enqueue(myStr);
                Emitted?.Invoke(this, new InMemoryLogEventArgs(myStr));
                output.Flush();
            }
        }
    }

    public IEnumerator<string> GetEnumerator()
        => ((IEnumerable<string>)queue).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)queue).GetEnumerator();

    private class FixedSizedQueue<T> : IEnumerable<T>
    {
        private readonly ConcurrentQueue<T> q = new();
        private readonly object lockObject = new();

        private int limit = 250;
        public int Limit
        {
            get => limit; set
            {
                lock (lockObject)
                {
                    limit = value;
                }
            }
        }

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                while (q.Count > Limit && q.TryDequeue(out _))
                {
                }
            }
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)q).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)q).GetEnumerator();
    }
}

public class InMemoryLogEventArgs : EventArgs
{
    public string Log { get; }

    public InMemoryLogEventArgs(string log)
        => Log = log;
}
