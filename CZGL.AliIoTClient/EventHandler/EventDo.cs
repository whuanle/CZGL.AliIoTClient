using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 属性设置 事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PublishPropertyEventHandler(object sender, MqttMsgPublishEventArgs e);
    /// <summary>
    /// 服务调用 Topic 事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PublishServiceEventHandler(object sender, MqttMsgPublishEventArgs e);
    /// <summary>
    /// 一般的 Toic 事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PublishCommonEventHandler(object sender, MqttMsgPublishEventArgs e);
}
