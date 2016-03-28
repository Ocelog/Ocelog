using System;
using System.Linq;

namespace Ocelog
{
    public static class BasicFormatting
    {
        public static ProcessedLogEvent Process(LogEvent logEvent)
        {
            var allFields = logEvent.AdditionalFields
                .Concat(new object[] { logEvent.Content });

            return ProcessedLogEvent.Process(ObjectMerging.Flatten(allFields), logEvent);
        }
    }
}
