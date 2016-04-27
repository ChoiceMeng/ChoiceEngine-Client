using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;

namespace Common.Log
{
    public class Logger
    {
        /// <summary>
        /// Log控制中心
        /// </summary>
        protected static LogCenter sLogCenter = new LogCenter();

        /// <summary>
        /// 获得Log
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        public static Log GetLog(string strSystemMark)
        {
            return sLogCenter.GetLog(strSystemMark);
        }

        /// <summary>
        /// 修改控制台日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public static void ChangeConsoleSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            sLogCenter.ChangeConsoleSwitch(strSystemMark, eLogType, bSwitch);
        }

        /// <summary>
        /// 修改文件日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public static void ChangeFileSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            sLogCenter.ChangeFileSwitch(strSystemMark, eLogType, bSwitch);
        }

        /// <summary>
        /// 把所有的控制台开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public static void ChangeAllConsoleSwitch(ELogType eLogType, bool bSwitch)
        {
            sLogCenter.ChangeAllConsoleSwitch(eLogType, bSwitch);
        }

        /// <summary>
        /// 把所有的文件开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public static void ChangeAllFileSwitch(ELogType eLogType, bool bSwitch)
        {
            sLogCenter.ChangeAllFileSwitch(eLogType, bSwitch);
        }

        public static void Update()
        {
            sLogCenter.ConsoleThread();
        }

        /// <summary>
        /// 停止日志
        /// </summary>
        public static void Stop()
        {
            sLogCenter.Stop();
        }
    }
}
