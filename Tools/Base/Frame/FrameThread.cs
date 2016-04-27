using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Common.Frame
{
    /// <summary>
    /// 限帧的线程类
    /// </summary>
    public class FrameThread
    {
        /// <summary>
        /// 帧限制
        /// </summary>
        protected FrameLimite mFrameLimit = new FrameLimite();

        /// <summary>
        /// 线程函数
        /// </summary>
        protected Thread mThread;

        /// <summary>
        /// 线程是否已经启动
        /// </summary>
        protected bool mIsThreadStart = false;

        /// <summary>
        /// 无参数构造函数
        /// </summary>
        public FrameThread()
        {

        }

        /// <summary>
        /// 带回调函数的构造函数
        /// </summary>
        /// <param name="frameCalled"></param>
        public FrameThread(FrameCalled frameCalled)
        {
            mFrameLimit.SetFrameCalled(frameCalled);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            mThread = new Thread(ThreadCalled);
            mThread.Start();
        }

        public void Stop()
        {
            SetExit(true);
        }

        /// <summary>
        /// 等待线程结束
        /// </summary>
        public void Join()
        {
            if (mThread == null)
                return;

            mThread.Join();
        }

        /// <summary>
        /// 线程是否已经启动
        /// 当且仅当线程已经启动并且没有结束的时候为true
        /// </summary>
        public bool IsThreadStart { get { return mIsThreadStart; } }

        /// <summary>
        /// 设置帧调用函数
        /// </summary>
        /// <param name="frameCalled"></param>
        public void SetFrameCalled(FrameCalled frameCalled)
        {
            mFrameLimit.SetFrameCalled(frameCalled);
        }

        /// <summary>
        /// 线程调用
        /// </summary>
        protected void ThreadCalled()
        {
            mIsThreadStart = true;
            mFrameLimit.StartFrame();
            mIsThreadStart = false;
        }

        /// <summary>
        /// 设置退出
        /// </summary>
        /// <param name="bExit"></param>
        protected void SetExit(bool bExit)
        {
            mFrameLimit.SetExit(bExit);
        }
    }
}
