using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// Alink json
    /// </summary>
    public class AliIoTClientJson
    {
        /// <summary>
        /// 一机一密
        /// </summary>
        /// <param name="deviceOptions">设备密钥等</param>
        public AliIoTClientJson(DeviceOptions deviceOptions)
        {
            //设备的密钥的信息不能为空
            if (string.IsNullOrEmpty(deviceOptions.ProductKey) || string.IsNullOrEmpty(deviceOptions.DeviceName) || string.IsNullOrEmpty(deviceOptions.DeviceSecret) || string.IsNullOrEmpty(deviceOptions.RegionId))
                throw new DeviceOptionsNullException("DeviceOptions any property can not be NULL or Empty");
            _deviceOptions = deviceOptions;
            Builder();
        }
        #region 实例对象


        MQTTUser user;

        //配置设备密钥等信息
        readonly DeviceOptions _deviceOptions;

        /// <summary>
        /// 生成设备属性、服务、事件通讯的topic
        /// </summary>
        public ThingJsonAddress thingAddress;
        // 通讯连接有关
        ConnectOptions connectOptions;
        //MQTT通讯客户端
        MqttClient client;

        /// <summary>
        /// 已订阅的Topic列表
        /// </summary>
        public Dictionary<string,byte> GetSubedList { get {return topicSubedList; } }

        private Dictionary<string, byte> topicSubedList = new Dictionary<string, byte>();
        OpenTopic openTopic = new OpenTopic();

        /// <summary>
        /// 是否与服务器保持连接
        /// </summary>
        public bool isConnected
        {
            get
            {
                if (client != null) return client.IsConnected;
                return false;
            }
        }


        #endregion


        #region 初始化

        /// <summary>
        /// 生成客户端信息和协议版本控制
        /// </summary>
        private void Builder()
        {
            thingAddress = new ThingJsonAddress(_deviceOptions.ProductKey, _deviceOptions.DeviceName);
            connectOptions = new ConnectOptions(_deviceOptions.ProductKey, _deviceOptions.RegionId, _deviceOptions.DeviceName);
            user = new MQTTUser();
            client = new MqttClient(connectOptions.targetServer);
            client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
            Init();
        }

        /// <summary>
        /// 初始化配置和对象
        /// </summary>
        private void Init()
        {
            // 生成唯一 ClientID
            string clientId = CreateClientId();
            string t = Convert.ToString(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            string signmethod = "hmacmd5";  // 使用hmacmd5加密 ,可使用其它验证方式：HmacMD5,HmacSHA1,HmacSHA256 签名算法验证

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("productKey", _deviceOptions.ProductKey);
            dict.Add("deviceName", _deviceOptions.DeviceName);
            dict.Add("clientId", clientId);
            dict.Add("timestamp", t);

            // 阿里云客户端连接认证说明 https://help.aliyun.com/document_detail/73742.html?spm=a2c4g.11186623.2.15.52003f86YTzGCo

            user.ClientId = clientId + "|securemode=3,signmethod=" + signmethod + ",timestamp=" + t + "|";
            user.UserName = _deviceOptions.DeviceName + "&" + _deviceOptions.ProductKey;
            user.Password = AliIoTEncryption.Sign(dict, _deviceOptions.DeviceSecret, signmethod);

        }

        #endregion


        #region 创建客户端、建立通讯

        /// <summary>
        /// 与服务器通讯，订阅需要的Topic
        /// </summary>
        /// <param name="SubTopic">订阅列表</param>
        /// <param name="QOS">QOS，0或1，默认全为0</param>
        /// <param name="keepAlivePeriod">反馈周期</param>
        public ConnectCode ConnectIoT(string[] SubTopic, byte[] QOS = null, ushort keepAlivePeriod = 60)
        {
            if (QOS == null)
            {
                QOS = new byte[SubTopic.Length];
                for (int i = 0; i < SubTopic.Length; i++)
                {
                    QOS[i] = 0x00;
                }
            }
            TopicAdd(SubTopic,QOS);
            // 设置各种触发事件
            AddPublishEvent();
            // 建立连接

            var returnCode = client.Connect(user.ClientId, user.UserName, user.Password, true, keepAlivePeriod);
            switch (returnCode)
            {
                case 0x00: return ConnectCode.conn_accepted;
                case 0x01: return ConnectCode.conn_refused_prot_vers;
                case 0x02: return ConnectCode.conn_refused_ident_rejected;
                case 0x03: return ConnectCode.conn_refused_server_unavailable;
                case 0x04: return ConnectCode.conn_refused_username_password;
                case 0x05: return ConnectCode.conn_refused_not_authorized;
            }
            return ConnectCode.unknown_error;
        }

        /// <summary>
        /// 与服务器通讯，订阅需要的Topic
        /// </summary>
        /// <param name="topics">订阅列表</param>
        /// <param name="QOS">QOS，0或1，默认全为0</param>
        /// <param name="keepAlivePeriod">反馈周期</param>
        public async Task<ConnectCode> ConnectIoTAsync(string[] topics, byte[] QOS = null, ushort keepAlivePeriod = 60)
        {
            byte returnCode;
            await Task.Run(() =>
            {
                if (QOS == null)
                {
                    QOS = new byte[topics.Length];
                    for (int i = 0; i < topics.Length; i++)
                    {
                        QOS[i] = 0;
                    }
                }
                client = new MqttClient(connectOptions.targetServer);
                client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
                // 订阅消息，若不指定Topic的QOS，则全部为 0
                TopicAdd(topics,QOS);
                // 设置各种触发事件
                AddPublishEvent();
                // 建立连接

            });
            returnCode = client.Connect(user.ClientId, user.UserName, user.Password, true, keepAlivePeriod);

            switch (returnCode)
            {
                case 0x00: return ConnectCode.conn_accepted;
                case 0x01: return ConnectCode.conn_refused_prot_vers;
                case 0x02: return ConnectCode.conn_refused_ident_rejected;
                case 0x03: return ConnectCode.conn_refused_server_unavailable;
                case 0x04: return ConnectCode.conn_refused_username_password;
                case 0x05: return ConnectCode.conn_refused_not_authorized;
            }
            return ConnectCode.unknown_error;
        }

        /// <summary>
        /// 断开通讯连接
        /// </summary>
        /// <returns></returns>
        public bool ConnectIoTClose()
        {
            client.Disconnect();
            return isConnected;
        }

        #endregion


        #region 增加或移除订阅

        /// <summary>
        /// 增加要订阅的Topic
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="QOS"></param>
        public void TopicAdd(string[] topics, byte[] QOS = null)
        {
            if (QOS == null)
            {
                QOS = new byte[topics.Length];
                for (int i = 0; i < topics.Length; i++)
                {
                    QOS[i] = 0x00;
                }
            }

            for (int i=0;i<topics.Length;i++)
            {
                if (topicSubedList.ContainsKey(topics[i]))
                {
                    topicSubedList[topics[i]] = QOS[i];
                    continue;
                }
                topicSubedList.Add(topics[i],QOS[i]);
            }

            string[] topiclist = new string[topicSubedList.Count];
            byte[] topicQos = new byte[topicSubedList.Count];

            int n = 0;
            foreach (var item in topicSubedList.Keys)
            {
                topiclist[n] = item;
                n+=1;
            }
            n = 0;
            foreach (var item in topicSubedList.Values)
            {
                topicQos[n] = item;
            }
            client.Subscribe(topiclist, topicQos);
        }
        /// <summary>
        /// 移除已经订阅的Topic
        /// </summary>
        /// <param name="topics"></param>
        public void TopicRemove(string[] topics)
        {
            client.Unsubscribe(topics);
            foreach (var item in topics)
            {
                topicSubedList.Remove(item);
            }
        }

        #endregion


        #region 发布与响应

        /*
         发布功能仅用于上传数据、发布、上传属性，格式由自己在服务器和本地定义
         */

        /*
         * 最终必须为 byte[]才能上传，默认以10进制处理。
         * 考虑到可能需要使用2进制或16进制，可先转换好byte[]再调用
         */


        #region 推送Topic 底架


        private int PublishToServer(string topicName, byte[] content)
        {
            return client.Publish(topicName, content);
        }

        private int PublishToServer(string topicName, string content, Encoding encoding)
        {
            return client.Publish(topicName, encoding.GetBytes(content));
        }

        private int PublishToServerUseBase(string topicName, byte[] bytes, Encoding encoding)
        {
            string base64 = Convert.ToBase64String(bytes);
            return client.Publish(topicName, encoding.GetBytes(base64));
        }

        #endregion


        #region 推送普通Topic到服务器

        /// <summary>
        /// 普通方式推送Topic到服务器
        /// </summary>
        /// <returns></returns>
        public int CommonToServer(string topicName, string content)
        {
            return PublishToServer(topicName, content, Encoding.UTF8);
        }

        /// <summary>
        /// 此种方式以byte[]形式上传数据，注意byte[]的进制
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServer(string topicName, byte[] content)
        {
            return PublishToServer(topicName, content);
        }
        /// <summary>
        /// 将content转为base64后上传
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServerBase64(string topicName, string content)
        {
            return PublishToServerUseBase(topicName, Encoding.UTF8.GetBytes(content), Encoding.UTF8);
        }

        /// <summary>
        /// 普通方式推送Topic到服务器
        /// </summary>
        /// <returns></returns>
        public int CommonToServer(string topicName, string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return PublishToServer(topicName, content, encoding);
        }

        /// <summary>
        /// 将content转为base64后上传
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServerBase64(string topicName, string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return PublishToServerUseBase(topicName, encoding.GetBytes(content), encoding);
        }

        #endregion


        #region 设备属性上报

        /// <summary>
        /// 接收服务器属性上传后的响应。设备上传属性后服务器会响应Alink json
        /// </summary>
        public void OpenPropertyPostReply()
        {
            openTopic.PropertyPostReplyTopic = true;
            TopicAdd(new string[] { thingAddress.propertyUP.post_reply });
        }
        /// <summary>
        /// 不接收接收服务器属性上传后的响应。设备上传属性后服务器会响应Alink json
        /// </summary>
        public void ClosePropertyPostReply()
        {
            openTopic.PropertyPostReplyTopic = false;
            TopicRemove(new string[] { thingAddress.propertyUP.post_reply });
        }

        /// <summary>
        /// 上传设备属性--Alink Json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public int Thing_Property_Post(byte[] json)
        {
            int id = PublishToServer(thingAddress.propertyUP.post, json);
            return id;
        }

        /// <summary>
        /// 上传设备属性--Alink Json
        /// </summary>
        /// <param name="json">josn</param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Property_Post(string json, bool isToLwer = true)
        {
            if (isToLwer == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyUP.post, json, Encoding.UTF8);
            return id;
        }
        /// <summary>
        /// 上传设备属性--Alink Json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="isToLwer">是否转为小写</param>
        /// <param name="encoding">指定编码格式</param>
        /// <returns></returns>
        public int Thing_Property_Post(string json, bool isToLwer = true, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            if (isToLwer == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyUP.post, json, encoding);
            return id;
        }


        /// <summary>
        /// 上传设备属性--Alink Json
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="model">模型</param>
        /// <param name="isToLower">是否全部转为小写</param>
        /// <returns></returns>
        public int Thing_Property_Post<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyUP.post, json, Encoding.UTF8);
            return id;
        }

        #endregion


        #region 设置设备属性
        /// <summary>
        /// 允许服务器下发设置属性命令
        /// </summary>
        public void OpenPropertyDownPost()
        {
            openTopic.PropertyDownPost = true;
            TopicAdd(new string[] { thingAddress.propertySET.set });
        }
        /// <summary>
        /// 不允许允许服务器下发设置属性命令
        /// </summary>
        public void ClosePropertyDownPost()
        {
            openTopic.PropertyDownPost = false;
            TopicRemove(new string[] { thingAddress.propertySET.set });
        }
        /// <summary>
        /// 收到服务器属性设置命令，返回响应
        /// </summary>
        /// <param name="content">响应内容</param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Property_set(PropertyDownModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            int id = PublishToServer(thingAddress.propertySET.set_reply, json, Encoding.UTF8);
            return id;
        }
        public int Thing_Property_set(PropertyDownModel model, bool isToLower = true, Encoding encoding = null)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServer(thingAddress.propertySET.set_reply, json, encoding);
            return id;
        }

        /// <summary>
        /// 设备属性下发设置
        /// </summary>
        /// <typeparam name="TModel">收到的json</typeparam>
        /// <param name="model">模型</param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Property_set<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertySET.set_reply, json, Encoding.UTF8);
            return id;
        }
        #endregion


        #region 设备事件上报

        /// <summary>
        /// 接收设备上传事件后服务器的响应
        /// </summary>
        public void OpenEventPostReply()
        {
            openTopic.EventPostReply = true;
            TopicAdd(new string[] { thingAddress.eventTopic.post_reply });
        }
        /// <summary>
        /// 不接收设备上传事件后服务器的响应
        /// </summary>
        public void CloseEventPostReply()
        {
            openTopic.EventPostReply = false;
            TopicRemove(new string[] { thingAddress.eventTopic.post_reply });
        }

        /// <summary>
        /// 设备事件上报 Alink JSON
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int Thing_Event_Post(string eventName, string content, bool isToLower = true)
        {
            if (isToLower == true)
                content = content.ToLower();
            thingAddress.eventTopic.post = eventName;
            int id = PublishToServer(thingAddress.eventTopic.post, content, Encoding.UTF8);
            return id;
        }
        public int Thing_Event_Post(string eventName, string content, bool isToLower = true, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            if (isToLower == true)
                content = content.ToLower();
            thingAddress.eventTopic.post = eventName;
            int id = PublishToServer(thingAddress.eventTopic.post, content, encoding);
            return id;
        }

        /// <summary>
        /// 设备事件上报 Alink JSON
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Event_Post<TModel>(TModel model, string eventName, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            thingAddress.eventTopic.post = eventName;
            int id = PublishToServer(thingAddress.eventTopic.post, json, Encoding.UTF8);
            return id;
        }

        /// <summary>
        /// 设备事件上报 Alink JSON
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Event_Post<TModel>(TModel model, string eventName, bool isToLower = true, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            thingAddress.eventTopic.post = eventName;
            int id = PublishToServer(thingAddress.eventTopic.post, json, encoding);
            return id;
        }

        #endregion


        #region 设备服务调用

        public void OpenServicePostRaw()
        {
            openTopic.ServicePostRaw = true;
            TopicAdd(new string[] { thingAddress.serviceTopic.identifier });
        }
        public void CloseServicePostRaw()
        {
            openTopic.ServicePostRaw = false;
            TopicRemove(new string[] { thingAddress.serviceTopic.identifier });
        }

        /// <summary>
        /// 服务器下发指令调用设备服务，对服务器作出响应
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Service_Identifier_Reply(ServiceReplyModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.serviceTopic.identifier_reply, json, Encoding.UTF8);
            return id;
        }

        /// <summary>
        /// 服务器下发指令调用设备服务，对服务器作出响应
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isToLower">是否转为小写</param>
        /// <returns></returns>
        public int Thing_Service_Identifier_Reply(ServiceReplyModel model, bool isToLower = true, Encoding encoding = null)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServer(thingAddress.serviceTopic.identifier_reply, json, encoding);
            return id;
        }


        #endregion


        #region 设备期望属性值
        /// <summary>
        /// 获取属性的期望值
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="isToLower"></param>
        /// <returns></returns>
        public int GetPropertyDesired<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyDesired.get, json, Encoding.UTF8);
            return id;
        }

        /// <summary>
        /// 清除属性的期望值
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="isToLower"></param>
        /// <returns></returns>
        public int DeletePropertyDesired<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyDesired.delete, json, Encoding.UTF8);
            return id;
        }
        #endregion
        #endregion

        #region 委托处理

        /// <summary>
        /// 添加订阅回调事件
        /// </summary>
        private void AddPublishEvent()
        {
            //订阅事件
            client.MqttMsgPublishReceived += PubEventHandler;
            client.MqttMsgPublished += PubedEventHandler;
            client.MqttMsgSubscribed += SubedEventHandler;
            client.MqttMsgUnsubscribed += UnSubedEventHandler;
            client.ConnectionClosed += ConnectionClosedEventHandler;
        }

        /// <summary>
        /// 全部使用预设的事件
        /// </summary>
        public void UseDefaultEventHandler()
        {
            PubEventHandler += Default_PubEventHandler;
            PubPropertyEventHandler += Default_PubPropertyEventHandler;
            PubServiceEventHandler += Default_PubServiceEventHandler;
            PubCommonEventHandler += Default_PubCommonEventHandler;

            PubedEventHandler += Default_PubedEventHandler;
            SubedEventHandler += Default_SubedEventHandler;
            UnSubedEventHandler += Default_UnSubedEventHandler;
            ConnectionClosedEventHandler += Default_ConnectionClosedEventHandler;
        }
        #endregion

        #region 设置回调事件
        /// <summary>
        /// 订阅回调 - 当收到服务器消息时
        /// </summary>
        private uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishEventHandler PubEventHandler;
        /// <summary>
        /// 服务器属性设置
        /// </summary>
        public PublishPropertyEventHandler PubPropertyEventHandler;
        /// <summary>
        /// 服务调用
        /// </summary>
        public PublishServiceEventHandler PubServiceEventHandler;
        /// <summary>
        /// 收到其它Topic时触发
        /// </summary>
        public PublishCommonEventHandler PubCommonEventHandler;
        /// <summary>
        /// 当 QOS=1或2时，收到订阅触发
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishedEventHandler PubedEventHandler;

        /// <summary>
        /// 向服务器发布 Topic 时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgSubscribedEventHandler SubedEventHandler;

        /// <summary>
        /// 向服务器发布 Topic 失败时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgUnsubscribedEventHandler UnSubedEventHandler;


        /// <summary>
        /// 断开连接时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.ConnectionClosedEventHandler ConnectionClosedEventHandler;

        #endregion

        #region 事件的默认方法

        /// <summary>
        /// 收到服务器的推送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Default_PubEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            if (topic.Contains("/thing/service/property/set"))
            {
                PubPropertyEventHandler(sender, e);
                return;
            }
            if (topic.Contains("/thing/service/"))
            {
                PubServiceEventHandler(sender, e);
                return;
            }
            PubCommonEventHandler(sender, e);
        }
        /// <summary>
        /// 一般的推送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_PubCommonEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        /// <summary>
        /// 收到属性设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_PubPropertyEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        /// <summary>
        /// 收到服务调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_PubServiceEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        /// <summary>
        /// 收到服务器QOS为1的推送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_PubedEventHandler(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("published,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId: " + e.MessageId + "    Is Published: " + e.IsPublished);
        }
        /// <summary>
        /// 向服务器推送成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_SubedEventHandler(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Sub topic,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId: " + e.MessageId);
            Console.WriteLine("List of granted QOS Levels:    " + Encoding.UTF8.GetString(e.GrantedQoSLevels));
        }
        /// <summary>
        /// 推送失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_UnSubedEventHandler(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Sub topic error,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId:    " + e.MessageId);
        }
        /// <summary>
        /// 连接发生异常，断网等
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Default_ConnectionClosedEventHandler(object sender, EventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Connect Closed error,Date: " + DateTime.Now.ToLongTimeString());
        }
        #endregion

        #region 其它函数

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetUnixTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 生成完整的Topic，只需传入 /user/后面的内容即可，如 update
        /// </summary>
        /// <param name="topicend"></param>
        /// <returns></returns>
        public string CombineHeadTopic(string topicend)
        {
            return connectOptions.TopicHead + topicend;
        }
        /// <summary>
        /// 用于生成唯一clientId
        /// </summary>
        /// <returns></returns>
        private static string CreateClientId()
        {
            return DeviceGUID.GetDeviceGuid();
        }

        /// <summary>
        /// 获取是否接收指令或接收响应
        /// </summary>
        /// <returns></returns>
        public OpenTopic getOpenTopic()
        {
            OpenTopic open = new OpenTopic();
            open.PropertyUpRawReplyTopic = openTopic.PropertyUpRawReplyTopic;
            open.PropertyPostReplyTopic = openTopic.PropertyPostReplyTopic;
            open.PropertyDownRaw = openTopic.PropertyDownRaw;
            open.PropertyDownPost = openTopic.PropertyDownPost;
            open.EventUpRawReply = openTopic.EventUpRawReply;
            open.EventPostReply = openTopic.EventPostReply;
            open.ServiceDownRaw = openTopic.ServiceDownRaw;
            open.ServicePostRaw = openTopic.ServicePostRaw;
            return open;
        }

        #endregion

    }



    /// <summary>
    /// 透传
    /// </summary>
    public class AliIoTClientBinary
    {
        /// <summary>
        /// 一机一密
        /// </summary>
        /// <param name="deviceOptions">设备密钥等</param>
        public AliIoTClientBinary(DeviceOptions deviceOptions)
        {
            //设备的密钥的信息不能为空
            if (string.IsNullOrEmpty(deviceOptions.ProductKey) || string.IsNullOrEmpty(deviceOptions.DeviceName) || string.IsNullOrEmpty(deviceOptions.DeviceSecret) || string.IsNullOrEmpty(deviceOptions.RegionId))
                throw new DeviceOptionsNullException("DeviceOptions any property can not be NULL or Empty");
            _deviceOptions = deviceOptions;
            Builder();
        }
        #region 实例对象


        MQTTUser user;

        //配置设备密钥等信息
        readonly DeviceOptions _deviceOptions;

        // 生成设备属性、服务、事件通讯的topic
        public ThingBinaryAddress thingAddress;
        // 通讯连接有关
        ConnectOptions connectOptions;
        //MQTT通讯客户端
        MqttClient client;

        /// <summary>
        /// 已订阅的Topic列表
        /// </summary>
        public Dictionary<string, byte> GetSubedList { get { return topicSubedList; } }
        private Dictionary<string, byte> topicSubedList = new Dictionary<string, byte>();
        OpenTopic openTopic = new OpenTopic();
        //是否已经连接
        public bool isConnected
        {
            get
            {
                if (client != null) return client.IsConnected;
                return false;
            }
        }


        #endregion


        #region 初始化

        /// <summary>
        /// 生成信息
        /// </summary>
        private void Builder()
        {
            thingAddress = new ThingBinaryAddress(_deviceOptions.ProductKey, _deviceOptions.DeviceName);
            connectOptions = new ConnectOptions(_deviceOptions.ProductKey, _deviceOptions.RegionId, _deviceOptions.DeviceName);
            user = new MQTTUser();
            client = new MqttClient(connectOptions.targetServer);
            client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
            Init();
        }

        /// <summary>
        /// 初始化配置和对象
        /// </summary>
        private void Init()
        {
            // 生成唯一 ClientID
            string clientId = CreateClientId();
            string t = Convert.ToString(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            string signmethod = "hmacmd5";  // 使用hmacmd5加密 ,可使用其它验证方式：HmacMD5,HmacSHA1,HmacSHA256 签名算法验证

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("productKey", _deviceOptions.ProductKey);
            dict.Add("deviceName", _deviceOptions.DeviceName);
            dict.Add("clientId", clientId);
            dict.Add("timestamp", t);

            // 阿里云客户端连接认证说明 https://help.aliyun.com/document_detail/73742.html?spm=a2c4g.11186623.2.15.52003f86YTzGCo

            user.ClientId = clientId + "|securemode=3,signmethod=" + signmethod + ",timestamp=" + t + "|";
            user.UserName = _deviceOptions.DeviceName + "&" + _deviceOptions.ProductKey;
            user.Password = AliIoTEncryption.Sign(dict, _deviceOptions.DeviceSecret, signmethod);

        }

        #endregion


        #region 创建客户端、建立通讯

        /// <summary>
        /// 与服务器通讯，订阅需要的Topic
        /// </summary>
        /// <param name="SubTopic">订阅列表</param>
        /// <param name="QOS">QOS，0或1，默认全为0</param>
        /// <param name="keepAlivePeriod">反馈周期</param>
        public ConnectCode ConnectIoT(string[] SubTopic, byte[] QOS = null, ushort keepAlivePeriod = 60)
        {
            if (QOS == null)
            {
                QOS = new byte[SubTopic.Length];
                for (int i = 0; i < SubTopic.Length; i++)
                {
                    QOS[i] = 0x00;
                }
            }
            TopicAdd(SubTopic, QOS);
            // 设置各种触发事件
            AddPublishEvent();
            // 建立连接

            var returnCode = client.Connect(user.ClientId, user.UserName, user.Password, true, keepAlivePeriod);
            switch (returnCode)
            {
                case 0x00: return ConnectCode.conn_accepted;
                case 0x01: return ConnectCode.conn_refused_prot_vers;
                case 0x02: return ConnectCode.conn_refused_ident_rejected;
                case 0x03: return ConnectCode.conn_refused_server_unavailable;
                case 0x04: return ConnectCode.conn_refused_username_password;
                case 0x05: return ConnectCode.conn_refused_not_authorized;
            }
            return ConnectCode.unknown_error;
        }

        /// <summary>
        /// 与服务器通讯，订阅需要的Topic
        /// </summary>
        /// <param name="topics">订阅列表</param>
        /// <param name="QOS">QOS，0或1，默认全为0</param>
        /// <param name="keepAlivePeriod">反馈周期</param>
        public async Task<ConnectCode> ConnectIoTAsync(string[] topics, byte[] QOS = null, ushort keepAlivePeriod = 60)
        {
            byte returnCode;
            await Task.Run(() =>
            {
                if (QOS == null)
                {
                    QOS = new byte[topics.Length];
                    for (int i = 0; i < topics.Length; i++)
                    {
                        QOS[i] = 0;
                    }
                }
                client = new MqttClient(connectOptions.targetServer);
                client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
                // 订阅消息，若不指定Topic的QOS，则全部为 0
                TopicAdd(topics, QOS);
                // 设置各种触发事件
                AddPublishEvent();
                // 建立连接

            });
            returnCode = client.Connect(user.ClientId, user.UserName, user.Password, true, keepAlivePeriod);

            switch (returnCode)
            {
                case 0x00: return ConnectCode.conn_accepted;
                case 0x01: return ConnectCode.conn_refused_prot_vers;
                case 0x02: return ConnectCode.conn_refused_ident_rejected;
                case 0x03: return ConnectCode.conn_refused_server_unavailable;
                case 0x04: return ConnectCode.conn_refused_username_password;
                case 0x05: return ConnectCode.conn_refused_not_authorized;
            }
            return ConnectCode.unknown_error;
        }

        /// <summary>
        /// 断开通讯连接
        /// </summary>
        /// <returns></returns>
        public bool ConnectIoTClose()
        {
            client.Disconnect();
            return isConnected;
        }

        #endregion


        #region 增加或移除订阅

        /// <summary>
        /// 增加要订阅的Topic
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="QOS"></param>
        public void TopicAdd(string[] topics, byte[] QOS = null)
        {
            if (QOS == null)
            {
                QOS = new byte[topics.Length];
                for (int i = 0; i < topics.Length; i++)
                {
                    QOS[i] = 0x00;
                }
            }

            for (int i = 0; i < topics.Length; i++)
            {
                if (topicSubedList.ContainsKey(topics[i]))
                {
                    topicSubedList[topics[i]] = QOS[i];
                    continue;
                }
                topicSubedList.Add(topics[i], QOS[i]);
            }

            string[] topiclist = new string[topicSubedList.Count];
            byte[] topicQos = new byte[topicSubedList.Count];

            int n = 0;
            foreach (var item in topicSubedList.Keys)
            {
                topiclist[n] = item;
                n += 1;
            }
            n = 0;
            foreach (var item in topicSubedList.Values)
            {
                topicQos[n] = item;
            }
            client.Subscribe(topiclist, topicQos);
        }
        /// <summary>
        /// 移除已经订阅的Topic
        /// </summary>
        /// <param name="topics"></param>
        public void TopicRemove(string[] topics)
        {
            client.Unsubscribe(topics);
            foreach (var item in topics)
            {
                topicSubedList.Remove(item);
            }
        }

        #endregion



        #region 发布与响应

        /*
         发布功能仅用于上传数据、发布、上传属性，格式由自己在服务器和本地定义
         */

        /*
         * 最终必须为 byte[]才能上传，默认以10进制处理。
         * 考虑到可能需要使用2进制或16进制，可先转换好byte[]再调用
         */


        #region 推送Topic 底架


        private int PublishToServer(string topicName, byte[] content)
        {
            return client.Publish(topicName, content);
        }

        private int PublishToServer(string topicName, string content, Encoding encoding)
        {
            return client.Publish(topicName, encoding.GetBytes(content));
        }

        private int PublishToServerUseBase(string topicName, byte[] bytes, Encoding encoding)
        {
            string base64 = Convert.ToBase64String(bytes);
            return client.Publish(topicName, encoding.GetBytes(base64));
        }

        #endregion


        #region 推送普通Topic到服务器

        /// <summary>
        /// 普通方式推送Topic到服务器
        /// </summary>
        /// <returns></returns>
        public int CommonToServer(string topicName, string content)
        {
            return PublishToServer(topicName, content, Encoding.UTF8);
        }

        /// <summary>
        /// 此种方式以byte[]形式上传数据，注意byte[]的进制
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServer(string topicName, byte[] content)
        {
            return PublishToServer(topicName, content);
        }
        /// <summary>
        /// 将content转为base64后上传
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServerBase64(string topicName, string content)
        {
            return PublishToServerUseBase(topicName, Encoding.UTF8.GetBytes(content), Encoding.UTF8);
        }

        /// <summary>
        /// 普通方式推送Topic到服务器
        /// </summary>
        /// <returns></returns>
        public int CommonToServer(string topicName, string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return PublishToServer(topicName, content, encoding);
        }

        /// <summary>
        /// 将content转为base64后上传
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public int CommonToServerBase64(string topicName, string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return PublishToServerUseBase(topicName, encoding.GetBytes(content), encoding);
        }

        #endregion


        #region 设备属性上报
        /// <summary>
        /// 接收服务器属性上传后的响应。设备上传属性后服务器会响应Alink json
        /// </summary>
        public void OpenPropertyUpRawReply()
        {
            openTopic.PropertyUpRawReplyTopic = true;
            TopicAdd(new string[] { thingAddress.propertyUP.up_raw_reply });
        }
        /// <summary>
        /// 不接收接收服务器属性上传后的响应。设备上传属性后服务器会响应Alink json
        /// </summary>
        public void ClosePropertyUpRawReply()
        {
            openTopic.PropertyUpRawReplyTopic = false;
            TopicRemove(new string[] { thingAddress.propertyUP.up_raw_reply });
        }
        /// <summary>
        /// 设备上传属性--透传
        /// </summary>
        /// <param name="betys">要发布的数据</param>
        public int Thing_Property_UpRaw(byte[] bytes)
        {
            int id = PublishToServer(thingAddress.propertyUP.up_raw, bytes);
            return id;
        }

        /// <summary>
        /// 设备上传属性--透传,转为 Base 64位加密后上传
        /// </summary>
        /// <param name="betys">要发布的数据</param>
        public int Thing_Property_UpRawToBase64(byte[] bytes, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServerUseBase(thingAddress.propertyUP.up_raw, bytes, encoding);
            return id;
        }
        #endregion


        #region 设置设备属性


        /// <summary>
        /// 接收服务器下发设置属性命令
        /// </summary>
        public void OpenPropertyDownRaw()
        {
            openTopic.PropertyDownRaw = true;
            TopicAdd(new string[] { thingAddress.propertySET.down_raw });
        }
        /// <summary>
        /// 忽略服务器下发设置属性命令
        /// </summary>
        public void ClosePropertyDownRaw()
        {
            openTopic.PropertyDownRaw = false;
            TopicRemove(new string[] { thingAddress.propertySET.down_raw });
        }

        /// <summary>
        /// 收到服务器属性设置命令，返回响应
        /// </summary>
        /// <param name="content">响应内容</param>
        /// <returns></returns>
        public int Thing_Property_down_raw_reply(PropertyDownModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            int id = PublishToServer(thingAddress.propertySET.down_raw_reply, json, Encoding.UTF8);
            return id;
        }
        public int Thing_Property_down_raw_reply(PropertyDownModel model, bool isToLower = true, Encoding encoding = null)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServer(thingAddress.propertySET.down_raw_reply, json, encoding);
            return id;
        }

        #endregion


        #region 设备事件上报

        /// <summary>
        /// 接收设备上传事件后服务器的响应
        /// </summary>
        public void OpenEventUpRawReply()
        {
            openTopic.EventUpRawReply = true;
            TopicAdd(new string[] { thingAddress.eventTopic.up_raw_reply });
        }
        /// <summary>
        /// 不接收设备上传事件后服务器的响应
        /// </summary>
        public void CloseEventUpRawReply()
        {
            openTopic.EventUpRawReply = false;
            TopicRemove(new string[] { thingAddress.eventTopic.up_raw_reply });
        }
        /// <summary>
        /// 设备事件上报--透传
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int Thing_Event_up_raw(string content)
        {
            int id = PublishToServer(thingAddress.eventTopic.up_raw, content, Encoding.UTF8);
            return id;
        }

        /// <summary>
        /// 设备事件上报--透传
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding">指定编码</param>
        /// <returns></returns>
        public int Thing_Event_up_raw(string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServer(thingAddress.eventTopic.up_raw, content, encoding);
            return id;
        }

        /// <summary>
        /// 设备事件上报-透传，Base64加密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int Thing_Event_up_raw_Base64(byte[] content)
        {
            int id = PublishToServerUseBase(thingAddress.eventTopic.up_raw, content, Encoding.UTF8);
            return id;
        }

        #endregion


        #region 设备服务调用

        
        public void OpenServiceDownRaw()
        {
            openTopic.ServiceDownRaw = true;
            TopicAdd(new string[] { thingAddress.serviceTopic.down_raw });
        }
        public void CloseServiceDownRaw()
        {
            openTopic.ServiceDownRaw = false;
            TopicRemove(new string[] { thingAddress.serviceTopic.down_raw });
        }

        /// <summary>
        /// 设备服务调用--透传
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int Thing_Service_Down_Reply(ServiceReplyModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            int id = PublishToServer(thingAddress.serviceTopic.down_raw_reply, json, Encoding.UTF8);
            return id;
        }
        /// <summary>
        /// 设备服务调用--透传
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int Thing_Service_Down_Reply(ServiceReplyModel model, bool isToLower = true, Encoding encoding = null)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();
            if (encoding == null) encoding = Encoding.UTF8;
            int id = PublishToServer(thingAddress.serviceTopic.down_raw_reply, json, encoding);
            return id;
        }
        #endregion


        #region 设备期望属性值
        /// <summary>
        /// 获取属性的期望值
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="isToLower"></param>
        /// <returns></returns>
        public int GetPropertyDesired<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyDesired.get, json, Encoding.UTF8);
            return id;
        }

        /// <summary>
        /// 清除属性的期望值
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="isToLower"></param>
        /// <returns></returns>
        public int DeletePropertyDesired<TModel>(TModel model, bool isToLower = true)
        {
            string json = JsonConvert.SerializeObject(model);
            if (isToLower == true)
                json = json.ToLower();

            int id = PublishToServer(thingAddress.propertyDesired.delete, json, Encoding.UTF8);
            return id;
        }
        #endregion
        #endregion

        #region 委托处理

        /// <summary>
        /// 添加订阅回调事件
        /// </summary>
        private void AddPublishEvent()
        {
            //订阅事件
            client.MqttMsgPublishReceived += PubEventHandler;
            client.MqttMsgPublished += PubedEventHandler;
            client.MqttMsgSubscribed += SubedEventHandler;
            client.MqttMsgUnsubscribed += UnSubedEventHandler;
            client.ConnectionClosed += ConnectionClosedEventHandler;
        }

        /// <summary>
        /// 全部使用预设的事件
        /// </summary>
        public void UseDefaultEventHandler()
        {
            PubEventHandler += Default_PubEventHandler;
            PubPropertyEventHandler += Default_PubPropertyEventHandler;
            PubServiceEventHandler += Default_PubServiceEventHandler;
            PubCommonEventHandler += Default_PubCommonEventHandler;

            PubedEventHandler += Default_PubedEventHandler;
            SubedEventHandler += Default_SubedEventHandler;
            UnSubedEventHandler += Default_UnSubedEventHandler;
            ConnectionClosedEventHandler += Default_ConnectionClosedEventHandler;
        }
        #endregion

        #region 设置回调事件
        /// <summary>
        /// 订阅回调 - 当收到服务器消息时
        /// </summary>
        private uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishEventHandler PubEventHandler;
        /// <summary>
        /// 服务器属性设置
        /// </summary>
        public PublishPropertyEventHandler PubPropertyEventHandler;
        /// <summary>
        /// 服务d调用
        /// </summary>
        public PublishServiceEventHandler PubServiceEventHandler;
        public PublishCommonEventHandler PubCommonEventHandler;
        /// <summary>
        /// 当 QOS=1或2时，收到订阅触发
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishedEventHandler PubedEventHandler;

        /// <summary>
        /// 向服务器发布 Topic 时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgSubscribedEventHandler SubedEventHandler;

        /// <summary>
        /// 向服务器发布 Topic 失败时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgUnsubscribedEventHandler UnSubedEventHandler;


        /// <summary>
        /// 断开连接时
        /// </summary>
        public uPLibrary.Networking.M2Mqtt.MqttClient.ConnectionClosedEventHandler ConnectionClosedEventHandler;

        #endregion

        #region 事件的默认方法


        private void Default_PubEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            if (topic.Contains("/thing/service/property/set"))
            {
                PubPropertyEventHandler(sender, e);
                return;
            }
            if (topic.Contains("/thing/service/"))
            {
                PubServiceEventHandler(sender, e);
                return;
            }
            PubCommonEventHandler(sender, e);
        }
        public void Default_PubCommonEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        public void Default_PubPropertyEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        public void Default_PubServiceEventHandler(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            string topic = e.Topic;
            string message = Encoding.ASCII.GetString(e.Message);
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("get topic message,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("get messgae :\n" + message);
        }
        public void Default_PubedEventHandler(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("published,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId: " + e.MessageId + "    Is Published: " + e.IsPublished);
        }

        public void Default_SubedEventHandler(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Sub topic,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId: " + e.MessageId);
            Console.WriteLine("List of granted QOS Levels:    " + Encoding.UTF8.GetString(e.GrantedQoSLevels));
        }
        public void Default_UnSubedEventHandler(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Sub topic error,Date: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("MessageId:    " + e.MessageId);
        }

        public void Default_ConnectionClosedEventHandler(object sender, EventArgs e)
        {
            Console.WriteLine("- - - - - - - - - - ");
            Console.WriteLine("Connect Closed error,Date: " + DateTime.Now.ToLongTimeString());
        }
        #endregion

        #region 其它函数

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetUnixTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 生成完整的Topic，只需传入 /user/后面的内容即可，如 update
        /// </summary>
        /// <param name="topicend"></param>
        /// <returns></returns>
        public string CombineHeadTopic(string topicend)
        {
            return connectOptions.TopicHead + topicend;
        }
        /// <summary>
        /// 用于生成唯一clientId
        /// </summary>
        /// <returns></returns>
        private static string CreateClientId()
        {
            return DeviceGUID.GetDeviceGuid();
        }

        /// <summary>
        /// 获取是否接收指令或接收响应
        /// </summary>
        /// <returns></returns>
        public OpenTopic getOpenTopic()
        {
            OpenTopic open = new OpenTopic();
            open.PropertyUpRawReplyTopic = openTopic.PropertyUpRawReplyTopic;
            open.PropertyPostReplyTopic = openTopic.PropertyPostReplyTopic;
            open.PropertyDownRaw = openTopic.PropertyDownRaw;
            open.PropertyDownPost = openTopic.PropertyDownPost;
            open.EventUpRawReply = openTopic.EventUpRawReply;
            open.EventPostReply = openTopic.EventPostReply;
            open.ServiceDownRaw = openTopic.ServiceDownRaw;
            open.ServicePostRaw = openTopic.ServicePostRaw;
            return open;
        }

        #endregion
    }
}
