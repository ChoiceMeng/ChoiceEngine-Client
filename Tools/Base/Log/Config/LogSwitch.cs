using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;

namespace Common.Log.Config
{
    /// <summary>
    /// 日志开关
    /// </summary>
    public class LogSwitch
    {
        /// <summary>
        /// 控制台日志开关
        /// </summary>
        protected IDictionary<ELogType, bool> mConsoleSwitch = new Dictionary<ELogType, bool>();

        /// <summary>
        /// 文件日志开关
        /// </summary>
        protected IDictionary<ELogType, bool> mFileSwitch = new Dictionary<ELogType, bool>();

        /// <summary>
        /// 日志开关
        /// </summary>
        public LogSwitch()
        {
            // 默认都是开的
            InitLogSwitch();
        }

        /// <summary>
        /// 初始化日志开关
        /// </summary>
        protected void InitLogSwitch()
        {
            mConsoleSwitch.Add(ELogType.ELT_Error, true);
            mConsoleSwitch.Add(ELogType.ELT_Warn, true);
            mConsoleSwitch.Add(ELogType.ELT_Info, true);
            mConsoleSwitch.Add(ELogType.ELT_Debug, true);

            mFileSwitch.Add(ELogType.ELT_Error, true);
            mFileSwitch.Add(ELogType.ELT_Warn, true);
            mFileSwitch.Add(ELogType.ELT_Info, true);
            mFileSwitch.Add(ELogType.ELT_Debug, true);
        }

        /// <summary>
        /// 修改开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeConsoleSwitch(ELogType eLogType, bool bSwitch)
        {
            if (!this.mConsoleSwitch.ContainsKey(eLogType))
                return ;

            this.mConsoleSwitch[eLogType] = bSwitch;
        }

        /// <summary>
        /// 是否打开了开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <returns></returns>
        public bool IsConsoleSwitch(ELogType eLogType)
        {
            if (!this.mConsoleSwitch.ContainsKey(eLogType))
                return true;

            return this.mConsoleSwitch[eLogType];
        }

        /// <summary>
        /// 修改开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="bSwitch"></param>
        public void ChangeFileSwitch(ELogType eLogType, bool bSwitch)
        {
            if (!this.mFileSwitch.ContainsKey(eLogType))
                return;

            this.mFileSwitch[eLogType] = bSwitch;
        }

        /// <summary>
        /// 是否打开了开关
        /// </summary>
        /// <param name="eLogType"></param>
        /// <returns></returns>
        public bool IsFileSwitch(ELogType eLogType)
        {
            if (!this.mFileSwitch.ContainsKey(eLogType))
                return true;

            return this.mFileSwitch[eLogType];
        }
    }
}
