using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Config;
using Common.Log.Write;
using System.Threading;
using Common.Log.Model;

namespace Common.Log
{
    public class LogCenter
    {
        /// <summary>
        /// 日志配置
        /// </summary>
        private LogConfig mConfig = new LogConfig();

        /// <summary>
        /// 控制台
        /// </summary>
        private LogConsole mConsole = new LogConsole();

        /// <summary>
        /// 日志
        /// </summary>
        private IDictionary<string, Log> mLog = new Dictionary<string, Log>();

        /// <summary>
        /// 日志锁
        /// </summary>
        protected object mLogLock = new object();

        /// <summary>
        /// 文件日志的介子对
        /// </summary>
        private LogFileMap mLogFileMap = new LogFileMap();

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogCenter()
        {
            this.mConsole.SetLogConfig(mConfig);
            this.mConsole.SetLogFileMap(mLogFileMap);
        }

        /// <summary>
        /// 控制台线程
        /// </summary>
        public void ConsoleThread()
        {
            mConsole.Write();
            mLogFileMap.Write();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            
        }

        /// <summary>
        /// 创建日志
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        public Log GetLog(string strSystemMark)
        {
            lock (mLogLock)
            {
                if (this.mLog.ContainsKey(strSystemMark))
                    return this.mLog[strSystemMark];

                Log log = new Log(strSystemMark, this.mConsole);
                this.mLog.Add(strSystemMark, log);
                this.mConfig.CreateLog(strSystemMark);
                this.mLogFileMap.CreateLogFile(strSystemMark);

                return log;
            }
        }

        /// <summary>
        /// 修改控制台日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeConsoleSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            if (this.mConfig == null)
                return;

            this.mConfig.ChangeConsoleSwitch(strSystemMark, eLogType, bSwitch);
        }

        /// <summary>
        /// 修改文件日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeFileSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            if (this.mConfig == null)
                return;

            this.mConfig.ChangeFileSwitch(strSystemMark, eLogType, bSwitch);
        }

        /// <summary>
        /// 把所有的控制台开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeAllConsoleSwitch(ELogType eLogType, bool bSwitch)
        {
            if (this.mConfig == null)
                return;

            this.mConfig.ChangeAllConsoleSwitch(eLogType, bSwitch);
        }

        /// <summary>
        /// 把所有的文件开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeAllFileSwitch(ELogType eLogType, bool bSwitch)
        {
            if (this.mConfig == null)
                return;

            this.mConfig.ChangeAllFileSwitch(eLogType, bSwitch);
        }
    }
}
