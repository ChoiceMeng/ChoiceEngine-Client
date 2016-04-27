using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Log.Model
{
    public class LogContent
    {
        /// <summary>
        /// 系统标示
        /// </summary>
        private string mSystemMark = string.Empty;

        /// <summary>
        /// 系统标示
        /// </summary>
        public string SystemMark
        {
            get { return mSystemMark; }
            set { mSystemMark = value; }
        }

        /// <summary>
        /// 日志时间
        /// </summary>
        private string mTime = string.Empty;

        /// <summary>
        /// 日志时间
        /// </summary>
        public string Time
        {
            get { return mTime; }
            set { mTime = value; }
        }

        /// <summary>
        /// 日志内容
        /// </summary>
        protected string mContent = string.Empty;

        /// <summary>
        /// 文本
        /// </summary>
        public string Content
        {
            get { return mContent; }
            set { mContent = value; }
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        private ELogType mLogType = ELogType.ELT_Warn;

        /// <summary>
        /// 日志类型
        /// </summary>
        public ELogType LogType
        {
            get { return mLogType; }
            set { mLogType = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="strContent"></param>
        public LogContent(ELogType logType, string strSystemMark, string strContent)
        {
            LogType = logType;
            Content = strContent;
            SystemMark = strSystemMark;
        }
    }
}
