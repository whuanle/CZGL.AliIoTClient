using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// MQTT通讯报文验证
    /// </summary>
    public class MQTTUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
    }
}
