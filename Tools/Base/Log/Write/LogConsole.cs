using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;
using Common.Log.Config;

namespace Common.Log.Write
{
    /// <summary>
    /// 控制台Log
    /// </summary>
    public class LogConsole : LogWrite
    {
        /// <summary>
        /// 之前的颜色
        /// </summary>
        protected WriteFunction oldColor;

        /// <summary>
        /// Log中心
        /// </summary>
        protected LogFileMap mLogFileMap;

        /// <summary>
        /// 日志配置信息
        /// </summary>
        protected LogConfig mConfig;

        /// <summary>
        /// 控制台中心
        /// </summary>
        /// <param name="logCenter"></param>
        public void SetLogFileMap(LogFileMap logFileMap)
        {
            this.mLogFileMap = logFileMap;
        }

        /// <summary>
        /// 设置Log的配置
        /// </summary>
        /// <param name="config"></param>
        public void SetLogConfig(LogConfig config)
        {
            this.mConfig = config;
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logContent"></param>
        protected override void WriteLog(LogContent logContent)
        {
            if (null == logContent)
                return;

            // 保存上时间
            LogTime(logContent);
            // 写控制台日志
            WriteConsole(logContent);
            // 写文件日志
            WriteFile(logContent);
        }

        /// <summary>
        /// 给日志写上时间
        /// </summary>
        /// <param name="logContent"></param>
        protected void LogTime(LogContent logContent)
        {
            if (null == logContent)
                return;

            logContent.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            logContent.Content = logContent.Content  + "\r\n";
        }

        /// <summary>
        /// 是否写控制台文件
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        protected bool IsWriteConsole(string strSystemMark, ELogType eLogType)
        {
            if (null == this.mConfig)
                return true;

            return this.mConfig.IsWriteConsole(strSystemMark, eLogType);
        }

        /// <summary>
        /// 写控制台信息
        /// </summary>
        /// <param name="logContent"></param>
        protected void WriteConsole(LogContent logContent)
        {
            // 传送内容为空或者这个系统现在不写控制台日志
            if (null == logContent || !IsWriteConsole(logContent.SystemMark, logContent.LogType))
                return;

            WriteFunction function = mConfig.GetLogColor(logContent.LogType);
            if (function == null)
                return;

            function(logContent.Time + logContent.Content);
        }

        /// <summary>
        /// 是否写文件日志
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        protected bool IsWriteFile(string strSystemMark, ELogType eLogType)
        {
            if (null == this.mConfig)
                return true;

            return this.mConfig.IsWriteFile(strSystemMark, eLogType);
        }

        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="logContent"></param>
        protected void WriteFile(LogContent logContent)
        {
            // 传送内容为空或者这个系统现在不写控制台日志
            if (null == this.mLogFileMap || null == logContent || !IsWriteFile(logContent.SystemMark, logContent.LogType))
                return;

            LogFile mFile = this.mLogFileMap.GetLogFile(logContent.SystemMark);
            if (mFile == null)
                return;

            mFile.PushContent(logContent);
        }
    }
}
