using System;

namespace Ocelog
{

    public class InvalidLogMessageTypeException : ArgumentException
    {
        public InvalidLogMessageTypeException(string message, string paramName) : base(message, paramName)
        {
        }
    }
}
