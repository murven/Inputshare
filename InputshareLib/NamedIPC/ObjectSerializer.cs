using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace InputshareLib.NamedIPC
{
    public static class ObjectSerializer
    {
        private static BinaryFormatter binF = new BinaryFormatter();

        public static byte[] Serialize(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                binF.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(0, SeekOrigin.Begin);
                T obj = (T)binF.Deserialize(ms);
                return obj;
            }
        }
    }
}
