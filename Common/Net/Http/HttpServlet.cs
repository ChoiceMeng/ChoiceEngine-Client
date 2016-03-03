/********************************************************************
	created:	2015/03/19
	author:		王萌	
	purpose:	httpservlet基类
	审核信息:   建议：1、封装从HttpListenerContext获取参数信息的函数,提供是否存在某个参数的函数判定
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Net.Http
{
    public abstract class HttpServlet
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        protected HttpListenerContext mContext;

        /// <summary>
        /// 是否已经处理完成
        /// </summary>
        protected bool bProcessFinish = false;

        /// <summary>
        /// 记住Processer指针
        /// </summary>
        protected HttpServer mHttpServer = null;

        /// <summary>
        /// 处理过程
        /// </summary>
        protected abstract void Process();

        /// <summary>
        /// 是否已经结束
        /// </summary>
        /// <returns></returns>
        public abstract bool IsHandleFinished();

        /// <summary>
        /// 开始处理
        /// </summary>
        public void HandleServlet()
        {
            Process();
        }

        /// <summary>
        /// respond
        /// </summary>
        public abstract void OnFinish();

        /// <summary>
        /// Http监听的上下文
        /// </summary>
        public HttpListenerContext Context
        {
            set { mContext = value; }
            get { return mContext; }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="processer"></param>
        public void SetServer(HttpServer processer)
        {
            mHttpServer = processer;
        }

        /// <summary>
        /// 设置处理完成
        /// </summary>
        /// <param name="bDone"></param>
        public void SetProcessDone(bool bDone)
        {
            bProcessFinish = bDone;
        }
    }
}
