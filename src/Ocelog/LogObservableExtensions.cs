using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Ocelog
{
    public static class LogObservableExtensions
    {
        public static IObservable<LogEvent> AddTimestamp(this IObservable<LogEvent> logEvents, DateTime? now = null)
        {
            return logEvents.Do(log => log.Timestamp = now ?? DateTime.Now);
        }

        public static IObservable<LogEvent> AddFields(this IObservable<LogEvent> logEvents, object additionalFields)
        {
            return logEvents.Do(log => log.AddField(additionalFields));
        }

        public static IObservable<LogEvent> AddTag(this IObservable<LogEvent> logEvents, string tag)
        {
            return logEvents.Do(log => log.AddTag(tag));
        }

        public static IObservable<LogEvent> HandleLoggingErrors(this IObservable<LogEvent> logEvents, Action<IObservable<LogEvent>> loggingPipelineFactory)
        {
            var wrappedLogEvents = new Subject<LogEvent>();
            logEvents.Subscribe(new ErrorHandlingObserver(wrappedLogEvents, loggingPipelineFactory));

            return wrappedLogEvents;
        }

        public static IObservable<LogEvent> IgnoreLoggingErrors(this IObservable<LogEvent> logEvents)
        {
            var wrappedLogEvents = new Subject<LogEvent>();
            logEvents.Subscribe(new ErrorHandlingObserver(wrappedLogEvents, _ => { }));

            return wrappedLogEvents;
        }
    }
}
