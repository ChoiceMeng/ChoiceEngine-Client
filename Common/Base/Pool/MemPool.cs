using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 内存池
/// </summary>
public class MemPool
{
    /// <summary>
    /// 所有的内存块
    /// </summary>
    protected List<Queue<MemBlock>> mAllMemList = new List<Queue<MemBlock>>();

    /// <summary>
    /// 锁
    /// </summary>
    protected object mMemLock = new object();

    /// <summary>
    /// 最小的内存块
    /// </summary>
    protected const int mMinBlock = 32;

    /// <summary>
    /// 最大列表
    /// </summary>
    protected const int mMaxList = 10;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MemPool()
    {
        InitPool();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected void InitPool()
    {
        for(int nIndex = 0; nIndex < mMaxList; ++nIndex)
        {
            mAllMemList.Add(new Queue<MemBlock>());
        }
    }

    /// <summary>
    /// 队列
    /// </summary>
    /// <param name="nLength"></param>
    /// <returns></returns>
    public MemBlock Alloc(int nLength)
    {
        lock(mMemLock)
        {
            int nIndex = GetIndex(nLength);
            if (nIndex >= mMaxList)
                return NewMemBlock(nLength, GetNewSize(nLength));

            Queue<MemBlock> queue = mAllMemList[nIndex];
            if (queue.Count == 0)
                return NewMemBlock(nLength, GetNewSize(nLength));
            else
            {
                MemBlock block = queue.Dequeue();
                block.UseSize = nLength;

                return block;
            }
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="block"></param>
    public void Free(MemBlock block)
    {
        lock(mMemLock)
        {
            if (block.GetBytes() == null)
                return;

            int nIndex = GetIndex(block.GetMaxLength());
            if (nIndex >= mMaxList)
                return;

            mAllMemList[nIndex].Enqueue(block);
        }
    }

    /// <summary>
    /// 获得索引
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    protected int GetIndex(int length)
    {
        if (length <= 32)
            return 0;
        else if (length > 32 && length <= 64)
            return 1;
        else if (length > 64 && length <= 128)
            return 2;
        else if (length > 128 && length <= 256)
            return 3;
        else if (length > 256 && length <= 512)
            return 4;
        else if (length > 512 && length <= 1024)
            return 5;
        else if (length > 1024 && length <= 2048)
            return 6;
        else if (length > 2048 && length <= 4096)
            return 7;
        else if (length > 4096 && length <= 8192)
            return 8;
        else if (length > 8192 && length <= 16384)
            return 9;

        return 10;
    }

    /// <summary>
    /// 新大小
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    protected int GetNewSize(int length)
    {
        int nIndex = GetIndex(length);
        if (nIndex >= mAllMemList.Count)
            return length;

        return (int)Math.Pow(2, nIndex) * mMinBlock;
    }

    /// <summary>
    /// 申请内存
    /// </summary>
    /// <param name="nSize"></param>
    /// <param name="nMaxSize"></param>
    /// <returns></returns>
    protected MemBlock NewMemBlock(int nSize, int nMaxSize)
    {
        MemBlock block = new MemBlock(nMaxSize);
        block.UseSize = nSize;

        return block;
    }
}
