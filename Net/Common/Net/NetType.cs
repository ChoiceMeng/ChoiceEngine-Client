using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCommon.Net
{
    /// <summary>
    /// 网络类型
    /// </summary>
    public enum ENet
    {
        /// <summary>
        /// Tcp的客户端
        /// </summary>
        TcpClient, 

        /// <summary>
        /// Tcp的服务器
        /// </summary>
        TcpServer, 

        /// <summary>
        /// Udp客户端
        /// </summary>
        UdpClient, 

        /// <summary>
        /// Udp服务器
        /// </summary>
        UdpServer, 

        /// <summary>
        /// Http客户端
        /// </summary>
        HttpClient, 

        /// <summary>
        /// Http服务器
        /// </summary>
        HttpServer, 
    }
}
