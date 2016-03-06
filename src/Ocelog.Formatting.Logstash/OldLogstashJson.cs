using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ocelog.Formatting.Logstash
{
    public static class OldLogstashJson
    {
        public static FormattedLogEvent Format(LogEvent logEvent)
        {
            var jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var json = new JObject();

            foreach (var fields in logEvent.AdditionalFields)
            {
                json.Merge(JObject.FromObject(fields, jsonSerializer));
            }

            var requiredFields = new Dictionary<string, object>()
            {
                { "@timestamp", logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) },
                { "@tags", logEvent.Tags },
                { "@fields", json }
            };

            json.Merge(JObject.FromObject(logEvent.Content, jsonSerializer));

            return new FormattedLogEvent() { Content = JsonConvert.SerializeObject(requiredFields) };
        }
    }
}
