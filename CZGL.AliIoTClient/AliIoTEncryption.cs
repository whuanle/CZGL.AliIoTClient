using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// MQTT 加密服务
    /// </summary>
    class AliIoTEncryption
    {
        /// <summary>
        /// 将相关参数生成MQTT客户端的用户名和密码
        /// </summary>
        /// <param name="param"></param>
        /// <param name="deviceSecret"></param>
        /// <param name="signMethod"></param>
        /// <returns></returns>
        public static string Sign(Dictionary<string, string> param, string deviceSecret, string signMethod)
        {
            string[] sortedKey = param.Keys.ToArray();
            Array.Sort(sortedKey);

            StringBuilder builder = new StringBuilder();
            foreach (var i in sortedKey)
            {
                builder.Append(i).Append(param[i]);
            }
            // deviceSecret
            byte[] key = Encoding.UTF8.GetBytes(deviceSecret);
            byte[] signContent = Encoding.UTF8.GetBytes(builder.ToString());
            //暂时只支持使用MD5加密， 其它验证方式：HmacMD5,HmacSHA1,HmacSHA256
            var hmac = new HMACMD5(key); // deviceSecret

            byte[] hashBytes = hmac.ComputeHash(signContent);

            StringBuilder signBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
                signBuilder.AppendFormat("{0:x2}", b);

            return signBuilder.ToString();
        }
    }
}
