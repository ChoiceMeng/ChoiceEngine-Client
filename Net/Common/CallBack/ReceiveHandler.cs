using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCommon.CallBack
{
    /// <summary>
    /// 接收回调
    /// </summary>
    /// <param name="net"></param>
    /// <param name="param"></param>
    /// <param name="byData"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    public delegate void ReceiveHandler(ENet net, object param, byte[] byData, int length, int offset);
}
