using System;
using System.Collections.Generic;

namespace Butterfly.OpenTracing
{
    public sealed class LogData
    {
        public DateTime Timestamp { get; }

        public string Message { get; }

        public LogField Fields { get; }

        public LogData(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }

        public LogData(DateTime timestamp, IDictionary<string, object> fields)
        {
            Timestamp = timestamp;
            if (fields == null)
            {
                Fields = new LogField();
            }
            else if (fields is LogField logField)
            {
                Fields = logField;
            }
            else
            {
                Fields = new LogField(fields);
            }
        }
    }
}