
using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 与连接服务器有关
    /// </summary>
    public class ConnectOptions
    {
        //通讯地址
        public string targetServer { get; set; }
        // 针对设备生成 Topic 头
        public string TopicHead{ get; set; }

        public ConnectOptions(string productKey,string regionId,string deviceName)
        {
            TopicHead = "/" + productKey + "/" + deviceName + "/user/";
            targetServer = productKey + ".iot-as-mqtt." + regionId + ".aliyuncs.com";
        }
    }
}
