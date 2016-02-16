using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Ocelog.Formatting.Logstash
{
    public static class JsonFormatter
    {
        public static FormattedLogEvent Format(ProcessedLogEvent processedLogEvent)
        {
            var document = processedLogEvent.Content;

            var jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var json = JObject.FromObject(document, jsonSerializer);

            return new FormattedLogEvent() { Content = JsonConvert.SerializeObject(json) };
        }
    }
}
