using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Log.Model
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum ELogType
    {
        ELT_Error,              // 错误信息
        ELT_Warn,               // 警告信息
        ELT_Info,               // 正常信息
        ELT_Debug,              // 调试信息
    }
}
