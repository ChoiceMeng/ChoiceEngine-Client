/********************************************************************
	created:	2015/03/25
	author:		王萌	
	purpose:	二进制servlet
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Common.Net;
using System.IO;
using Common.Log;
//using Common.Log;

namespace Net.Http
{
    /// <summary>
    /// 二进制类型的Servlet
    /// </summary>
    public class BinaryServlet : HttpServlet
    {
        /// <summary>
        /// 内存池
        /// </summary>
        protected static MemPool mPool = new MemPool();

        /// <summary>
        /// 读取内存
        /// </summary>
        protected MemBlock mReadMem;

        /// <summary>
        /// 写入内存
        /// </summary>
        protected MemBlock mWriteMem;

        /// <summary>
        /// 构造函数
        /// </summary>
        protected BinaryServlet(){}

        /// <summary>
        /// 创建过程
        /// </summary>
        /// <returns></returns>
        public static HttpServlet ServletCreate()
        {
            return new BinaryServlet();
        }

        /// <summary>
        /// 具体的处理逻辑
        /// </summary>
        protected override void Process()
        {
            try
            {
                Stream stream = mContext.Request.InputStream;
                mReadMem = mPool.Alloc((int)stream.Length);
                stream.Read(mReadMem.GetBytes(), 0, (int)stream.Length);

                mHttpServer.ReceiveData(this, mReadMem.GetBytes(), mReadMem.UseSize);
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error("Http Message Read Error:Process");
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 是否已经处理结束
        /// </summary>
        /// <returns></returns>
        public override bool IsHandleFinished()
        {
            return bProcessFinish;
        }

        /// <summary>
        /// 当处理完成的时候
        /// </summary>
        public override void OnFinish()
        {
            RespondMessage();
            FreeMem();
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        protected void RespondMessage()
        {
            try
            {
                Stream stream = mContext.Response.OutputStream;
                stream.Write(mWriteMem.GetBytes(), 0, (int)mWriteMem.UseSize);
            }
            catch(Exception ex)
            {
                Logger.GetLog("NetCommon").Error("Http Message Write Error:RespondMessage");
                Logger.GetLog("NetCommon").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        protected void FreeMem()
        {
            mPool.Free(mReadMem);
            mPool.Free(mWriteMem);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        public void SendMessage(byte[] byData, int length, int offset)
        {
            mWriteMem = mPool.Alloc(length);
            Buffer.BlockCopy(byData, offset, mWriteMem.GetBytes(), 0, length);
            // 可以结束了
            SetProcessDone(true);
        }
    }
}
