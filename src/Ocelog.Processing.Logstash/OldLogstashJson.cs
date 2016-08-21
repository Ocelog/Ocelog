﻿using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Ocelog.Processing.Logstash
{
    public static class OldLogstashJson
    {
        public static ProcessedLogEvent Process(LogEvent logEvent)
        {
            var allFields = logEvent.AdditionalFields
                .Concat(new object[] { logEvent.Content });

            var requiredFields = new Dictionary<string, object>()
            {
                { "@timestamp", logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) },
                { "@tags", logEvent.Tags },
                { "@fields", ObjectMerging.Flatten(allFields) }
            };

            return ProcessedLogEvent.Process(requiredFields, logEvent);
        }
    }
}
