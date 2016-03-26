using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ocelog.Test
{
    public class TimestampTests
    {
        [Fact]
        public void should_set_timestamp_to_specified_with_obsolete_overload()
        {
            var output = new List<LogEvent>();

            var now = DateTime.Now;

#pragma warning disable 618
            var logger = new Logger(logEvents => logEvents
                .AddTimestamp(now)
                .Subscribe(log => output.Add(log))
                );
#pragma warning restore 618

            var obj = new { message = "Hi" };
            logger.Info(obj);

            Assert.Collection(output,
                log => Assert.Equal(now, log.Timestamp));
        }

        [Fact]
        public void should_set_timestamp_to_specified()
        {
            var output = new List<LogEvent>();

            var now = DateTime.Now;

            var logger = new Logger(logEvents => logEvents
                .AddTimestamp(() => now)
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);

            Assert.Collection(output,
                log => Assert.Equal(now, log.Timestamp));
        }

        [Fact]
        public void should_set_timestamp_to_now_if_not_specified()
        {
            var output = new List<LogEvent>();

            var now = DateTime.Now;

            var logger = new Logger(logEvents => logEvents
                .AddTimestamp()
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);

            Assert.Collection(output,
                log => Assert.True(log.Timestamp >= now));
        }
    }
}
