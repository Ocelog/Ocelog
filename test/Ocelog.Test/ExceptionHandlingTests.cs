using System;
using System.Collections.Generic;
using Xunit;

namespace Ocelog.Test
{
    public class ExceptionHandlingTests
    {
        [Fact]
        public void should_throw_logging_exceptions_into_calling_code_by_default()
        {
            var exception = new Exception();
            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => { throw exception; })
                );

            Assert.Throws<Exception>(() => logger.Info(new { }));
        }

        [Fact]
        public void should_allow_logging_exceptions_to_be_directed_into_another_logger()
        {
            var output = new List<LogEvent>();
            var exception = new Exception();
            var logger = new Logger(logEvents => logEvents
                .HandleLoggingErrors(exceptionLogsEvents => exceptionLogsEvents
                    .Subscribe(log => output.Add(log)))
                .Subscribe(log => { throw exception; })
                );

            logger.Info(new { });

            Assert.Collection(output,
                log => Assert.Equal(exception, log.Content));
            Assert.Collection(output,
                log => Assert.Equal(LogLevel.Error, log.Level));
        }

        [Fact]
        public void should_allow_ignoring_exceptions()
        {
            var exception = new Exception();
            var logger = new Logger(logEvents => logEvents
                .IgnoreLoggingErrors()
                .Subscribe(log => { throw exception; })
                );

            logger.Info(new { });
        }
    }
}
