using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reactive.Linq;

namespace Ocelog.Test
{
    public class FilteringTests
    {
        [Fact]
        public void should_allow_filtering_of_events_being_logged()
        {
            var output = new List<LogEvent>();
            var exception = new Exception();
            var logger = new Logger(logEvents => logEvents
                .Where(log => log.Level == LogLevel.Warn)
                .Subscribe(log => output.Add(log))
                );

            logger.Info(new { });
            logger.Warn(new { });
            logger.Error(new { });

            Assert.Collection(output,
                log => Assert.Equal(LogLevel.Warn, log.Level));
        }

        [Fact]
        public void should_allow_filtering_of_events_being_logged_into_different_outputs()
        {
            var warnOutput = new List<LogEvent>();
            var errorOutput = new List<LogEvent>();
            var allOutput = new List<LogEvent>();
            var exception = new Exception();
            var logger = new Logger(logEvents =>
            {
                logEvents
                    .Where(log => log.Level == LogLevel.Warn)
                    .Subscribe(log => warnOutput.Add(log));

                logEvents
                    .Where(log => log.Level == LogLevel.Error)
                    .Subscribe(log => errorOutput.Add(log));

                logEvents
                    .Subscribe(log => allOutput.Add(log));
            });

            logger.Info(new { });
            logger.Warn(new { });
            logger.Error(new { });

            Assert.Collection(warnOutput,
                log => Assert.Equal(LogLevel.Warn, log.Level));

            Assert.Collection(errorOutput,
                log => Assert.Equal(LogLevel.Error, log.Level));

            Assert.Collection(allOutput,
                log => Assert.Equal(LogLevel.Info, log.Level),
                log => Assert.Equal(LogLevel.Warn, log.Level),
                log => Assert.Equal(LogLevel.Error, log.Level));
        }
    }
}
