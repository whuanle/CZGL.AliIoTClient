using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{
    /*
     * 参考阿里云官方
     https://help.aliyun.com/document_detail/89301.html?spm=a2c4g.11174283.6.661.3a8b1668RBT8Nv
     */

     /// <summary>
     /// 与IoT通讯的各类地址
     /// </summary>
    public class ThingJsonAddress
    {
        /// <summary>
        /// 设备属性上传
        /// </summary>
        public PropertyUP propertyUP { get; set; }
        /// <summary>
        /// 设置设备属性
        /// </summary>
        public PropertySET propertySET { get; set; }
        /// <summary>
        /// 设备上报事件
        /// </summary>
        public EventTopic eventTopic { get; set; }
        /// <summary>
        /// 服务器调用设备服务
        /// </summary>
        public ServiceTopic serviceTopic { get; set; }
        /// <summary>
        /// 设备期望值
        /// </summary>
        public PropertyDesired propertyDesired { get; set; }

        public class PropertyDesired
        {
            public PropertyDesired(string productKey,string deviceName)
            {
                get = $"/sys/{productKey}/{deviceName}/thing/property/desired/get";
                get_reply = $"/sys/{productKey}/{deviceName}/thing/property/desired/get_reply";
                delete = $"/sys/{productKey}/{deviceName}/thing/property/desired/delete";
                delete_reply = $"/sys/{productKey}/{deviceName}/thing/property/desired/delete_reply";
            }
            public string get { get; set; }
            public string get_reply { get; set; }
            public string delete { get; set; }
            public string delete_reply { get; set; }
        }
        public ThingJsonAddress(string productKey, string deviceName)
        {
            propertyUP = new PropertyUP(productKey, deviceName);
            propertySET = new PropertySET(productKey, deviceName);
            eventTopic = new EventTopic(productKey, deviceName);
            serviceTopic = new ServiceTopic(productKey, deviceName);
            propertyDesired = new PropertyDesired(productKey, deviceName);
        }
        /// <summary>
        /// 设备属性上传
        /// </summary>
        public class PropertyUP
        {
            /// <summary>
            /// 设备上报属性请求Topic--Alink JSON
            /// </summary>
            public string post { get; set; }

            /// <summary>
            /// 设备上报属性响应Topic--Alink JSON
            /// </summary>
            public string post_reply { get; set; }


            public PropertyUP(string productKey, string deviceName)
            {
                post = $"/sys/{productKey}/{deviceName}/thing/event/property/post";
                post_reply = $"/sys/{productKey}/{deviceName}/thing/event/property/post_reply";
            }

        }

        /// <summary>
        /// 设置设备属性
        /// </summary>
        public class PropertySET
        {

            /// <summary>
            /// 下行（Alink JSON）请求Topic
            /// </summary>
            public string set { get; set; }
            /// <summary>
            /// 下行（Alink JSON）响应Topic
            /// </summary>
            public string set_reply { get; set; }

            public PropertySET(string productKey, string deviceName)
            {
                set = $"/sys/{productKey}/{deviceName}/thing/service/property/set";
                set_reply = $"/sys/{productKey}/{deviceName}/thing/service/property/set_reply";
            }
        }

        /// <summary>
        /// 设备事件上报
        /// </summary>
        public class EventTopic
        {
            string eventHeader { get; set; }
            /// <summary>
            /// 上行（Alink JSON）请求Topic
            /// </summary>
            public string post
            {
                get { return eventPortName; }
                set
                {
                    eventPortName = eventHeader + $"{value}/post";
                }
            }
            private string eventPortName = "{tsl.event.identifier}";
            private string eventPortReplyName = "{tsl.event.identifier}";
            /// <summary>
            /// 上行（Alink JSON）响应Topic
            /// </summary>
            public string post_reply { get { return eventPortReplyName; } set { eventPortReplyName = eventHeader + $"{value}/post_reply"; } }
            public EventTopic(string productKey, string deviceName)
            {
                eventHeader = $"/sys/{productKey}/{deviceName}/thing/event/";
                post = "{tsl.event.identifier}";
                post_reply = "{tsl.event.identifier}";
            }
        }

        /// <summary>
        /// 设备服务调用
        /// </summary>
        public class ServiceTopic
        {

            /// <summary>
            ///  下行（Alink JSON）请求Topic
            /// </summary>
            public string identifier { get; set; }
            /// <summary>
            /// 下行（Alink JSON）Topic
            /// </summary>
            public string identifier_reply { get; set; }

            public ServiceTopic(string productKey, string deviceName)
            {
                identifier = $"/sys/{productKey}/{deviceName}/thing/service/";//   + "{tsl.service.identifier}";
                identifier_reply = $"/sys/{productKey}/{deviceName}/thing/service/";    // + "{tsl.service.identifier}_reply";
            }
        }

        /// <summary>
        /// 属性、事件、服务 Alink响应数据格式
        /// </summary>
        public class post_replyModel
        {

            public string id { get; set; }
            public int code { get; set; }
            public string data { get; set; }

        }
    }

    /// <summary>
    /// 与IoT通讯的各类地址
    /// </summary>
    public class ThingBinaryAddress
    {
        /// <summary>
        /// 设备属性上传
        /// </summary>
        public PropertyUP propertyUP { get; set; }
        /// <summary>
        /// 设置设备属性
        /// </summary>
        public PropertySET propertySET { get; set; }
        /// <summary>
        /// 设备上报事件
        /// </summary>
        public EventTopic eventTopic { get; set; }
        /// <summary>
        /// 服务器调用设备服务
        /// </summary>
        public ServiceTopic serviceTopic { get; set; }
        /// <summary>
        /// 设备期望值
        /// </summary>
        public PropertyDesired propertyDesired { get; set; }

        public class PropertyDesired
        {
            public PropertyDesired(string productKey, string deviceName)
            {
                get = $"/sys/{productKey}/{deviceName}/thing/property/desired/get";
                get_reply = $"/sys/{productKey}/{deviceName}/thing/property/desired/get_reply";
                delete = $"/sys/{productKey}/{deviceName}/thing/property/desired/delete";
                delete_reply = $"/sys/{productKey}/{deviceName}/thing/property/desired/delete_reply";
            }
            public string get { get; set; }
            public string get_reply { get; set; }
            public string delete { get; set; }
            public string delete_reply { get; set; }
        }
        public ThingBinaryAddress(string productKey, string deviceName)
        {
            propertyUP = new PropertyUP(productKey, deviceName);
            propertySET = new PropertySET(productKey, deviceName);
            eventTopic = new EventTopic(productKey, deviceName);
            serviceTopic = new ServiceTopic(productKey, deviceName);
            propertyDesired = new PropertyDesired(productKey, deviceName);
        }
        /// <summary>
        /// 设备属性上传
        /// </summary>
        public class PropertyUP
        {
            /// <summary>
            /// 设备上报属性请求Topic--透传
            /// </summary>
            public string up_raw { get; set; }

            /// <summary>
            /// 设备上报属性响应Topic--透传
            /// </summary>
            public string up_raw_reply { get; set; }


            public PropertyUP(string productKey, string deviceName)
            {
                up_raw = $"/sys/{productKey}/{deviceName}/thing/model/up_raw";
                up_raw_reply = $"/sys/{productKey}/{deviceName}/thing/model/up_raw_reply";
            }

        }

        /// <summary>
        /// 设置设备属性
        /// </summary>
        public class PropertySET
        {
            /// <summary>
            /// 下行（透传）请求Topic
            /// </summary>
            public string down_raw { get; set; }
            /// <summary>
            /// 下行（透传）响应Topic
            /// </summary>
            public string down_raw_reply { get; set; }


            public PropertySET(string productKey, string deviceName)
            {
                down_raw = $"/sys/{productKey}/{deviceName}/thing/model/down_raw";
                down_raw_reply = $"/sys/{productKey}/{deviceName}/thing/model/down_raw_reply";
            }
        }

        /// <summary>
        /// 设备事件上报
        /// </summary>
        public class EventTopic
        {
            /// <summary>
            /// 上行（透传） 请求Topic
            /// </summary>
            public string up_raw { get; set; }
            /// <summary>
            /// 上行（透传）响应Topic
            /// </summary>
            public string up_raw_reply { get; set; }
            string eventHeader { get; set; }
            public EventTopic(string productKey, string deviceName)
            {
                eventHeader = $"/sys/{productKey}/{deviceName}/thing/event/";
                up_raw = $"/sys/{productKey}/{deviceName}/thing/model/up_raw";
                up_raw_reply = $"/sys/{productKey}/{deviceName}/thing/model/up_raw_reply";

            }
        }

        /// <summary>
        /// 设备服务调用
        /// </summary>
        public class ServiceTopic
        {
            /// <summary>
            /// 下行（透传）请求Topic
            /// </summary>
            public string down_raw { get; set; }
            /// <summary>
            /// 下行（透传）响应Topic
            /// </summary>
            public string down_raw_reply { get; set; }

            public ServiceTopic(string productKey, string deviceName)
            {
                down_raw = $"/sys/{productKey}/{deviceName}/thing/model/down_raw";
                down_raw_reply = $"/sys/{productKey}/{deviceName}/thing/model/down_raw_reply";
            }
        }

        /// <summary>
        /// 透传-请求属性、事件、服务返回格式
        /// </summary>
        public class up_rawModel
        {
            public string id { get; set; }
            public int code { get; set; }
            public string method { get; set; }
            public string data { get; set; }
        }
    }
}
