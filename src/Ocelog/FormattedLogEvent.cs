using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelog
{
    public class FormattedLogEvent : ILogEventContext
    {
        private FormattedLogEvent(string content, LogLevel level, DateTime timestamp, IEnumerable<string> tags)
        {
            Level = level;
            Timestamp = timestamp;
            Tags = tags;
            Content = content;
        }

        public static FormattedLogEvent Format(string content, ILogEventContext logEvent)
        {
            return new FormattedLogEvent(content, logEvent.Level, logEvent.Timestamp, logEvent.Tags);
        }

        public LogLevel Level { get; }
        public DateTime Timestamp { get; }
        public IEnumerable<string> Tags { get; }

        public string Content { get; }


    }
}
