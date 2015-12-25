using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

            Assert.Collection(output,
                log => Assert.Collection(log.AdditionalFields,
                    field => Assert.Equal(new { Val = 1 }, field),
                    field => Assert.Equal(new { Name = "Some name" }, field)));
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
