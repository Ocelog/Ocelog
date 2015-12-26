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
            return DidLog(LogLevel.Info, content);
        }

        public bool DidWarn(object content)
        {
            return DidLog(LogLevel.Warn, content);
        }

        public bool DidError(object content)
        {
            return DidLog(LogLevel.Error, content);
        }

        private void RecordLog(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }

        public bool DidLog(LogLevel logLevel, object content)
        {
            return _logEvents.Any(item => item.Level == logLevel && DidMatch(item.Content, content));
        }

        private bool DidMatch(object actualContent, object expectedContent)
        {
            if (actualContent == null && expectedContent == null)
                return true;

            if (expectedContent == null)
                return true;

            if (IsMatchingPredicate(expectedContent, actualContent))
                return InvokePredicate(expectedContent, actualContent);

            if (actualContent == null)
                return false;

            if (IsComparableWithEquals(expectedContent, actualContent))
                return actualContent.Equals(expectedContent);

            var expectedProperties = GetValueProperties(expectedContent);
            var actualProperties = GetValueProperties(actualContent);

            var matchingProperties = actualProperties
                .Join(expectedProperties, left => left.Name, right => right.Name, (left, right) => new { left, right });

            if (matchingProperties.Count() != expectedProperties.Count())
                return false;

            return matchingProperties.All(match => DidMatch(match.left.GetValue(actualContent), match.right.GetValue(expectedContent)));
        }

        private bool InvokePredicate(object expectedContent, object actualContent)
        {
            return (bool)expectedContent
                .GetType()
                .GetMethod("DynamicInvoke")
                .Invoke(expectedContent, new[] { new[] { actualContent } });
        }

        private bool IsComparableWithEquals(object expectedContent, object actualContent)
        {
            return actualContent.GetType().IsValueType
                || expectedContent.GetType().IsValueType
                || actualContent.GetType() == typeof(string)
                || expectedContent.GetType() == typeof(string);
        }

        private bool IsMatchingPredicate(object expectedContent, object actualContent)
        {
            return expectedContent.GetType().IsGenericType
                   && expectedContent.GetType().GetGenericTypeDefinition() == typeof(Predicate<>)
                   && expectedContent.GetType().GetGenericArguments().Length == 1
                   && expectedContent.GetType().GetGenericArguments()[0] == GetPredicateTypeParam(actualContent);
        }

        private Type GetPredicateTypeParam(object actualContent)
        {
            return (actualContent == null ? typeof(object) : actualContent.GetType());
        }

        private IEnumerable<PropertyInfo> GetValueProperties(object content)
        {
            return content.GetType().GetProperties()
                .Where(prop => prop.CanRead
                    && prop.GetAccessors().Any(access => access.ReturnType != typeof(void) && access.GetParameters().Length == 0));
        }
    }
}
