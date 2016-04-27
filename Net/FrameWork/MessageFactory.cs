using Common.Net;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 消息工厂
/// </summary>
public class MessageFactory : ModelManage
{
    /// <summary>
    /// 消息队列
    /// </summary>
    protected Dictionary<Type, Queue<Message>> mMsgQueue = new Dictionary<Type, Queue<Message>>();

    /// <summary>
    /// 消息缓存的锁
    /// </summary>
    protected object msgCacheLock = new object();

    /// <summary>
    /// 静态变量
    /// </summary>
    protected static MessageFactory sInstance = null;

    /// <summary>
    /// 消息工厂
    /// </summary>
    public MessageFactory()
    {
        sInstance = this;
    }

    /// <summary>
    /// 分配消息
    /// </summary>
    /// <returns></returns>
    public static T Alloc<T>() where T : Message
    {
        return sInstance.AllocMessage<T>();
    }

    /// <summary>
    /// 生成一个消息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Message Alloc(Type type)
    {
        return sInstance.AllocMessage(type);
    }

    /// <summary>
    /// 释放消息
    /// </summary>
    /// <param name="msg"></param>
    public static void Free(Message msg)
    {
        sInstance.FreeMessage(msg);
    }

    /// <summary>
    /// 分配消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msgType"></param>
    public Message AllocMessage(Type type)
    {
        lock (msgCacheLock)
        {
            Queue<Message> queue = null;
            mMsgQueue.TryGetValue(type, out queue);
            if (queue == null || queue.Count == 0)
            {
                // 用反射创建类
                return Activator.CreateInstance(type) as Message;
            }
            else
            {
                Message msg = queue.Dequeue();
                msg.ClearData();

                return msg as Message;
            }
        }
    }

    /// <summary>
    /// 分配消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T AllocMessage<T>() where T : Message
    {
        return AllocMessage(typeof(T)) as T;
    }

    /// <summary>
    /// 释放消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    public void FreeMessage(Message msg)
    {
        if (msg == null)
            return;
        lock (msgCacheLock)
        {
            Queue<Message> queue = null;
            mMsgQueue.TryGetValue(msg.GetType(), out queue);
            if (queue == null)
            {
                queue = new Queue<Message>();
                mMsgQueue.Add(msg.GetType(), queue);
            }

            queue.Enqueue(msg);
        }
    }

    public override void MainThread()
    {
    }

    public override void Stop()
    {
    }
}
