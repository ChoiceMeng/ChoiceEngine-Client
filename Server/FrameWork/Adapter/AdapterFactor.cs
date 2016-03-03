using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Net.Adapter
{
    /// <summary>
    /// 适配器工厂
    /// </summary>
    public class AdapterFactor
    {
        /// <summary>
        /// 适配器对应关系
        /// </summary>
        protected Dictionary<ENet, Type> mAdapterDict = new Dictionary<ENet, Type>();

        public AdapterFactor()
        {
            InitAdapter();
        }

        /// <summary>
        /// 初始化适配器
        /// </summary>
        protected void InitAdapter()
        {
            AddAdapter(ENet.HttpClient, typeof(HttpClientAdapter));
            AddAdapter(ENet.HttpServer, typeof(HttpServerAdapter));
            AddAdapter(ENet.TcpClient, typeof(TcpClientAdapter));
            AddAdapter(ENet.TcpServer, typeof(TcpServerAdapter));
            AddAdapter(ENet.UdpClient, typeof(UdpClientAdapter));
            AddAdapter(ENet.UdpServer, typeof(UdpServerAdapter));
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="net"></param>
        /// <param name="type"></param>
        protected void AddAdapter(ENet net, Type type)
        {
            if (mAdapterDict.ContainsKey(net))
                return;

            mAdapterDict.Add(net, type);
        }

        /// <summary>
        /// 创建适配器
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public NetAdapter CreateAdapter(ENet net)
        {
            if (!mAdapterDict.ContainsKey(net))
                return null;

            Type type = mAdapterDict[net];
            if (type == null)
                return null;

            NetAdapter adapter = Activator.CreateInstance(type) as NetAdapter;

            return adapter;
        }
    }
}
