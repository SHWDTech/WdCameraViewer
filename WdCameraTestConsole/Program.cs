using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WdCameraTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = new DataContractJsonSerializer(typeof(CameraInfo));
            var info = new CameraInfo
            {
                DomainName = "wdnvr0002",
                CommandPort = 8000,
                User = "admin",
                Password = "juli#406"
            };

            var jsonStr = string.Empty;

            using (var stream = new MemoryStream())
            {
                json.WriteObject(stream, info);
                jsonStr = Encoding.UTF8.GetString(stream.ToArray());
            }
            Console.WriteLine(jsonStr);

            CameraInfo infotwo;
            using (var streamTwo = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                var dcj = new DataContractJsonSerializer(typeof(CameraInfo));
                infotwo = (CameraInfo) dcj.ReadObject(streamTwo);
            }

            Console.WriteLine(infotwo.User);
            Console.WriteLine(infotwo.DomainName);
            Console.WriteLine(infotwo.Password);
            Console.WriteLine(infotwo.CommandPort);

            Console.ReadKey();
        }
    }

    public class CameraInfo
    {
        public string DomainName { get; set; }

        public ushort CommandPort { get; set; }

        public string User { get; set; }

        public string Password { get; set; }
    }
}
