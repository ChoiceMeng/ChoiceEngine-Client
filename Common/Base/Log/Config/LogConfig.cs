using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;
using UnityEngine;

namespace Common.Log.Config
{
	/// <summary>
	/// Write function.
	/// </summary>
	public delegate void WriteFunction(object param);

    /// <summary>
    /// 日志配置
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// Log颜色[Debug]
        /// </summary>
		protected IDictionary<ELogType, WriteFunction> mLogMark = new Dictionary<ELogType, WriteFunction>();

        /// <summary>
        /// 日志开关
        /// </summary>
        protected IDictionary<string, LogSwitch> mSwitch = new Dictionary<string, LogSwitch>();

        /// <summary>
        /// 日志开关的锁
        /// </summary>
        protected object mLock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogConfig()
        {
            // 初始化日志颜色
            InitLogColor();
        }

        /// <summary>
        /// 初始化日志颜色
        /// </summary>
        protected void InitLogColor()
        {
            // 红色是错误
            mLogMark.Add(ELogType.ELT_Error, Debug.LogError);
            // 黄色是警告
            mLogMark.Add(ELogType.ELT_Warn, Debug.LogWarning);
            // 正常信息是就默认吧
            mLogMark.Add(ELogType.ELT_Info, Debug.Log);
            // 调试信息也是默认吧
            mLogMark.Add(ELogType.ELT_Debug, Debug.LogWarning);
        }

        /// <summary>
        /// 获取日志颜色
        /// </summary>
        /// <param name="eLogType"></param>
        /// <returns></returns>
		public WriteFunction GetLogColor(ELogType eLogType)
        {
            if (!this.mLogMark.ContainsKey(eLogType))
                return Debug.Log;

            return this.mLogMark[eLogType];
        }

        /// <summary>
        /// 创建日志
        /// </summary>
        /// <param name="strSystemMark"></param>
        public LogSwitch CreateLog(string strSystemMark)
        {
            lock (mLock)
            {
                if (this.mSwitch.ContainsKey(strSystemMark))
                    return this.mSwitch[strSystemMark];

                LogSwitch logSwitch = new LogSwitch();
                this.mSwitch.Add(strSystemMark, logSwitch);

                return logSwitch;
            }
        }

        /// <summary>
        /// 获取日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        protected LogSwitch GetLogSwitch(string strSystemMark)
        {
            lock (mLock)
            {
                if (!this.mSwitch.ContainsKey(strSystemMark))
                    return null;

                return this.mSwitch[strSystemMark];
            }
        }

        /// <summary>
        /// 是否写控制台
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <returns></returns>
        public bool IsWriteConsole(string strSystemMark, ELogType eLogType)
        {
            LogSwitch logSwitch = GetLogSwitch(strSystemMark);
            if (null == logSwitch)
                return true;

            return logSwitch.IsConsoleSwitch(eLogType);
        }

        /// <summary>
        /// 是否写文件
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <returns></returns>
        public bool IsWriteFile(string strSystemMark, ELogType eLogType)
        {
            LogSwitch logSwitch = GetLogSwitch(strSystemMark);
            if (null == logSwitch)
                return true;

            return logSwitch.IsFileSwitch(eLogType);
        }

        /// <summary>
        /// 修改控制台日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeConsoleSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            LogSwitch logSwitch = GetLogSwitch(strSystemMark);
            if (null == logSwitch)
            {
                logSwitch = CreateLog(strSystemMark);
                if (logSwitch == null)
                    return;
            }

            logSwitch.ChangeConsoleSwitch(eLogType, bSwitch);
        }

        /// <summary>
        /// 修改文件日志开关
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeFileSwitch(string strSystemMark, ELogType eLogType, bool bSwitch)
        {
            LogSwitch logSwitch = GetLogSwitch(strSystemMark);
            if (null == logSwitch)
            {
                logSwitch = CreateLog(strSystemMark);
                if (logSwitch == null)
                    return;
            }

            logSwitch.ChangeFileSwitch(eLogType, bSwitch);
        }

        /// <summary>
        /// 获得所有的Key值
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllKey()
        {
            lock (this.mLock)
            {
                return this.mSwitch.Keys;
            }
        }

        /// <summary>
        /// 把所有的控制台开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeAllConsoleSwitch(ELogType eLogType, bool bSwitch)
        {
            ICollection<string> AllKey = GetAllKey();
            foreach (string strSystemMark in AllKey)
            {
                ChangeConsoleSwitch(strSystemMark, eLogType, bSwitch);
            }
        }

        /// <summary>
        /// 把所有的文件开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeAllFileSwitch(ELogType eLogType, bool bSwitch)
        {
            ICollection<string> AllKey = GetAllKey();
            foreach (string strSystemMark in AllKey)
            {
                ChangeFileSwitch(strSystemMark, eLogType, bSwitch);
            }
        }
    }
}
