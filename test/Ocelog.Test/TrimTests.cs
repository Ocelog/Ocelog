using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Reactive.Linq;

namespace Ocelog.Test
{
    public class TrimTests
    {
        [Theory]
        [InlineData("A very long message that should be trimmed", 6, "A very")]
        [InlineData("A very long message that should be trimmed", 600, "A very long message that should be trimmed")]
        [InlineData("1234567890", 10, "1234567890")]
        public void should_trim_fields_that_are_too_long(string originalMessage, int trimLength, string trimmedMessage)
        {
            var obj = new { Message = originalMessage };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(MessageTrimming.TrimFields(trimLength))
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var message = output[0].Content["Message"];

            Assert.Equal(trimmedMessage, message);
        }

        [Fact]
        public void should_trim_fields_that_are_too_long_at_any_depth()
        {
            var obj = new { Thing = new { Message = "A very long message that should be trimmed" } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(MessageTrimming.TrimFields(6))
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var message = ((Dictionary<string, object>)output[0].Content["Thing"])["Message"];

            Assert.Equal("A very", message);
        }

        [Fact]
        public void should_leave_enums_ints_and_doubles_unaffected()
        {
            var obj = new { Message = "A very long message that should be trimmed", Number = 123456, Enumer = LogLevel.Error, Doub = 1234.56789 };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(MessageTrimming.TrimFields(2))
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var number = output[0].Content["Number"];
            var enumer = output[0].Content["Enumer"];
            var doub = output[0].Content["Doub"];

            Assert.Equal(123456, number);
            Assert.Equal(LogLevel.Error, enumer);
            Assert.Equal(1234.56789, doub);
        }
    }
}

