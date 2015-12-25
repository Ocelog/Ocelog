using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reflection;

namespace Ocelog.Testing
{
    public class LoggingSpy
    {
        private List<LogEvent> _logEvents = new List<LogEvent>();

        public Logger Logger { get; private set; }

        public LoggingSpy()
        {
            Logger = new Logger(events => events.Subscribe(RecordLog));
        }

        public bool DidInfo(object content)
        {
            return _logEvents.Any(item => DidMatch(item.Content, content) && item.Level == LogLevel.Info);
        }

        public bool DidWarn(object content)
        {
            return _logEvents.Any(item => DidMatch(item.Content, content) && item.Level == LogLevel.Warn);
        }

        public bool DidError(object content)
        {
            return _logEvents.Any(item => DidMatch(item.Content, content) && item.Level == LogLevel.Error);
        }

        private void RecordLog(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }

        private bool DidMatch(object actualContent, object expectedContent)
        {
            if (actualContent.GetType().IsValueType || expectedContent.GetType().IsValueType)
                return actualContent.Equals(expectedContent);

            if (actualContent.GetType() == typeof(string) || expectedContent.GetType() == typeof(string))
                return actualContent.Equals(expectedContent);

            var expectedProperties = GetValueProperties(expectedContent);
            var actualProperties = GetValueProperties(actualContent);

            var matchingProperties = actualProperties
                .Join(expectedProperties, left => left.Name, right => right.Name, (left, right) => new { left, right });

            if (matchingProperties.Count() != expectedProperties.Count())
                return false;

            return matchingProperties.All(match => DidMatch(match.left.GetValue(actualContent), match.right.GetValue(expectedContent)));
        }

        private IEnumerable<PropertyInfo> GetValueProperties(object content)
        {
            return content.GetType().GetProperties()
                .Where(prop => prop.CanRead
                    && prop.GetAccessors().Any(access => access.ReturnType != typeof(void) && access.GetParameters().Length == 0));
        }
    }
}
