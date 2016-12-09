using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ViewerTestForm
{
    public class JsonHelper
    {
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serialObject"></param>
        /// <returns></returns>
        public static string Serialize<T>(T serialObject)
        {
            using (var stream = new MemoryStream())
            {
                var json = new DataContractJsonSerializer(typeof(T));
                json.WriteObject(stream, serialObject);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string jsonString)
        {
            using (var streamTwo = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                var dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(streamTwo);
            }
        }
    }
}
