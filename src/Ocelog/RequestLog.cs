using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ocelog
{
    public class RequestLog : IDisposable
    {
        private readonly ConcurrentQueue<object> _fields = new ConcurrentQueue<object>();
        private readonly Subject<LogEvent> _logEvents;

        internal RequestLog(Subject<LogEvent> logEvents)
        {
            _logEvents = logEvents;
        }

        public void Add(object newFields)
        {
            _fields.Enqueue(newFields);
        }
        public void Complete([CallerFilePath] string callerFilePath = "", [CallerLineNumber]int callerLineNumber = 0)
        {
            var logEvent = new LogEvent
            {
                Level = LogLevel.Info,
                CallerInfo = new CallerInfo() {FilePath = callerFilePath, LineNum = callerLineNumber},
                Content = ObjectMerging.Flatten(_fields)
            };

            _logEvents.OnNext(logEvent);
        }

        public void Dispose()
        {
            Complete();
        }
    }
}
