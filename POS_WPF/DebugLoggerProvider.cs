using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

public class DebugLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new DebugLogger();

    public void Dispose() { }

    private class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
                                TState state, Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            Debug.WriteLine(formatter(state, exception));
        }
    }
}
