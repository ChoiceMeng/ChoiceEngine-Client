using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetCommon.UDP
{
    public enum Protocol
    {
        /// <summary>
        /// Ping值
        /// </summary>
        Ping,

        /// <summary>
        /// 正常命令
        /// </summary>
        Cmd,

        /// <summary>
        /// 确认消息
        /// </summary>
        Verify,
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="ipEndPoint"></param>
    /// <param name="byData"></param>
    public delegate void ReceiveEvent(IPEndPoint ipEndPoint, byte[] byData);

    /// <summary>
    /// 消息处理
    /// </summary>
    /// <param name="session"></param>
    /// <param name="byData"></param>
    public delegate void HandleReceive(UDPSession session, byte[] byData);
}
