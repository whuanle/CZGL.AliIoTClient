using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 设备一机一密
    /// </summary>
    public class DeviceOptions
    {
        public string ProductKey { get; set; }
        public string DeviceName { get; set; }
        public string DeviceSecret { get; set; }
        public string RegionId { get; set; }

    }
}
