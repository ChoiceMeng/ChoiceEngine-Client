using NetCommon.CallBack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetCommon.UDP
{
    public class UDPClient
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
        /// 网络连接
        /// </summary>
        protected UDPSocket mSocket = new UDPSocket();

        /// <summary>
        /// 会话
        /// </summary>
        protected UDPSession mSession = null;

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            //// 暂时先这样，以后修改为在外部处理 2015年12月23日 16:25:48
            //Close();

            mSession = mSocket.Connect(ip, port);
            mSession.SetHandleReceive(Receive);
            mSocket.ReceiveCompleted = OnReceive;
            mSession.mNetType = Net.ENet.UdpClient;

            if (Connected != null)
                Connected(Net.ENet.UdpClient, mSession);
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
            if (mSession == null)
                return;

            mSession.SendMessage(byData, length, offset, verify);
        }

        /// <summary>
        /// 有消息过来
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <param name="byData"></param>
        protected void OnReceive(IPEndPoint ipEndPoint, byte[] byData)
        {
            mSession.Receive(byData);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        protected void Close()
        {
            mSocket.Close();
            //if (Disconnect != null)
            //    Disconnect(Net.ENet.HttpClient, mSession);
        }
    }

}