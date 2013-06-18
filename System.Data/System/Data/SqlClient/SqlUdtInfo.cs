namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Generic;

    internal class SqlUdtInfo
    {
        internal readonly bool IsByteOrdered;
        internal readonly bool IsFixedLength;
        [ThreadStatic]
        private static Dictionary<Type, SqlUdtInfo> m_types2UdtInfo;
        internal readonly int MaxByteSize;
        internal readonly string Name;
        internal readonly Format SerializationFormat;
        internal readonly string ValidationMethodName;

        private SqlUdtInfo(SqlUserDefinedTypeAttribute attr)
        {
            this.SerializationFormat = attr.Format;
            this.IsByteOrdered = attr.IsByteOrdered;
            this.IsFixedLength = attr.IsFixedLength;
            this.MaxByteSize = attr.MaxByteSize;
            this.Name = attr.Name;
            this.ValidationMethodName = attr.ValidationMethodName;
        }

        internal static SqlUdtInfo GetFromType(Type target)
        {
            SqlUdtInfo info = TryGetFromType(target);
            if (info == null)
            {
                throw InvalidUdtException.Create(target, "SqlUdtReason_NoUdtAttribute");
            }
            return info;
        }

        internal static SqlUdtInfo TryGetFromType(Type target)
        {
            if (m_types2UdtInfo == null)
            {
                m_types2UdtInfo = new Dictionary<Type, SqlUdtInfo>();
            }
            SqlUdtInfo info = null;
            if (!m_types2UdtInfo.TryGetValue(target, out info))
            {
                object[] customAttributes = target.GetCustomAttributes(typeof(SqlUserDefinedTypeAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length == 1))
                {
                    info = new SqlUdtInfo((SqlUserDefinedTypeAttribute) customAttributes[0]);
                }
                m_types2UdtInfo.Add(target, info);
            }
            return info;
        }
    }
}

