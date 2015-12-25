using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Ocelog.Test
{
    public class LoggerTests
    {
        [Fact]
        public void should_pass_object_in_logevent()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);
            logger.Warn(obj);
            logger.Error(obj);

            Assert.Collection(output,
                log => Assert.Equal(obj, log.Content),
                log => Assert.Equal(obj, log.Content),
                log => Assert.Equal(obj, log.Content));
        }

        [Fact]
        public void should_pass_level_in_logevent()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);
            logger.Warn(obj);
            logger.Error(obj);

            Assert.Collection(output,
                log => Assert.Equal(LogLevel.Info, log.Level),
                log => Assert.Equal(LogLevel.Warn, log.Level),
                log => Assert.Equal(LogLevel.Error, log.Level));
        }

        [Fact]
        public void should_capture_callerfilepath()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            var obj = new { message = "Hi" };
            logger.Info(obj);
            logger.Warn(obj);
            logger.Error(obj);

            Assert.Collection(output,
                log => Assert.Equal(callerFilePath, log.CallerInfo.FilePath),
                log => Assert.Equal(callerFilePath, log.CallerInfo.FilePath),
                log => Assert.Equal(callerFilePath, log.CallerInfo.FilePath));
        }

        [Fact]
        public void should_capture_callerlinenumber()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );
            var callerLineNumber = GetCallerLineNumber();

            var obj = new { message = "Hi" };
            logger.Info(obj);
            logger.Warn(obj);
            logger.Error(obj);

            Assert.Collection(output,
                log => Assert.Equal(callerLineNumber + 3, log.CallerInfo.LineNum),
                log => Assert.Equal(callerLineNumber + 4, log.CallerInfo.LineNum),
                log => Assert.Equal(callerLineNumber + 5, log.CallerInfo.LineNum));
        }

        [Theory]
        [InlineData(1)]
        [InlineData("string")]
        [InlineData(1.45)]
        [InlineData(1L)]
        public void should_reject_message_that_are_not_objects(object invalidMessage)
        {
            var output = new List<LogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            Assert.Throws<InvalidLogMessageTypeException>(() => logger.Info(invalidMessage));
        }

        [Theory]
        [InlineData(1)]
        [InlineData("string")]
        [InlineData(1.45)]
        [InlineData(1L)]
        public void should_reject_additionalfields_that_are_not_objects(object invalidMessage)
        {
            var output = new List<LogEvent>();

            Logger logger = new Logger(logEvents => logEvents
                .AddFields(invalidMessage)
                .Subscribe(log => output.Add(log))
                );
            var callerFilePath = GetCallerFilePath();

            Assert.Throws<InvalidLogMessageTypeException>(() => logger.Info(new { }));
        }

        private string GetCallerFilePath([CallerFilePath]string callerFilePath = "Shouldn't Get This")
        {
            return callerFilePath;
        }

        private int GetCallerLineNumber([CallerLineNumber]int callerLineNumber = -999)
        {
            return callerLineNumber;
        }
    }
}
