using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Model;
using System.IO;
using UnityEngine;

namespace Common.Log.Write
{
    /// <summary>
    /// 文件Log
    /// </summary>
    public class LogFile : LogWrite
    {
        /// <summary>
        /// 文件长度 10M大小
        /// </summary>
        protected static long mFileLength = 1024 * 1024 * 10;

        /// <summary>
        /// 文件路径
        /// </summary>
        protected readonly string mLogPath = "/Log/";

        /// <summary>
        /// 系统标示
        /// </summary>
        private string mSytemMark = string.Empty;

        /// <summary>
        /// 文件流
        /// </summary>
        private FileInfo mFileStream = null;

        /// <summary>
        /// 文件夹
        /// </summary>
        private DirectoryInfo mFolder = null;

        /// <summary>
        /// 写文件
        /// </summary>
        private StreamWriter mWriter = null;

        /// <summary>
        /// 系统标示
        /// </summary>
        protected string SytemMark
        {
            get { return mSytemMark; }
            set { mSytemMark = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strSystemMark"></param>
        public LogFile(string strSystemMark)
        {
            SytemMark = strSystemMark;
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logContent"></param>
        protected override void WriteLog(LogContent logContent)
        {
            if (null == logContent)
                return;

            // 打开文件
            OpenFile(logContent.SystemMark);
            // 写内容
            WriteContent(logContent.Time + logContent.Content);
            // 关闭文件
            CloseFile();
            // 检查长度
            CheckFileLength(logContent.SystemMark);
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="strSystemMark"></param>
        protected void OpenFile(string strSystemMark)
        {
            if (this.mFileStream != null)
                return;

            CheckFolder(strSystemMark);
            OpenLogFile(strSystemMark);
        }

        /// <summary>
        /// 打开Log文件
        /// </summary>
        /// <param name="strSystemMark"></param>
        protected void OpenLogFile(string strSystemMark)
        {
            string strLogPath = GetLogPath(strSystemMark);
            mFileStream = new FileInfo(strLogPath);
            if (mFileStream.Exists)
            {
                mWriter = mFileStream.AppendText();
            }
            else
            {
                mWriter = mFileStream.CreateText();
            }
        }

        /// <summary>
        /// 检查文件夹
        /// </summary>
        /// <param name="strSystemMark"></param>
        protected void CheckFolder(string strSystemMark)
        {
            if (mFolder != null)
                return;

            mFolder = new DirectoryInfo(Application.persistentDataPath + mLogPath);
            if (mFolder.Exists)
                return;

            mFolder.Create();
        }

        /// <summary>
        /// 关闭文件
        /// </summary>
        protected void CloseFile()
        {
            if (mWriter != null)
            {
                mWriter.Close();
                mWriter = null;
            }

            if (mFileStream != null)
            {
                mFileStream = null;
            }
        }

        /// <summary>
        /// 写具体内容
        /// </summary>
        /// <param name="strContent"></param>
        protected void WriteContent(string strContent)
        {
            if (this.mWriter == null)
                return;

            mWriter.Write(strContent);

            mWriter.Flush();
        }

        /// <summary>
        /// 检查文件长度
        /// </summary>
        protected void CheckFileLength(string strSystemMark)
        {
            if (null == this.mFileStream)
                return;

            if (this.mFileStream.Length < LogFile.mFileLength)
                return;

            mFileStream.Delete();
        }

        /// <summary>
        /// 获得日志路径
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        protected string GetLogPath(string strSystemMark)
        {
            return Application.persistentDataPath + mLogPath + this.mSytemMark + ".log";
        }

        /// <summary>
        /// 按日期的文件路径
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        protected string GetLogPathTime(string strSystemMark)
        {
            return Application.persistentDataPath + mLogPath + this.mSytemMark + ".log";
        }
    }
}
