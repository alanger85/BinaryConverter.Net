﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BinaryConverter
{
    public static class Deserializer
    {
        public static T DeserializeObject<T>(byte[] buf)
        {
            return (T)DeserializeObject(buf, typeof(T));
        }

        public static object DeserializeObject(byte[] buf, Type type)
        {
            using (MemoryStream ms = new MemoryStream(buf))
            {

                using (BinaryTypesReader br = new BinaryTypesReader(ms))
                {
                    return DeserializeObject(br, type);
                }
            }
        }

        private static object DeserializeObject(BinaryTypesReader br, Type type)
        {
            if (type.IsPrimitive)
            {
                switch (type.FullName)
                {
                    case SystemTypeDefs.FullNameBoolean:
                    case SystemTypeDefs.FullNameByte:
                    case SystemTypeDefs.FullNameSByte:
                    case SystemTypeDefs.FullNameInt16:
                    case SystemTypeDefs.FullNameUInt16:
                    case SystemTypeDefs.FullNameInt32:
                    case SystemTypeDefs.FullNameUInt32:
                    case SystemTypeDefs.FullNameInt64:
                    case SystemTypeDefs.FullNameUInt64:
                    case SystemTypeDefs.FullNameChar:
                        {
                            var val = br.Read7BitLong();
                            return Convert.ChangeType(val, type);
                        }
                    case SystemTypeDefs.FullNameSingle: // todo: compact
                        return br.ReadSingle();
                    case SystemTypeDefs.FullNameDouble: // todo: compact
                        return br.ReadDouble();

                }
            }

            if (type.FullName == typeof(decimal).FullName)
            {
                return br.ReadCompactDecimal();
            }

            if (type.IsEnum)
            {
                int val = (int)br.Read7BitLong();
                return Enum.ToObject(type, val);
            }

            if (type.IsValueType)
            {
                switch (type.FullName)
                {
                    case SystemTypeDefs.FullNameDateTime:
                        return new DateTime(br.Read7BitLong());
                }
            }

            if (type.IsClass)
            {
                switch (type.FullName)
                {
                    case SystemTypeDefs.FullNameString:
                        return br.ReadString();
                    default:
                        return DeserializeClass(br, type);
                }
            }

            return null;
        }

        private static object DeserializeClass(BinaryTypesReader br, Type type)
        {
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(x => x.MetadataToken);

            var instance = Activator.CreateInstance(type);


            foreach (var prop in props)
            {
                prop.SetValue(instance, DeserializeObject(br, prop.PropertyType));
            }

            return instance;
        }
    }
}
