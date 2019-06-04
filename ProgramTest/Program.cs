using System;
using System.Threading;
using CZGL.AliIoTClient;

namespace ProgramTest
{
    class Program
    {
        static AliIoTClientJson client;
        static void Main(string[] args)
        {
            // 创建客户端
            client = new AliIoTClientJson(new DeviceOptions
            {
                ProductKey = "a1xrkGSkb5R",
                DeviceName = "mire",
                DeviceSecret = "CqGMkOBDiKJfrOWp1evLZC2O6fsMtEXw",
                RegionId = "cn-shanghai"
            });

            // 设置要订阅的Topic、运行接收内容的Topic
            string[] topics = new string[] { client.CombineHeadTopic("get") };
            // 使用默认事件
            client.UseDefaultEventHandler();
            client.ConnectIoT(topics,null,60);
            // 连接服务器
            client.OpenPropertyPostReply();
            // 循环上传熟属性
            while (true)
            {
                var model = new PropertyModel();

                client.Thing_Property_Post<PropertyModel>(model,true);
                Thread.Sleep(500);
            }
            Console.ReadKey();
        }
    }
}
