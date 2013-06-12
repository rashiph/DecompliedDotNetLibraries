namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;

    internal sealed class AssemblyCache
    {
        private AssemblyCache()
        {
        }

        internal static SqlUdtInfo GetInfoFromType(Type t)
        {
            Type type = t;
            while (true)
            {
                SqlUdtInfo info = SqlUdtInfo.TryGetFromType(t);
                if (info != null)
                {
                    return info;
                }
                t = t.BaseType;
                if (t == null)
                {
                    throw SQL.UDTInvalidSqlType(type.AssemblyQualifiedName);
                }
            }
        }

        internal static int GetLength(object inst)
        {
            return SerializationHelperSql9.SizeInBytes(inst);
        }
    }
}

