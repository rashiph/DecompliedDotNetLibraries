namespace System.Data.Design
{
    using System;
    using System.Data;

    internal class TypeConvertions
    {
        private static object[] dbTypeToUrtTypeMap = new object[] { 
            DbType.AnsiString, typeof(string), DbType.AnsiStringFixedLength, typeof(string), DbType.Binary, typeof(byte[]), DbType.Boolean, typeof(bool), DbType.Byte, typeof(byte), DbType.Currency, typeof(decimal), DbType.Date, typeof(DateTime), DbType.DateTime, typeof(DateTime), 
            DbType.DateTime2, typeof(DateTime), DbType.DateTimeOffset, typeof(DateTimeOffset), DbType.Decimal, typeof(decimal), DbType.Double, typeof(double), DbType.Guid, typeof(Guid), DbType.Int16, typeof(short), DbType.Int32, typeof(int), DbType.Int64, typeof(long), 
            DbType.Object, typeof(object), DbType.SByte, typeof(byte), DbType.Single, typeof(float), DbType.String, typeof(string), DbType.StringFixedLength, typeof(string), DbType.Time, typeof(DateTime), DbType.UInt16, typeof(ushort), DbType.UInt32, typeof(uint), 
            DbType.UInt64, typeof(ulong), DbType.VarNumeric, typeof(decimal)
         };
        private static int[] oleDbToAdoPlusDirectionMap = new int[] { 1, 1, 2, 3, 3, 2, 4, 6 };
        private static int[] oleDbTypeToDbTypeMap = new int[] { 
            8, 0, 20, 12, 0x80, 1, 11, 3, 0x81, 0, 6, 4, 7, 5, 0x85, 5, 
            0x86, 6, 0x87, 6, 14, 7, 5, 8, 0, 13, 10, 13, 0x40, 6, 0x48, 9, 
            9, 13, 13, 13, 3, 11, 0xcd, 1, 0xc9, 0, 0xcb, 0x10, 0x83, 7, 0x8a, 13, 
            4, 15, 2, 10, 0x10, 14, 0x15, 20, 0x13, 0x13, 0x12, 0x12, 0x11, 2, 0xcc, 1, 
            200, 0, 0x8b, 0x15, 0xca, 0x10, 12, 13, 130, 0x10
         };
        private static object[] sqlTypeToSqlDbTypeMap = new object[] { 
            typeof(SqlBinary), SqlDbType.Binary, typeof(SqlInt64), SqlDbType.BigInt, typeof(SqlBoolean), SqlDbType.Bit, typeof(SqlString), SqlDbType.Char, typeof(SqlDateTime), SqlDbType.DateTime, typeof(SqlDecimal), SqlDbType.Decimal, typeof(SqlDouble), SqlDbType.Float, typeof(SqlBinary), SqlDbType.Image, 
            typeof(SqlInt32), SqlDbType.Int, typeof(SqlMoney), SqlDbType.Money, typeof(SqlString), SqlDbType.NChar, typeof(SqlString), SqlDbType.NText, typeof(SqlString), SqlDbType.NVarChar, typeof(SqlSingle), SqlDbType.Real, typeof(SqlDateTime), SqlDbType.SmallDateTime, typeof(SqlInt16), SqlDbType.SmallInt, 
            typeof(SqlMoney), SqlDbType.SmallMoney, typeof(object), SqlDbType.Variant, typeof(SqlString), SqlDbType.VarChar, typeof(SqlString), SqlDbType.Text, typeof(SqlBinary), SqlDbType.Timestamp, typeof(SqlByte), SqlDbType.TinyInt, typeof(SqlBinary), SqlDbType.VarBinary, typeof(SqlString), SqlDbType.VarChar, 
            typeof(SqlGuid), SqlDbType.UniqueIdentifier
         };
        private static object[] sqlTypeToUrtType = new object[] { 
            typeof(SqlBinary), typeof(byte[]), typeof(SqlByte), typeof(byte), typeof(SqlDecimal), typeof(decimal), typeof(SqlDouble), typeof(double), typeof(SqlGuid), typeof(Guid), typeof(SqlString), typeof(string), typeof(SqlSingle), typeof(float), typeof(SqlDateTime), typeof(DateTime), 
            typeof(SqlInt16), typeof(short), typeof(SqlInt32), typeof(int), typeof(SqlInt64), typeof(long), typeof(SqlMoney), typeof(decimal), typeof(object), typeof(object)
         };

        private TypeConvertions()
        {
        }

        internal static Type DbTypeToUrtType(DbType dbType)
        {
            for (int i = 0; i < dbTypeToUrtTypeMap.Length; i += 2)
            {
                if (dbType == ((DbType) dbTypeToUrtTypeMap[i]))
                {
                    return (Type) dbTypeToUrtTypeMap[i + 1];
                }
            }
            return null;
        }

        internal static Type SqlDbTypeToSqlType(SqlDbType sqlDbType)
        {
            for (int i = 1; i < sqlTypeToSqlDbTypeMap.Length; i += 2)
            {
                if (sqlDbType == ((SqlDbType) sqlTypeToSqlDbTypeMap[i]))
                {
                    return (Type) sqlTypeToSqlDbTypeMap[i - 1];
                }
            }
            return null;
        }
    }
}

