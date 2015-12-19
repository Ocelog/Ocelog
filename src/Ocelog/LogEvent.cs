using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelog
{
    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }

    public class LogEvent
    {
        private List<string> _tags = new List<string>();
        private List<object> _fields= new List<object>();
        private object _content;

        public object Content
        {
            get { return _content; }
            set
            {
                EnsureValidType(value, nameof(value));
                _content = value;
            }
        }

        public LogLevel Level { get; set; }
        public CallerInfo CallerInfo { get; set; }
        public DateTime Timestamp { get; set; }
        public IEnumerable<string> Tags { get { return _tags; } }
        public IEnumerable<object> AdditionalFields { get { return _fields; } }

        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        public void AddField(object additionalFields)
        {
            EnsureValidType(additionalFields, nameof(additionalFields));

            _fields.Add(additionalFields);
        }

        private void EnsureValidType(object value, string nameof)
        {
            if (value.GetType().IsPrimitive)
                throw new InvalidLogMessageTypeException(nameof, $"{nameof} cannot be a primitive type.");
            if (value is string)
                throw new InvalidLogMessageTypeException(nameof, $"{nameof} cannot be a string.");
        }
    }
}
