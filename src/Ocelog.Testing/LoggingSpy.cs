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

        public void AssertDidInfo(object content)
        {
            AssertDidLog(LogLevel.Info, content);
        }

        public void AssertDidWarn(object content)
        {
            AssertDidLog(LogLevel.Warn, content);
        }

        public void AssertDidError(object content)
        {
            AssertDidLog(LogLevel.Error, content);
        }

        private bool DidLog(LogLevel logLevel, object content)
        {
            return _logEvents.Any(item => item.Level == logLevel && DidMatch(item.Content, content, "").Item1);
        }

        private void AssertDidLog(LogLevel logLevel, object content)
        {
            var matches = _logEvents
                .Where(item => item.Level == logLevel)
                .Select(item => DidMatch(item.Content, content, "log"));

            if (!matches.Any(match => match.Item1))
                throw new LoggingAssertionFailed(string.Join("\n", matches.Where(match => !match.Item1).Select(match => match.Item2)));
        }

        private void RecordLog(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }

        private Tuple<bool, string> DidMatch(object actualContent, object expectedContent, string path)
        {
            if (actualContent == null && expectedContent == null)
                return Pass();

            if (expectedContent == null)
                return Pass();

            if (IsMatchingPredicate(expectedContent, actualContent))
                return InvokePredicate(expectedContent, actualContent, path);

            if (actualContent == null)
                return Fail($"Missing field ({path})");

            if (object.ReferenceEquals(expectedContent, actualContent))
                return Pass();

            if (IsComparableWithEquals(expectedContent, actualContent))
                return actualContent.Equals(expectedContent) ? Pass() : Fail($"Not equal ({path}) Expected: {expectedContent} but got {actualContent}");

            return DidPropertiesMatch(actualContent, expectedContent, path);

        }

        private Tuple<bool, string> DidPropertiesMatch(object actualContent, object expectedContent, string path)
        { 
            var expectedProperties = GetValueProperties(expectedContent);
            var actualProperties = GetValueProperties(actualContent);

            var matchingProperties = actualProperties
                .Join(expectedProperties, left => left.Name, right => right.Name, (left, right) => new { left, right });

            if (matchingProperties.Count() != expectedProperties.Count())
                return Fail($"Missing fields ({GetFieldNames(expectedProperties, path)})");

            var notMatching = matchingProperties
                .Select(match => DidMatch(match.left.GetValue(actualContent), match.right.GetValue(expectedContent), path + "." + match.right.Name))
                .FirstOrDefault(match => !match.Item1);

            return notMatching ?? Pass();
        }

        private Tuple<bool, string> Pass()
        {
            return new Tuple<bool, string>(true, "");
        }

        private Tuple<bool, string> Fail(string reason)
        {
            return new Tuple<bool, string>(false, reason);
        }

        private string GetFieldNames(IEnumerable<Property> properties, string path)
        {
            return string.Join(", ", properties.Select(prop => path + "." + prop.Name));
        }

        private Tuple<bool, string> InvokePredicate(object expectedContent, object actualContent, string path)
        {
            if ((bool)expectedContent
                .GetType()
                .GetMethod("DynamicInvoke")
                .Invoke(expectedContent, new[] { new[] { actualContent } }))
            {
                return Pass();
            }
            else
            {
                return Fail($"Predicate did not match field ({path})");
            };
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

        private IEnumerable<Property> GetValueProperties(object content)
        {
            if (content is Dictionary<string, object>)
            {
                var dictionary = (Dictionary<string, object>)content;
                return dictionary
                    .Select(pair => new Property() { Name = pair.Key, GetValue = (o => ((Dictionary<string, object>)o)[pair.Key]) });
            }

            return content.GetType().GetProperties()
                .Where(prop => prop.CanRead
                    && prop.GetAccessors().Any(access => access.ReturnType != typeof(void) && access.GetParameters().Length == 0))
                .Select(prop => new Property() { Name = prop.Name, GetValue = prop.GetValue });
        }

        private class Property
        {
            public string Name;
            public Func<object, object> GetValue;
        }
    }
}
