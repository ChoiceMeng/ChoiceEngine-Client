using Common.Log;
using Common.Net;
using Common.Pool;
using Common.Struct;
using FrameWork.Net.Adapter;
using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FrameWork.Net
{
    /// <summary>
    /// 消息管理器
    /// </summary>
    public class MessageManage : ModelManage
    {
        /// <summary>
        /// 锁标志
        /// </summary>
		protected int lockMark = 0;

        /// <summary>
        /// 适配工厂
        /// </summary>
        protected AdapterFactor mAdapterFactor = new AdapterFactor();

        /// <summary>
        /// 适配器ID
        /// </summary>
        protected long mAdapterID = 1000L;

        /// <summary>
        /// 适配器Id的锁
        /// </summary>
        protected object mAdapterIDLock = new object();

        /// <summary>
        /// 所有的网络适配器 AdapterID -- INetAdapter
        /// </summary>
        protected Dictionary<long, NetAdapter> mAdapterDict = new Dictionary<long, NetAdapter>();

        /// <summary>
        /// 网络适配器的锁
        /// </summary>
        protected object mAdapterLock = new object();

        /// <summary>
        /// 所有的网络适配器 与mAdapterDict的定义正好相反 
        /// ENet --> object(TcpSocket, IConnection, UdpSession, HttpClient, BinaryServlet) --> INetAdapter
        /// </summary>
        protected Dictionary<ENet, Dictionary<object, NetAdapter>> mAllAdapterDict = new Dictionary<ENet, Dictionary<object, NetAdapter>>();

        /// <summary>
        /// 所有适配器的锁
        /// </summary>
        protected object mAllAdapterLock = new object();

        /// <summary>
        /// 所有的消息回调
        /// </summary>
        protected Dictionary<int, MessageHandler> mAllMessageHandler = new Dictionary<int, MessageHandler>();

        /// <summary>
        /// 所有的消息类型
        /// </summary>
        protected Dictionary<int, Type> mAllMessageType = new Dictionary<int, Type>();

        /// <summary>
        /// 消息队列
        /// </summary>
        protected Queue<MessageCache> mMsgQueue = new Queue<MessageCache>();

        /// <summary>
        /// 消息队列锁
        /// </summary>
        protected object mMsgQueueLock = new object();

        /// <summary>
        /// 最大的处理数量
        /// </summary>
        protected const int mMaxProcessCount = 1000;

        /// <summary>
        /// 读取字符串
        /// </summary>
        protected ByteArray mReadMessage = new ByteArray();

        /// <summary>
        /// 写字符串
        /// </summary>
        protected ByteArray mWriteMessage = new ByteArray();

        /// <summary>
        /// 有发送信息的队列
        /// </summary>
        protected UniqueQueue<long> mAdapterQueue = new UniqueQueue<long>();

        /// <summary>
        /// 连接上的适配器
        /// </summary>
        protected UniqueQueue<long> mConnectAdapter = new UniqueQueue<long>();

        /// <summary>
        /// 断开的适配器
        /// </summary>
        protected UniqueQueue<long> mDisconnectAdapter = new UniqueQueue<long>();

        /// <summary>
        /// 下一个会话Id
        /// </summary>
        /// <returns></returns>
        protected long NextAdapterID()
        {
            lock (mAdapterIDLock)
            {
                return ++mAdapterID;
            }
        }

        public override void MainThread()
        {
            ProcessMessage();
            ProcessSend();
            ProcessConnectAdapter();
            ProcessDisconnectAdapter();
        }

        public override void Stop()
        {
        }

        #region Adapter Operate
        /// <summary>
        /// 获取适配器
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        protected NetAdapter GetAdapter(ENet net, object session)
        {
            lock(mAllAdapterLock)
            {
                if (session == null)
                    return null;
                Dictionary<object, NetAdapter> adapterDict;
                mAllAdapterDict.TryGetValue(net, out adapterDict);
                if (adapterDict == null)
                    return null;

                NetAdapter adapter;
                adapterDict.TryGetValue(session, out adapter);

                return adapter;
            }
        }

        /// <summary>
        /// 获取适配器
        /// </summary>
        /// <param name="lAdapterID"></param>
        /// <returns></returns>
        protected NetAdapter GetAdapter(long lAdapterID)
        {
            lock(mAdapterLock)
            {
                NetAdapter adapter;
                mAdapterDict.TryGetValue(lAdapterID, out adapter);

                return adapter;
            }
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        /// <param name="adapter"></param>
        protected void AddAdapter(ENet net, object session, NetAdapter adapter)
        {
            lock(mAllAdapterLock)
            {
                Dictionary<object, NetAdapter> adapterDict;
                mAllAdapterDict.TryGetValue(net, out adapterDict);
                if (adapterDict == null)
                {
                    adapterDict = new Dictionary<object,NetAdapter>();
                    mAllAdapterDict.Add(net, adapterDict);
                }

                // 重复了
                if( adapterDict.ContainsKey(session) )
                {
                    return;
                }

                adapterDict.Add(session, adapter);
            }
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="lAdapterID"></param>
        /// <param name="adapter"></param>
        protected void AddAdapter(long lAdapterID, NetAdapter adapter)
        {
            lock(mAdapterLock)
            {
                if (mAdapterDict.ContainsKey(lAdapterID))
                    return;

                mAdapterDict.Add(lAdapterID, adapter);
            }
        }

        /// <summary>
        /// 删除适配器
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        protected void RemoveAdapter(ENet net, object session)
        {
            lock (mAllAdapterLock)
            {
                Dictionary<object, NetAdapter> adapterDict;
                mAllAdapterDict.TryGetValue(net, out adapterDict);
                if (adapterDict == null)
                    return;

                adapterDict.Remove(session);
            }
        }

        /// <summary>
        /// 删除适配器
        /// </summary>
        /// <param name="lAdapterID"></param>
        protected void RemoveAdapter(long lAdapterID)
        {
            lock(mAdapterLock)
            {
                mAdapterDict.Remove(lAdapterID);
            }
        }
        #endregion

        #region Event Handle
        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        protected long ConnectionHandle(ENet net, object session)
        {
            NetAdapter adapter = GetAdapter(net, session);
            // 已经存在这个适配器了
            if (adapter != null)
            {
                if( net == ENet.UdpClient )
                    mConnectAdapter.Enqueue(adapter.AdapterID);
                return adapter.AdapterID;
            }

            // 创建适配器
            adapter = mAdapterFactor.CreateAdapter(net);
            adapter.Init(NextAdapterID(), net, session);

            AddAdapter(adapter.AdapterID, adapter);
            AddAdapter(net, session, adapter);

            mConnectAdapter.Enqueue(adapter.AdapterID);

            return adapter.AdapterID;
        }

        /// <summary>
        /// 处理连接的适配器
        /// </summary>
        protected void ProcessConnectAdapter()
        {
            int nCount = mConnectAdapter.Count;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                long lAdapterId = mConnectAdapter.Dequeue();
                NetAdapter adapter = GetAdapter(lAdapterId);
                if (adapter == null)
                    continue;

                OnConnected(adapter);
            }
        }

        /// <summary>
        /// 当有连接连接上的时候
        /// </summary>
        /// <param name="param"></param>
        protected virtual void OnConnected(NetAdapter adapter)
        {

        }

        /// <summary>
        /// 断开连接处理
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        protected void DisconectionHandle(ENet net, object session)
        {
            NetAdapter adapter = GetAdapter(net, session);
            if (adapter == null)
                return;

            mDisconnectAdapter.Enqueue(adapter.AdapterID);
        }

        /// <summary>
        /// 处理掉线
        /// </summary>
        protected void ProcessDisconnectAdapter()
        {
            int nCount = mDisconnectAdapter.Count;
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                long lAdapterId = mDisconnectAdapter.Dequeue();
                NetAdapter adapter = GetAdapter(lAdapterId);
                if (adapter == null)
                    continue;

                OnDisconnect(adapter);

                RemoveAdapter(adapter.NetType, adapter.GetSession());
                RemoveAdapter(adapter.AdapterID);
            }
        }

        /// <summary>
        /// 断线
        /// </summary>
        /// <param name="adapter"></param>
        protected virtual void OnDisconnect(NetAdapter adapter)
        {

        }

        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        /// <param name="byData"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        protected void ReceiveHandle(ENet net, object session, byte[] byData, int length, int offset)
        {
            NetAdapter adapter = GetAdapter(net, session);
            if (adapter == null)
                return;

            // 这里处理接收消息，转化成Message
            if (byData == null || byData.Length < (length + offset) || length < sizeof(short) + sizeof(int))
                return;

            MemBlock block = NetAdapter.Alloc(length);
            Buffer.BlockCopy(byData, offset, block.GetBytes(), 0, length);

            MessageCache cache = new MessageCache();
            cache.AdapterID = adapter.AdapterID;
            cache.Block = block;

            PushMessageQueue(cache);
        }

        /// <summary>
        /// 添加消息队列
        /// </summary>
        /// <param name="msgCache"></param>
        protected void PushMessageQueue(MessageCache msgCache)
        {
#if !UNITY_IPHONE
			while (true) {
				if(Interlocked.CompareExchange(ref lockMark,1,0) == 0)
				{
					mMsgQueue.Enqueue(msgCache);
					Interlocked.Decrement(ref lockMark);
					break;
				}else
				{
					Thread.Sleep(50);
				}

			}
#else
            lock (mMsgQueueLock)
            {
                mMsgQueue.Enqueue(msgCache);
            }
#endif
        }

        /// <summary>
        /// 返回一个消息
        /// </summary>
        /// <returns></returns>
		/*
        protected MessageCache PopMessageQueue()
        {
            lock (mMsgQueueLock)
            {
                if (mMsgQueue.Count == 0)
                    return null;

                return mMsgQueue.Dequeue();
            }
        }
*/
        /// <summary>
        /// 消息数量
        /// </summary>
        /// <returns></returns>
        /*
		protected int GetMessageQueueCount()
        {
            lock (mMsgQueueLock)
            {
                return mMsgQueue.Count;
            }
        }
        */
        #endregion

        #region Message
        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="msgHandler"></param>
        /// <param name="type"></param>
        protected override void RegisterMessage(int msgId, MessageHandler msgHandler, Type type)
        {
            RegisterMessage(msgId, msgHandler);
            RegisterMessage(msgId, type);
        }
        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="msgHandler"></param>
        protected void RegisterMessage(int msgId, MessageHandler msgHandler)
        {
            // 已经注册过了
            if (mAllMessageHandler.ContainsKey(msgId))
                return;

            mAllMessageHandler.Add(msgId, msgHandler);
        }

        /// <summary>
        /// 派发消息
        /// </summary>
        /// <param name="lAdapterID"></param>
        /// <param name="msg"></param>
        protected void DispatchMessage(long lAdapterID, Message msg)
        {
            msg.AdapterId = lAdapterID;

            MessageHandler handler;
            mAllMessageHandler.TryGetValue(msg.MsgId, out handler);
            if (handler == null)
                return;

            handler(msg);
        }

        /// <summary>
        /// 注册消息类型
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="type"></param>
        protected void RegisterMessage(int msgId, Type type)
        {
            if (mAllMessageType.ContainsKey(msgId))
                return;

            mAllMessageType.Add(msgId, type);
        }

        /// <summary>
        /// 创建消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        protected Message CreateMessage(int msgId)
        {
            Type type;
            mAllMessageType.TryGetValue(msgId, out type);
            if (type == null)
                return new BinaryMessage();

            return Activator.CreateInstance(type) as Message;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        protected void ProcessMessage()
        {
#if !UNITY_IPHONE
			while (true) {
				if(Interlocked.CompareExchange(ref lockMark,1,0) == 0)
				{
					MessageCache[] msgs = null;
					lock(mMsgQueueLock)
					{
						msgs = mMsgQueue.ToArray();
						mMsgQueue.Clear();
					}
					Interlocked.Decrement(ref lockMark);
					int count = msgs.Length;
					for(int i = 0; i < count; i++)
					{
						MessageCache msgCache = msgs[i];
						
						ProcessMessage(msgCache);
						FreeMessage(msgCache);
					}
					break;
				}else
				{
					Thread.Sleep(5);
				}
			}
#else
            int nCount = Math.Min(mMaxProcessCount, GetMessageQueueCount());
            if (nCount > 0)
                Logger.GetLog("Message").Debug("ProcessMessage All Count = " + nCount);

            for(int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                MessageCache msgCache = PopMessageQueue();
                if (msgCache == null)
                    continue;

                ProcessMessage(msgCache);
                FreeMessage(msgCache);
            }
#endif
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="msgCache"></param>
        protected void ProcessMessage(MessageCache msgCache)
        {
            Message msg = ReadMessage(msgCache);
            if (msg == null)
                return;

            try
            {
                TerminalFunction.LogMessage(msg.MsgId);
                DispatchMessage(msgCache.AdapterID, msg);
                OnDispatchMessage(msg);
                OnFreeMessage(msg);
            }
            catch(Exception ex)
            {
                Logger.GetLog("FrameWork").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 提前处理消息
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnDispatchMessage(Message message)
        {
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        /// <param name="msgCache"></param>
        /// <returns></returns>
        protected Message ReadMessage(MessageCache msgCache)
        {
            Message msg = null;
            try
            {
                int length = BitConverter.ToInt32(msgCache.Block.GetBytes(), 0);
                int msgId = BitConverter.ToInt32(msgCache.Block.GetBytes(), sizeof(int));
                msg = CreateMessage(msgId);
                OnCreateMessage(msg, msgCache.Block.UseSize);
                Buffer.BlockCopy(msgCache.Block.GetBytes(), 0, mReadMessage.GetBytes(), 0, msgCache.Block.UseSize);
                msg.SetByteArray(mReadMessage);
                msg.ParseObj();
            }
            catch(Exception ex)
            {
                Logger.GetLog("FrameWork").Error(ex.ToString());
            }

            return msg;
        }

        /// <summary>
        /// 创建消息时候处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="length"></param>
        protected void OnCreateMessage(Message msg, int length)
        {
            BinaryMessage binaryMessage = msg as BinaryMessage;
            if (binaryMessage == null)
                return;

            binaryMessage.mBlock = NetAdapter.Alloc(length - msg.HeadLength);
        }

        /// <summary>
        /// 释放消息
        /// </summary>
        /// <param name="msgCache"></param>
        protected void FreeMessage(MessageCache msgCache)
        {
            //NetAdapter.Free(msgCache.Block);
        }

        /// <summary>
        /// 释放消息的同时调用
        /// </summary>
        /// <param name="msg"></param>
        protected void OnFreeMessage(Message msg)
        {
            //BinaryMessage binaryMessage = msg as BinaryMessage;
            //if (binaryMessage == null)
            //    return;

            //NetAdapter.Free(binaryMessage.mBlock);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="lAdapterID"></param>
        /// <param name="msg"></param>
        /// <param name="Immediate">true: 即刻发送; false: 等待发送</param>
        public override void SendMessage(long lAdapterID, Message msg, bool Immediate = true)
        {
            NetAdapter adapter = GetAdapter(lAdapterID);
            if (adapter == null)
                return;

            msg.SetByteArray(mWriteMessage);
            msg.DeconstructObj();

            MemBlock block = NetAdapter.SendAlloc(msg.GetWritePos());
            Buffer.BlockCopy(msg.GetBytes(), 0, block.GetBytes(), 0, msg.GetWritePos());
            adapter.PushSend(block);

            if (Immediate)
                ImmediateSendMessage(lAdapterID);

            //TerminalFunction.LogMessage(msg.MsgId);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="net"></param>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        /// <param name="Immediate"></param>
        public override void SendMessage(ENet net, object session, Message msg, bool Immediate = true)
        {
            NetAdapter adapter = GetAdapter(net, session);
            if (adapter == null)
                return;

            SendMessage(adapter.AdapterID, msg, Immediate);
        }

        /// <summary>
        /// 回复消息，Http服务器用
        /// </summary>
        /// <param name="lAdapterID"></param>
        protected override void ImmediateSendMessage(long lAdapterID)
        {
            NetAdapter adapter = GetAdapter(lAdapterID);
            if (adapter == null)
                return;

            mAdapterQueue.Enqueue(lAdapterID);
        }

        /// <summary>
        /// 处理发送
        /// </summary>
        protected void ProcessSend()
        {
            long lAdapterId = 0L;
            NetAdapter adapter = null;
            int nCount = Math.Min(mMaxProcessCount, mAdapterQueue.Count);
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                lAdapterId = mAdapterQueue.Dequeue();
                adapter = GetAdapter(lAdapterId);
                if (adapter == null)
                    continue;

                adapter.ImmediateSendMessage();
            }
        }
        #endregion
    }
}


