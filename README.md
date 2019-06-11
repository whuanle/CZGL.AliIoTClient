Welcome to the CZGL.AliIoTClient wiki!

CZGL.AliIoTClient 是一个以 .NET Core 编写的阿里云物联网 SDK，里面做了很多兼容性测试，能够应用于 X86/ARM 的 CPU，兼容 32/64 位系统。  

你可以在 Nuget 中搜索 CZGL.AliIoTClient，安装最新版本即可。

SDK提供了对接阿里云物联网平台的类库，采用MQTT协议(M2MQTT.DotNetCore)，可以快速开发物联网设备对接阿里云物联网的程序。
包括通讯连接与加密、Topic推送和订阅、设备属性上传和设置、事件上报、服务调用、位置上传，支持透传和Alink json两种数据方式。
documentation address：https://www.cnblogs.com/whuanle/

不多说什么，看一下 Wiki 就知道是干嘛的了。  

使用 Apache Licence 2.0 开源许可证  


PS：开发这个东西，是为了兴趣。另外，可以加个学分。。。

Alink json已经测试完毕，而且Demo都是用Alink json模式写的，  
CZGL.AliIoTClient 里有两个客户端类，分Alink json/透传，互不影响。

**透传部分由于时间原因，服务调用和事件上报有些地方没有完整测试，不过不会影响使用**。  

需要在 Docker 中运行，我可以提供编写 Dockerfile 文件的模板和镜像环境。.NET Core 是很优秀的平台，与 Docker、K8S 完美契合。  

后面有时间会更新在工业板、树莓派上实际开发的案例。    

目前华为智能边缘平台公测，免费使用，可以申请下。https://www.huaweicloud.com/product/ief.html
华为智能边缘可以快速部署容器、应用，主要还是需要开发者准备好镜像和硬件(需要能够运行Docker)。

笔者现在大三，博客 https://www.cnblogs.com/whuanle
