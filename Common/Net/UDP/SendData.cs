using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetCommon.UDP
{
    /// <summary>
    /// 发送数据
    /// </summary>
    public class SendData
    {
        /// <summary>
        /// 进入时间
        /// </summary>
        public long EnterTime
        {
            get;
            set;
        }

        /// <summary>
        /// 数据包
        /// </summary>
        public MemBlock Packet
        {
            get;
            set;
        }

        /// <summary>
        /// 网络地址
        /// </summary>
        public IPEndPoint EndPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 发送次数
        /// </summary>
        public int SendTimes = 0;

        /// <summary>
        /// 唯一Id
        /// </summary>
        public long UUID
        {
            get;
            set;
        }
    }
}
