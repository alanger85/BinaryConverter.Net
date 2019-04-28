using System;

namespace BinaryConverter
{
    public static class BinaryConvert
    {
        public static T DeserializeObject<T>(byte[] buf)
        {
            return Deserializer.DeserializeObject<T>(buf);  
        }

        public static byte[] SerializeObject<T>(T value)
        {
            return Serializer.SerializeObject(value);
        }
    }
}
