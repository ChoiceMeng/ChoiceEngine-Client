using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Pool
{
    public class ObjectPool<T> where T : new()
    {
        Queue<T> mAllQueue = new Queue<T>();

        protected object mLock = new object();

        public void Init(int count)
        {
            mAllQueue = new Queue<T>(count);
            for (int index = 0; index < count;++index )
            { 
                mAllQueue.Enqueue(new T());
            }
              
        }
        public T CreateObject()
        {
            lock (mLock)
            {
                if (0 == mAllQueue.Count)
                    return new T();

                return mAllQueue.Dequeue();
            }
        }

        public void DestroyObject(T obj)
        {
            lock (mLock)
            {
                mAllQueue.Enqueue(obj);
            }
            
        }
    }
}
