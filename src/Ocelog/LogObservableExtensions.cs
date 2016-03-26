using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Ocelog
{
    public static class LogObservableExtensions
    {
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

        public static IObservable<ProcessedLogEvent> Process(this IObservable<LogEvent> logEvents, Func<LogEvent, ProcessedLogEvent> func)
        {
            return logEvents.Select(func);
        }

        public static IObservable<FormattedLogEvent> Format(this IObservable<ProcessedLogEvent> logEvents, Func<ProcessedLogEvent, FormattedLogEvent> func)
        {
            return logEvents.Select(func);
        }
    }
}
