using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Ocelog.Formatting.Logstash.Test
{
    public class LogstashJsonFormatterTests
    {
        [Fact]
        public void should_add_version()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Val = 1 });

            JObject parsed = GetJObject(output);

            Assert.Equal(1, parsed.Value<int>("@version"));
        }

        [Fact]
        public void should_add_timestamp_in_correct_format()
        {
            string expectedTimestamp = "2015-12-07T18:05:22.352Z";
            var timestamp = DateTime.Parse(expectedTimestamp, CultureInfo.InvariantCulture);

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddTimestamp(timestamp)
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Val = 1 });

            JObject parsed = GetJObject(output);

            Assert.Equal(expectedTimestamp, parsed.Value<string>("@timestamp"));
        }

        [Fact]
        public void should_add_fields_from_message()
        {
            var message = new { Val = 1, Complex = new { Other = "string" } };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            JObject parsed = GetJObject(output);

            Assert.Equal(1, parsed.Value<int>("Val"));
            Assert.Equal("string", parsed.Value<JObject>("Complex").Value<string>("Other"));
        }

        [Fact]
        public void should_add_additional_fields_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { MyField = 34 })
                .AddFields(new { OtherField = "Hello" })
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            JObject parsed = GetJObject(output);

            Assert.Equal(34, parsed.Value<int>("MyField"));
            Assert.Equal("Hello", parsed.Value<string>("OtherField"));
        }

        [Fact]
        public void should_add_tags_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddTag("mytag")
                .AddTag("mytag2")
                .AddTagsToAdditionalFields()
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(message);

            JObject parsed = GetJObject(output);

            Assert.Equal(new[] { "mytag", "mytag2" }, parsed.Value<JArray>("tags").ToObject<string[]>());
        }

        [Fact]
        public void should_add_callerinfo_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddCallerInfoToAdditionalFields()
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Info(message);

            JObject parsed = GetJObject(output);

            Assert.Equal(callerFilePath, parsed.Value<JObject>("CallerInfo").Value<string>("FilePath"));
        }

        [Fact]
        public void should_add_level_to_message()
        {
            var message = new { Val = 1 };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddLevelToAdditionalFields()
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Warn(message);

            JObject parsed = GetJObject(output);

            Assert.Equal("Warn", parsed.Value<string>("Level"));
        }

        [Fact]
        public void should_add_level_to_tags()
        {
            var message = new { Val = 1 };

            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddLevelToTags()
                .AddTagsToAdditionalFields()
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Warn(message);

            JObject parsed = GetJObject(output);

            Assert.Contains("Warn", parsed.Value<JArray>("tags").ToObject<string[]>());
        }

        [Fact]
        public void should_merge_arrays_maintaining_order()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new[] { "A", "B" } })
                .AddFields(new { Things = new[] { "C" } })
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Info(new { Things = new[] { "D" } });

            JObject parsed = GetJObject(output);

            Assert.Equal(new[] { "A", "B", "C", "D" }, parsed.Value<JArray>("Things").ToObject<string[]>());
        }

        [Fact]
        public void should_merge_complex_sub_objects()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new { A = 1, B = 2 } })
                .AddFields(new { Things = new { C = 3 } })
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            logger.Info(new { Things = new { D = 4 } });

            JObject parsed = GetJObject(output);

            Assert.Equal(new Dictionary<string, int> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 } }, parsed.Value<JObject>("Things").ToObject<Dictionary<string, int>>());
        }

        [Fact]
        public void should_merge_dictionaries()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Things = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } } })
                .AddFields(new { Things = new Dictionary<string, int> { { "C", 3 } } })
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Things = new Dictionary<string, int> { { "D", 4 } } });

            JObject parsed = GetJObject(output);

            Assert.Equal(new Dictionary<string, int> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 } }, parsed.Value<JObject>("Things").ToObject<Dictionary<string, int>>());
        }

        [Fact]
        public void should_prefer_message_fields_over_additional_fields()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(new { Thing = "Hello" })
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Thing = "There" });

            JObject parsed = GetJObject(output);

            Assert.Equal("There", parsed.Value<string>("Thing"));
        }

        [Fact]
        public void should_ingore_null_fields()
        {
            var output = new List<FormattedLogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Select(LogstashJson.Process)
                .Select(JsonFormatter.Format)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { Thing = (string)null });

            JObject parsed = GetJObject(output);

            Assert.Null(parsed.GetValue("Thing"));
        }

        private static JObject GetJObject(List<FormattedLogEvent> output)
        {
            var logOutput = output.First().Content;

            var parsed = (JObject)JsonConvert.DeserializeObject(logOutput, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            return parsed;
        }

        private string GetCallerFilePath([CallerFilePath]string callerFilePath = "Shouldn't Get This")
        {
            return callerFilePath;
        }
    }
}
