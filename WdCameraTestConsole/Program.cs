using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace WdCameraTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //SerializeTest();
                GetIpServerAddress();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            //var ori = (PtzControl)Enum.Parse(typeof(PtzControl), "Up1");
            //Console.WriteLine(ori);
            //Console.WriteLine((int)ori);
            //Console.ReadKey();
        }

        private static void GetIpServerAddress()
        {
            var inited = CHCNetSDK.NET_DVR_Init();
            if (inited)
            {
                var senum = Encoding.UTF8.GetBytes("DS-7804N-K1/C0420161112CCRR677808992WVU");
                byte[] devIp = new byte[2048];
                uint port = 0;
                var ret = CHCNetSDK.NET_DVR_GetDVRIPByResolveSvr_EX("114.55.175.99", 7071, null, 0, senum, (ushort)senum.Length, devIp,
                    ref port);
                Console.WriteLine(ret);
                if (!ret)
                {
                    Console.WriteLine(CHCNetSDK.NET_DVR_GetLastError());
                }
            }
        }

        private static void SerializeTest()
        {
            //生成测试对象
            var info = new CameraInfo
            {
                DomainName = "wdnvr0002",
                User = "admin",
                Password = "juli#406"
            };

            //测试对象序列化为JSON字符串
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(typeof(CameraInfo));
            serializer.Serialize(stringwriter, info);
            var jsonStr = stringwriter.ToString();
            Console.WriteLine($"对象未加密XML字符串：\r\n{jsonStr}");

            //生成RSA加密密钥
            var rsa = new RSACryptoServiceProvider(4096);
            var publicKey = rsa.ToXmlString(false);
            var privateKey = rsa.ToXmlString(true);

            //加密JSON字符串
            var encryptString = EncryptString(jsonStr, publicKey);
            Console.WriteLine($"JSON字符串加密后的字符串：\r\n{encryptString}");

            //用加密后的JSON字符串生成BASE64字符串
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptString));
            Console.WriteLine($"BASE64字符串:\r\n{base64String}");

            //还原BASE64字符串为JSON加密字符串
            var oringinal = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
            Console.WriteLine($"JSON加密字符串:\r\n{oringinal}");

            //解密为原始JSON字符串
            var decryptString = DecryptString(encryptString, privateKey);
            Console.WriteLine($"解密后的原始JSON字符串：\r\n{decryptString}");

            //反序列化解密后的JSON字符串为测试对象
            var stringReader = new StringReader(decryptString);
            var serializer2 = new XmlSerializer(typeof(CameraInfo));
            var infotwo = (CameraInfo)serializer2.Deserialize(stringReader);

            //输出测试对象属性
            Console.WriteLine(infotwo.User);
            Console.WriteLine(infotwo.DomainName);
            Console.WriteLine(infotwo.Password);

            foreach (var domainByte in infotwo.DomainBytes)
            {
                Console.Write($"{domainByte},");
            }

            Console.ReadKey();
        }

        private static string EncryptString(string sSource, string sPublicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            var plaintext = sSource;
            rsa.FromXmlString(sPublicKey);
            var cipherbytes = rsa.Encrypt(Encoding.ASCII.GetBytes(plaintext), false);

            var sbString = new StringBuilder();
            foreach (var t in cipherbytes)
            {
                sbString.Append(t + ",");
            }

            return sbString.ToString();
        }

        private static string DecryptString(string sSource, string sPrivateKey)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(sPrivateKey);
            var byteEn = rsa.Encrypt(Encoding.ASCII.GetBytes("a"), false);
            var sBytes = sSource.Split(',');

            for (var j = 0; j < sBytes.Length; j++)
            {
                if (sBytes[j] != "")
                {
                    byteEn[j] = byte.Parse(sBytes[j]);
                }
            }
            var plaintbytes = rsa.Decrypt(byteEn, false);
            return Encoding.ASCII.GetString(plaintbytes);
        }
    }

    public class CameraInfo
    {
        public string DomainName { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public byte[] DomainBytes => Encoding.UTF8.GetBytes(DomainName);
    }

    public enum PtzControl
    {
        Up = 0x01,

        Down = 0x02,

        Left = 0x03,

        Right = 0x04
    }
}
