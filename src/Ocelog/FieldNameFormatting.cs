using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelog
{
    public static class FieldNameFormatting
    {
        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToSnakeCase()
        {
            return (logEvent) => new ProcessedLogEvent() { Content = ToSnakeCase(logEvent.Content) };
        }

        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToPascalCase()
        {
            return (logEvent) => new ProcessedLogEvent() { Content = ToPascalCase(logEvent.Content) };
        }

        public static Func<ProcessedLogEvent, ProcessedLogEvent> ToCamelCase()
        {
            return (logEvent) => new ProcessedLogEvent() { Content = ToCamelCase(logEvent.Content) };
        }

        private static Dictionary<string, object> ToSnakeCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToSnakeCase(pair.Key), pair => pair.Value);
        }

        private static string ToSnakeCase(string key)
        {
            return string.Join("_", Tokenize(key));
        }

        private static Dictionary<string, object> ToPascalCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToPascalCase(pair.Key), pair => pair.Value);
        }

        private static string ToPascalCase(string key)
        {
            return string.Concat(Tokenize(key).Select(word => CapitalizeFirstChar(word)));
        }

        private static Dictionary<string, object> ToCamelCase(Dictionary<string, object> content)
        {
            return content.ToDictionary(pair => ToCamelCase(pair.Key), pair => pair.Value);
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
