using System;
using log4net;

namespace AutoBackup.Class
{
    public class Logger
    {
        public static void LogInfo(string str)
        {
            Logger.logger.Info(str);
        }

        public static void LogError(string str)
        {
            Logger.logger.Error(str);
        }

        public static void LogFatal(string str)
        {
            Logger.logger.Fatal(str);
        }

        private static ILog logger = LogManager.GetLogger("FileAppender");
    }
}
