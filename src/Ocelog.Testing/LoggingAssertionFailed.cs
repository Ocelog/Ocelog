using System;
using System.Runtime.Serialization;

namespace Ocelog.Testing
{
    [Serializable]
    public class LoggingAssertionFailed : Exception
    {
        public LoggingAssertionFailed()
        {
        }

        public LoggingAssertionFailed(string message) : base(message)
        {
        }

        public LoggingAssertionFailed(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LoggingAssertionFailed(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}