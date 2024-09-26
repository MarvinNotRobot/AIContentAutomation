using Microsoft.Extensions.Logging;

namespace TestHelper
{
    public class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
                Message = formatter(state, exception)
            });
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public object State { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }

}