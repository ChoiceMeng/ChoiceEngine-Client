using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Common.Frame
{
    /// <summary>
    /// 限帧
    /// </summary>
    public class FrameLimite
    {
        /// <summary>
        /// 帧率调用函数
        /// </summary>
        protected FrameCalled mFrameCalled;

        /// <summary>
        /// 默认限定为100帧
        /// </summary>
        protected int mFPS = 100;

        /// <summary>
        /// 每帧100
        /// </summary>
        private const int mMaxTick = 30;

        /// <summary>
        /// 是否退出
        /// </summary>
        protected bool mExit = false;

        /// <summary>
        /// 开始计算
        /// </summary>
        public void StartFrame()
        {
            int nStartTick = Environment.TickCount;
            int nEndTick = nStartTick;
            int nExeTime = 0;
            int nSleepTime = 0;

            while (!mExit)
            {
                nStartTick = Environment.TickCount;

                // 主线程处理
                if (mFrameCalled != null)
                    mFrameCalled();

                nEndTick = Environment.TickCount;

                nExeTime = nEndTick - nStartTick;
                nSleepTime = Math.Max(mMaxTick - nExeTime, 0);
                if (nSleepTime > 0)
                    Thread.Sleep(nSleepTime);

                this.mFPS = 1000 / (nExeTime + nSleepTime);
            }
        }

        /// <summary>
        /// 获得帧率
        /// </summary>
        /// <returns></returns>
        public int GetFPS()
        {
            return this.mFPS;
        }

        /// <summary>
        /// 设置退出
        /// </summary>
        /// <param name="bExit"></param>
        public void SetExit(bool bExit)
        {
            this.mExit = bExit;
        }

        /// <summary>
        /// 设置每帧回调
        /// </summary>
        /// <param name="frameCalled"></param>
        public void SetFrameCalled(FrameCalled frameCalled)
        {
            this.mFrameCalled -= frameCalled;
            this.mFrameCalled += frameCalled;
        }
    }
}
