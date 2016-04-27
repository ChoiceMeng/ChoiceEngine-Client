using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Net
{
    public struct MessageCache
    {
        /// <summary>
        /// 内存
        /// </summary>
        private MemBlock mBlock;

        public MemBlock Block
        {
            get { return mBlock; }
            set { mBlock = value; }
        }

        /// <summary>
        /// 适配器
        /// </summary>
        private long lAdapterID;

        public long AdapterID
        {
            get { return lAdapterID; }
            set { lAdapterID = value; }
        }
    }
}
