using NetCommon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCommon.CallBack
{
    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="net"></param>
    /// <param name="param"></param>
    public delegate long ConnectedHandler(ENet net, object param);
}
