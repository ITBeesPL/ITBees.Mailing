using System;
using Microsoft.Extensions.Logging;

namespace ITBees.Mailing.UnitTests.InMemory
{
    public class InMemmoryLogger<T> : ILogger<EmailSendingService>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(exception);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return  true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}