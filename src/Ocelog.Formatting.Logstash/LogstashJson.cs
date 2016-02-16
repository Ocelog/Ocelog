using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Ocelog.Formatting.Logstash
{
    public static class LogstashJson
    {
        public static FormattedLogEvent Format(LogEvent logEvent)
        {
            var requiredFields = new Dictionary<string, object>()
            {
                { "@version", 1 },
                { "@timestamp", logEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) }
            };

            var jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            jsonSerializer.Converters.Add(new StringEnumConverter() { AllowIntegerValues = false });

            var document = ToDictionary(requiredFields);

            foreach (var fields in logEvent.AdditionalFields)
            {
                document = Merge(ToDictionary(fields), document);
            }

            document = Merge(ToDictionary(logEvent.Content), document);

            var json = JObject.FromObject(document, jsonSerializer);

            return new FormattedLogEvent() { Content = JsonConvert.SerializeObject(json) };
        }

        private static Dictionary<string, object> Merge(Dictionary<string, object> doc1, Dictionary<string, object> doc2)
        {
            var mergedDictionary = new Dictionary<string, object>(doc1);
            foreach (var pair in doc2)
            {
                var key = pair.Key;
                var val = pair.Value;
                if (mergedDictionary.ContainsKey(key))
                {
                    val = Merge(mergedDictionary[key], val);
                }
                mergedDictionary[key] = ToDictionaryOrObject(val);
            }
            return mergedDictionary;
        }

        private static object Merge(object doc1, object doc2)
        {
            if (IsSimpleType(doc1))
                return doc1;

            if (doc1.GetType().IsArray && doc2.GetType().IsArray)
                return ((object[])doc2).Concat((object[])doc1);

            if (typeof(IEnumerable<object>).IsAssignableFrom(doc1.GetType()) && typeof(IEnumerable<object>).IsAssignableFrom(doc2.GetType()))
                return ((IEnumerable<object>)doc2).Concat((IEnumerable<object>)doc1);

            return Merge(ToDictionary(doc1), ToDictionary(doc2));
        }

        private static Dictionary<string, object> ToDictionary(object fields)
        {
            var type = fields.GetType();
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && type.GetGenericArguments().First() == typeof(string))
                return BoxDictionaryValues(type.GetGenericArguments()[1], fields);

            return fields.GetType().GetProperties()
                .Where(prop => prop.CanRead
                    && prop.GetAccessors().Any(access => access.ReturnType != typeof(void) && access.GetParameters().Length == 0))
                .Where(prop => prop.GetValue(fields) != null)
                .ToDictionary(prop => prop.Name, prop => ToDictionaryOrObject(prop.GetValue(fields)));
        }

        private static Dictionary<string, object> BoxDictionaryValues(Type valueType, object fields)
        {
            var method = typeof(LogstashJson).GetMethod("GenericBoxDictionaryValues", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(valueType);
            return (Dictionary<string, object>)genericMethod.Invoke(null, new[] { fields });
        }

        private static Dictionary<string, object> GenericBoxDictionaryValues<T>(object fields)
        {
            var dictionary = (Dictionary<string, T>)fields;

            return dictionary.ToDictionary<KeyValuePair<string, T>, string, object>(pair => pair.Key, pair => pair.Value);
        }

        private static object ToDictionaryOrObject(object fields)
        {
            if (IsSimpleType(fields))
                return fields;

            if (fields.GetType().IsArray)
                return ToList((object[])fields);

            if (typeof(IEnumerable<object>).IsAssignableFrom(fields.GetType()))
                return ToList((IEnumerable<object>)fields);

            return ToDictionary(fields);
        }

        private static object ToList(IEnumerable<object> fields)
        {
            return fields.Select(ToDictionaryOrObject);
        }

        private static bool IsSimpleType(object fields)
        {
            return fields.GetType().IsValueType
                || fields.GetType() == typeof(string)
                || fields.GetType().IsEnum;
        }
    }
}
