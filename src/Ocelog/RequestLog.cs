using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ocelog
{
    public class RequestLog
    {
        private LogEvent _logEvent;
        private Subject<LogEvent> _logEvents;
        
        internal RequestLog(Subject<LogEvent> _logEvents)
        {
            this._logEvents = _logEvents;
            _logEvent = new LogEvent() { Level = LogLevel.Info };
        }

        public void Add(object newFields)
        {
            _logEvent.AddField(newFields);
        }

        public void Complete([CallerFilePath] string callerFilePath = "", [CallerLineNumber]int callerLineNumber = 0)
        {
            _logEvent.CallerInfo = new CallerInfo() { FilePath = callerFilePath, LineNum = callerLineNumber };
            _logEvent.Content = new { };

            _logEvents.OnNext(_logEvent);
        }
    }
}
