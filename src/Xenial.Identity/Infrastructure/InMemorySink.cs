using Serilog.Core;
using Serilog.Events;
using Serilog;
using System.Collections.Concurrent;
using System.Collections;

#nullable enable

namespace Xenial.Identity.Infrastructure;

public sealed class InMemorySink : ILogEventSink, IEnumerable<LogEvent>
{
    private class FixedSizedQueue<T> : IEnumerable<T>
    {
        private readonly ConcurrentQueue<T> q = new();
        private readonly object lockObject = new();

        private int limit = 1000;
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

    private readonly FixedSizedQueue<LogEvent> queue = new FixedSizedQueue<LogEvent>() { Limit = 1000 };

    public event EventHandler? Emitted;

    public void Emit(LogEvent logEvent)
    {
        queue.Enqueue(logEvent);
        Emitted?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<LogEvent> GetEnumerator()
        => ((IEnumerable<LogEvent>)queue).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)queue).GetEnumerator();
}
