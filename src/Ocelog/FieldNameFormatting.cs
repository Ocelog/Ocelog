using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelog
{
    public static class FieldNameFormatting
    {
        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToSnakeCase()
        {
            return (logEvent) => ProcessedLogEvent.Process(ToSnakeCase(logEvent.Content), logEvent);
        }

        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToPascalCase()
        {
            return (logEvent) => ProcessedLogEvent.Process(ToPascalCase(logEvent.Content), logEvent);
        }

        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToCamelCase()
        {
            return (logEvent) => ProcessedLogEvent.Process(ToCamelCase(logEvent.Content), logEvent);
        }

        private static Dictionary<string, object> ToSnakeCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToSnakeCase(pair.Key), pair => ObjectToSnakeCase(pair.Value));
        }

        private static object ObjectToSnakeCase(object val)
        {
            if (val is Dictionary<string, object>)
            {
                return ToSnakeCase((Dictionary<string, object>)val);
            }

            var list = val as IEnumerable<object>;
            if (list != null)
            {
                return list.Select(ObjectToSnakeCase);
            }

            return val;
        }

        private static string ToSnakeCase(string key)
        {
            return string.Join("_", Tokenize(key));
        }

        private static Dictionary<string, object> ToPascalCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToPascalCase(pair.Key), pair => ObjectToPascalCase(pair.Value));
        }

        private static object ObjectToPascalCase(object val)
        {
            if (val is Dictionary<string, object>)
            {
                return ToPascalCase((Dictionary<string, object>)val);
            }

            var list = val as IEnumerable<object>;
            if (list != null)
            {
                return list.Select(ObjectToPascalCase);
            }

            return val;
        }

        private static string ToPascalCase(string key)
        {
            return string.Concat(Tokenize(key).Select(word => CapitalizeFirstChar(word)));
        }

        private static Dictionary<string, object> ToCamelCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToCamelCase(pair.Key), pair => ObjectToCamelCase(pair.Value));
        }

        private static object ObjectToCamelCase(object val)
        {
            if (val is Dictionary<string, object>)
            {
                return ToCamelCase((Dictionary<string, object>)val);
            }

            var list = val as IEnumerable<object>;
            if (list != null)
            {
                return list.Select(ObjectToCamelCase);
            }

            return val;
        }

        private static string ToCamelCase(string key)
        {
            var tokens = Tokenize(key);
            return string.Concat(tokens.Select((word, j) => j == 0 ? word : CapitalizeFirstChar(word)));
        }

        private static string CapitalizeFirstChar(string word)
        {
            return char.ToUpper(word.First()) + word.Substring(1);
        }

        private static string[] Tokenize(string key)
        {
            return string.Concat(key.Select((x, j) => ReplaceChar(x, j))).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ReplaceChar(char x, int j)
        {
            if (j == 0)
                return x.ToString().ToLower();
            else if (char.IsUpper(x))
                return " " + x.ToString().ToLower();
            else if (char.IsWhiteSpace(x) || x == '_')
                return " ";
            else
                return x.ToString();
        }
    }
}
