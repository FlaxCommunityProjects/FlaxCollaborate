using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace CollaboratePlugin
{
    public class SelectiveLogHandler : ILogHandler
    {
        public event LogDelegate SendLog;

        public event LogExceptionDelegate SendExceptionLog;

        public void Log(LogType logType, FlaxEngine.Object context, string message)
        {
            Debug.Logger.LogHandler.Log(logType, context, message);

#if DEBUG
            string stackTrace = Environment.StackTrace;
#else
            string stackTrace = string.Empty;
#endif

            SendLog?.Invoke(logType, message, context, stackTrace);
        }

        public void LogException(Exception exception, FlaxEngine.Object context)
        {
            Debug.Logger.LogHandler.LogException(exception, context);

            SendExceptionLog?.Invoke(exception, context);
        }

        public void LogWrite(LogType logType, string message)
        {
            Debug.Logger.LogHandler.LogWrite(logType, message);
        }
    }
}