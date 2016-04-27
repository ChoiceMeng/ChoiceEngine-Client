using Common.Net;
using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Net.Adapter
{
    /// <summary>
    /// 网络适配器
    /// </summary>
    public abstract class NetAdapter
    {
        /// <summary>
        /// 网络类型
        /// </summary>
        protected ENet mNetType;

        /// <summary>
        /// 网络类型
        /// </summary>
        public ENet NetType
        {
            get { return mNetType; }
        }

        /// <summary>
        /// 适配器Id
        /// </summary>
        protected long mAdapterID;

        /// <summary>
        /// 适配器Id
        /// </summary>
        public long AdapterID
        {
            get { return mAdapterID; }
        }

        /// <summary>
        /// 会话类
        /// </summary>
        protected object mObjSession;

        /// <summary>
        /// 发送队列
        /// </summary>
        protected List<MemBlock> mSendQueue = new List<MemBlock>();

        /// <summary>
        /// 内存池
        /// </summary>
        protected static MemPoolNoLock mPool = new MemPoolNoLock();

        /// <summary>
        /// 发送的对象池
        /// </summary>
        protected static MemPoolNoLock mSendPool = new MemPoolNoLock();

        /// <summary>
        /// 内存池
        /// </summary>
        public static MemBlock Alloc(int length)
        {
            return mPool.Alloc(length);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="block"></param>
        public static void SendFree(MemBlock block)
        {
            mSendPool.Free(block);
        }

        /// <summary>
        /// 内存池
        /// </summary>
        public static MemBlock SendAlloc(int length)
        {
            return mSendPool.Alloc(length);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="block"></param>
        public static void Free(MemBlock block)
        {
            mPool.Free(block);
        }

        /// <summary>
        /// 发送队列数量
        /// </summary>
        /// <returns></returns>
        protected int GetSendQueueCount()
        {
            return mSendQueue.Count;
        }

        /// <summary>
        /// 添加一个发送数据
        /// </summary>
        /// <param name="block"></param>
        public void PushSend(MemBlock block)
        {
            mSendQueue.Add(block);
        }

        /// <summary>
        /// 弹出一个发送数据
        /// </summary>
        /// <returns></returns>
        protected MemBlock GetSend(int nIndex)
        {
            if (nIndex < 0 || nIndex >= mSendQueue.Count)
                return new MemBlock();

            return mSendQueue[nIndex];
        }

        /// <summary>
        /// 清空发送
        /// </summary>
        protected void ClearSend()
        {
            mSendQueue.Clear();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="AdapterID">唯一值</param>
        /// <param name="net">网络模型</param>
        /// <param name="param">参数值</param>
        public virtual void Init(long lAdapterID, ENet net, object param)
        {
            mAdapterID = lAdapterID;
            mNetType = net;
            mObjSession = param;
        }

        /// <summary>
        /// 即刻发送信息
        /// </summary>
        public void ImmediateSendMessage()
        {
            SendMessage();
            ClearSend();
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        protected abstract void SendMessage();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="byData">发送数据</param>
        /// <param name="length">长度</param>
        /// <param name="offset">偏移量</param>
        /// <param name="verify">是否验证，Udp的时候才有用</param>
        protected abstract void SendMessage(byte[] byData, int length, int offset, bool verify = true);

        /// <summary>
        /// 获取Session, 连接的会话
        /// </summary>
        /// <returns></returns>
        public virtual object GetSession()
        {
            return mObjSession;
        }
    }
}
