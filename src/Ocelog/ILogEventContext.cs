using System;
using System.Collections.Generic;

namespace Ocelog
{
    public interface ILogEventContext
    {
        LogLevel Level { get; }
        DateTime Timestamp { get; }
        IEnumerable<string> Tags { get; }
    }
}