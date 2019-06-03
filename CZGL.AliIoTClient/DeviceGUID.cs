using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;

namespace CZGL.AliIoTClient
{
    /// <summary>
    /// 设备GUID
    /// </summary>
    public static class DeviceGUID
    {
        /// <summary>
        /// 获取设备GUID
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceGuid()
        {
            string dir = Directory.GetCurrentDirectory() + "/Info";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string file = "deviceGuid";
            if (File.Exists(dir + "/" + file))
            {
                byte[] content = File.ReadAllBytes(dir + "/" + file);
                if (content.Length > 10)
                    return Encoding.Default.GetString(content);

                string guidinfo = GetGuid();
                File.WriteAllText(dir + "/" + file, guidinfo);
                return guidinfo;
            }
            string guid = GetGuid();
            using (File.CreateText(dir + "/" + file))
            {
                
            }File.WriteAllText(dir + "/" + file, guid);
            return guid;
        }

        /// <summary>
        /// 生成GUID
        /// </summary>
        /// <returns></returns>
        private static string GetGuid()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string clientId = "";
            try
            {
                clientId = host.AddressList.FirstOrDefault(
                ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                clientId += "-";
            }
            catch
            {

            }
            var guid = Guid.NewGuid();
            clientId += guid.ToString("D");
            return clientId;
        }
    }
}
