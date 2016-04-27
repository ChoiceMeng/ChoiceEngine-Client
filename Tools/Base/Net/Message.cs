/********************************************************************
	created:	2015/03/23
	author:		王萌	
	purpose:	消息基类
	审核信息:   建议: 1、MessageDef这个类放到单独的文件中, dataFlag用const修饰
 *                   2、Deserialize函数用protected修饰
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Common;

namespace Common.Net
{
    public class Message
    {
        /// <summary>
        /// 适配器Id
        /// </summary>
        private long nAdapterId;
        
        /// <summary>
        /// 消息Id
        /// </summary>
        private int mMsgId = 0;

        /// <summary>
        /// 二进制流读取器
        /// </summary>
        private ByteArray mStream = null;

        /// <summary>
        /// 账号id
        /// </summary>
        public long mAccountId = 0;

        /// <summary>
        /// 消息头长度
        /// </summary>
        protected readonly int mHeadLength = sizeof(int) + sizeof(int) + sizeof(long);

        /// <summary>
        /// 构造函数
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// 消息Id
        /// </summary>
        public int MsgId
        {
            get { return mMsgId; }
            set { mMsgId = value; }
        }

        /// <summary>
        /// 适配器Id，即会话Id
        /// </summary>
        public long AdapterId
        {
            get { return nAdapterId; }
            set { nAdapterId = value; }
        }

        /// <summary>
        /// 消息头长度
        /// </summary>
        public int HeadLength
        {
            get { return mHeadLength; }
        }

        /// <summary>
        /// 账户
        /// </summary>
        public long AccountId
        {
            get { return mAccountId; }
            set { mAccountId = value; }
        }

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="mContent"></param>
        public void SetContent(byte[] mContent, int offset, int length)
        {
            mStream.ResetWritePos();
            mStream.Write(mContent, offset, length);
        }

        /// <summary>
        /// 解构消息:发送消息
        /// </summary>
        /// <param name="?"></param>
        public void DeconstructObj()
        {
            mStream.ResetWritePos();

            int nConetLength = 0;
            mStream.Write(nConetLength);
            mStream.Write(mMsgId);
            mStream.Write(mAccountId);

            // 序列化
            Serial();

            // 序列化完成之后再覆盖消息长度数据
            mStream.SeekAndWrite(0, mStream.GetWritePos() - sizeof(int));
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void Serial() { }

        /// <summary>
        /// 解析消息:接收消息
        /// </summary>
        /// <param name="?"></param>
        public void ParseObj()
        {
            mStream.ResetReadPos();
            // 长度
            ReadInt32();
            mMsgId = mStream.ReadInt32();
            mAccountId = mStream.ReadInt64();

            // 反序列化
            Deserialize();
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void Deserialize() { }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected short ReadShort()
        {
            return mStream.ReadInt16();
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected int ReadInt32()
        {
            return mStream.ReadInt32();
        }

        /// <summary>
        /// 获得一个UInt
        /// </summary>
        /// <returns></returns>
        protected uint ReadUInt32()
        {
            return mStream.ReadUInt32();
        }

        /// <summary>
        /// 获得一个Int64
        /// </summary>
        /// <returns></returns>
        protected long ReadInt64()
        {
            return mStream.ReadInt64();
        }

        /// <summary>
        /// 读一个字节
        /// </summary>
        /// <returns></returns>
        protected byte ReadByte()
        {
            return mStream.ReadByte();
        }

        /// <summary>
        /// 读字符串
        /// </summary>
        /// <returns></returns>
        protected bool ReadBool()
        {
            return mStream.ReadByte() == 1;
        }

        /// <summary>
        /// 读字符串
        /// </summary>
        /// <returns></returns>
        protected string ReadString()
        {
            return mStream.ReadString();
        }

        /// <summary>
        /// 读字小数
        /// </summary>
        /// <returns></returns>
        protected float ReadFloat()
        {
            return mStream.ReadSingle();
        }

        protected void ReadBytes(byte[] value, int length)
        {
            mStream.ReadBytes(value, length);
        }
        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(int nValue)
        {
            mStream.Write(nValue);
        }

        /// <summary>
        /// 写一个UInt
        /// </summary>
        /// <returns></returns>
        protected void Write(uint nValue)
        {
            mStream.Write(nValue);
        }

        /// <summary>
        /// 写一个Int64
        /// </summary>
        /// <returns></returns>
        protected void Write(long nValue)
        {
            mStream.Write(nValue);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(byte byValue)
        {
            mStream.Write(byValue);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(bool bValue)
        {
            mStream.Write(bValue ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// 写一个字符串
        /// </summary>
        /// <returns></returns>
        protected void Write(string sValue)
        {
            mStream.Write(sValue);
        }

        /// <summary>
        /// 写一个short
        /// </summary>
        /// <returns></returns>
        protected void Write(short sValue)
        {
            mStream.Write(sValue);
        }

        /// <summary>
        /// 写一个short
        /// </summary>
        /// <returns></returns>
        protected void Write(float sValue)
        {
            mStream.Write(sValue);
        }

        protected void Write(byte[] byValue, int length)
        {
            mStream.Write(byValue, length);
        }

        /// <summary>
        /// 清理
        /// </summary>
        public virtual void ClearData()
        {

        }

        /// <summary>
        /// 设置序列化
        /// </summary>
        /// <param name="byteArray"></param>
        public void SetByteArray(ByteArray byteArray)
        {
            mStream = byteArray;
            mStream.ResetReadPos();
            mStream.ResetWritePos();
        }

        /// <summary>
        /// 获取内存
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return mStream.GetBytes();
        }

        public int GetWritePos()
        {
            return mStream.GetWritePos();
        }

        public int GetReadPos()
        {
            return mStream.GetReadPos();
        }
    }
}
