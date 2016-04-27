using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Net.Adapter
{
    /// <summary>
    /// Tcp服务器的网络适配器
    /// </summary>
    class TcpServerAdapter : NetAdapter
    {
        /// <summary>
        /// 会话连接
        /// </summary>
        // protected IConnection mSession;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="AdapterID">唯一值</param>
        /// <param name="net">网络模型</param>
        /// <param name="param">参数值</param>
        public override void Init(long lAdapterID, ENet net, object param)
        {
            base.Init(lAdapterID, net, param);
            // mSession = param as IConnection;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="byData">发送数据</param>
        /// <param name="length">长度</param>
        /// <param name="offset">偏移量</param>
        /// <param name="verify">是否验证，Udp的时候才有用</param>
        protected override void SendMessage(byte[] byData, int length, int offset, bool verify = false)
        {
            //if (mSession == null)
                return;

            //byte[] bySend = new byte[length];
            //Buffer.BlockCopy(byData, offset, bySend, offset, length);

            // var packet = PacketBuilder.ToAsyncBinary("BinProcess", 0, bySend, length);

            // mSession.BeginSend(packet);
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        protected override void SendMessage()
        {

        }
    }
}
