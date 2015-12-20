using System;
using System.Reactive.Subjects;

namespace Ocelog
{
    internal class ErrorHandlingObserver : IObserver<LogEvent>
    {
        private Subject<LogEvent> _logEvents;
        private Subject<LogEvent> _logExceptionEvents =  new Subject<LogEvent>();

        public ErrorHandlingObserver(Subject<LogEvent> logEvents, Action<IObservable<LogEvent>> loggingPipelineFactory)
        {
            _logEvents = logEvents;
            loggingPipelineFactory(_logExceptionEvents);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(LogEvent value)
        {
            try
            {
                _logEvents.OnNext(value);
            }
            catch (Exception e)
            {
                _logExceptionEvents.OnNext(new LogEvent() { Level = LogLevel.Error, Content = e, CallerInfo = value.CallerInfo });
            }
        }
    }
}