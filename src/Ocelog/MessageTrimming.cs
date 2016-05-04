using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelog
{
    public static class MessageTrimming
    {
        public static Func<ProcessedLogEvent, ProcessedLogEvent> TrimFields(int maxFieldLength)
        {
            return (logEvent) => new ProcessedLogEvent() { Content = TrimFields(logEvent.Content, maxFieldLength) };
        }

        private static Dictionary<string, object> TrimFields(Dictionary<string, object> content, int maxFieldLength)
        {
            return content.ToDictionary(pair => pair.Key, pair => TrimFields(pair.Value, maxFieldLength));
        }

        private static object TrimFields(object value, int maxFieldLength)
        {
            var text = value as string;

            if (text != null)
                return text.Substring(0, Math.Min(maxFieldLength, text.Length));

            if (value is Dictionary<string, object>)
                return TrimFields((Dictionary<string, object>)value, maxFieldLength);

            var list = value as IEnumerable<object>;
            if (list != null)
                return list.Select(item => TrimFields(item, maxFieldLength));

            return value;
        }
    }
}
