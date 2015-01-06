using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace System
{
    public static class LogHelper
    {
        public static void LogException(this Exception ex, Type locationOfException, string errorType)
        {
            ILog logger = LogManager.GetLogger(locationOfException);
            switch (errorType)
            {
                case "info":
                    logger.Info(ex.Message);
                    break;
                case "debug":
                    logger.Debug(ex.Message);
                    break;
                case "error":
                    logger.Error(ex.Message);
                    break;
                case "fatal":
                    logger.Fatal(ex.Message);
                    break;
                case "warn":
                    logger.Warn(ex.Message);
                    break;
                default:
                    logger.Info(ex.Message);
                    break;
            }
        }
    }
}