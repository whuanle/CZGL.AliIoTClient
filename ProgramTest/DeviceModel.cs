using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramTest
{
    /// <summary>
    /// 设备属性模型
    /// </summary>
    public class PropertyModel
    {
        public string id { get { return DateTime.Now.Ticks.ToString(); } set { } }
        public string version { get { return "1.0"; } set { } }
        public Params @params { get; set; }
        public PropertyModel()
        {
            @params = new Params();
        }
        public class Params
        {
            public Params()
            {
                cpu_temperature = new Cpu_temperature();
                cpu_used = new Cpu_used();
                memory_used = new Memory_used();
                currentvoltage = new Currentvoltage();
                current = new Current();
                envhumidity = new Envhumidity();
                GeoLocation = new geoLocation();
            }
            public static float cpu_temperature_value = 45;
            public static Int32 cpu_used_value = 60;
            public static Int32 memory_used_value = 35;
            public static double currentvoltage_value = 5;
            public static double current_value = 3;
            public static float envhumidity_value = 53;
            public Cpu_temperature cpu_temperature { get; set; }
            public Cpu_used cpu_used { get; set; }
            public Memory_used memory_used { get; set; }
            public Currentvoltage currentvoltage { get; set; }
            public Current current { get; set; }
            public Envhumidity envhumidity { get; set; }
            public geoLocation GeoLocation { get; set; }
            public class geoLocation
            {
                public Value value { get; set; }
                public geoLocation()
                {
                    value = new Value();
                }
                public class Value
                {
                    public double Longitude { get { return 113.9506673813; } set { } }
                    public double Latitude { get { return 22.5871239416; } set { } }
                    public double Altitude { get { return 39.9935723; } set { } }
                    public int CoordinateSystem { get { return 2; } set { } }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
            public class Cpu_temperature
            {
                public float value
                {
                    get
                    {
                        
                        return DeviceSimulate.Property(ref cpu_temperature_value, 40, 50);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }

            public class Cpu_used
            {
                public Int32 value
                {
                    get
                    {
                        
                        return DeviceSimulate.Property(ref cpu_used_value, 1, 45, 65);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
            public class Memory_used
            {
                public Int32 value
                {
                    get
                    {
                        return DeviceSimulate.Property(ref memory_used_value, 1, 30, 70);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
            public class Currentvoltage
            {
                public double value
                {
                    get
                    {
                        return  DeviceSimulate.Property(ref currentvoltage_value, 4.5, 5.5);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
            public class Current
            {
                public double value
                {
                    get
                    {
                        return DeviceSimulate.Property(ref current_value, 2.0, 3.6);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
            public class Envhumidity
            {
                public float value
                {
                    get
                    {
                        return DeviceSimulate.Property(ref envhumidity_value, 40, 66);
                    }
                    set { }
                }
                public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            }
        }
        public string methoD { get { return "thing.event.property.post"; } set { } }
    }

    /// <summary>
    /// 温度警报事件
    /// </summary>
    public class cpuwarn
    {
        public string name = "cpuwarn";
        public cpuwarn()
        {
            paramS = new Params();
        }
        public string id { get { return DateTime.Now.Ticks.ToString(); } set { } }
        public string version { get { return "1.0"; } set { } }
        public Params paramS { get; set; }
        public class Params
        {
            public Params()
            {
                value = new Value();
            }
            public Value value { get; set; }
            public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            public class Value
            {
                public float cpuwarn { get { return PropertyModel.Params.cpu_temperature_value; } set { } }
            }

        }
        public string methoD { get { return $"thing.event.{name}.post"; } set { } }
    }
    public class currentwarn
    {
        string name = "currentwarn";
        public currentwarn()
        {
            paramS = new Params();
        }
        public string id { get { return DateTime.Now.Ticks.ToString(); } set { } }
        public string version { get { return "1.0"; } set { } }
        public Params paramS { get; set; }
        public class Params
        {
            public Params()
            {
                value = new Value();
            }
            public Value value { get; set; }
            public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            public class Value
            {
            }

        }
        public string methoD { get { return $"thing.event.{name}.post"; } set { } }
    }
    public class deviceinfo
    {
        string name = "deviceinfo";
        public deviceinfo()
        {
            paramS = new Params();
        }
        public string id { get { return DateTime.Now.Ticks.ToString(); } set { } }
        public string version { get { return "1.0"; } set { } }
        public Params paramS { get; set; }
        public class Params
        {
            public Params()
            {
                value = new Value();
            }
            public Value value { get; set; }
            public long time { get { return DeviceSimulate.ConvertDateTimeToInt(); } set { } }
            public class Value
            {
                public string info { get { return "设备运行正常"; } set { } }
            }

        }
        public string methoD { get { return $"thing.event.{name}.post"; } set { } }
    }

   
    /// <summary>
    /// 模拟数据
    /// </summary>
    public static class DeviceSimulate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original">原始数据</param>
        /// <param name="range">波动范围</param>
        /// <param name="min">阈值最小值</param>
        /// <param name="max">阈值最大值</param>
        /// <returns></returns>
        public static int Property(ref int original, int range, int min, int max)
        {
            int num = (new Random()).Next(0, range + 1);
            bool addorrm;
            if (original + num > max || original > max)
                addorrm = false;
            else if (original < min || original - num < min)
                addorrm = true;
            else addorrm = ((new Random()).Next(1, 3) > 1) ? true : false;

            if (addorrm == true)
                 original += num;
            else
            original -= num;
            return original;
        }

        public static float Property(ref float original, float min, float max)
        {
            original = float.Parse(original.ToString("#0.00"));
            float num = float.Parse(((new Random()).NextDouble() / 8).ToString("#0.00"));
            bool addorrm;
            if (original + num > max || original > max)
                addorrm = false;
            else if (original < min || original - num < min)
                addorrm = true;
            else addorrm = ((new Random()).Next(1, 3) > 1) ? true : false;

            if (addorrm == true)
                 original += num;
            else
           original -= num;
            original = float.Parse(original.ToString("#0.00"));
            return original;
        }
        public static double Property(ref double original, double min, double max)
        {
            original = double.Parse(original.ToString("#0.000"));
            double num = double.Parse(((new Random()).NextDouble() / 8).ToString("#0.000"));
            bool addorrm;
            if (original + num > max || original > max)
                addorrm = false;
            else if (original < min || original - num < min)
                addorrm = true;
            else addorrm = ((new Random()).Next(1, 3) > 1) ? true : false;


            if (addorrm == true)
               original += num;
            else original -= num;
            original = double.Parse(original.ToString("#0.000"));
            return original;
        }

        [Obsolete]
        public static long ConvertDateTimeToInt()
        {
            System.DateTime time = DateTime.Now;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
    }
}
