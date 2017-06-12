using System;
using System.Configuration;
using NLog;
using NLog.Config;

namespace WebProxy.Net.Utility
{
    public class LogHelper
    {
        private static readonly string NlogConfigPath = ConfigurationManager.AppSettings["NlogConfig"];
        private static readonly string LoggerName = "WebProxy";
        private static readonly object LockObj = new object();

        private static Logger _log;
        private static Logger NLogger
        {
            get
            {
                if (_log == null)
                {
                    lock (LockObj)
                    {
                        if (_log == null)
                        {
                            XmlLoggingConfiguration config = new XmlLoggingConfiguration(NlogConfigPath, false);
                            LogManager.Configuration = config;
                            _log = string.IsNullOrEmpty(LoggerName) ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(LoggerName);
                        }
                    }
                }
                return _log;
            }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="title">日志消息标题，字数不超过300</param>
        /// <param name="message">日志详细内容，字数建议不超过3000</param>
        public static void Debug(string title, string message)
        {
            NLogger.Debug("Title:{0},Message:{1}", title, message);
        }

        /// <summary>
        /// 运行日志
        /// </summary>
        /// <param name="title">日志消息标题，字数不超过300</param>
        /// <param name="message">日志详细内容，字数建议不超过3000</param>
        public static void Info(string title, string message)
        {
            NLogger.Info("Title:{0},Message:{1}", title, message);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="title">预警消息标题，字数不超过300</param>
        /// <param name="message">预警详细内容，字数建议不超过3000</param>
        /// <param name="ex">具体错误</param>
        public static void Error(string title, string message, Exception ex = null)
        {
            string alarmText = ex == null ? message : string.Join("------", message, ex.ToString());

            NLogger.Error(ex, "Title:{0},Message:{1}", title, alarmText);
        }
    }
}