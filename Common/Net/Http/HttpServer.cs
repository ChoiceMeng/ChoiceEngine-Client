/********************************************************************
	created:	2015/03/19
	author:		王萌	
	purpose:	http消息处理器
	审核信息:   建议: 1、FuncDelegate单独列一个文件
 *                   2、函数和变量统一加public、private或者protected，并都以此为开头
 *                   3、变量与函数分开放置，建议类前头放置变量,后面放置函数
 *                   4、从队列中Dequeue变量之前判定数量, 防止误操作
 *                   5、建议把Http启动部分加到这里来
*********************************************************************/
using System;
using System.Net;
using System.Collections.Generic;
//using Common.Log;
using System.Net.Sockets;
using NetCommon.CallBack;
using Common.Log;

namespace Net.Http
{
    /// <summary>
    /// Http 服务器
    /// </summary>
    public class HttpServer
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
        /// Servlet 创建器
        /// </summary>
        Dictionary<string, ServletCreater> mPathToServletDic = new Dictionary<string,ServletCreater>();

        /// <summary>
        /// Servlet 创建器的锁
        /// </summary>
        private object mServletCreateLock = new object();

        /// <summary>
        /// 正在处理中的servlet队列
        /// </summary>
        private Queue<HttpServlet> mProcessServletQueue = new Queue<HttpServlet>();

        /// <summary>
        /// Servlet事务操作锁
        /// </summary>
        private object mServletLock = new object();

        /// <summary>
        /// HTTP的监听
        /// </summary>
        private HttpListener mListener = new HttpListener();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="nPort">监听端口</param>
        public void Init(int nPort)
        {
            mPathToServletDic.Clear();
            RegisterServlet(MessageDef.BinaryPath, BinaryServlet.ServletCreate);
            StartListener(nPort);
        }

        /// <summary>
        /// 开始监听网络
        /// </summary>
        /// <param name="nPort"></param>
        private bool StartListener(int nPort)
        {
            try
            {
                List<string> allIpList = GetHostIp();
                if (allIpList == null)
                    return false;

                foreach (string strIpAddress in allIpList)
                {
                    mListener.Prefixes.Add("http://" + strIpAddress + ":" + nPort + "/");
                }
                
                mListener.Start();
                mListener.BeginGetContext(new AsyncCallback(HttpProcesserCallBack), mListener);
            }
            catch (Exception ex)
            {
                Logger.GetLog("Net").Error("Http Listener Error Port:" + nPort + ", Error List:");
                Logger.GetLog("Net").Error(ex.ToString());

                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取服务器的所有Ip
        /// </summary>
        /// <returns></returns>
        private List<string> GetHostIp()
        {
            List<string> mList = new List<string>();
            string mHostName = Dns.GetHostName();
            if( mHostName == null )
                return mList;

            IPAddress[] allIpAddress = Dns.GetHostAddresses(mHostName);
            foreach (IPAddress ipAddress in allIpAddress)
            {
                if( ipAddress.AddressFamily.Equals(AddressFamily.InterNetwork) )
                    mList.Add(ipAddress.ToString());
            }

            mList.Add("127.0.0.1");
            return mList;
        }

        /// <summary>
        /// Http的处理回调
        /// </summary>
        /// <param name="result"></param>
        protected void HttpProcesserCallBack(IAsyncResult result)
        {
            HttpListener listener = result.AsyncState as HttpListener;
            // 结束异步操作
            HttpListenerContext context = listener.EndGetContext(result);
            // 重新启动异步请求处理
            listener.BeginGetContext(new AsyncCallback(HttpProcesserCallBack), listener);
            // 处理这个消息
            MsgProcesser(context);
        }

        /// <summary>
        /// http消息处理函数
        /// </summary>
        /// <param name="context"></param>
        protected void MsgProcesser(HttpListenerContext context)
        {
            // 处理请求
            HttpListenerRequest reqe = context.Request;
            string path = reqe.Url.LocalPath;

            // 已经注册过相应的servlet去处理
            HttpServlet servlet = GetServlet(path);
            if (servlet == null) return;

            servlet.Context = context;
            servlet.SetServer(this);

            // 处理连接
            HandleConnected(servlet);

            // 直接处理, 以后有需要再修改 czx
            servlet.HandleServlet();

            while (!servlet.IsHandleFinished())
            {

            }

            servlet.OnFinish();

            // 连接断开
            HandleDisconnect(servlet);
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="servlet"></param>
        protected void HandleConnected(HttpServlet servlet)
        {
            if (servlet == null || Connected == null)
                return;

            Connected(NetCommon.Net.ENet.HttpServer, servlet);
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="servlet"></param>
        protected void HandleDisconnect(HttpServlet servlet)
        {
            if (servlet == null || Disconnect == null)
                return;

            Disconnect(NetCommon.Net.ENet.HttpServer, servlet);
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="servlet"></param>
        /// <param name="byData"></param>
        /// <param name="length"></param>
        public void ReceiveData(HttpServlet servlet, byte[] byData, int length)
        {
            if (servlet == null || Receive == null)
                return;

            Receive(NetCommon.Net.ENet.HttpServer, servlet, byData, length, 0);
        }

        /// <summary>
        /// 获取Servlet
        /// </summary>
        protected HttpServlet GetServlet(string path)
        {
            lock (mServletCreateLock)
            {
                if (mPathToServletDic.ContainsKey(path))
                {
                    return mPathToServletDic[path]();
                }

                return null;
            }
        }
        
        /// <summary>
        /// 处理器主函数
        /// </summary>
        public void MainThread()
        {
        }

        /// <summary>
        /// 处理器主函数
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="servlet"></param>
        /// <returns></returns>
        protected bool RegisterServlet(string strPath, ServletCreater creater)
        {
            lock (mServletCreateLock)
            {
                if (mPathToServletDic.ContainsKey(strPath))
                    return false;

                mPathToServletDic.Add(strPath, creater);

                return true;
            }
        }
    }
}
