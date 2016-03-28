using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Ocelog.Formatting.Elasticsearch
{
    public static class ElasticsearchBulkFormatter
    {
        public static Func<IList<ProcessedLogEvent>, FormattedLogEvent> Format(Func<ProcessedLogEvent, string> getIndexName, Func<ProcessedLogEvent, string> getTypeName)
        {
            return events => FormatEvents(events, getIndexName, getTypeName);
        }

        public static FormattedLogEvent FormatEvents(IList<ProcessedLogEvent> events, Func<ProcessedLogEvent, string> getIndexName, Func<ProcessedLogEvent, string> getTypeName)
        {
            var lines = events.SelectMany(logEvent => FormatEvent(logEvent, getIndexName, getTypeName));
            var content = string.Concat(lines);
            return FormattedLogEvent.Format(content, events.First());
        }

        private static IEnumerable<string> FormatEvent(ProcessedLogEvent logEvent, Func<ProcessedLogEvent, string> getIndexName, Func<ProcessedLogEvent, string> getTypeName)
        {
            var document = logEvent.Content;

            var jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var json = JObject.FromObject(document, jsonSerializer);

            var doc = JsonConvert.SerializeObject(json);

            var action = JsonConvert.SerializeObject(JObject.FromObject(new { index = new { _index = getIndexName(logEvent), _type = getTypeName(logEvent) } }, jsonSerializer));

            return new[] { action, "\n", doc, "\n" };
        }
    }
}
