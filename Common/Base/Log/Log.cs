using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;
using Common.Log.Write;

namespace Common.Log
{
    public class Log
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        protected string mSystemMark = string.Empty;

        /// <summary>
        /// 控制台
        /// </summary>
        private LogConsole mConsole;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strSystemMark">系统标示</param>
        public Log(string strSystemMark, LogConsole console)
        {
            this.mSystemMark = strSystemMark;
            this.mConsole = console;
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="strContent"></param>
        public void Debug(string strContent)
        {
            LogContent content = new LogContent(ELogType.ELT_Debug, this.mSystemMark, "[Debug]:" + strContent);
            mConsole.PushContent(content);
        }

        /// <summary>
        /// 重要信息
        /// </summary>
        /// <param name="strContent"></param>
        public void Info(string strContent)
        {
            LogContent content = new LogContent(ELogType.ELT_Info, this.mSystemMark, "[Info ]:" + strContent);
            mConsole.PushContent(content);
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="strContent"></param>
        public void Error(string strContent)
        {
            LogContent content = new LogContent(ELogType.ELT_Error, this.mSystemMark, "[Error]:" + strContent);
            mConsole.PushContent(content);
        }

        /// <summary>
        /// 打印警告信息
        /// </summary>
        /// <param name="strContent"></param>
        public void Warn(string strContent)
        {
            LogContent content = new LogContent(ELogType.ELT_Warn, this.mSystemMark, "[Warn ]:" + strContent);
            mConsole.PushContent(content);
        }
    }
}
