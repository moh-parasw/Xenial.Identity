﻿using System;

using Microsoft.Extensions.Logging;

namespace Xenial.Identity.Xpo.Storage.Tests
{
    public class FakeLogger<T> : FakeLogger, ILogger<T>
    {
        public static ILogger<T> Create()
            => new FakeLogger<T>();
    }

    public class FakeLogger : ILogger, IDisposable
    {
        public IDisposable BeginScope<TState>(TState state)
            => this;

        public void Dispose() { }

        public bool IsEnabled(LogLevel logLevel)
            => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
