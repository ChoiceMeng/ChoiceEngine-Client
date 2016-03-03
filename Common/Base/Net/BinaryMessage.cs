using Common.Net;
using System.Collections;

/// <summary>
/// 二进制的消息
/// </summary>
public class BinaryMessage : Message
{
    /// <summary>
    /// 内存块
    /// </summary>
    public MemBlock mBlock;

    protected override void Serial()
    {
        if (mBlock.GetBytes() == null)
            return;

        Write(mBlock.GetBytes(), mBlock.UseSize);
    }

    protected override void Deserialize()
    {
        if (mBlock.GetBytes() == null)
            return;

        ReadBytes(mBlock.GetBytes(), mBlock.UseSize);
    }
}
