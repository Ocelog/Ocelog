﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

namespace Ocelog.Test
{
    public class RequestLogTests
    {
        [Fact]
        public void should_log_all_fields_on_complete()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            var requestLog = logger.StartRequestLog();

            requestLog.Add(new { Val = 1 });
            requestLog.Add(new { Name = "Some name" });
            requestLog.Complete();

            Assert.Equal(new Dictionary<string, object>()
            {
                { "Val", 1},
                { "Name", "Some name"}
            }, output[0].Content);
        }

        [Fact]
        public void should_log_as_info()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            var requestLog = logger.StartRequestLog();
            
            requestLog.Complete();

            Assert.Collection(output,
                log => Assert.Equal(LogLevel.Info, log.Level));
        }

        [Fact]
        public void should_log_caller_info()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            var requestLog = logger.StartRequestLog();

            var callerFilePath = GetCallerFilePath();
            
            requestLog.Complete();

            Assert.Collection(output,
                log => Assert.Equal(callerFilePath, log.CallerInfo.FilePath));
        }

        [Fact]
        public void should_log_on_dispose()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .Subscribe(log => output.Add(log))
                );

            using (var requestLog = logger.StartRequestLog())
            {
                requestLog.Add(new { Val = 1 });
                requestLog.Add(new { Name = "Some name" });
            }

            Assert.Equal(new Dictionary<string, object>()
            {
                { "Val", 1},
                { "Name", "Some name"}
            }, output[0].Content);
        }

        private string GetCallerFilePath([CallerFilePath]string callerFilePath = "Shouldn't Get This")
        {
            return callerFilePath;
        }
    }
}
