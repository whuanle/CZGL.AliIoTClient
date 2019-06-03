using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    public class ServiceReplyModel
    {
        public long code { get; set; }
        public string message { get; set; }
        public string describe { get; set; }
    }
}
