using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PingTicoVPN.Modules
{
    public enum LogLevel {
        FATAL = 0,
        ERROR = 1,
        WARN = 2,
        INFO = 3,
        DEBUG = 4
    }
    public static class Log
    {
        public static LogLevel logLevel = LogLevel.DEBUG;
        public static void ToConsole(LogLevel ll, string msg)
        {
            //Check if current logging level allows current log to be outputed
            switch (ll)
            {
                case LogLevel.FATAL:
                    if(logLevel < LogLevel.FATAL) { return; }

                    break;
                case LogLevel.ERROR:
                    if (logLevel < LogLevel.ERROR) { return; }
                    break;
                case LogLevel.WARN:
                    if (logLevel < LogLevel.WARN) { return; }
                    break;
                case LogLevel.INFO:
                    if (logLevel < LogLevel.INFO) { return; }
                    break;
                case LogLevel.DEBUG:
                    if (logLevel < LogLevel.DEBUG) { return; }
                    break;
            }

            Trace.WriteLine(String.Format("[{0}][{1}] - {2}", Enum.GetName(typeof(LogLevel), ll), DateTime.Now.ToString("s"), msg));
        }

        public static void HandleError(Exception ex)
        {
            ToConsole(LogLevel.ERROR, String.Format("Exception\nMessage:\n{0}\n\nInner Exception:\n{1}\n\nStack Trace:{2}", ex.Message, ex.InnerException, ex.StackTrace));
        }
    }
}
