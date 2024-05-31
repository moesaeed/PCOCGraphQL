using System;

namespace DF2023.Mvc.Models
{
    public class NoStackTraceException : Exception
    {
        public NoStackTraceException(string message) : base(message)
        {
        }

        public override string StackTrace
        {
            get { return string.Empty; }
        }
    }
}