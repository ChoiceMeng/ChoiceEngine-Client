using Common.Frame;
using Common.Log;
//using Common.Log;
using NetCommon.CallBack;
using NetCommon.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetCommon.TCP
{
    /// <summary>
    /// TCP 的客户端
    /// </summary>
    public class TCPClient
    {
        /// <summary>
        /// 连接处理
        /// </summary>
        public ConnectedHandler Connected;

        /// <summary>
        /// 断开连接
        /// </summary>
        public DisconnectHandler Disconnect;

        /// <summary>
        /// 接收数据
        /// </summary>
        public ReceiveHandler Receive;

        /// <summary>
        /// 连接Socket
        /// </summary>
        private Socket mSocket = null;

        /// <summary>
        /// 连接服务器Ip
        /// </summary>
        private string mRemoteIp = "127.0.0.1";

        /// <summary>
        /// 连接的端口
        /// </summary>
        private int mRemotePort = 8401;

        /// <summary>
        /// 最大大小
        /// </summary>
        private const int mMaxSize = 102400;

        /// <summary>
        /// 最大的发送数量
        /// </summary>
        private const int mMaxSendSize = 20;

        /// <summary>
        /// 读的数据
        /// </summary>
        private byte[] mReadData = new byte[mMaxSize];

        /// <summary>
        /// 数据
        /// </summary>
        private byte[] mReceiveData = new byte[mMaxSize];

        /// <summary>
        /// 缓冲二进制数组从哪里开始读
        /// </summary>
        private int mReadOffset = 0;

        /// <summary>
        /// 接收偏移
        /// </summary>
        private int mReceiveOffset = 0;

        /// <summary>
        /// 是否已经连接了
        /// </summary>
        protected bool mIsConnected = false;

        /// <summary>
        /// 是否正在发送
        /// </summary>
        protected bool mIsSending = false;

        /// <summary>
        /// 消息队列
        /// </summary>
        protected Queue<MemBlock> mMsgQueue = new Queue<MemBlock>();

        /// <summary>
        /// 消息队列的锁
        /// </summary>
        protected object mMsgQueueLock = new object();

        /// <summary>
        /// 内存池子
        /// </summary>
        protected MemPool mPool = new MemPool();

        /// <summary>
        /// 发送内存
        /// </summary>
        protected MemBlock mMem = new MemBlock(mMaxSize);

        /// <summary>
        /// 帧限制
        /// </summary>
        protected FrameLimite mFrame = new FrameLimite();

        /// <summary>
        /// 发送线程
        /// </summary>
        protected Thread mSendThread;

        /// <summary>
        /// 连接Timer
        /// </summary>
        protected Timer mConnectTimer;

        /// <summary>
        /// 重连间隔
        /// </summary>
        protected long mReConnectSpace = 3000L;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TCPClient()
        {
            ResetSocket();
        }

        /// <summary>
        /// 用户保存数据
        /// </summary>
        public object UserData
        {
            get;
            set;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            mSendThread = new Thread(SendThread);
            mSendThread.Start();
        }

        public void Stop()
        {
            mFrame.SetExit(true);
            ResetSocket();
        }

        /// <summary>
        /// 发送线程
        /// </summary>
        protected void SendThread()
        {
            mFrame.SetFrameCalled(SendFrame);
            mFrame.StartFrame();
        }

        /// <summary>
        /// 发送线程的帧
        /// </summary>
        protected void SendFrame()
        {
            SendQueueMessage();
        }

        /// <summary>
        /// 重置Socket
        /// </summary>
        public void ResetSocket()
        {
            if (mSocket != null)
                mSocket.Close();

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mIsConnected = false;
            mIsSending = false;
            mReadOffset = 0;
            mReceiveOffset = 0;
        }

        /// <summary>
        /// 设置服务器信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SetRemoteEndPoint(string ip, int port)
        {
            this.mRemoteIp = ip;
            this.mRemotePort = port;
        }

        /// <summary>
        /// 开始连接网络
        /// </summary>
        /// <param name="strIp"></param>
        /// <param name="nPort"></param>
        public void Connect()
        {
            try
            {
                IPEndPoint address = new IPEndPoint(IPAddress.Parse(mRemoteIp), mRemotePort);
                IAsyncResult iar = mSocket.BeginConnect(address, new AsyncCallback(Connected_CallBack), mSocket); //建立异步连接服务 , Connected 进行监听
                mConnectTimer = new Timer(Connected_TimeOut, iar, mReConnectSpace, mReConnectSpace);
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 连接超时
        /// </summary>
        /// <param name="state"></param>
        protected void Connected_TimeOut(object state)
        {
            try
            {
                if (mConnectTimer != null)
                {
                    lock (mConnectTimer)
                    {
                        mConnectTimer.Dispose();
                        mConnectTimer = null;
                    }
                }
                
                IAsyncResult iar = state as IAsyncResult;
                if (!iar.IsCompleted)
                {
                    ResetSocket();
                    Connect();
                }
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 建立连接成功的回调
        /// </summary>
        /// <param name="iar"></param>
        private void Connected_CallBack(IAsyncResult iar)
        {
            try
            {
                Socket client = (Socket)iar.AsyncState;
                client.EndConnect(iar);                

                mIsConnected = true;

                if (Connected != null)
                    Connected(ENet.TcpClient, this);

                StartReceive();

                if (mConnectTimer != null)
                {
                    lock (mConnectTimer)
                    {
                        mConnectTimer.Dispose();
                        mConnectTimer = null;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
                lock (mConnectTimer)
                {
                    mConnectTimer.Dispose();
                    mConnectTimer = null;
                }
                Connect();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public void SendMessage(byte[] byData, int length, int offset)
        {
            MemBlock block = mPool.Alloc(length);
            Buffer.BlockCopy(byData, offset, block.GetBytes(), 0, length);

            AddSendQueue(block);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        protected void SendData(MemBlock data)
        {
            // 连接了之后才能发送
            if (!mIsConnected)
                return;

            try
            {
                mIsSending = true;

                IAsyncResult iar = mSocket.BeginSend(data.GetBytes(), 0, data.UseSize, SocketFlags.None, new AsyncCallback(SendDataCallBack), data);
            }
            catch (SocketException ex)
            {
                string strError = ex.ToString();
                Logger.GetLog("NetCommon").Error(strError);

            }
            catch (Exception ex)
            {
                string strError = ex.ToString();
                Logger.GetLog("NetCommon").Error(strError);
            }
        }

        /// <summary>
        /// 添加到消息队列
        /// </summary>
        /// <param name="data"></param>
        protected void AddSendQueue(MemBlock data)
        {
            lock (mMsgQueueLock)
            {
                mMsgQueue.Enqueue(data);
            }
        }

        protected int GetSendQueueCount()
        {
            lock(mMsgQueueLock)
            {
                return mMsgQueue.Count;
            }
        }

        /// <summary>
        /// 弹出一个消息
        /// </summary>
        /// <returns></returns>
        protected MemBlock PopSendQueue()
        {
            lock(mMsgQueue)
            {
                if (mMsgQueue.Count == 0)
                    return new MemBlock();

                return mMsgQueue.Dequeue();
            }
        }

        /// <summary>
        /// 发送数据结束
        /// </summary>
        /// <param name="iar"></param>
        private void SendDataCallBack(IAsyncResult iar)
        {
            try
            {
                //var block = iar.AsyncState as MemBlock;
                //mPool.Free(block);
                int sent = mSocket.EndSend(iar);
            }
            catch(SocketException ex)
            {
                string strError = ex.ToString();
            }
            finally
            {
                mIsSending = false;
            }
        }

        /// <summary>
        /// 发送队列
        /// </summary>
        protected void SendQueueMessage()
        {
            if (mIsSending || !mIsConnected || GetSendQueueCount() == 0)
                return;

            mMem.UseSize = 0;
            int nCount = Math.Min(GetSendQueueCount(), mMaxSendSize);
            for (int nIndex = 0; nIndex < nCount; ++nIndex )
            {
                MemBlock block = PopSendQueue();

                Buffer.BlockCopy(block.GetBytes(), 0, mMem.GetBytes(), mMem.UseSize, block.UseSize);
                mMem.UseSize += block.UseSize;

                mPool.Free(block);
            }

                // 发送数据
            SendData(mMem);
        }

        /// <summary>
        /// 开始接收消息
        /// </summary>
        private void StartReceive()
        {
            if (mSocket == null || !mSocket.Connected)
                return;

            mSocket.BeginReceive(mReadData, mReceiveOffset, mReadData.Length - mReceiveOffset, SocketFlags.None, new AsyncCallback(EndReceive), mSocket);
        }

        /// <summary>
        /// 接收消息结束
        /// </summary>
        /// <param name="iar"></param>
        private void EndReceive(IAsyncResult iar)
        {
            try
            {
                Logger.GetLog("Message").Debug("EndReceive: IP Remote :" + mSocket.RemoteEndPoint);
                Socket remote = (Socket)iar.AsyncState;
                int recv = remote.EndReceive(iar);

                mReceiveOffset += recv;

                // 消息长度异常
                if (recv <= 0)
                {
                    if (Disconnect != null)
                        Disconnect(ENet.TcpClient, this);

                    return;
                }

                int nReceive = 0;
                while (mReadOffset != mReceiveOffset)
                {
                    nReceive = ReadReceive();
                    if (0 >= nReceive)
                        break;
                }

                if (nReceive == 0 || mReadOffset == mReceiveOffset)
                {
                    ReceiveSuccess();
                }
                else if( nReceive == -1 )
                {
                    ResetReceive();
                }
                else if (mReadOffset !=  mReceiveOffset)
                {
                    Logger.GetLog("NetCommon").Error("异常了, 重新来吧，不要影响整体消息");
                    ResetReceive();
                }
                else
                {
                    Logger.GetLog("NetCommon").Error("未处理异常");
                    ResetReceive();
                }
            }
            catch (SocketException ex)
            {
                int nErrorCode = ex.ErrorCode;
                if ((int)SocketError.ConnectionReset == nErrorCode)
                {
                    ResetSocket();
                    if (Disconnect != null)
                        Disconnect(ENet.TcpClient, this);
                }

                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
            finally
            {
                if (mSocket != null && mSocket.Connected == true)
                    StartReceive();
            }
        }

        /// <summary>
        /// 接收成功处理
        /// </summary>
        protected void ReceiveSuccess()
        {
            // 剩余没有读取的
            if (mReadOffset < mReceiveOffset && mReadOffset != 0)
            {
                int nUnReadLength = mReceiveOffset - mReadOffset;

                Buffer.BlockCopy(mReadData, mReadOffset, mReceiveData, 0, nUnReadLength);

                Array.Clear(mReadData, 0, mReceiveOffset);

                Buffer.BlockCopy(mReceiveData, 0, mReadData, 0, nUnReadLength);
            }

            mReceiveOffset = mReceiveOffset - mReadOffset;
            mReadOffset = 0;
        }

        /// <summary>
        /// 接收状态重置
        /// </summary>
        protected void ResetReceive()
        {
            mReadOffset = 0;
            mReceiveOffset = 0;
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <returns></returns>
        protected int ReadReceive()
        {
            try
            {
                if (mReadOffset + sizeof(int) > mReceiveOffset)
                    return 0;

                int nLength = BitConverter.ToInt32(mReadData, mReadOffset);
                if (nLength < 0 || nLength >= 102400)
                    return -1;

                // 需要粘包
                if (mReadOffset + nLength + sizeof(int) > mReceiveOffset)
                    return 0;

                // 处理数据接收
                if (Receive != null)
                    Receive(ENet.TcpClient, this, mReadData, nLength + sizeof(int), mReadOffset);

                // 偏移量增加
                mReadOffset += (nLength + sizeof(int));

                return nLength + sizeof(int);
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }

            return -1;
        }
    }
}
