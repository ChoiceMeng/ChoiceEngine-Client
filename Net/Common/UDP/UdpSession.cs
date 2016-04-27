using Common.Log;
using NetCommon.CallBack;
using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetCommon.UDP
{
    /// <summary>
    /// UDP回话
    /// </summary>
    public class UDPSession
    {
        /// <summary>
        /// 网络类型
        /// </summary>
        public ENet mNetType = ENet.UdpClient;

        /// <summary>
        /// 远程Ip, 发送消息使用
        /// </summary>
        protected IPEndPoint mIPEndPoint;

        /// <summary>
        /// UDP的网络连接
        /// </summary>
        protected UDPSocket mUdpSocket;

        /// <summary>
        /// 拆分大小
        /// </summary>
        protected const int mSpliteSize = 1024;

        /// <summary>
        /// 消息包大小
        /// 唯一id + 是否确认 + 协议类型
        /// </summary>
        protected const int mPacketHeader = sizeof(long) + sizeof(bool) + sizeof(byte);

        /// <summary>
        /// 会话Id
        /// </summary>
        protected long lSessionID = 0;

        /// <summary>
        /// 包列表
        /// </summary>
        protected Dictionary<long, List<byte[]>> mPacketDict = new Dictionary<long, List<byte[]>>();

        /// <summary>
        /// 包的锁
        /// </summary>
        protected object mPacketLock = new object();

        /// <summary>
        /// 处理消息
        /// </summary>
        protected ReceiveHandler mHandleReceive;

        /// <summary>
        /// 包Id
        /// </summary>
        protected int mPacketId = 0;

        /// <summary>
        /// 包Id的锁
        /// </summary>
        protected object mPacketIdLock = new object();

        /// <summary>
        /// 上次Ping时间
        /// </summary>
        protected long lLastPingTime = 0;

        /// <summary>
        /// 接收包的Id
        /// </summary>
        protected FastQueue<long> mReceivePacketId = new FastQueue<long>();

        /// <summary>
        /// 接收最大的包数量
        /// </summary>
        protected const int MaxReceivePacketCount = 1000;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        public UDPSession(UDPSocket socket, IPEndPoint ip)
        {
            mUdpSocket = socket;
            mIPEndPoint = ip;
        }

        /// <summary>
        /// 获取Ip地址与端口
        /// </summary>
        /// <returns></returns>
        public IPEndPoint GetIPEndPoint()
        {
            return mIPEndPoint;
        }

        public long SessionID
        {
            get { return lSessionID; }
            set { lSessionID = value; }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <param name="verify"></param>
        public void SendMessage(byte[] byData, int length, int offset, bool verify)
        {
            ulong packetId = (ulong)SpawerPacketID();

            int nCount = length / mSpliteSize;
            if (length % mSpliteSize != 0)
                nCount += 1;

            int writeOffset = 0;
            int writeLength = 0;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                writeLength = (nIndex < nCount - 1) ? mSpliteSize : (length - nIndex * mSpliteSize);
                // 长度
                MemBlock block = mUdpSocket.Pool.Alloc(mPacketHeader + writeLength);
                long lUUID = (long)((packetId) << 32 | (uint)nCount << 16 | (uint)nIndex);

                writeOffset = 0;
                // 包唯一id
                Buffer.BlockCopy(BitConverter.GetBytes(lUUID), 0, block.GetBytes(), writeOffset, sizeof(long));
                writeOffset += sizeof(long);
                // 是否确认
                Buffer.BlockCopy(BitConverter.GetBytes(verify), 0, block.GetBytes(), writeOffset, sizeof(bool));
                writeOffset += sizeof(bool);
                // 协议类型
                Buffer.BlockCopy(BitConverter.GetBytes((byte)Protocol.Cmd), 0, block.GetBytes(), writeOffset, sizeof(byte));
                writeOffset += sizeof(byte);
                // 发送内容
                Buffer.BlockCopy(byData, offset, block.GetBytes(), writeOffset, writeLength);

                // 偏移量增加
                offset += writeLength;

                // 发送消息
                if (verify)
                    mUdpSocket.SendVerifyMessage(block, mIPEndPoint);
                else
                    mUdpSocket.SendMessage(block, mIPEndPoint);
            }
        }

        /// <summary>
        /// 发送确认包
        /// </summary>
        /// <param name="uuid"></param>
        protected void SendVerifyMessage(long uuid)
        {
            // 长度
            MemBlock block = mUdpSocket.Pool.Alloc(mPacketHeader);

            int writeOffset = 0;
            // 包唯一id
            Buffer.BlockCopy(BitConverter.GetBytes(uuid), 0, block.GetBytes(), writeOffset, sizeof(long));
            writeOffset += sizeof(long);
            // 是否确认
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, block.GetBytes(), writeOffset, sizeof(bool));
            writeOffset += sizeof(bool);
            // 协议类型
            Buffer.BlockCopy(BitConverter.GetBytes((byte)Protocol.Verify), 0, block.GetBytes(), writeOffset, sizeof(byte));

            // 发送回其他电脑
            mUdpSocket.SendMessage(block, mIPEndPoint);
        }

        /// <summary>
        /// 发送Ping消息
        /// </summary>
        /// <param name="verify">是否需要回执，客户端需要</param>
        protected void SendPingMessage(bool verify = true)
        {
            ulong packetId = (ulong)SpawerPacketID();
            long uuid = (long)((packetId) << 32 | (uint)1 << 16 | (uint)0);
            // 长度
            MemBlock block = mUdpSocket.Pool.Alloc(mPacketHeader);

            int writeOffset = 0;
            // 包唯一id
            Buffer.BlockCopy(BitConverter.GetBytes(uuid), 0, block.GetBytes(), writeOffset, sizeof(long));
            writeOffset += sizeof(long);
            // 是否确认
            Buffer.BlockCopy(BitConverter.GetBytes(verify), 0, block.GetBytes(), writeOffset, sizeof(bool));
            writeOffset += sizeof(bool);
            // 协议类型
            Buffer.BlockCopy(BitConverter.GetBytes((byte)Protocol.Ping), 0, block.GetBytes(), writeOffset, sizeof(byte));

            // 发送回其他电脑
            mUdpSocket.SendMessage(block, mIPEndPoint);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="byData"></param>
        public void Receive(byte[] byData)
        {
            if (byData.Length < mPacketHeader)
                return;

            Protocol protocol = GetProtocol(byData);
            switch(protocol)
            {
                case Protocol.Cmd:
                    if (!HandleRepeat(byData))
                        return;
                    DoVerify(byData);
                    ReceivePacket(byData);
                    break;
                case Protocol.Ping:
                    DoPing(byData);
                    break;
                case Protocol.Verify:
                    HandleVerify(byData);
                    break;
            }
        }

        /// <summary>
        /// 是否可以处理
        /// </summary>
        /// <param name="byData"></param>
        /// <returns></returns>
        protected bool HandleRepeat(byte[] byData)
        {
            long uuid = BitConverter.ToInt64(byData, 0);
            //Logger.GetLog("UDP").Debug("" + uuid);
            if (mReceivePacketId.Contains(uuid))
                return false;

            mReceivePacketId.Enqueue(uuid);
            if (mReceivePacketId.Count > MaxReceivePacketCount)
                mReceivePacketId.Dequeue();

            return true;
        }

        /// <summary>
        /// 获取协议
        /// </summary>
        /// <param name="byData"></param>
        /// <returns></returns>
        protected Protocol GetProtocol(byte[] byData)
        {
            return (Protocol)(byData[mPacketHeader - sizeof(byte)]);
        }

        /// <summary>
        /// 处理确认
        /// </summary>
        /// <param name="byData"></param>
        protected void DoVerify(byte[] byData)
        {
            // 是否需要确认
            bool bVerify = BitConverter.ToBoolean(byData, sizeof(long));
            if (bVerify)
            {
                long uuid = BitConverter.ToInt64(byData, 0);
                SendVerifyMessage(uuid);
            }
        }

        /// <summary>
        /// 做Ping 消息
        /// </summary>
        /// <param name="byData"></param>
        protected void DoPing(byte[] byData)
        {
            lLastPingTime = DateTime.Now.Ticks;

            // 看是否需要验证
            bool bVerify = BitConverter.ToBoolean(byData, sizeof(long));
            if (bVerify)
                SendPingMessage(false);
        }

        /// <summary>
        /// 处理确认
        /// </summary>
        /// <param name="byData"></param>
        protected bool HandleVerify(byte[] byData)
        {
            long uuid = BitConverter.ToInt64(byData, 0);
            mUdpSocket.PushDelMessage(uuid);

            return true;
        }

        /// <summary>
        /// 接收的分组的包
        /// </summary>
        /// <param name="byData"></param>
        protected void ReceivePacket(byte[] byData)
        {
            long uuid = BitConverter.ToInt64(byData, 0);
            InsertReceive(byData, uuid);
            if (!IsCompletePacket(uuid))
                return;

            byte[] byPacket = GetCompletePacket(uuid);
            RemoveCompletePacket(uuid);

            OnReceive(byPacket);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="byPacket"></param>
        protected void OnReceive(byte[] byPacket)
        {
            if (mHandleReceive != null)
                mHandleReceive(mNetType, this, byPacket, byPacket.Length, 0);

            //string strCode = Encoding.Default.GetString(byPacket);
            //Console.WriteLine(strCode);
        }

        /// <summary>
        /// 插入接收
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="uuid"></param>
        protected void InsertReceive(byte[] byData, long uuid)
        {
            lock (mPacketLock)
            {
                long packetId = uuid & 0XFFFFFFFF000000;
                int nCount = (int)(uuid & 0X00000000FFFF0000) >> 16;
                int nPacketIndex = (int)(uuid & 0X000000000000FFFF);

                List<byte[]> mPacketList;
                mPacketDict.TryGetValue(packetId, out mPacketList);
                if (mPacketList == null)
                {
                    mPacketList = new List<byte[]>();
                    for (int nIndex = 0; nIndex < nCount; ++nIndex)
                        mPacketList.Add(null);

                    mPacketDict.Add(packetId, mPacketList);
                }

                if (nPacketIndex < 0 || nPacketIndex >= mPacketList.Count)
                    return;

                mPacketList[nPacketIndex] = byData;
            }
        }

        /// <summary>
        /// 是否是完整的包
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        protected bool IsCompletePacket(long uuid)
        {
            lock (mPacketLock)
            {
                long packetId = uuid & 0XFFFFFFFF000000;
                List<byte[]> mPacketList;
                mPacketDict.TryGetValue(packetId, out mPacketList);
                if (mPacketList == null)
                    return false;

                return !mPacketList.Contains(null);
            }
        }

        /// <summary>
        /// 获取完整包
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        protected byte[] GetCompletePacket(long uuid)
        {
            lock (mPacketLock)
            {
                long packetId = uuid & 0XFFFFFFFF000000;
                List<byte[]> mPacketList;
                mPacketDict.TryGetValue(packetId, out mPacketList);
                if (mPacketList == null)
                    return null;

                int nCount = 0;
                foreach (byte[] byData in mPacketList)
                    nCount += (byData.Length - mPacketHeader);

                byte[] byPacket = new byte[nCount];
                int offset = 0;
                foreach (byte[] byData in mPacketList)
                {
                    Buffer.BlockCopy(byData, mPacketHeader, byPacket, offset, byData.Length - mPacketHeader);
                    offset += (byData.Length - mPacketHeader);
                }

                return byPacket;
            }
        }

        /// <summary>
        /// 删除完成的包
        /// </summary>
        /// <param name="uuid"></param>
        protected void RemoveCompletePacket(long uuid)
        {
            lock (mPacketLock)
            {
                long packetId = uuid & 0XFFFFFFFF000000;
                mPacketDict.Remove(packetId);
            }
        }

        /// <summary>
        /// 获取Hash值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return mIPEndPoint.ToString().GetHashCode();
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            UDPSession udpSession = obj as UDPSession;
            if (mIPEndPoint == null || udpSession == null)
                return base.Equals(obj);

            return mIPEndPoint.Equals(udpSession.GetIPEndPoint());
        }

        /// <summary>
        /// 产生唯一Id
        /// </summary>
        /// <returns></returns>
        protected int SpawerPacketID()
        {
            lock (mPacketIdLock)
            {
                return ++mPacketId;
            }
        }

        /// <summary>
        /// 重置包Id的锁
        /// </summary>
        protected void ResetPacketId()
        {
            lock (mPacketIdLock)
            {
                mPacketId = 0;
            }
        }

        /// <summary>
        /// 设置处理接收
        /// </summary>
        /// <param name="receive"></param>
        public void SetHandleReceive(ReceiveHandler receive)
        {
            mHandleReceive = receive;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Close()
        {

        }
    }
}