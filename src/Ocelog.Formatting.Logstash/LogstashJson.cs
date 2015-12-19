using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ocelog.Formatting.Logstash
{
    public static class LogstashJson
    {
        public static FormattedLogEvent Format(LogEvent logEvent)
        {
            var requiredFields = new Dictionary<string, object>()
            {
                { "@version", 1 },
                { "@timestamp", logEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) }
            };

            var jsonSerializer = new JsonSerializer() { };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var json = JObject.FromObject(requiredFields, jsonSerializer);

            foreach (var fields in logEvent.AdditionalFields)
            {
                json.Merge(JObject.FromObject(fields, jsonSerializer));
            }

            json.Merge(JObject.FromObject(logEvent.Content, jsonSerializer));

            return new FormattedLogEvent() { Content = JsonConvert.SerializeObject(json) };
        }
    }
}
