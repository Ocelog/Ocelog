using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ocelog
{
    public class RequestLog : IDisposable
    {
        private ConcurrentQueue<object> _fields = new ConcurrentQueue<object>();
        private Subject<LogEvent> _logEvents;
        
        internal RequestLog(Subject<LogEvent> _logEvents)
        {
            this._logEvents = _logEvents;
        }

        public void Add(object newFields)
        {
            _fields.Enqueue(newFields);
        }

        public void Complete([CallerFilePath] string callerFilePath = "", [CallerLineNumber]int callerLineNumber = 0)
        {
            var logEvent = new LogEvent() { Level = LogLevel.Info };
            logEvent.CallerInfo = new CallerInfo() { FilePath = callerFilePath, LineNum = callerLineNumber };
            logEvent.Content = ObjectMerging.Flatten(_fields);

            _logEvents.OnNext(logEvent);
        }

        public void Dispose()
        {
            Complete();
        }
    }
}
