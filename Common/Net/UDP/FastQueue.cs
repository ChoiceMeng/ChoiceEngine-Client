using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCommon.UDP
{
    /// <summary>
    /// 快速查询队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FastQueue<T> where T : new()
    {
        /// <summary>
        /// 队列信息
        /// </summary>
        protected Queue<T> mQueue = new Queue<T>();

        /// <summary>
        /// 队列信息
        /// </summary>
        protected HashSet<T> mSet = new HashSet<T>();

        /// <summary>
        /// 加锁
        /// </summary>
        protected object mLock = new object();

        /// <summary>
        /// 数量
        /// </summary>
        public int Count
        {
            get 
            {
                lock (mLock)
                {
                    return mQueue.Count; 
                }
            }
        }

        /// <summary>
        /// 取出一个队列
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (mLock)
            {
                if( mQueue.Count == 0 )
                    return default(T);

                T value = mQueue.Dequeue();
                mSet.Remove(value);

                return value;
            }
        }

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(T item)
        {
            lock (mLock)
            {
                mQueue.Enqueue(item);
                mSet.Add(item);
            }
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            lock (mLock)
            {
                return mSet.Contains(item);
            }
        }
    }
}
