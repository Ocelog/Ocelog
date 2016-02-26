using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ocelog
{
    public class RequestLog : IDisposable
    {
        private List<object> _fields = new List<object>();
        private Subject<LogEvent> _logEvents;
        
        internal RequestLog(Subject<LogEvent> _logEvents)
        {
            this._logEvents = _logEvents;
        }

        public void Add(object newFields)
        {
            _fields.Add(newFields);
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
