using System;
using System.Collections.Generic;
using System.Text;

namespace PingTicoVPNServer.Modules
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
        public static void LogToConsole(LogLevel ll, string msg)
        {
            //Check if current logging level allows current log to be outputed
            switch (ll)
            {
                case LogLevel.FATAL:
                    if(Config.config.log_level < LogLevel.FATAL) { return; }

                    break;
                case LogLevel.ERROR:
                    if (Config.config.log_level < LogLevel.ERROR) { return; }
                    break;
                case LogLevel.WARN:
                    if (Config.config.log_level < LogLevel.WARN) { return; }
                    break;
                case LogLevel.INFO:
                    if (Config.config.log_level < LogLevel.INFO) { return; }
                    break;
                case LogLevel.DEBUG:
                    if (Config.config.log_level < LogLevel.DEBUG) { return; }
                    break;
            }

            Console.WriteLine(String.Format("[{0}][{1}] - {2}", Enum.GetName(typeof(LogLevel), ll), DateTime.Now.ToString("s"), msg));
        }

        public static void HandleError(Exception ex)
        {
            LogToConsole(LogLevel.ERROR, String.Format("Exception\nMessage:\n{0}\n\nInner Exception:\n{1}\n\nStack Trace:{2}", ex.Message, ex.InnerException, ex.StackTrace));
        }
    }
}
