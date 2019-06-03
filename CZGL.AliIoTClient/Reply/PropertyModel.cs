using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AliIoTClient
{

    /// <summary>
    /// 设备属性上传,服务器响应
    /// </summary>
    public class PropertyReplyModel
    {
        public long code { get; set; }
        public string data { get; set; }
        public string id { get; set; }

        //public string message { get; set; }
        ////"method": "thing.event.property.post"
        //public string method { get; set; }
        //public string version { get; set; }
        //{
        //	"code": 200,
        //	"data": {},
        //	"id": "636941560559592037",
        //	"message": "success",
        //	"method": "thing.event.property.post",
        //	"version": "1.0"
        //}
    }
    /// <summary>
    /// 属性上报报错代码说明
    /// </summary>
    public class PropertyReplyCodeModel
    {
        public long code { get; set; }
        public string message { get; set; }
        public string describe { get; set; }

        public PropertyReplyCodeModel(long _code)
        {
            switch (_code)
            {
                case 460: code = 460; message = "request parameter error"; describe = "请求参数错误"; break;
                case 6106: code = 6106; message = "map size must less than 200"; describe = "一次最多只能上报200条属性"; break;
                case 6313: code = 6313; message = "tsl service not available"; describe = "物模型校验服务不可用"; break;
                default: code = 0; message = null; describe = null; break;
            }
        }
    }
}
