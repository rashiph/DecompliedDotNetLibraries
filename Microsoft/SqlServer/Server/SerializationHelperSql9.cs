namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;

    internal class SerializationHelperSql9
    {
        [ThreadStatic]
        private static Hashtable m_types2Serializers;

        private SerializationHelperSql9()
        {
        }

        internal static object Deserialize(Stream s, Type resultType)
        {
            return GetSerializer(resultType).Deserialize(s);
        }

        private static object[] GetCustomAttributes(Type t)
        {
            return t.GetCustomAttributes(typeof(SqlUserDefinedTypeAttribute), false);
        }

        private static Format GetFormat(Type t)
        {
            return GetUdtAttribute(t).Format;
        }

        private static Serializer GetNewSerializer(Type t)
        {
            GetUdtAttribute(t);
            Format format = GetFormat(t);
            switch (format)
            {
                case Format.Native:
                    return new NormalizedSerializer(t);

                case Format.UserDefined:
                    return new BinarySerializeSerializer(t);
            }
            throw ADP.InvalidUserDefinedTypeSerializationFormat(format);
        }

        private static Serializer GetSerializer(Type t)
        {
            if (m_types2Serializers == null)
            {
                m_types2Serializers = new Hashtable();
            }
            Serializer newSerializer = (Serializer) m_types2Serializers[t];
            if (newSerializer == null)
            {
                newSerializer = GetNewSerializer(t);
                m_types2Serializers[t] = newSerializer;
            }
            return newSerializer;
        }

        internal static SqlUserDefinedTypeAttribute GetUdtAttribute(Type t)
        {
            object[] customAttributes = GetCustomAttributes(t);
            if ((customAttributes == null) || (customAttributes.Length != 1))
            {
                throw InvalidUdtException.Create(t, "SqlUdtReason_NoUdtAttribute");
            }
            return (SqlUserDefinedTypeAttribute) customAttributes[0];
        }

        internal static int GetUdtMaxLength(Type t)
        {
            SqlUdtInfo fromType = SqlUdtInfo.GetFromType(t);
            if (Format.Native == fromType.SerializationFormat)
            {
                return SizeInBytes(t);
            }
            return fromType.MaxByteSize;
        }

        internal static void Serialize(Stream s, object instance)
        {
            GetSerializer(instance.GetType()).Serialize(s, instance);
        }

        internal static int SizeInBytes(object instance)
        {
            GetFormat(instance.GetType());
            DummyStream s = new DummyStream();
            GetSerializer(instance.GetType()).Serialize(s, instance);
            return (int) s.Length;
        }

        internal static int SizeInBytes(Type t)
        {
            return SizeInBytes(Activator.CreateInstance(t));
        }
    }
}

