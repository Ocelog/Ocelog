using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ocelog
{
    public static class LogObservableExtentions
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
    }
}
