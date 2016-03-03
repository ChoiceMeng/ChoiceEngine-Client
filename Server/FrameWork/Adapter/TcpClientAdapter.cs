using NetCommon.Net;
using NetCommon.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Net.Adapter
{
    /// <summary>
    /// Tcp 客户端网络适配器
    /// </summary>
    class TcpClientAdapter : NetAdapter
    {
        /// <summary>
        /// 会话类
        /// </summary>
        protected TCPClient mSession;

        /// <summary>
        /// 最大的组包个数
        /// </summary>
        protected const int mMaxSendPacket = 30;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="AdapterID">唯一值</param>
        /// <param name="net">网络模型</param>
        /// <param name="param">参数值</param>
        public override void Init(long lAdapterID, ENet net, object param)
        {
            base.Init(lAdapterID, net, param);
            mSession = param as TCPClient;
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
            if (mSession == null)
                return;

            mSession.SendMessage(byData, length, offset);
        }

        /// <summary>
        /// 起算发送长度
        /// </summary>
        /// <returns></returns>
        protected int CalcSendLength()
        {
            int nCount = Math.Min(GetSendQueueCount(), mMaxSendPacket);
            MemBlock block;
            int nLength = 0;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                block = GetSend(nIndex);
                if (block.GetBytes() == null)
                    continue;

                nLength += block.UseSize;
            }

            return nLength;
        }

        /// <summary>
        /// 计算发送块
        /// </summary>
        /// <returns></returns>
        protected MemBlock CalcSendBlock()
        {
            int nCount = Math.Min(GetSendQueueCount(), mMaxSendPacket);
            MemBlock sendBlock = Alloc(CalcSendLength());

            MemBlock block;
            int offset = 0;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                block = GetSend(nIndex);
                if (block.GetBytes() == null)
                    continue;

                Buffer.BlockCopy(block.GetBytes(), 0, sendBlock.GetBytes(), offset, block.UseSize);
                offset += block.UseSize;
                Free(block);
            }

            return sendBlock;
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        protected override void SendMessage()
        {
            int nCount = GetSendQueueCount();
            MemBlock block;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                block = GetSend(nIndex);
                if (block.GetBytes() == null)
                    continue;

                SendMessage(block.GetBytes(), block.UseSize, 0);
                SendFree(block);
            }
        }
    }
}
