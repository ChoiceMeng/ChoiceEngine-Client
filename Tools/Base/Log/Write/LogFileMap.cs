using System;
using System.Collections.Generic;
using System.Text;
using Common.Log.Config;

namespace Common.Log.Write
{
    /// <summary>
    /// 文件日志的映射
    /// </summary>
    public class LogFileMap
    {
        /// <summary>
        /// 文件日志
        /// </summary>
        protected IDictionary<string, LogFile> mFile = new Dictionary<string, LogFile>();

        /// <summary>
        /// 锁
        /// </summary>
        protected object mMapLock = new object();

        /// <summary>
        /// 日志列表
        /// </summary>
        protected List<LogFile> mFileList = new List<LogFile>();

        /// <summary>
        /// 列表
        /// </summary>
        protected object mListLock = new object();

        /// <summary>
        /// 创建文件日志
        /// </summary>
        /// <param name="strSystemMark"></param>
        public void CreateLogFile(string strSystemMark)
        {
            lock (mMapLock)
            {
                if (this.mFile.ContainsKey(strSystemMark))
                    return;

                LogFile logFile = new LogFile(strSystemMark);
                // 添加到队列中去
                AddLogFile(logFile);
                this.mFile.Add(strSystemMark, logFile);
            }
        }

        /// <summary>
        /// 获得Log日志
        /// </summary>
        /// <param name="strSystemMark"></param>
        /// <returns></returns>
        public LogFile GetLogFile(string strSystemMark)
        {
            lock (mMapLock)
            {
                if (!this.mFile.ContainsKey(strSystemMark))
                    return null;

                return this.mFile[strSystemMark];
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public void Write()
        {
            int nCount = GetLogFileCount();
            for (int nIndex = 0; nIndex < nCount; ++nIndex)
            {
                LogFile mLogFile = GetLogFileByIndex(nIndex);
                if (mLogFile == null)
                    continue;

                mLogFile.Write();
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        /// <returns></returns>
        public int GetLogFileCount()
        {
            lock (this.mListLock)
            {
                return this.mFileList.Count;
            }
        }

        /// <summary>
        /// 添加日志文件
        /// </summary>
        /// <param name="logFile"></param>
        public void AddLogFile(LogFile logFile)
        {
            lock (this.mListLock)
            {
                this.mFileList.Add(logFile);
            }
        }

        /// <summary>
        /// 根据索引获取LogFile
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public LogFile GetLogFileByIndex(int nIndex)
        {
            lock (this.mListLock)
            {
                if (nIndex < 0 || nIndex >= this.mFileList.Count)
                    return null;

                return this.mFileList[nIndex];
            }
        }
    }
}
