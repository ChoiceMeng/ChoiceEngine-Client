using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;

namespace Common.Log.Write
{
    /// <summary>
    /// 写日志接口
    /// </summary>
    interface ILogWrite
    {
        /// <summary>
        /// 添加一个日志
        /// </summary>
        /// <param name="logContent"></param>
        void PushContent(LogContent logContent);

        /// <summary>
        /// 写文件
        /// </summary>
        void Write();
    }
}
