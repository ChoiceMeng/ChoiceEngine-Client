/********************************************************************
	created:	2015/03/25
	author:		王萌	
	purpose:	常用const变量
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Net.Http;
using Common.Net;

namespace Net
{
    public delegate HttpServlet ServletCreater();

    public class MessageDef
    {
        public const string DataFlag = "msgdata";
        public const string FunctionPath = "/functionservlet";
        public const string BinaryPath = "/binaryservlet";
    }
}
