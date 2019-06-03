using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 收到服务器下发设置属性的命令时，返回响应json
    /// </summary>
    public class PropertyDownModel
    {
        public string id { get; set; }
        public long code { get; set; }
        public string data { get; set; }
    }
}
