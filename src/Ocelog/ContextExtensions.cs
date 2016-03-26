using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ocelog
{
    public static class ContextExtensions
    {
        [Obsolete("You probably didn't mean to use this, use override with function instead")]
        public static IObservable<LogEvent> AddTimestamp(this IObservable<LogEvent> logEvents, DateTime now)
        {
            return logEvents.AddTimestamp(() => now);
        }

        public static IObservable<LogEvent> AddTimestamp(this IObservable<LogEvent> logEvents, Func<DateTime> getCurrentTime = null)
        {
            return logEvents.Do(log => log.Timestamp = getCurrentTime == null ? DateTime.Now : getCurrentTime());
        }

        public static IObservable<LogEvent> AddFields(this IObservable<LogEvent> logEvents, object additionalFields)
        {
            return logEvents.Do(log => log.AddField(additionalFields));
        }

        public static IObservable<LogEvent> AddTag(this IObservable<LogEvent> logEvents, string tag)
        {
            return logEvents.Do(log => log.AddTag(tag));
        }

        public static IObservable<LogEvent> AddTagsToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { tags = logEvent.Tags }));
        }

        public static IObservable<LogEvent> AddCallerInfoToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { CallerInfo = logEvent.CallerInfo }));
        }

        public static IObservable<LogEvent> AddLevelToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { Level = logEvent.Level }));
        }

        public static IObservable<LogEvent> AddLevelToTags(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddTag(logEvent.Level.ToString()));
        }
    }
}
