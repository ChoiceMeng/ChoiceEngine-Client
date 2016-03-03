using Common.Frame;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetCommon.UDP
{
    /// <summary>
    /// UDP网络
    /// </summary>
    public class UDPSocket
    {
        /// <summary>
        /// Udp的连接
        /// </summary>
        protected UdpClient mUdpClient = null;

        /// <summary>
        /// 远程Ip
        /// </summary>
        protected IPEndPoint mRemote = null;

        /// <summary>
        /// 发送线程
        /// </summary>
        private FrameThread mSendThread = null;

        /// <summary>
        /// 发送队列
        /// </summary>
        protected Dictionary<long, SendData> mSendQueue = new Dictionary<long, SendData>();

        /// <summary>
        /// 发送队列的Key值
        /// </summary>
        protected List<long> mSendQueueKey = new List<long>();

        /// <summary>
        /// 发送的锁
        /// </summary>
        protected object mSendLock = new object();

        /// <summary>
        /// 内存池
        /// </summary>
        protected MemPoolNoLock mMemPool = new MemPoolNoLock();

        /// <summary>
        /// 接收完成
        /// </summary>
        public ReceiveEvent ReceiveCompleted;

        /// <summary>
        /// 重新发送
        /// </summary>
        protected const int mReSendSpace = 1000;

        /// <summary>
        /// 重新发送次数
        /// </summary>
        protected const int mReSendCount = 20;

        /// <summary>
        /// 删除列表
        /// </summary>
        protected Queue<long> mDelList = new Queue<long>();

        /// <summary>
        /// 删除的锁
        /// </summary>
        protected object mDelLock = new object();

        /// <summary>
        /// 是否已经连接上
        /// </summary>
        protected bool IsConnected = false;

        /// <summary>
        /// 端口
        /// </summary>
        protected int mPort = 0;

        /// <summary>
        /// 内存池
        /// </summary>
        public MemPoolNoLock Pool
        {
            get { return mMemPool; }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UDPSession Connect(string ip, int port)
        {
            mRemote = new IPEndPoint(IPAddress.Parse(ip), port);

            mUdpClient = new UdpClient();

            //mUdpClient.AllowNatTraversal(true);
            mUdpClient.ExclusiveAddressUse = false;

            ConnectRemote(mRemote);

            return new UDPSession(this, mRemote);
        }

        /// <summary>
        /// 远程连接
        /// </summary>
        /// <param name="remoteIPEndPoint"></param>
        /// <returns></returns>
        protected bool ConnectRemote(IPEndPoint remoteIPEndPoint)
        {
            try {
                mUdpClient.Connect(remoteIPEndPoint);
            }
            catch (Exception ex) {
                Logger.GetLog("NetCommon").Error(ex.ToString());
                return false;
            }

            CreateThread();
            IsConnected = true;

            return true;
        }

        /// <summary>
        /// 绑定一个端口
        /// </summary>
        /// <param name="port"></param>
        public void Bind(int port)
        {
            mUdpClient = new UdpClient(port);
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            mUdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

            CreateThread();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            mUdpClient.Close();
            mUdpClient = null;
            mSendThread.Stop();
        }

        /// <summary>
        /// 创建线程
        /// </summary>
        protected void CreateThread()
        {
            mSendThread = new FrameThread(SendVerifyThread);
            mSendThread.Start();

            StartReceive();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="block"></param>
        /// <param name="ipEndPoint"></param>
        public virtual void SendMessage(MemBlock block, IPEndPoint ipEndPoint)
        {
            try
            {
                // 已经Connect过的连接不能发送到远程端口
                if (mRemote == null)
                    mUdpClient.Send(block.GetBytes(), block.UseSize, ipEndPoint);
                else
                    mUdpClient.Send(block.GetBytes(), block.UseSize);
            }
            catch(SocketException ex)
            {
                // 发送失败，一般是网络连接出错
                if( ex.SocketErrorCode == SocketError.InvalidArgument )
                {
                    Logger.GetLog("NetCommon").Error("Net Link Error, Check Network Card Start Or Network Link Is OK!");
                }
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 发送验证消息
        /// </summary>
        /// <param name="block"></param>
        /// <param name="ipEndPoint"></param>
        public void SendVerifyMessage(MemBlock block, IPEndPoint ipEndPoint)
        {
            SendMessage(block, ipEndPoint);
            PushSendData(block, ipEndPoint);
        }

        /// <summary>
        /// 添加到发送队列，以后定时检查，超时重发
        /// </summary>
        /// <param name="block"></param>
        /// <param name="ipEndPoint"></param>
        /// <param name="sendEvent"></param>
        protected void PushSendData(MemBlock block, IPEndPoint ipEndPoint)
        {
            SendData sendData = new SendData();
            sendData.EndPoint = ipEndPoint;
            sendData.EnterTime = Environment.TickCount;
            sendData.Packet = block;
            sendData.UUID = BitConverter.ToInt64(block.GetBytes(), 0);

            PushSend(sendData);
        }

        /// <summary>
        /// 添加发送
        /// </summary>
        /// <param name="byData"></param>
        protected void PushSend(SendData sendData)
        {
            lock (mSendLock)
            {
                if (mSendQueue.ContainsKey(sendData.UUID))
                {
                    return;
                }

                mSendQueueKey.Add(sendData.UUID);
                mSendQueue.Add(sendData.UUID, sendData);
            }
        }

        /// <summary>
        /// 弹出一个发送消息
        /// </summary>
        /// <returns></returns>
        protected SendData GetSend(long uuid)
        {
            lock (mSendLock)
            {
                SendData data;
                mSendQueue.TryGetValue(uuid, out data);

                return data;
            }
        }

        /// <summary>
        /// 根据索引获取发送的包
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        protected SendData GetSendByIndex(int nIndex)
        {
            lock (mSendLock)
            {
                long uuid = 0;
                if (nIndex < mSendQueueKey.Count && nIndex >= 0)
                    uuid = mSendQueueKey[nIndex];

                SendData data;
                mSendQueue.TryGetValue(uuid, out data);

                return data;
            }
        }

        /// <summary>
        /// 发送队列的数量
        /// </summary>
        /// <returns></returns>
        protected int GetSendQueueCount()
        {
            lock (mSendLock)
            {
                return mSendQueue.Count;
            }
        }

        /// <summary>
        /// 发送验证线程
        /// </summary>
        protected void SendVerifyThread()
        {
            DoDelMessage();
            ReSendMessage();
        }

        /// <summary>
        /// 重新发送
        /// </summary>
        protected void ReSendMessage()
        {
            int nCount = GetSendQueueCount();
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                SendData data = GetSendByIndex(nIndex);
                if (data == null)
                    continue;

                if (Environment.TickCount - data.EnterTime < mReSendSpace)
                    continue;

                SendMessage(data.Packet, data.EndPoint);
                data.EnterTime = Environment.TickCount;
                data.SendTimes += 1;

                if (data.SendTimes >= mReSendCount)
                    PushDelMessage(data.UUID);
            }
        }

        /// <summary>
        /// 添加删除消息
        /// </summary>
        /// <param name="uuid"></param>
        public void PushDelMessage(long uuid)
        {
            lock (mDelLock)
            {
                mDelList.Enqueue(uuid);
            }
        }

        /// <summary>
        /// 弹出一个删除消息
        /// </summary>
        /// <returns></returns>
        protected long PopDelMessage()
        {
            lock (mDelLock)
            {
                if (mDelList.Count == 0)
                    return 0;
                return mDelList.Dequeue();
            }
        }

        /// <summary>
        /// 删除消息的数量
        /// </summary>
        /// <returns></returns>
        protected int GetDelMessageCount()
        {
            lock (mDelLock)
            {
                return mDelList.Count;
            }
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="uuid"></param>
        public void DelMessage(long uuid)
        {
            SendData sendData;
            lock (mSendLock)
            {
                mSendQueueKey.Remove(uuid);
                mSendQueue.TryGetValue(uuid, out sendData);
                mSendQueue.Remove(uuid);
            }

            //if (sendData != null)
            //{
            //    mMemPool.Free(sendData.Packet);
            //}
        }

        /// <summary>
        /// 处理删除消息
        /// </summary>
        protected void DoDelMessage()
        {
            int nCount = GetDelMessageCount();
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                long uuid = PopDelMessage();
                DelMessage(uuid);
            }
        }

        /// <summary>
        /// 开始接收
        /// </summary>
        protected void StartReceive()
        {
            try
            {
                mUdpClient.BeginReceive(ReceiveCallBack, null);
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error("Network Host Not Start!" + ex.ToString());
                Close();
                if (mRemote == null)
                    Bind(mPort);
                else
                    Connect(mRemote.Address.ToString(), mRemote.Port);
            }
        }

        /// <summary>
        /// 接收回调
        /// </summary>
        /// <param name="iar"></param>
        protected void ReceiveCallBack(IAsyncResult iar)
        {
            IPEndPoint remoteEP = null;
            byte[] byReceive = null;
            try
            {
                byReceive = mUdpClient.EndReceive(iar, ref remoteEP);

                // 接收回调
                if (ReceiveCompleted != null && byReceive != null)
                    ReceiveCompleted(remoteEP, byReceive);
            }
            catch (SocketException ex)
            {
                // 远程主机关闭了这个链接, 可以进行重新连接了
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Logger.GetLog("NetCommon").Error("Network Host Not Start!");
                }
            }
            finally
            {
                // 再接收
                StartReceive();
            }
        }
    }
}