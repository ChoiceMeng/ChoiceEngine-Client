using System.Collections;

/// <summary>
/// 内存块
/// </summary>
public struct MemBlock
{
    /// <summary>
    /// 内存
    /// </summary>
    public byte[] mMem;

    /// <summary>
    /// 使用大小
    /// </summary>
    public int mUseSize;

    public MemBlock(int length)
    {
        mMem = new byte[length];
        mUseSize = 0;
    }

    /// <summary>
    /// 获取最大大小
    /// </summary>
    /// <returns></returns>
    public int GetMaxLength()
    {
        return mMem.Length;
    }

    /// <summary>
    /// 使用大小
    /// </summary>
    public int UseSize
    {
        get { return mUseSize; }
        set { mUseSize = value; }
    }

    public byte[] GetBytes()
    {
        return mMem;
    }
}
