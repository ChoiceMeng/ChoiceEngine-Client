using NetCommon.CallBack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace NetCommon.UDP
{
    /// <summary>
    /// Udp服务器
    /// </summary>
    public class UDPServer
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
        /// 连接Id
        /// </summary>
        private long mSessionID = 1000000L;

        /// <summary>
        /// 网络连接
        /// </summary>
        protected UDPSocket mSocket = new UDPSocket();

        /// <summary>
        /// 所有的会话列表
        /// </summary>
        protected Dictionary<long, UDPSession> mSessionDict = new Dictionary<long, UDPSession>();

        /// <summary>
        /// 会话列表
        /// </summary>
        protected Dictionary<IPEndPoint, long> mSessionSet = new Dictionary<IPEndPoint, long>();

        /// <summary>
        /// 会话的锁
        /// </summary>
        protected object mSessionLock = new object();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="port"></param>
        public virtual void Init(int port)
        {
            mSocket.Bind(port);
            mSocket.ReceiveCompleted += OnReceive;
        }

        /// <summary>
        /// 下一个SessionId
        /// </summary>
        /// <returns></returns>
        protected long NetSessionID()
        {
            return Interlocked.Increment(ref mSessionID);
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="udpSession"></param>
        /// <param name="byData"></param>
        protected void OnReceive(IPEndPoint ipEndPoint, byte[] byData)
        {
            UDPSession udpSession;
            if (HasSession(ipEndPoint))
            {
                long lSessionID = GetSessionID(ipEndPoint);
                udpSession = GetSession(lSessionID);
            }
            else
            {
                udpSession = new UDPSession(mSocket, ipEndPoint);
                udpSession.SetHandleReceive(Receive);
                InsertSession(udpSession);
            }

            // 接收处理
            udpSession.Receive(byData);
        }

        /// <summary>
        /// 根据SessionID获取到已经存在的会话
        /// </summary>
        /// <param name="lSessionID"></param>
        /// <returns></returns>
        protected UDPSession GetSession(long lSessionID)
        {
            lock (mSessionLock)
            {
                UDPSession session;
                mSessionDict.TryGetValue(lSessionID, out session);
                return session;
            }
        }

        /// <summary>
        /// 是否存在这个Session
        /// </summary>
        /// <param name="lSessionID"></param>
        /// <returns></returns>
        protected bool HasSession(long lSessionID)
        {
            lock (mSessionLock)
            {
                return mSessionDict.ContainsKey(lSessionID);
            }
        }

        /// <summary>
        /// 是否存在这个Session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected bool HasSession(IPEndPoint ipEndPoint)
        {
            lock (mSessionLock)
            {
                return mSessionSet.ContainsKey(ipEndPoint);
            }
        }

        /// <summary>
        /// 获取会话ID
        /// </summary>
        /// <param name="udpSession"></param>
        /// <returns></returns>
        protected long GetSessionID(IPEndPoint ipEndPoint)
        {
            lock (mSessionLock)
            {
                long lSessionID;
                mSessionSet.TryGetValue(ipEndPoint, out lSessionID);

                return lSessionID;
            }
        }

        /// <summary>
        /// 删除Session
        /// </summary>
        /// <param name="lSessionID"></param>
        protected void RemoveSession(long lSessionID)
        {
            lock (mSessionLock)
            {
                UDPSession session;
                mSessionDict.TryGetValue(lSessionID, out session);

                mSessionDict.Remove(lSessionID);
                mSessionSet.Remove(session.GetIPEndPoint());
            }
        }

        protected void RemoveSession(UDPSession session)
        {
            lock (mSessionLock)
            {
                mSessionSet.Remove(session.GetIPEndPoint());

                if (session != null)
                    mSessionDict.Remove(session.SessionID);
            }
        }

        /// <summary>
        /// 插入Session
        /// </summary>
        /// <param name="session"></param>
        protected void InsertSession(UDPSession session)
        {
            lock (mSessionLock)
            {
                if (mSessionSet.ContainsKey(session.GetIPEndPoint()))
                    return;

                session.SessionID = NetSessionID();
                mSessionSet.Add(session.GetIPEndPoint(), session.SessionID);

                mSessionDict.Add(session.SessionID, session);

                session.mNetType = Net.ENet.UdpServer;
                if (Connected != null)
                    Connected(Net.ENet.UdpServer, session);
            }
        }
    }
}
