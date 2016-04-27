using Common.Frame;
using Common.Log;
//using Common.Log;
using Net;
using NetCommon.CallBack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace NetCommon.Http
{
    public class HttpClient
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
        /// 发送线程
        /// </summary>
        protected Thread mSendThread = null;

        /// <summary>
        /// 帧限制
        /// </summary>
        protected FrameLimite mFrameLimit = new FrameLimite();

        /// <summary>
        /// 内存池
        /// </summary>
        protected static MemPool mPool = new MemPool();

        /// <summary>
        /// 服务器信息
        /// </summary>
        protected string mServerInfo = string.Empty;

        /// <summary>
        /// 发送队列
        /// </summary>
        protected Queue<MemBlock> mSendQueue = new Queue<MemBlock>();

        /// <summary>
        /// 发送的锁
        /// </summary>
        protected object mSendLock = new object();

        /// <summary>
        /// 是否线程已经启动
        /// </summary>
        protected bool mIsThreadStart = false;

        /// <summary>
        /// 计时器
        /// </summary>
        protected System.Timers.Timer mTimer;

        /// <summary>
        /// 同步管理
        /// </summary>
        protected SyncManage mSyncManage;

        /// <summary>
        /// 客户端
        /// </summary>
        public HttpClient()
        {
            ServicePointManager.DefaultConnectionLimit = 200;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public long Init()
        {
            mSyncManage = HJGameManage.FindModel<SyncManage>();
            long lAdapterId = 0;
            mSendThread = new Thread(SendThread);
            mSendThread.Start();

            if( Connected != null )
            {
                lAdapterId = Connected(Net.ENet.HttpClient, this);
            }

            return lAdapterId;
        }

        /// <summary>
        /// 是否已经开始
        /// </summary>
        public bool IsStart
        {
            get { return mIsThreadStart; }
        }

        /// <summary>
        /// 设置服务器信息
        /// </summary>
        /// <param name="serverInfo"></param>
        public void SetServerInfo(string serverInfo)
        {
            mServerInfo = serverInfo;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        public void SendMessage(byte[] byData, int length, int offset)
        {
            MemBlock block = mPool.Alloc(length);
            Buffer.BlockCopy(byData, offset, block.GetBytes(), 0, length);
            PushMessage(block);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            mFrameLimit.SetExit(true);

            if (Disconnect != null)
                Disconnect(Net.ENet.HttpClient, this);
        }

        /// <summary>
        /// 添加到发送队列
        /// </summary>
        /// <param name="block"></param>
        protected void PushMessage(MemBlock block)
        {
            lock(mSendLock)
            {
                if (block.GetBytes() == null)
                    return;

                mSendQueue.Enqueue(block);
            }
        }

        /// <summary>
        /// 获取消息数量
        /// </summary>
        /// <returns></returns>
        protected int GetMessageCount()
        {
            lock(mSendLock)
            {
                return mSendQueue.Count;
            }
        }

        /// <summary>
        /// 弹出一个消息
        /// </summary>
        /// <returns></returns>
        protected MemBlock PopMessage()
        {
            lock(mSendLock)
            {
                if (mSendQueue.Count == 0)
                    return new MemBlock();

                return mSendQueue.Dequeue();
            }
        }

        /// <summary>
        /// 发送线程
        /// </summary>
        protected void SendThread()
        {
            mIsThreadStart = true;
            mFrameLimit.SetFrameCalled(SendFrame);
            mFrameLimit.StartFrame();
        }

        /// <summary>
        /// 发送的帧回调
        /// </summary>
        protected void SendFrame()
        {
            int nCount = GetMessageCount();
            for(int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                MemBlock block = PopMessage();
                ThreadPool.QueueUserWorkItem(SendBlock, block);
            }
        }

        /// <summary>
        /// 发送内容
        /// </summary>
        /// <param name="block"></param>
        protected void SendBlock(object block)
        {
            MemBlock memBlock = (MemBlock)block;
            string strText = Convert.ToBase64String(memBlock.GetBytes(), 0, memBlock.UseSize);
            byte[] byResultSend = Encoding.UTF8.GetBytes(strText);

            mPool.Free(memBlock);
            try
            {
                GC.Collect();
                Thread.Sleep(2);
                HttpWebRequest request = HttpWebRequest.Create(mServerInfo + MessageDef.BinaryPath) as HttpWebRequest;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 3000;
                request.ReadWriteTimeout = 3000;
                request.AllowAutoRedirect = false;
                request.KeepAlive = false;
                request.GetRequestStream().Write(byResultSend, 0, byResultSend.Length);
                request.GetRequestStream().Close();

                request.BeginGetResponse(HttpResponseCallBack, request);

                OpenTimer();
            }
            catch (Exception ex)
            {
                TimeoutCallBack(null, null);
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 超时回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TimeoutCallBack(object sender, System.Timers.ElapsedEventArgs e)
        {
            CloseTimer();
            mSyncManage.DispatchSync("NetManage_Timerout");
        }

        /// <summary>
        /// Http回复消息
        /// </summary>
        /// <param name="iar"></param>
        protected void HttpResponseCallBack(IAsyncResult iar)
        {
            try
            {
                CloseTimer();
                    
                HttpWebRequest request = iar.AsyncState as HttpWebRequest;
                Logger.GetLog("Message").Debug("HttpResponseCallBack0000000");
                WebResponse response = request.EndGetResponse(iar);

                Stream myResponseStream = request.GetResponse().GetResponseStream();

                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();

                myStreamReader.Close();
                response.Close();
                response = null;
                myResponseStream.Close();
                request.Abort();
                request = null;

                byte[] byData = Convert.FromBase64String(retString);
                if( Receive != null )
                {
                    Receive(Net.ENet.HttpClient, this, byData, byData.Length, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 打开计时器
        /// </summary>
        protected void OpenTimer()
        {
            mTimer = new System.Timers.Timer();
            mTimer.Interval = 4000;
            mTimer.Elapsed += TimeoutCallBack;

            mTimer.Enabled = true;
            mTimer.Start();
        }

        /// <summary>
        /// 关闭计时器
        /// </summary>
        protected void CloseTimer()
        {
            if (mTimer == null)
                return;

            mTimer.Close();
            mTimer = null;
        }
    }
}
