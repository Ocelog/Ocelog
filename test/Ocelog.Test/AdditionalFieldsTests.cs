using System;
using System.Collections.Generic;
using Xunit;

namespace Ocelog.Test
{
    public class AdditionalFieldsTests
    {
        [Fact]
        public void should_have_empty_list_of_fields()
        {
            var logEvent = new LogEvent();

            Assert.Empty(logEvent.AdditionalFields);
        }

        [Fact]
        public void should_add_additional_fields()
        {
            var output = new List<LogEvent>();

            var additional = new { MyField = 2 };

            var logger = new Logger(logEvents => logEvents
                .AddFields(additional)
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);

            Assert.Collection(output,
                log => Assert.All(log.AdditionalFields, field => Assert.Equal(additional, field)));
        }
    }
}
