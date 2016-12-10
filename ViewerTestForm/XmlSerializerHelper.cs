using System.IO;
using System.Xml.Serialization;

namespace ViewerTestForm
{
    public class XmlSerializerHelper
    {
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string Serialize<T>(T target)
        {
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stringwriter, target);
            return stringwriter.ToString();
        }

        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string jsonString)
        {
            var stringReader = new StringReader(jsonString);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stringReader);
        }
    }
}
