using System;
using System.Collections.Generic;

namespace Ocelog
{
    public class ProcessedLogEvent : ILogEventContext
    {
        private ProcessedLogEvent(Dictionary<string, object> content, LogLevel level, DateTime timestamp, IEnumerable<string> tags)
        {
            Level = level;
            Timestamp = timestamp;
            Tags = tags;
            Content = content;
        }

        public static ProcessedLogEvent Process(Dictionary<string, object> content, ILogEventContext logEvent)
        {
            return new ProcessedLogEvent(content, logEvent.Level, logEvent.Timestamp, logEvent.Tags);
        }

        public LogLevel Level { get; }
        public DateTime Timestamp { get; }
        public IEnumerable<string> Tags { get; }

        public Dictionary<string, object> Content { get; }
    }
}
