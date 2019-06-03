using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 定义设备连接异常
    /// </summary>
    public class DeviceOptionsException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        public DeviceOptionsException(string errorMessage) : base(errorMessage)
        {

        }
        /// <summary>
        /// 异常信息
        /// </summary>
        public override string Message
        {
            get
            {
                return "Device configuration Settings are abnormal.";
            }
        }
    }
    /// <summary>
    /// 初始化设备密钥等信息时，某个字段为空
    /// </summary>
    public class DeviceOptionsNullException : Exception
    {
        string _errorMessage;
        public DeviceOptionsNullException(string errorMessage) : base(errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public override string Message
        {
            get
            {
                return _errorMessage ;
            }
        }
    }
}
