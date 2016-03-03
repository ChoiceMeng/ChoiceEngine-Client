using System;
using System.Collections.Generic;
using System.Text;

public class ByteArray
{
    /// <summary>
    /// byte 列表
    /// </summary>
    protected byte[] mBytes = new byte[102400];

    /// <summary>
    /// 字符集编码
    /// </summary>
    protected Encoding mEndcoding = Encoding.UTF8;

    /// <summary>
    /// 读的位置
    /// </summary>
    protected int mReadPos = 0;

    /// <summary>
    /// 写的位置
    /// </summary>
    protected int mWritePos = 0;

    public void Write(string sValue)
    {
        byte[] byValues = mEndcoding.GetBytes(sValue);
        Write(byValues.Length);
        Write(byValues);
    }

    /// <summary>
    /// 写字节
    /// </summary>
    /// <param name="byValues"></param>
    public void Write(byte[] byValues)
    {
        Buffer.BlockCopy(byValues, 0, mBytes, mWritePos, byValues.Length);
        mWritePos += byValues.Length;
    }

    public void Write(byte[] byValues, int nLength)
    {
        Buffer.BlockCopy(byValues, 0, mBytes, mWritePos, nLength);
        mWritePos += nLength;
    }

    public void Write(byte[] byValues, int offset, int nLength)
    {
        Buffer.BlockCopy(byValues, offset, mBytes, mWritePos, nLength);
        mWritePos += nLength;
    }

    public void Write(bool value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(bool));
        mWritePos += sizeof(bool);
    }

    public void Write(byte value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(byte));
        mWritePos += sizeof(byte);
    }

    public void Write(char value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(char));
        mWritePos += sizeof(char);
    }

    public void Write(double value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(double));
        mWritePos += sizeof(double);
    }

    public void Write(float value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(float));
        mWritePos += sizeof(float);
    }

    public void Write(int value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(int));
        mWritePos += sizeof(int);
    }

    public void Write(long value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(long));
        mWritePos += sizeof(long);
    }

    public void Write(short value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(short));
        mWritePos += sizeof(short);
    }

    public void Write(uint value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(uint));
        mWritePos += sizeof(uint);
    }

    public void Write(ulong value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(ulong));
        mWritePos += sizeof(ulong);
    }

    public void Write(ushort value)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, mBytes, mWritePos, sizeof(ushort));
        mWritePos += sizeof(ushort);
    }

    public string ReadString()
    {
        byte[] value = ReadBytes();
        return mEndcoding.GetString(value);
    }

    public byte[] ReadBytes()
    {
        int nLength = ReadInt();
        byte[] value = new byte[nLength];
        Buffer.BlockCopy(mBytes, mReadPos, value, 0, nLength);
        mReadPos += nLength;
        return value;
    }

    public void ReadBytes(byte[] value, int length)
    {
        Buffer.BlockCopy(mBytes, mReadPos, value, 0, length);
        mReadPos += length;
    }

    public int ReadInt()
    {
        int value = BitConverter.ToInt32(mBytes, mReadPos);
        mReadPos += sizeof(int);
        return value;
    }

    public byte ReadByte()
    {
        byte value = mBytes[mReadPos];
        mReadPos += sizeof(byte);
        return value;
    }

    public char ReadChar()
    {
        char value = BitConverter.ToChar(mBytes, mReadPos);
        mReadPos += sizeof(char);
        return value;
    }

    public bool ReadBoolean()
    {
        bool value = BitConverter.ToBoolean(mBytes, mReadPos);
        mReadPos += sizeof(bool);
        return value;
    }

    public double ReadDouble()
    {
        double value = BitConverter.ToDouble(mBytes, mReadPos);
        mReadPos += sizeof(double);
        return value;
    }

    public short ReadInt16()
    {
        short value = BitConverter.ToInt16(mBytes, mReadPos);
        mReadPos += sizeof(short);
        return value;
    }

    public int ReadInt32()
    {
        int value = BitConverter.ToInt32(mBytes, mReadPos);
        mReadPos += sizeof(int);
        return value;
    }

    public long ReadInt64()
    {
        long value = BitConverter.ToInt64(mBytes, mReadPos);
        mReadPos += sizeof(long);
        return value;
    }

    public float ReadSingle()
    {
        float value = BitConverter.ToSingle(mBytes, mReadPos);
        mReadPos += sizeof(float);
        return value;
    }

    public ushort ReadUInt16()
    {
        ushort value = BitConverter.ToUInt16(mBytes, mReadPos);
        mReadPos += sizeof(ushort);
        return value;
    }

    public uint ReadUInt32()
    {
        uint value = BitConverter.ToUInt32(mBytes, mReadPos);
        mReadPos += sizeof(uint);
        return value;
    }

    public ulong ReadUInt64()
    {
        ulong value = BitConverter.ToUInt64(mBytes, mReadPos);
        mReadPos += sizeof(ulong);
        return value;
    }

    //public Byte[] GetContent()
    //{
    //    byte[] value = new byte[mWritePos];
    //    Buffer.BlockCopy(mBytes, 0, value, 0, mWritePos);
    //    return value;
    //}

    public void SeekAndWrite(int nOffset, int nData)
    {
        Buffer.BlockCopy(BitConverter.GetBytes(nData), 0, mBytes, nOffset, 4);
    }

    public void ResetWritePos()
    {
        mWritePos = 0;
    }

    public void ResetReadPos()
    {
        mReadPos = 0;
    }

    /// <summary>
    /// 获取内存
    /// </summary>
    /// <returns></returns>
    public byte[] GetBytes()
    {
        return mBytes;
    }

    public int GetWritePos()
    {
        return mWritePos;
    }

    public int GetReadPos()
    {
        return mReadPos;
    }
}
