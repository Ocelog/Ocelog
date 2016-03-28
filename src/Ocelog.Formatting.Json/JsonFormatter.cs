using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Ocelog.Formatting.Json
{
    public static class JsonFormatter
    {
        public static FormattedLogEvent Format(ProcessedLogEvent logEvent)
        {
            var document = logEvent.Content;

            var jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var json = JObject.FromObject(document, jsonSerializer);

            return FormattedLogEvent.Format(JsonConvert.SerializeObject(json), logEvent);
        }
    }
}
