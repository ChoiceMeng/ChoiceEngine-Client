using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;
using Common.Log.Config;

namespace Common.Log.Write
{
    public class LogWrite : ILogWrite
    {
        /// <summary>
        /// 队列列表
        /// </summary>
        protected Queue<LogContent> mContentQueue = new Queue<LogContent>();

        /// <summary>
        /// 队列的锁
        /// </summary>
        protected object mLock = new object();

        /// <summary>
        /// 添加一个日志
        /// </summary>
        /// <param name="logContent"></param>
        public void PushContent(LogContent logContent)
        {
            lock (mLock)
            {
                if (null == logContent)
                    return;

                this.mContentQueue.Enqueue(logContent);
            }
        }

        /// <summary>
        /// 获得文本数量
        /// </summary>
        /// <returns></returns>
        protected int GetContentCount()
        {
            lock (mLock)
            {
                return this.mContentQueue.Count;
            }
        }

        /// <summary>
        /// 弹出一个日志
        /// </summary>
        /// <returns></returns>
        protected LogContent PopContent()
        {
            lock (mLock)
            {
                if (0 == this.mContentQueue.Count)
                    return null;

                return this.mContentQueue.Dequeue();
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public void Write()
        {
            int nCount = GetContentCount();
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                LogContent logContent = PopContent();
                if (null == logContent)
                    continue;

                WriteLog(logContent);
                logContent = null;
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logContent"></param>
        protected virtual void WriteLog(LogContent logContent)
        {
        }
    }
}
