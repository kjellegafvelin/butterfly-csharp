using System;

namespace Butterfly.OpenTracing
{
    public static class LogFieldExtensions
    {
        private static LogField Set(this LogField logField, string key, object value)
        {
            if (logField == null)
            {
                throw new ArgumentNullException(nameof(logField));
            }

            logField[key] = value;
            return logField;
        }

        public static LogField Event(this LogField logField, string eventName)
        {
            return logField.Set(LogFields.Event, eventName);
        }
        
        public static LogField ServerSend(this LogField logField)
        {
            return logField?.Event("Server Send");
        }
        
        public static LogField ServerReceive(this LogField logField)
        {
            return logField?.Event("Server Receive");
        }
    }
}