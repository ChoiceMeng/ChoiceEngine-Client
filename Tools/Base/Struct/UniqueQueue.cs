using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Struct
{
    public class UniqueQueue<T>
    {
        /// <summary>
        /// 队列
        /// </summary>
        protected Queue<T> mQueue = new Queue<T>();

        /// <summary>
        /// 插入数据检验用
        /// </summary>
        protected Dictionary<T, byte> mDict = new Dictionary<T, byte>();

        /// <summary>
        /// 锁
        /// </summary>
        protected object mLock = new object();

        public int Count
        {
            get { lock (mLock) { return mQueue.Count; } }
        }

        public void Enqueue(T item)
        {
            lock (mLock)
            {
                if (mDict.ContainsKey(item))
                    return;

                mDict.Add(item, 0);
                mQueue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            lock (mLock)
            {
                T item = mQueue.Dequeue();
                mDict.Remove(item);

                return item;
            }
        }

        public bool Contains(T item)
        {
            lock (mLock)
            {
                return mDict.ContainsKey(item);
            }
        }
    }
}
