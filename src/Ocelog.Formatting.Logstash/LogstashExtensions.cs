using System;
using System.Reactive.Linq;

namespace Ocelog.Formatting.Logstash
{
    public static class LogstashExtensions
    {
        public static IObservable<LogEvent> AddTagsToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { tags = logEvent.Tags }));
        }

        public static IObservable<LogEvent> AddCallerInfoToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { logEvent.CallerInfo }));
        }

        public static IObservable<LogEvent> AddLevelToAdditionalFields(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddField(new { logEvent.Level }));
        }

        public static IObservable<LogEvent> AddLevelToTags(this IObservable<LogEvent> logEvents)
        {
            return logEvents.Do(logEvent => logEvent.AddTag(logEvent.Level.ToString()));
        }
    }
}
