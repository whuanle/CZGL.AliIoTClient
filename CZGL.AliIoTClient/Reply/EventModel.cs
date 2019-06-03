using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 设备上报事件后，服务器响应
    /// </summary>
    public class EventReplyModel
    {
        public long code { get; set; }
        public string message { get; set; }
        public string describe { get; set; }
    }
}
