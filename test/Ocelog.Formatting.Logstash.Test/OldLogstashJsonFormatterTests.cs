using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Ocelog.Formatting.Logstash.Test
{
    public class OldLogstashJsonFormatterTests
    {
        [Theory]
        [InlineData("2015-06-07T18:05:22.352Z")]
        [InlineData("2015-12-07T18:05:22.352Z")]
        public void should_add_timestamp_in_correct_format(string expectedTimestamp)
        {
            var timestamp = DateTime.Parse(expectedTimestamp, CultureInfo.InvariantCulture);

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddTimestamp(() => timestamp)
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Val = 1 });

            var parsed = output[0].Content;

            Assert.Equal(expectedTimestamp, parsed["@timestamp"]);
        }

        [Fact]
        public void should_add_fields_from_message()
        {
            var message = new { Val = 1, Complex = new { Other = "string" } };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            var parsed = output[0].Content;

            Assert.Equal(1, GetObject(parsed, "@fields")["Val"]);
            Assert.Equal("string", GetObject(GetObject(parsed, "@fields"), "Complex")["Other"]);
        }

        [Fact]
        public void should_add_additional_fields_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { MyField = 34 })
                .AddFields(new { OtherField = "Hello" })
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            var parsed = output[0].Content;

            Assert.Equal(34, GetObject(parsed, "@fields")["MyField"]);
            Assert.Equal("Hello", GetObject(parsed, "@fields")["OtherField"]);
        }

        [Fact]
        public void should_add_tags_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddTag("mytag")
                .AddTag("mytag2")
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            var parsed = output[0].Content;

            Assert.Equal(new object[] { "mytag", "mytag2" }, GetArray(parsed, "@tags"));
        }

        [Fact]
        public void should_add_callerinfo_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddCallerInfoToAdditionalFields()
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Info(message);

            var parsed = output[0].Content;

            Assert.Equal(callerFilePath, GetObject(GetObject(parsed, "@fields"), "CallerInfo")["FilePath"]);
        }

        [Fact]
        public void should_add_level_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddLevelToAdditionalFields()
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Warn(message);

            var parsed = output[0].Content;

            Assert.Equal(LogLevel.Warn, GetObject(parsed, "@fields")["Level"]);
        }

        [Fact]
        public void should_add_level_to_tags()
        {
            var message = new { Val = 1 };

            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddLevelToTags()
                .AddTagsToAdditionalFields()
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Warn(message);

            var parsed = output[0].Content;

            Assert.Contains("Warn", GetArray(parsed, "@tags"));
        }

        [Fact]
        public void should_merge_arrays_maintaining_order()
        {
            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new[] { "A", "B" } })
                .AddFields(new { Things = new[] { "C" } })
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Things = new[] { "D" } });

            var parsed = output[0].Content;

            Assert.Equal(new object[] { "A", "B", "C", "D" }, GetArray(GetObject(parsed, "@fields"), "Things"));
        }

        [Fact]
        public void should_merge_complex_sub_objects()
        {
            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new { A = 1, B = 2 } })
                .AddFields(new { Things = new { C = 3 } })
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Things = new { D = 4 } });

            var parsed = output[0].Content;

            Assert.Equal(new Dictionary<string, object> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 } }, GetObject(GetObject(parsed, "@fields"), "Things"));
        }

        [Fact]
        public void should_merge_dictionaries()
        {
            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } } })
                .AddFields(new { Things = new Dictionary<string, int> { { "C", 3 } } })
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Things = new Dictionary<string, int> { { "D", 4 } } });

            var parsed = output[0].Content;

            Assert.Equal(new Dictionary<string, object> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 } }, GetObject(GetObject(parsed, "@fields"), "Things"));
        }

        [Fact]
        public void should_prefer_message_fields_over_additional_fields()
        {
            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Thing = "Hello" })
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Thing = "There" });

            var parsed = output[0].Content;

            Assert.Equal("There", GetObject(parsed, "@fields")["Thing"]);
        }

        [Fact]
        public void should_ignore_null_fields()
        {
            var output = new List<ProcessedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Select(OldLogstashJson.Process)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Thing = (string)null });

            var parsed = output[0].Content;

            Assert.True(!GetObject(parsed, "@fields").ContainsKey("Thing"));
        }

        private Dictionary<string, object> GetObject(Dictionary<string, object> parsed, string param1)
        {
            return (Dictionary<string, object>)parsed[param1];
        }

        private object[] GetArray(Dictionary<string, object> parsed, string v)
        {
            return ((IEnumerable<object>)parsed[v]).ToArray();
        }

        private string GetCallerFilePath([CallerFilePath]string callerFilePath = "Shouldn't Get This")
        {
            return callerFilePath;
        }
    }
}
