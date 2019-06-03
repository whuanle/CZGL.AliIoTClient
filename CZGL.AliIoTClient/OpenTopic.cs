using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 是否接收服务器下发指令和响应
    /// </summary>
    public class OpenTopic
    {
        public bool CommonTopic { get { return true; } set { } }
        /// <summary>
        /// 接收设备上传透传属性数据后服务器的响应
        /// </summary>
        public bool PropertyUpRawReplyTopic { get; set; }
        /// <summary>
        /// 接收设备上传Alink json属性数据后服务器的响应
        /// </summary>
        public bool PropertyPostReplyTopic { get; set; }
        /// <summary>
        /// 接收服务器下发设置属性的命令，透传
        /// </summary>
        public bool PropertyDownRaw { get; set; }
        /// <summary>
        /// 接收服务器下发设置属性的命令，Alink json
        /// </summary>
        public bool PropertyDownPost { get; set; }

        /// <summary>
        /// 设备事件上报，接收服务器的响应，透传
        /// </summary>
        public bool EventUpRawReply { get; set; }
        /// <summary>
        /// 设备事件上报，接收服务器的响应，Alink json
        /// </summary>
        public bool EventPostReply { get; set; }
        /// <summary>
        /// 接收服务器调用服务，透传
        /// </summary>
        public bool ServiceDownRaw { get; set; }
        /// <summary>
        /// 接收服务器调用服务，Alink json
        /// </summary>
        public bool ServicePostRaw { get; set; }
    }
}
