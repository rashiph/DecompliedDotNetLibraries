namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal static class ValueUtilsSmi
    {
        private const bool _ = false;
        private static bool[,] __canAccessGetterDirectly;
        private static bool[,] __canAccessSetterDirectly;
        private static SqlBuffer.StorageType[] __dbTypeToStorageType;
        private const int __maxByteChunkSize = 0x1f40;
        private const int __maxCharChunkSize = 0xfa0;
        private static object[] __typeSpecificNullForSqlValue;
        private const int NoLengthLimit = -1;
        private const bool X = true;
        private static readonly DateTime x_dtSmallMax;
        private static readonly DateTime x_dtSmallMin;
        private static readonly TimeSpan x_timeMax;
        private static readonly TimeSpan x_timeMin;

        static ValueUtilsSmi()
        {
            object[] objArray = new object[0x23];
            objArray[0] = SqlInt64.Null;
            objArray[1] = SqlBinary.Null;
            objArray[2] = SqlBoolean.Null;
            objArray[3] = SqlString.Null;
            objArray[4] = SqlDateTime.Null;
            objArray[5] = SqlDecimal.Null;
            objArray[6] = SqlDouble.Null;
            objArray[7] = SqlBinary.Null;
            objArray[8] = SqlInt32.Null;
            objArray[9] = SqlMoney.Null;
            objArray[10] = SqlString.Null;
            objArray[11] = SqlString.Null;
            objArray[12] = SqlString.Null;
            objArray[13] = SqlSingle.Null;
            objArray[14] = SqlGuid.Null;
            objArray[15] = SqlDateTime.Null;
            objArray[0x10] = SqlInt16.Null;
            objArray[0x11] = SqlMoney.Null;
            objArray[0x12] = SqlString.Null;
            objArray[0x13] = SqlBinary.Null;
            objArray[20] = SqlByte.Null;
            objArray[0x15] = SqlBinary.Null;
            objArray[0x16] = SqlString.Null;
            objArray[0x17] = DBNull.Value;
            objArray[0x19] = SqlXml.Null;
            objArray[0x1f] = DBNull.Value;
            objArray[0x20] = DBNull.Value;
            objArray[0x21] = DBNull.Value;
            objArray[0x22] = DBNull.Value;
            __typeSpecificNullForSqlValue = objArray;
            SqlBuffer.StorageType[] typeArray = new SqlBuffer.StorageType[0x23];
            typeArray[0] = SqlBuffer.StorageType.Int64;
            typeArray[1] = SqlBuffer.StorageType.SqlBinary;
            typeArray[2] = SqlBuffer.StorageType.Boolean;
            typeArray[3] = SqlBuffer.StorageType.String;
            typeArray[4] = SqlBuffer.StorageType.DateTime;
            typeArray[5] = SqlBuffer.StorageType.Decimal;
            typeArray[6] = SqlBuffer.StorageType.Double;
            typeArray[7] = SqlBuffer.StorageType.SqlBinary;
            typeArray[8] = SqlBuffer.StorageType.Int32;
            typeArray[9] = SqlBuffer.StorageType.Money;
            typeArray[10] = SqlBuffer.StorageType.String;
            typeArray[11] = SqlBuffer.StorageType.String;
            typeArray[12] = SqlBuffer.StorageType.String;
            typeArray[13] = SqlBuffer.StorageType.Single;
            typeArray[14] = SqlBuffer.StorageType.SqlGuid;
            typeArray[15] = SqlBuffer.StorageType.DateTime;
            typeArray[0x10] = SqlBuffer.StorageType.Int16;
            typeArray[0x11] = SqlBuffer.StorageType.Money;
            typeArray[0x12] = SqlBuffer.StorageType.String;
            typeArray[0x13] = SqlBuffer.StorageType.SqlBinary;
            typeArray[20] = SqlBuffer.StorageType.Byte;
            typeArray[0x15] = SqlBuffer.StorageType.SqlBinary;
            typeArray[0x16] = SqlBuffer.StorageType.String;
            typeArray[0x19] = SqlBuffer.StorageType.SqlXml;
            typeArray[0x1f] = SqlBuffer.StorageType.Date;
            typeArray[0x20] = SqlBuffer.StorageType.Time;
            typeArray[0x21] = SqlBuffer.StorageType.DateTime2;
            typeArray[0x22] = SqlBuffer.StorageType.DateTimeOffset;
            __dbTypeToStorageType = typeArray;
            x_dtSmallMax = new DateTime(0x81f, 6, 6, 0x17, 0x3b, 0x1d, 0x3e6);
            x_dtSmallMin = new DateTime(0x76b, 12, 0x1f, 0x17, 0x3b, 0x1d, 0x3e7);
            x_timeMin = TimeSpan.Zero;
            x_timeMax = new TimeSpan(0xc92a69bfffL);
            __canAccessGetterDirectly = new bool[,] { 
                { 
                    false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, 
                    false, true, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, true, false, false, false, true, false, false, false, false, false, false, 
                    false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, 
                { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, true, false, false, false, true, false, false, true, true, true, false, false, false, 
                    false, false, true, true, false, true, true, false, false, true, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, true, false, true, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, 
                    false, true, false
                 }, { 
                    false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                    false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, 
                { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, true, false, true, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, true
                 }
             };
            __canAccessSetterDirectly = new bool[,] { 
                { 
                    false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, true, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, 
                    false, true, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, true, false, false, false, true, false, false, false, false, false, false, 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, true, false, true, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, 
                { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, true, false, true, false, true, false, true, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, true, false, true, false, true, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, true, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, 
                    false, true, false
                 }, { 
                    false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, 
                { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, 
                    false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, true, false, true, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, true, false, false, false, false, false, false, true, true, true, false, false, false, 
                    false, false, true, false, false, false, true, true, false, false, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, true, false, true, false, true, false, false, false, false, false, true, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, 
                    false, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    true, false, false
                 }, { 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, true
                 }
             };
        }

        private static bool CanAccessGetterDirectly(SmiMetaData metaData, ExtendedClrTypeCode setterTypeCode)
        {
            bool flag = __canAccessGetterDirectly[(int) setterTypeCode, (int) metaData.SqlDbType];
            if (!flag || (((ExtendedClrTypeCode.DataTable != setterTypeCode) && (ExtendedClrTypeCode.DbDataReader != setterTypeCode)) && (ExtendedClrTypeCode.IEnumerableOfSqlDataRecord != setterTypeCode)))
            {
                return flag;
            }
            return metaData.IsMultiValued;
        }

        private static bool CanAccessSetterDirectly(SmiMetaData metaData, ExtendedClrTypeCode setterTypeCode)
        {
            bool flag = __canAccessSetterDirectly[(int) setterTypeCode, (int) metaData.SqlDbType];
            if (!flag || (((ExtendedClrTypeCode.DataTable != setterTypeCode) && (ExtendedClrTypeCode.DbDataReader != setterTypeCode)) && (ExtendedClrTypeCode.IEnumerableOfSqlDataRecord != setterTypeCode)))
            {
                return flag;
            }
            return metaData.IsMultiValued;
        }

        private static int CheckXetParameters(SqlDbType dbType, long maxLength, long actualLength, long fieldOffset, int bufferLength, int bufferOffset, int length)
        {
            if (0L > fieldOffset)
            {
                throw ADP.NegativeParameter("fieldOffset");
            }
            if (bufferOffset < 0)
            {
                throw ADP.InvalidDestinationBufferIndex(bufferLength, bufferOffset, "bufferOffset");
            }
            if (bufferLength < 0)
            {
                length = (int) PositiveMin((long) length, PositiveMin(maxLength, actualLength));
                if (length < -1)
                {
                    length = -1;
                }
                return length;
            }
            if (bufferOffset > bufferLength)
            {
                throw ADP.InvalidDestinationBufferIndex(bufferLength, bufferOffset, "bufferOffset");
            }
            if ((length + bufferOffset) > bufferLength)
            {
                throw ADP.InvalidBufferSizeOrIndex(length, bufferOffset);
            }
            if (length < 0)
            {
                throw ADP.InvalidDataLength((long) length);
            }
            if ((0L <= actualLength) && (actualLength <= fieldOffset))
            {
                return 0;
            }
            length = Math.Min(length, bufferLength - bufferOffset);
            if (SqlDbType.Variant == dbType)
            {
                length = Math.Min(length, 0x1f40);
            }
            if (0L <= actualLength)
            {
                length = (int) Math.Min((long) length, actualLength - fieldOffset);
            }
            else if ((SqlDbType.Udt != dbType) && (0L <= maxLength))
            {
                length = (int) Math.Min((long) length, maxLength - fieldOffset);
            }
            if (length < 0)
            {
                return 0;
            }
            return length;
        }

        internal static Stream CopyIntoNewSmiScratchStream(Stream source, SmiEventSink_Default sink, SmiContext context)
        {
            Stream stream;
            int length;
            int num2;
            if (context == null)
            {
                stream = new MemoryStream();
            }
            else
            {
                stream = new SqlClientWrapperSmiStream(sink, context.GetScratchStream(sink));
            }
            if (source.CanSeek && (0x1f40L > source.Length))
            {
                length = (int) source.Length;
            }
            else
            {
                length = 0x1f40;
            }
            byte[] buffer = new byte[length];
            while ((num2 = source.Read(buffer, 0, length)) != 0)
            {
                stream.Write(buffer, 0, num2);
            }
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }

        internal static SqlStreamChars CopyIntoNewSmiScratchStreamChars(Stream source, SmiEventSink_Default sink, SmiContext context)
        {
            int length;
            int num2;
            SqlClientWrapperSmiStreamChars chars = new SqlClientWrapperSmiStreamChars(sink, context.GetScratchStream(sink));
            if (source.CanSeek && (0x1f40L > source.Length))
            {
                length = (int) source.Length;
            }
            else
            {
                length = 0x1f40;
            }
            byte[] buffer = new byte[length];
            while ((num2 = source.Read(buffer, 0, length)) != 0)
            {
                chars.Write(buffer, 0, num2);
            }
            chars.Flush();
            chars.Seek(0L, SeekOrigin.Begin);
            return chars;
        }

        internal static void FillCompatibleITypedSettersFromReader(SmiEventSink_Default sink, ITypedSettersV3 setters, SmiMetaData[] metaData, SqlDataReader reader)
        {
            for (int i = 0; i < metaData.Length; i++)
            {
                if (reader.IsDBNull(i))
                {
                    SetDBNull_Unchecked(sink, setters, i);
                    continue;
                }
                switch (metaData[i].SqlDbType)
                {
                    case SqlDbType.BigInt:
                    {
                        SetInt64_Unchecked(sink, setters, i, reader.GetInt64(i));
                        continue;
                    }
                    case SqlDbType.Binary:
                    {
                        SetSqlBytes_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlBytes(i), 0);
                        continue;
                    }
                    case SqlDbType.Bit:
                    {
                        SetBoolean_Unchecked(sink, setters, i, reader.GetBoolean(i));
                        continue;
                    }
                    case SqlDbType.Char:
                    {
                        SetSqlChars_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlChars(i), 0);
                        continue;
                    }
                    case SqlDbType.DateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], reader.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Decimal:
                    {
                        SetSqlDecimal_Unchecked(sink, setters, i, reader.GetSqlDecimal(i));
                        continue;
                    }
                    case SqlDbType.Float:
                    {
                        SetDouble_Unchecked(sink, setters, i, reader.GetDouble(i));
                        continue;
                    }
                    case SqlDbType.Image:
                    {
                        SetSqlBytes_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlBytes(i), 0);
                        continue;
                    }
                    case SqlDbType.Int:
                    {
                        SetInt32_Unchecked(sink, setters, i, reader.GetInt32(i));
                        continue;
                    }
                    case SqlDbType.Money:
                    {
                        SetSqlMoney_Unchecked(sink, setters, i, metaData[i], reader.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    {
                        SetSqlChars_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlChars(i), 0);
                        continue;
                    }
                    case SqlDbType.Real:
                    {
                        SetSingle_Unchecked(sink, setters, i, reader.GetFloat(i));
                        continue;
                    }
                    case SqlDbType.UniqueIdentifier:
                    {
                        SetGuid_Unchecked(sink, setters, i, reader.GetGuid(i));
                        continue;
                    }
                    case SqlDbType.SmallDateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], reader.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.SmallInt:
                    {
                        SetInt16_Unchecked(sink, setters, i, reader.GetInt16(i));
                        continue;
                    }
                    case SqlDbType.SmallMoney:
                    {
                        SetSqlMoney_Checked(sink, setters, i, metaData[i], reader.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.Text:
                    {
                        SetSqlChars_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlChars(i), 0);
                        continue;
                    }
                    case SqlDbType.Timestamp:
                    {
                        SetSqlBytes_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlBytes(i), 0);
                        continue;
                    }
                    case SqlDbType.TinyInt:
                    {
                        SetByte_Unchecked(sink, setters, i, reader.GetByte(i));
                        continue;
                    }
                    case SqlDbType.VarBinary:
                    {
                        SetSqlBytes_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlBytes(i), 0);
                        continue;
                    }
                    case SqlDbType.VarChar:
                    {
                        SetSqlChars_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlChars(i), 0);
                        continue;
                    }
                    case SqlDbType.Variant:
                    {
                        object sqlValue = reader.GetSqlValue(i);
                        ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCode(sqlValue);
                        SetCompatibleValue(sink, setters, i, metaData[i], sqlValue, typeCode, 0);
                        continue;
                    }
                    case SqlDbType.Xml:
                    {
                        SetSqlXml_Unchecked(sink, setters, i, reader.GetSqlXml(i));
                        continue;
                    }
                    case SqlDbType.Udt:
                    {
                        SetSqlBytes_LengthChecked(sink, setters, i, metaData[i], reader.GetSqlBytes(i), 0);
                        continue;
                    }
                }
                throw ADP.NotSupported();
            }
        }

        internal static void FillCompatibleITypedSettersFromRecord(SmiEventSink_Default sink, ITypedSettersV3 setters, SmiMetaData[] metaData, SqlDataRecord record)
        {
            FillCompatibleITypedSettersFromRecord(sink, setters, metaData, record, null);
        }

        internal static void FillCompatibleITypedSettersFromRecord(SmiEventSink_Default sink, ITypedSettersV3 setters, SmiMetaData[] metaData, SqlDataRecord record, SmiDefaultFieldsProperty useDefaultValues)
        {
            for (int i = 0; i < metaData.Length; i++)
            {
                if ((useDefaultValues != null) && useDefaultValues[i])
                {
                    continue;
                }
                if (record.IsDBNull(i))
                {
                    SetDBNull_Unchecked(sink, setters, i);
                    continue;
                }
                switch (metaData[i].SqlDbType)
                {
                    case SqlDbType.BigInt:
                    {
                        SetInt64_Unchecked(sink, setters, i, record.GetInt64(i));
                        continue;
                    }
                    case SqlDbType.Binary:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Bit:
                    {
                        SetBoolean_Unchecked(sink, setters, i, record.GetBoolean(i));
                        continue;
                    }
                    case SqlDbType.Char:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.DateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], record.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Decimal:
                    {
                        SetSqlDecimal_Unchecked(sink, setters, i, record.GetSqlDecimal(i));
                        continue;
                    }
                    case SqlDbType.Float:
                    {
                        SetDouble_Unchecked(sink, setters, i, record.GetDouble(i));
                        continue;
                    }
                    case SqlDbType.Image:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Int:
                    {
                        SetInt32_Unchecked(sink, setters, i, record.GetInt32(i));
                        continue;
                    }
                    case SqlDbType.Money:
                    {
                        SetSqlMoney_Unchecked(sink, setters, i, metaData[i], record.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Real:
                    {
                        SetSingle_Unchecked(sink, setters, i, record.GetFloat(i));
                        continue;
                    }
                    case SqlDbType.UniqueIdentifier:
                    {
                        SetGuid_Unchecked(sink, setters, i, record.GetGuid(i));
                        continue;
                    }
                    case SqlDbType.SmallDateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], record.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.SmallInt:
                    {
                        SetInt16_Unchecked(sink, setters, i, record.GetInt16(i));
                        continue;
                    }
                    case SqlDbType.SmallMoney:
                    {
                        SetSqlMoney_Checked(sink, setters, i, metaData[i], record.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.Text:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Timestamp:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.TinyInt:
                    {
                        SetByte_Unchecked(sink, setters, i, record.GetByte(i));
                        continue;
                    }
                    case SqlDbType.VarBinary:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.VarChar:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Variant:
                    {
                        object sqlValue = record.GetSqlValue(i);
                        ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCode(sqlValue);
                        SetCompatibleValue(sink, setters, i, metaData[i], sqlValue, typeCode, 0);
                        continue;
                    }
                    case SqlDbType.Xml:
                    {
                        SetSqlXml_Unchecked(sink, setters, i, record.GetSqlXml(i));
                        continue;
                    }
                    case SqlDbType.Udt:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                }
                throw ADP.NotSupported();
            }
        }

        internal static void FillCompatibleSettersFromReader(SmiEventSink_Default sink, SmiTypedGetterSetter setters, IList<SmiExtendedMetaData> metaData, DbDataReader reader)
        {
            for (int i = 0; i < metaData.Count; i++)
            {
                object sqlValue;
                DateTimeOffset dateTimeOffset;
                TimeSpan timeSpan;
                ExtendedClrTypeCode code;
                if (reader.IsDBNull(i))
                {
                    SetDBNull_Unchecked(sink, setters, i);
                    continue;
                }
                switch (metaData[i].SqlDbType)
                {
                    case SqlDbType.BigInt:
                    {
                        SetInt64_Unchecked(sink, setters, i, reader.GetInt64(i));
                        continue;
                    }
                    case SqlDbType.Binary:
                    {
                        SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Bit:
                    {
                        SetBoolean_Unchecked(sink, setters, i, reader.GetBoolean(i));
                        continue;
                    }
                    case SqlDbType.Char:
                    {
                        SetCharsOrString_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.DateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], reader.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Decimal:
                    {
                        SqlDataReader reader6 = reader as SqlDataReader;
                        if (reader6 == null)
                        {
                            break;
                        }
                        SetSqlDecimal_Unchecked(sink, setters, i, reader6.GetSqlDecimal(i));
                        continue;
                    }
                    case SqlDbType.Float:
                    {
                        SetDouble_Unchecked(sink, setters, i, reader.GetDouble(i));
                        continue;
                    }
                    case SqlDbType.Image:
                    {
                        SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Int:
                    {
                        SetInt32_Unchecked(sink, setters, i, reader.GetInt32(i));
                        continue;
                    }
                    case SqlDbType.Money:
                    {
                        SetSqlMoney_Checked(sink, setters, i, metaData[i], new SqlMoney(reader.GetDecimal(i)));
                        continue;
                    }
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    {
                        SetCharsOrString_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Real:
                    {
                        SetSingle_Unchecked(sink, setters, i, reader.GetFloat(i));
                        continue;
                    }
                    case SqlDbType.UniqueIdentifier:
                    {
                        SetGuid_Unchecked(sink, setters, i, reader.GetGuid(i));
                        continue;
                    }
                    case SqlDbType.SmallDateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], reader.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.SmallInt:
                    {
                        SetInt16_Unchecked(sink, setters, i, reader.GetInt16(i));
                        continue;
                    }
                    case SqlDbType.SmallMoney:
                    {
                        SetSqlMoney_Checked(sink, setters, i, metaData[i], new SqlMoney(reader.GetDecimal(i)));
                        continue;
                    }
                    case SqlDbType.Text:
                    {
                        SetCharsOrString_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Timestamp:
                    {
                        SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.TinyInt:
                    {
                        SetByte_Unchecked(sink, setters, i, reader.GetByte(i));
                        continue;
                    }
                    case SqlDbType.VarBinary:
                    {
                        SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.VarChar:
                    {
                        SetCharsOrString_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Variant:
                    {
                        SqlDataReader reader4 = reader as SqlDataReader;
                        if (reader4 == null)
                        {
                            goto Label_0311;
                        }
                        sqlValue = reader4.GetSqlValue(i);
                        goto Label_0319;
                    }
                    case SqlDbType.Xml:
                    {
                        SqlDataReader reader5 = reader as SqlDataReader;
                        if (reader5 == null)
                        {
                            goto Label_02E4;
                        }
                        SetSqlXml_Unchecked(sink, setters, i, reader5.GetSqlXml(i));
                        continue;
                    }
                    case SqlDbType.Udt:
                    {
                        SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                        continue;
                    }
                    case SqlDbType.Date:
                    case SqlDbType.DateTime2:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], reader.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Time:
                    {
                        SqlDataReader reader3 = reader as SqlDataReader;
                        if (reader3 == null)
                        {
                            goto Label_03A0;
                        }
                        timeSpan = reader3.GetTimeSpan(i);
                        goto Label_03AE;
                    }
                    case SqlDbType.DateTimeOffset:
                    {
                        SqlDataReader reader2 = reader as SqlDataReader;
                        if (reader2 == null)
                        {
                            goto Label_03D5;
                        }
                        dateTimeOffset = reader2.GetDateTimeOffset(i);
                        goto Label_03E2;
                    }
                    default:
                        throw ADP.NotSupported();
                }
                SetSqlDecimal_Unchecked(sink, setters, i, new SqlDecimal(reader.GetDecimal(i)));
                continue;
            Label_02E4:
                SetBytes_FromReader(sink, setters, i, metaData[i], reader, 0);
                continue;
            Label_0311:
                sqlValue = reader.GetValue(i);
            Label_0319:
                code = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(metaData[i].SqlDbType, metaData[i].IsMultiValued, sqlValue, null, 210L);
                SetCompatibleValueV200(sink, setters, i, metaData[i], sqlValue, code, 0, 0, null);
                continue;
            Label_03A0:
                timeSpan = (TimeSpan) reader.GetValue(i);
            Label_03AE:
                SetTimeSpan_Checked(sink, setters, i, metaData[i], timeSpan);
                continue;
            Label_03D5:
                dateTimeOffset = (DateTimeOffset) reader.GetValue(i);
            Label_03E2:
                SetDateTimeOffset_Unchecked(sink, setters, i, dateTimeOffset);
            }
        }

        internal static void FillCompatibleSettersFromRecord(SmiEventSink_Default sink, SmiTypedGetterSetter setters, SmiMetaData[] metaData, SqlDataRecord record, SmiDefaultFieldsProperty useDefaultValues)
        {
            for (int i = 0; i < metaData.Length; i++)
            {
                DateTimeOffset dateTimeOffset;
                TimeSpan timeSpan;
                if ((useDefaultValues != null) && useDefaultValues[i])
                {
                    continue;
                }
                if (record.IsDBNull(i))
                {
                    SetDBNull_Unchecked(sink, setters, i);
                    continue;
                }
                switch (metaData[i].SqlDbType)
                {
                    case SqlDbType.BigInt:
                    {
                        SetInt64_Unchecked(sink, setters, i, record.GetInt64(i));
                        continue;
                    }
                    case SqlDbType.Binary:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Bit:
                    {
                        SetBoolean_Unchecked(sink, setters, i, record.GetBoolean(i));
                        continue;
                    }
                    case SqlDbType.Char:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.DateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], record.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Decimal:
                    {
                        SetSqlDecimal_Unchecked(sink, setters, i, record.GetSqlDecimal(i));
                        continue;
                    }
                    case SqlDbType.Float:
                    {
                        SetDouble_Unchecked(sink, setters, i, record.GetDouble(i));
                        continue;
                    }
                    case SqlDbType.Image:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Int:
                    {
                        SetInt32_Unchecked(sink, setters, i, record.GetInt32(i));
                        continue;
                    }
                    case SqlDbType.Money:
                    {
                        SetSqlMoney_Unchecked(sink, setters, i, metaData[i], record.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Real:
                    {
                        SetSingle_Unchecked(sink, setters, i, record.GetFloat(i));
                        continue;
                    }
                    case SqlDbType.UniqueIdentifier:
                    {
                        SetGuid_Unchecked(sink, setters, i, record.GetGuid(i));
                        continue;
                    }
                    case SqlDbType.SmallDateTime:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], record.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.SmallInt:
                    {
                        SetInt16_Unchecked(sink, setters, i, record.GetInt16(i));
                        continue;
                    }
                    case SqlDbType.SmallMoney:
                    {
                        SetSqlMoney_Checked(sink, setters, i, metaData[i], record.GetSqlMoney(i));
                        continue;
                    }
                    case SqlDbType.Text:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Timestamp:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.TinyInt:
                    {
                        SetByte_Unchecked(sink, setters, i, record.GetByte(i));
                        continue;
                    }
                    case SqlDbType.VarBinary:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.VarChar:
                    {
                        SetChars_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Variant:
                    {
                        object sqlValue = record.GetSqlValue(i);
                        ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCode(sqlValue);
                        SetCompatibleValueV200(sink, setters, i, metaData[i], sqlValue, typeCode, 0, -1, null);
                        continue;
                    }
                    case SqlDbType.Xml:
                    {
                        SetSqlXml_Unchecked(sink, setters, i, record.GetSqlXml(i));
                        continue;
                    }
                    case SqlDbType.Udt:
                    {
                        SetBytes_FromRecord(sink, setters, i, metaData[i], record, 0);
                        continue;
                    }
                    case SqlDbType.Date:
                    case SqlDbType.DateTime2:
                    {
                        SetDateTime_Checked(sink, setters, i, metaData[i], record.GetDateTime(i));
                        continue;
                    }
                    case SqlDbType.Time:
                    {
                        SqlDataRecord record3 = record;
                        if (record3 == null)
                        {
                            break;
                        }
                        timeSpan = record3.GetTimeSpan(i);
                        goto Label_02EF;
                    }
                    case SqlDbType.DateTimeOffset:
                    {
                        SqlDataRecord record2 = record;
                        if (record2 == null)
                        {
                            goto Label_030C;
                        }
                        dateTimeOffset = record2.GetDateTimeOffset(i);
                        goto Label_0319;
                    }
                    default:
                        throw ADP.NotSupported();
                }
                timeSpan = (TimeSpan) record.GetValue(i);
            Label_02EF:
                SetTimeSpan_Checked(sink, setters, i, metaData[i], timeSpan);
                continue;
            Label_030C:
                dateTimeOffset = (DateTimeOffset) record.GetValue(i);
            Label_0319:
                SetDateTimeOffset_Unchecked(sink, setters, i, dateTimeOffset);
            }
        }

        internal static bool GetBoolean(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Boolean))
            {
                return GetBoolean_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (bool) obj2;
        }

        private static bool GetBoolean_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            bool boolean = getters.GetBoolean(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return boolean;
        }

        internal static byte GetByte(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Byte))
            {
                return GetByte_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (byte) obj2;
        }

        private static byte GetByte_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            byte @byte = getters.GetByte(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return @byte;
        }

        private static byte[] GetByteArray_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long bytesLength = getters.GetBytesLength(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            int length = (int) bytesLength;
            byte[] buffer = new byte[length];
            getters.GetBytes(sink, ordinal, 0L, buffer, 0, length);
            sink.ProcessMessagesAndThrow();
            return buffer;
        }

        internal static long GetBytes(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiExtendedMetaData metaData, long fieldOffset, byte[] buffer, int bufferOffset, int length, bool throwOnNull)
        {
            if (((-1L != metaData.MaxLength) && (((SqlDbType.VarChar == metaData.SqlDbType) || (SqlDbType.NVarChar == metaData.SqlDbType)) || ((SqlDbType.Char == metaData.SqlDbType) || (SqlDbType.NChar == metaData.SqlDbType)))) || (SqlDbType.Xml == metaData.SqlDbType))
            {
                throw SQL.NonBlobColumn(metaData.Name);
            }
            return GetBytesInternal(sink, getters, ordinal, metaData, fieldOffset, buffer, bufferOffset, length, throwOnNull);
        }

        private static int GetBytes_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            int num = getters.GetBytes(sink, ordinal, fieldOffset, buffer, bufferOffset, length);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        private static long GetBytesConversion(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, long fieldOffset, byte[] buffer, int bufferOffset, int length, bool throwOnNull)
        {
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            SqlBinary binary = (SqlBinary) obj2;
            if (binary.IsNull)
            {
                if (throwOnNull)
                {
                    throw SQL.SqlNullValue();
                }
                return 0L;
            }
            if (buffer == null)
            {
                return (long) binary.Length;
            }
            length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength * 2L, (long) binary.Length, fieldOffset, buffer.Length, bufferOffset, length);
            Array.Copy(binary.Value, (int) fieldOffset, buffer, bufferOffset, length);
            return (long) length;
        }

        internal static long GetBytesInternal(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, long fieldOffset, byte[] buffer, int bufferOffset, int length, bool throwOnNull)
        {
            if (!CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.ByteArray))
            {
                return GetBytesConversion(sink, getters, ordinal, metaData, fieldOffset, buffer, bufferOffset, length, throwOnNull);
            }
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                if (throwOnNull)
                {
                    throw SQL.SqlNullValue();
                }
                CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, 0L, fieldOffset, buffer.Length, bufferOffset, length);
                return 0L;
            }
            long actualLength = GetBytesLength_Unchecked(sink, getters, ordinal);
            if (buffer == null)
            {
                return actualLength;
            }
            if (MetaDataUtilsSmi.IsCharOrXmlType(metaData.SqlDbType))
            {
                length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength * 2L, actualLength, fieldOffset, buffer.Length, bufferOffset, length);
            }
            else
            {
                length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, actualLength, fieldOffset, buffer.Length, bufferOffset, length);
            }
            if (length > 0)
            {
                length = GetBytes_Unchecked(sink, getters, ordinal, fieldOffset, buffer, bufferOffset, length);
            }
            return (long) length;
        }

        private static long GetBytesLength_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long bytesLength = getters.GetBytesLength(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return bytesLength;
        }

        private static char[] GetCharArray_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long charsLength = getters.GetCharsLength(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            int length = (int) charsLength;
            char[] buffer = new char[length];
            getters.GetChars(sink, ordinal, 0L, buffer, 0, length);
            sink.ProcessMessagesAndThrow();
            return buffer;
        }

        internal static long GetChars(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.CharArray))
            {
                long actualLength = GetCharsLength_Unchecked(sink, getters, ordinal);
                if (buffer == null)
                {
                    return actualLength;
                }
                length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, actualLength, fieldOffset, buffer.Length, bufferOffset, length);
                if (length > 0)
                {
                    length = GetChars_Unchecked(sink, getters, ordinal, fieldOffset, buffer, bufferOffset, length);
                }
                return (long) length;
            }
            string str = (string) GetValue(sink, getters, ordinal, metaData, null);
            if (str == null)
            {
                throw ADP.InvalidCast();
            }
            if (buffer == null)
            {
                return (long) str.Length;
            }
            length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength * 2L, (long) str.Length, fieldOffset, buffer.Length, bufferOffset, length);
            str.CopyTo((int) fieldOffset, buffer, bufferOffset, length);
            return (long) length;
        }

        private static int GetChars_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            int num = getters.GetChars(sink, ordinal, fieldOffset, buffer, bufferOffset, length);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        private static long GetCharsLength_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long charsLength = getters.GetCharsLength(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return charsLength;
        }

        internal static DateTime GetDateTime(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.DateTime))
            {
                return GetDateTime_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (DateTime) obj2;
        }

        private static DateTime GetDateTime_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            DateTime dateTime = getters.GetDateTime(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return dateTime;
        }

        internal static DateTimeOffset GetDateTimeOffset(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.DateTimeOffset))
            {
                return GetDateTimeOffset_Unchecked(sink, getters, ordinal);
            }
            return (DateTimeOffset) GetValue200(sink, getters, ordinal, metaData, null);
        }

        internal static DateTimeOffset GetDateTimeOffset(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, bool gettersSupportKatmaiDateTime)
        {
            if (gettersSupportKatmaiDateTime)
            {
                return GetDateTimeOffset(sink, (SmiTypedGetterSetter) getters, ordinal, metaData);
            }
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (DateTimeOffset) obj2;
        }

        private static DateTimeOffset GetDateTimeOffset_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal)
        {
            DateTimeOffset dateTimeOffset = getters.GetDateTimeOffset(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return dateTimeOffset;
        }

        internal static decimal GetDecimal(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Decimal))
            {
                return GetDecimal_PossiblyMoney(sink, getters, ordinal, metaData);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (decimal) obj2;
        }

        private static decimal GetDecimal_PossiblyMoney(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (SqlDbType.Decimal == metaData.SqlDbType)
            {
                return GetSqlDecimal_Unchecked(sink, getters, ordinal).Value;
            }
            return GetSqlMoney_Unchecked(sink, getters, ordinal).Value;
        }

        internal static double GetDouble(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Double))
            {
                return GetDouble_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (double) obj2;
        }

        private static double GetDouble_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            double num = getters.GetDouble(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        internal static Guid GetGuid(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Guid))
            {
                return GetGuid_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (Guid) obj2;
        }

        private static Guid GetGuid_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            Guid guid = getters.GetGuid(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return guid;
        }

        internal static short GetInt16(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Int16))
            {
                return GetInt16_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (short) obj2;
        }

        private static short GetInt16_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            short num = getters.GetInt16(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        internal static int GetInt32(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Int32))
            {
                return GetInt32_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (int) obj2;
        }

        private static int GetInt32_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            int num = getters.GetInt32(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        internal static long GetInt64(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Int64))
            {
                return GetInt64_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (long) obj2;
        }

        private static long GetInt64_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long num = getters.GetInt64(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        private static void GetNullOutputParameterSmi(SmiMetaData metaData, SqlBuffer targetBuffer, ref object result)
        {
            if (SqlDbType.Udt == metaData.SqlDbType)
            {
                result = NullUdtInstance(metaData);
            }
            else
            {
                SqlBuffer.StorageType storageType = __dbTypeToStorageType[(int) metaData.SqlDbType];
                if (storageType == SqlBuffer.StorageType.Empty)
                {
                    result = DBNull.Value;
                }
                else if (SqlBuffer.StorageType.SqlBinary == storageType)
                {
                    targetBuffer.SqlBinary = SqlBinary.Null;
                }
                else if (SqlBuffer.StorageType.SqlGuid == storageType)
                {
                    targetBuffer.SqlGuid = SqlGuid.Null;
                }
                else
                {
                    targetBuffer.SetToNullOfType(storageType);
                }
            }
        }

        internal static object GetOutputParameterV200Smi(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal, SmiMetaData metaData, SmiContext context, SqlBuffer targetBuffer)
        {
            object result = null;
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                GetNullOutputParameterSmi(metaData, targetBuffer, ref result);
                return result;
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.Date:
                    targetBuffer.SetToDate(GetDateTime_Unchecked(sink, getters, ordinal));
                    return result;

                case SqlDbType.Time:
                    targetBuffer.SetToTime(GetTimeSpan_Unchecked(sink, getters, ordinal), metaData.Scale);
                    return result;

                case SqlDbType.DateTime2:
                    targetBuffer.SetToDateTime2(GetDateTime_Unchecked(sink, getters, ordinal), metaData.Scale);
                    return result;

                case SqlDbType.DateTimeOffset:
                    targetBuffer.SetToDateTimeOffset(GetDateTimeOffset_Unchecked(sink, getters, ordinal), metaData.Scale);
                    return result;

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    GetOutputParameterV200Smi(sink, getters, ordinal, metaData, context, targetBuffer);
                    return result;
            }
            return GetOutputParameterV3Smi(sink, getters, ordinal, metaData, context, targetBuffer);
        }

        internal static object GetOutputParameterV3Smi(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context, SqlBuffer targetBuffer)
        {
            object result = null;
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                GetNullOutputParameterSmi(metaData, targetBuffer, ref result);
                return result;
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.BigInt:
                    targetBuffer.Int64 = GetInt64_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    targetBuffer.SqlBinary = GetSqlBinary_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Bit:
                    targetBuffer.Boolean = GetBoolean_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    targetBuffer.SetToString(GetString_Unchecked(sink, getters, ordinal));
                    return result;

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                {
                    SqlDateTime time = new SqlDateTime(GetDateTime_Unchecked(sink, getters, ordinal));
                    targetBuffer.SetToDateTime(time.DayTicks, time.TimeTicks);
                    return result;
                }
                case SqlDbType.Decimal:
                {
                    SqlDecimal num = GetSqlDecimal_Unchecked(sink, getters, ordinal);
                    targetBuffer.SetToDecimal(num.Precision, num.Scale, num.IsPositive, num.Data);
                    return result;
                }
                case SqlDbType.Float:
                    targetBuffer.Double = GetDouble_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Int:
                    targetBuffer.Int32 = GetInt32_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    targetBuffer.SetToMoney(GetInt64_Unchecked(sink, getters, ordinal));
                    return result;

                case SqlDbType.Real:
                    targetBuffer.Single = GetSingle_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.UniqueIdentifier:
                    targetBuffer.SqlGuid = new SqlGuid(GetGuid_Unchecked(sink, getters, ordinal));
                    return result;

                case SqlDbType.SmallInt:
                    targetBuffer.Int16 = GetInt16_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.TinyInt:
                    targetBuffer.Byte = GetByte_Unchecked(sink, getters, ordinal);
                    return result;

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    GetOutputParameterV3Smi(sink, getters, ordinal, metaData, context, targetBuffer);
                    return result;

                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                    return result;

                case SqlDbType.Xml:
                    targetBuffer.SqlXml = GetSqlXml_Unchecked(sink, getters, ordinal, null);
                    return result;

                case SqlDbType.Udt:
                    return GetUdt_LengthChecked(sink, getters, ordinal, metaData);
            }
            return result;
        }

        internal static float GetSingle(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.Single))
            {
                return GetSingle_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (float) obj2;
        }

        private static float GetSingle_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            float single = getters.GetSingle(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return single;
        }

        internal static SqlBinary GetSqlBinary(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlBinary))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlBinary.Null;
                }
                return GetSqlBinary_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlBinary) obj2;
        }

        private static SqlBinary GetSqlBinary_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            return new SqlBinary(GetByteArray_Unchecked(sink, getters, ordinal));
        }

        internal static SqlBoolean GetSqlBoolean(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlBoolean))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlBoolean.Null;
                }
                return new SqlBoolean(GetBoolean_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlBoolean) obj2;
        }

        internal static SqlByte GetSqlByte(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlByte))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlByte.Null;
                }
                return new SqlByte(GetByte_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlByte) obj2;
        }

        internal static SqlBytes GetSqlBytes(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlBytes))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlBytes.Null;
                }
                long num = GetBytesLength_Unchecked(sink, getters, ordinal);
                if ((0L <= num) && (num < 0x1f40L))
                {
                    return new SqlBytes(GetByteArray_Unchecked(sink, getters, ordinal));
                }
                Stream source = new SmiGettersStream(sink, getters, ordinal, metaData);
                return new SqlBytes(CopyIntoNewSmiScratchStream(source, sink, context));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            SqlBinary binary = (SqlBinary) obj2;
            if (binary.IsNull)
            {
                return SqlBytes.Null;
            }
            return new SqlBytes(binary.Value);
        }

        internal static SqlChars GetSqlChars(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlChars))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlChars.Null;
                }
                if ((GetCharsLength_Unchecked(sink, getters, ordinal) < 0xfa0L) || !InOutOfProcHelper.InProc)
                {
                    return new SqlChars(GetCharArray_Unchecked(sink, getters, ordinal));
                }
                Stream source = new SmiGettersStream(sink, getters, ordinal, metaData);
                return new SqlChars(CopyIntoNewSmiScratchStreamChars(source, sink, context));
            }
            if (SqlDbType.Xml == metaData.SqlDbType)
            {
                SqlXml xml = GetSqlXml_Unchecked(sink, getters, ordinal, null);
                if (xml.IsNull)
                {
                    return SqlChars.Null;
                }
                return new SqlChars(xml.Value.ToCharArray());
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            SqlString str = (SqlString) obj2;
            if (str.IsNull)
            {
                return SqlChars.Null;
            }
            return new SqlChars(str.Value.ToCharArray());
        }

        internal static SqlDateTime GetSqlDateTime(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlDateTime))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlDateTime.Null;
                }
                return new SqlDateTime(GetDateTime_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlDateTime) obj2;
        }

        internal static SqlDecimal GetSqlDecimal(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlDecimal))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlDecimal.Null;
                }
                return GetSqlDecimal_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlDecimal) obj2;
        }

        private static SqlDecimal GetSqlDecimal_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            SqlDecimal sqlDecimal = getters.GetSqlDecimal(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return sqlDecimal;
        }

        internal static SqlDouble GetSqlDouble(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlDouble))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlDouble.Null;
                }
                return new SqlDouble(GetDouble_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlDouble) obj2;
        }

        internal static SqlGuid GetSqlGuid(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlGuid))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlGuid.Null;
                }
                return new SqlGuid(GetGuid_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlGuid) obj2;
        }

        internal static SqlInt16 GetSqlInt16(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlInt16))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlInt16.Null;
                }
                return new SqlInt16(GetInt16_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlInt16) obj2;
        }

        internal static SqlInt32 GetSqlInt32(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlInt32))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlInt32.Null;
                }
                return new SqlInt32(GetInt32_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlInt32) obj2;
        }

        internal static SqlInt64 GetSqlInt64(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlInt64))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlInt64.Null;
                }
                return new SqlInt64(GetInt64_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlInt64) obj2;
        }

        internal static SqlMoney GetSqlMoney(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlMoney))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlMoney.Null;
                }
                return GetSqlMoney_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlMoney) obj2;
        }

        private static SqlMoney GetSqlMoney_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            long num = getters.GetInt64(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return new SqlMoney(num, 1);
        }

        internal static SqlSingle GetSqlSingle(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlSingle))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlSingle.Null;
                }
                return new SqlSingle(GetSingle_Unchecked(sink, getters, ordinal));
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlSingle) obj2;
        }

        internal static SqlString GetSqlString(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlString))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlString.Null;
                }
                return new SqlString(GetString_Unchecked(sink, getters, ordinal));
            }
            if (SqlDbType.Xml == metaData.SqlDbType)
            {
                SqlXml xml = GetSqlXml_Unchecked(sink, getters, ordinal, null);
                if (xml.IsNull)
                {
                    return SqlString.Null;
                }
                return new SqlString(xml.Value);
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlString) obj2;
        }

        internal static object GetSqlValue(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            object obj2 = null;
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                if (SqlDbType.Udt == metaData.SqlDbType)
                {
                    return NullUdtInstance(metaData);
                }
                return __typeSpecificNullForSqlValue[(int) metaData.SqlDbType];
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.BigInt:
                    return new SqlInt64(GetInt64_Unchecked(sink, getters, ordinal));

                case SqlDbType.Binary:
                    return GetSqlBinary_Unchecked(sink, getters, ordinal);

                case SqlDbType.Bit:
                    return new SqlBoolean(GetBoolean_Unchecked(sink, getters, ordinal));

                case SqlDbType.Char:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.DateTime:
                    return new SqlDateTime(GetDateTime_Unchecked(sink, getters, ordinal));

                case SqlDbType.Decimal:
                    return GetSqlDecimal_Unchecked(sink, getters, ordinal);

                case SqlDbType.Float:
                    return new SqlDouble(GetDouble_Unchecked(sink, getters, ordinal));

                case SqlDbType.Image:
                    return GetSqlBinary_Unchecked(sink, getters, ordinal);

                case SqlDbType.Int:
                    return new SqlInt32(GetInt32_Unchecked(sink, getters, ordinal));

                case SqlDbType.Money:
                    return GetSqlMoney_Unchecked(sink, getters, ordinal);

                case SqlDbType.NChar:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.NText:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.NVarChar:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.Real:
                    return new SqlSingle(GetSingle_Unchecked(sink, getters, ordinal));

                case SqlDbType.UniqueIdentifier:
                    return new SqlGuid(GetGuid_Unchecked(sink, getters, ordinal));

                case SqlDbType.SmallDateTime:
                    return new SqlDateTime(GetDateTime_Unchecked(sink, getters, ordinal));

                case SqlDbType.SmallInt:
                    return new SqlInt16(GetInt16_Unchecked(sink, getters, ordinal));

                case SqlDbType.SmallMoney:
                    return GetSqlMoney_Unchecked(sink, getters, ordinal);

                case SqlDbType.Text:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.Timestamp:
                    return GetSqlBinary_Unchecked(sink, getters, ordinal);

                case SqlDbType.TinyInt:
                    return new SqlByte(GetByte_Unchecked(sink, getters, ordinal));

                case SqlDbType.VarBinary:
                    return GetSqlBinary_Unchecked(sink, getters, ordinal);

                case SqlDbType.VarChar:
                    return new SqlString(GetString_Unchecked(sink, getters, ordinal));

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    return GetSqlValue(sink, getters, ordinal, metaData, context);

                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                    return obj2;

                case SqlDbType.Xml:
                    return GetSqlXml_Unchecked(sink, getters, ordinal, context);

                case SqlDbType.Udt:
                    return GetUdt_LengthChecked(sink, getters, ordinal, metaData);
            }
            return obj2;
        }

        internal static object GetSqlValue200(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                if (SqlDbType.Udt == metaData.SqlDbType)
                {
                    return NullUdtInstance(metaData);
                }
                return __typeSpecificNullForSqlValue[(int) metaData.SqlDbType];
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.Date:
                case SqlDbType.DateTime2:
                    return GetDateTime_Unchecked(sink, getters, ordinal);

                case SqlDbType.Time:
                    return GetTimeSpan_Unchecked(sink, getters, ordinal);

                case SqlDbType.DateTimeOffset:
                    return GetDateTimeOffset_Unchecked(sink, getters, ordinal);

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    return GetSqlValue200(sink, getters, ordinal, metaData, context);
            }
            return GetSqlValue(sink, getters, ordinal, metaData, context);
        }

        internal static SqlXml GetSqlXml(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.SqlXml))
            {
                if (IsDBNull_Unchecked(sink, getters, ordinal))
                {
                    return SqlXml.Null;
                }
                return GetSqlXml_Unchecked(sink, getters, ordinal, context);
            }
            object obj2 = GetSqlValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (SqlXml) obj2;
        }

        private static SqlXml GetSqlXml_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiContext context)
        {
            if ((context == null) && InOutOfProcHelper.InProc)
            {
                context = SmiContextFactory.Instance.GetCurrentContext();
            }
            Stream source = new SmiGettersStream(sink, getters, ordinal, SmiMetaData.DefaultXml);
            return new SqlXml(CopyIntoNewSmiScratchStream(source, sink, context));
        }

        internal static string GetString(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.String))
            {
                return GetString_Unchecked(sink, getters, ordinal);
            }
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (string) obj2;
        }

        private static string GetString_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            string str = getters.GetString(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return str;
        }

        internal static TimeSpan GetTimeSpan(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal, SmiMetaData metaData)
        {
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            if (CanAccessGetterDirectly(metaData, ExtendedClrTypeCode.TimeSpan))
            {
                return GetTimeSpan_Unchecked(sink, getters, ordinal);
            }
            return (TimeSpan) GetValue200(sink, getters, ordinal, metaData, null);
        }

        internal static TimeSpan GetTimeSpan(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, bool gettersSupportKatmaiDateTime)
        {
            if (gettersSupportKatmaiDateTime)
            {
                return GetTimeSpan(sink, (SmiTypedGetterSetter) getters, ordinal, metaData);
            }
            ThrowIfITypedGettersIsNull(sink, getters, ordinal);
            object obj2 = GetValue(sink, getters, ordinal, metaData, null);
            if (obj2 == null)
            {
                throw ADP.InvalidCast();
            }
            return (TimeSpan) obj2;
        }

        private static TimeSpan GetTimeSpan_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal)
        {
            TimeSpan timeSpan = getters.GetTimeSpan(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return timeSpan;
        }

        private static object GetUdt_LengthChecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                return metaData.Type.InvokeMember("Null", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static, null, null, new object[0], CultureInfo.InvariantCulture);
            }
            Stream s = new SmiGettersStream(sink, getters, ordinal, metaData);
            return SerializationHelperSql9.Deserialize(s, metaData.Type);
        }

        internal static object GetValue(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            object obj2 = null;
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                return DBNull.Value;
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.BigInt:
                    return GetInt64_Unchecked(sink, getters, ordinal);

                case SqlDbType.Binary:
                    return GetByteArray_Unchecked(sink, getters, ordinal);

                case SqlDbType.Bit:
                    return GetBoolean_Unchecked(sink, getters, ordinal);

                case SqlDbType.Char:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.DateTime:
                    return GetDateTime_Unchecked(sink, getters, ordinal);

                case SqlDbType.Decimal:
                    return GetSqlDecimal_Unchecked(sink, getters, ordinal).Value;

                case SqlDbType.Float:
                    return GetDouble_Unchecked(sink, getters, ordinal);

                case SqlDbType.Image:
                    return GetByteArray_Unchecked(sink, getters, ordinal);

                case SqlDbType.Int:
                    return GetInt32_Unchecked(sink, getters, ordinal);

                case SqlDbType.Money:
                    return GetSqlMoney_Unchecked(sink, getters, ordinal).Value;

                case SqlDbType.NChar:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.NText:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.NVarChar:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.Real:
                    return GetSingle_Unchecked(sink, getters, ordinal);

                case SqlDbType.UniqueIdentifier:
                    return GetGuid_Unchecked(sink, getters, ordinal);

                case SqlDbType.SmallDateTime:
                    return GetDateTime_Unchecked(sink, getters, ordinal);

                case SqlDbType.SmallInt:
                    return GetInt16_Unchecked(sink, getters, ordinal);

                case SqlDbType.SmallMoney:
                    return GetSqlMoney_Unchecked(sink, getters, ordinal).Value;

                case SqlDbType.Text:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.Timestamp:
                    return GetByteArray_Unchecked(sink, getters, ordinal);

                case SqlDbType.TinyInt:
                    return GetByte_Unchecked(sink, getters, ordinal);

                case SqlDbType.VarBinary:
                    return GetByteArray_Unchecked(sink, getters, ordinal);

                case SqlDbType.VarChar:
                    return GetString_Unchecked(sink, getters, ordinal);

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    return GetValue(sink, getters, ordinal, metaData, context);

                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                    return obj2;

                case SqlDbType.Xml:
                    return GetSqlXml_Unchecked(sink, getters, ordinal, context).Value;

                case SqlDbType.Udt:
                    return GetUdt_LengthChecked(sink, getters, ordinal, metaData);
            }
            return obj2;
        }

        internal static object GetValue200(SmiEventSink_Default sink, SmiTypedGetterSetter getters, int ordinal, SmiMetaData metaData, SmiContext context)
        {
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                return DBNull.Value;
            }
            switch (metaData.SqlDbType)
            {
                case SqlDbType.Date:
                case SqlDbType.DateTime2:
                    return GetDateTime_Unchecked(sink, getters, ordinal);

                case SqlDbType.Time:
                    return GetTimeSpan_Unchecked(sink, getters, ordinal);

                case SqlDbType.DateTimeOffset:
                    return GetDateTimeOffset_Unchecked(sink, getters, ordinal);

                case SqlDbType.Variant:
                    metaData = getters.GetVariantType(sink, ordinal);
                    sink.ProcessMessagesAndThrow();
                    return GetValue200(sink, getters, ordinal, metaData, context);
            }
            return GetValue(sink, getters, ordinal, metaData, context);
        }

        internal static bool IsDBNull(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            return IsDBNull_Unchecked(sink, getters, ordinal);
        }

        private static bool IsDBNull_Unchecked(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            bool flag = getters.IsDBNull(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            return flag;
        }

        internal static object NullUdtInstance(SmiMetaData metaData)
        {
            return metaData.Type.InvokeMember("Null", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static, null, null, new object[0], CultureInfo.InvariantCulture);
        }

        private static long PositiveMin(long first, long second)
        {
            if (first < 0L)
            {
                return second;
            }
            if (second < 0L)
            {
                return first;
            }
            return Math.Min(first, second);
        }

        internal static void SetBoolean(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, bool value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Boolean);
            SetBoolean_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetBoolean_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, bool value)
        {
            setters.SetBoolean(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetByte(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, byte value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Byte);
            SetByte_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetByte_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, byte value)
        {
            setters.SetByte(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetByteArray_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, byte[] buffer, int offset)
        {
            int length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, buffer.Length, offset, buffer.Length - offset);
            SetByteArray_Unchecked(sink, setters, ordinal, buffer, offset, length);
        }

        private static void SetByteArray_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, byte[] buffer, int bufferOffset, int length)
        {
            if (length > 0)
            {
                setters.SetBytes(sink, ordinal, 0L, buffer, bufferOffset, length);
                sink.ProcessMessagesAndThrow();
            }
            setters.SetBytesLength(sink, ordinal, (long) length);
            sink.ProcessMessagesAndThrow();
        }

        internal static long SetBytes(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.ByteArray);
            if (buffer == null)
            {
                throw ADP.ArgumentNull("buffer");
            }
            length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, fieldOffset, buffer.Length, bufferOffset, length);
            if (length == 0)
            {
                fieldOffset = 0L;
                bufferOffset = 0;
            }
            return (long) SetBytes_Unchecked(sink, setters, ordinal, fieldOffset, buffer, bufferOffset, length);
        }

        private static void SetBytes_FromReader(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DbDataReader reader, int offset)
        {
            long num6;
            int num4 = 0;
            num4 = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, -1, offset, -1);
            int length = 0x1f40;
            byte[] buffer = new byte[length];
            long num2 = 1L;
            long dataOffset = offset;
            for (long i = 0L; ((num4 < 0) || (i < num4)) && ((0L != (num6 = reader.GetBytes(ordinal, dataOffset, buffer, 0, length))) && (0L != num2)); i += num2)
            {
                num2 = setters.SetBytes(sink, ordinal, dataOffset, buffer, 0, (int) num6);
                sink.ProcessMessagesAndThrow();
                dataOffset += num2;
            }
            setters.SetBytesLength(sink, ordinal, dataOffset);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetBytes_FromRecord(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDataRecord record, int offset)
        {
            int num6;
            long num7;
            int num = 0;
            long num4 = record.GetBytes(ordinal, 0L, null, 0, 0);
            if (num4 > 0x7fffffffL)
            {
                num4 = -1L;
            }
            num = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, (int) num4, offset, (int) num4);
            if ((num > 0x1f40) || (num < 0))
            {
                num6 = 0x1f40;
            }
            else
            {
                num6 = num;
            }
            byte[] buffer = new byte[num6];
            long num3 = 1L;
            long fieldOffset = offset;
            for (long i = 0L; ((num < 0) || (i < num)) && ((0L != (num7 = record.GetBytes(ordinal, fieldOffset, buffer, 0, num6))) && (0L != num3)); i += num3)
            {
                num3 = setters.SetBytes(sink, ordinal, fieldOffset, buffer, 0, (int) num7);
                sink.ProcessMessagesAndThrow();
                fieldOffset += num3;
            }
            setters.SetBytesLength(sink, ordinal, fieldOffset);
            sink.ProcessMessagesAndThrow();
        }

        private static int SetBytes_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            int num = setters.SetBytes(sink, ordinal, fieldOffset, buffer, bufferOffset, length);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        internal static long SetBytesLength(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, long length)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.ByteArray);
            if (length < 0L)
            {
                throw ADP.InvalidDataLength(length);
            }
            if ((metaData.MaxLength >= 0L) && (length > metaData.MaxLength))
            {
                length = metaData.MaxLength;
            }
            setters.SetBytesLength(sink, ordinal, length);
            sink.ProcessMessagesAndThrow();
            return length;
        }

        private static void SetCharArray_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, char[] buffer, int offset)
        {
            int length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, buffer.Length, offset, buffer.Length - offset);
            SetCharArray_Unchecked(sink, setters, ordinal, buffer, offset, length);
        }

        private static void SetCharArray_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, char[] buffer, int bufferOffset, int length)
        {
            if (length > 0)
            {
                setters.SetChars(sink, ordinal, 0L, buffer, bufferOffset, length);
                sink.ProcessMessagesAndThrow();
            }
            setters.SetCharsLength(sink, ordinal, (long) length);
            sink.ProcessMessagesAndThrow();
        }

        internal static long SetChars(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.CharArray);
            if (buffer == null)
            {
                throw ADP.ArgumentNull("buffer");
            }
            length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, fieldOffset, buffer.Length, bufferOffset, length);
            if (length == 0)
            {
                fieldOffset = 0L;
                bufferOffset = 0;
            }
            return (long) SetChars_Unchecked(sink, setters, ordinal, fieldOffset, buffer, bufferOffset, length);
        }

        private static void SetChars_FromReader(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DbDataReader reader, int offset)
        {
            int num4;
            long num6;
            int num5 = 0;
            num5 = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, -1, offset, -1);
            if (MetaDataUtilsSmi.IsAnsiType(metaData.SqlDbType))
            {
                num4 = 0x1f40;
            }
            else
            {
                num4 = 0xfa0;
            }
            char[] buffer = new char[num4];
            long num2 = 1L;
            long dataOffset = offset;
            for (long i = 0L; ((num5 < 0) || (i < num5)) && ((0L != (num6 = reader.GetChars(ordinal, dataOffset, buffer, 0, num4))) && (0L != num2)); i += num2)
            {
                num2 = setters.SetChars(sink, ordinal, dataOffset, buffer, 0, (int) num6);
                sink.ProcessMessagesAndThrow();
                dataOffset += num2;
            }
            setters.SetCharsLength(sink, ordinal, dataOffset);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetChars_FromRecord(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDataRecord record, int offset)
        {
            int num4;
            long num7;
            int num = 0;
            long num5 = record.GetChars(ordinal, 0L, null, 0, 0);
            if (num5 > 0x7fffffffL)
            {
                num5 = -1L;
            }
            num = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, (int) num5, offset, ((int) num5) - offset);
            if ((num > 0xfa0) || (num < 0))
            {
                if (MetaDataUtilsSmi.IsAnsiType(metaData.SqlDbType))
                {
                    num4 = 0x1f40;
                }
                else
                {
                    num4 = 0xfa0;
                }
            }
            else
            {
                num4 = num;
            }
            char[] buffer = new char[num4];
            long num3 = 1L;
            long fieldOffset = offset;
            for (long i = 0L; ((num < 0) || (i < num)) && ((0L != (num7 = record.GetChars(ordinal, fieldOffset, buffer, 0, num4))) && (0L != num3)); i += num3)
            {
                num3 = setters.SetChars(sink, ordinal, fieldOffset, buffer, 0, (int) num7);
                sink.ProcessMessagesAndThrow();
                fieldOffset += num3;
            }
            setters.SetCharsLength(sink, ordinal, fieldOffset);
            sink.ProcessMessagesAndThrow();
        }

        private static int SetChars_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            int num = setters.SetChars(sink, ordinal, fieldOffset, buffer, bufferOffset, length);
            sink.ProcessMessagesAndThrow();
            return num;
        }

        private static void SetCharsOrString_FromReader(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DbDataReader reader, int offset)
        {
            bool flag = false;
            try
            {
                SetChars_FromReader(sink, setters, ordinal, metaData, reader, offset);
                flag = true;
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
            }
            if (!flag)
            {
                SetString_FromReader(sink, setters, ordinal, metaData, reader, offset);
            }
        }

        internal static void SetCompatibleValue(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, object value, ExtendedClrTypeCode typeCode, int offset)
        {
            switch (typeCode)
            {
                case ExtendedClrTypeCode.Invalid:
                    throw ADP.UnknownDataType(value.GetType());

                case ExtendedClrTypeCode.Boolean:
                    SetBoolean_Unchecked(sink, setters, ordinal, (bool) value);
                    return;

                case ExtendedClrTypeCode.Byte:
                    SetByte_Unchecked(sink, setters, ordinal, (byte) value);
                    return;

                case ExtendedClrTypeCode.Char:
                {
                    char[] chArray2 = new char[] { (char) value };
                    SetCompatibleValue(sink, setters, ordinal, metaData, chArray2, ExtendedClrTypeCode.CharArray, 0);
                    return;
                }
                case ExtendedClrTypeCode.DateTime:
                    SetDateTime_Checked(sink, setters, ordinal, metaData, (DateTime) value);
                    return;

                case ExtendedClrTypeCode.DBNull:
                    SetDBNull_Unchecked(sink, setters, ordinal);
                    return;

                case ExtendedClrTypeCode.Decimal:
                    SetDecimal_PossiblyMoney(sink, setters, ordinal, metaData, (decimal) value);
                    return;

                case ExtendedClrTypeCode.Double:
                    SetDouble_Unchecked(sink, setters, ordinal, (double) value);
                    return;

                case ExtendedClrTypeCode.Empty:
                    SetDBNull_Unchecked(sink, setters, ordinal);
                    return;

                case ExtendedClrTypeCode.Int16:
                    SetInt16_Unchecked(sink, setters, ordinal, (short) value);
                    return;

                case ExtendedClrTypeCode.Int32:
                    SetInt32_Unchecked(sink, setters, ordinal, (int) value);
                    return;

                case ExtendedClrTypeCode.Int64:
                    SetInt64_Unchecked(sink, setters, ordinal, (long) value);
                    return;

                case ExtendedClrTypeCode.SByte:
                    throw ADP.InvalidCast();

                case ExtendedClrTypeCode.Single:
                    SetSingle_Unchecked(sink, setters, ordinal, (float) value);
                    return;

                case ExtendedClrTypeCode.String:
                    SetString_LengthChecked(sink, setters, ordinal, metaData, (string) value, offset);
                    return;

                case ExtendedClrTypeCode.UInt16:
                    throw ADP.InvalidCast();

                case ExtendedClrTypeCode.UInt32:
                    throw ADP.InvalidCast();

                case ExtendedClrTypeCode.UInt64:
                    throw ADP.InvalidCast();

                case ExtendedClrTypeCode.Object:
                    SetUdt_LengthChecked(sink, setters, ordinal, metaData, value);
                    return;

                case ExtendedClrTypeCode.ByteArray:
                    SetByteArray_LengthChecked(sink, setters, ordinal, metaData, (byte[]) value, offset);
                    return;

                case ExtendedClrTypeCode.CharArray:
                    SetCharArray_LengthChecked(sink, setters, ordinal, metaData, (char[]) value, offset);
                    return;

                case ExtendedClrTypeCode.Guid:
                    SetGuid_Unchecked(sink, setters, ordinal, (Guid) value);
                    return;

                case ExtendedClrTypeCode.SqlBinary:
                    SetSqlBinary_LengthChecked(sink, setters, ordinal, metaData, (SqlBinary) value, offset);
                    return;

                case ExtendedClrTypeCode.SqlBoolean:
                    SetSqlBoolean_Unchecked(sink, setters, ordinal, (SqlBoolean) value);
                    return;

                case ExtendedClrTypeCode.SqlByte:
                    SetSqlByte_Unchecked(sink, setters, ordinal, (SqlByte) value);
                    return;

                case ExtendedClrTypeCode.SqlDateTime:
                    SetSqlDateTime_Checked(sink, setters, ordinal, metaData, (SqlDateTime) value);
                    return;

                case ExtendedClrTypeCode.SqlDouble:
                    SetSqlDouble_Unchecked(sink, setters, ordinal, (SqlDouble) value);
                    return;

                case ExtendedClrTypeCode.SqlGuid:
                    SetSqlGuid_Unchecked(sink, setters, ordinal, (SqlGuid) value);
                    return;

                case ExtendedClrTypeCode.SqlInt16:
                    SetSqlInt16_Unchecked(sink, setters, ordinal, (SqlInt16) value);
                    return;

                case ExtendedClrTypeCode.SqlInt32:
                    SetSqlInt32_Unchecked(sink, setters, ordinal, (SqlInt32) value);
                    return;

                case ExtendedClrTypeCode.SqlInt64:
                    SetSqlInt64_Unchecked(sink, setters, ordinal, (SqlInt64) value);
                    return;

                case ExtendedClrTypeCode.SqlMoney:
                    SetSqlMoney_Checked(sink, setters, ordinal, metaData, (SqlMoney) value);
                    return;

                case ExtendedClrTypeCode.SqlDecimal:
                    SetSqlDecimal_Unchecked(sink, setters, ordinal, (SqlDecimal) value);
                    return;

                case ExtendedClrTypeCode.SqlSingle:
                    SetSqlSingle_Unchecked(sink, setters, ordinal, (SqlSingle) value);
                    return;

                case ExtendedClrTypeCode.SqlString:
                    SetSqlString_LengthChecked(sink, setters, ordinal, metaData, (SqlString) value, offset);
                    return;

                case ExtendedClrTypeCode.SqlChars:
                    SetSqlChars_LengthChecked(sink, setters, ordinal, metaData, (SqlChars) value, offset);
                    return;

                case ExtendedClrTypeCode.SqlBytes:
                    SetSqlBytes_LengthChecked(sink, setters, ordinal, metaData, (SqlBytes) value, offset);
                    return;

                case ExtendedClrTypeCode.SqlXml:
                    SetSqlXml_Unchecked(sink, setters, ordinal, (SqlXml) value);
                    return;
            }
        }

        internal static void SetCompatibleValueV200(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, object value, ExtendedClrTypeCode typeCode, int offset, int length, ParameterPeekAheadValue peekAhead)
        {
            switch (typeCode)
            {
                case ExtendedClrTypeCode.DataTable:
                    SetDataTable_Unchecked(sink, setters, ordinal, metaData, (DataTable) value);
                    return;

                case ExtendedClrTypeCode.DbDataReader:
                    SetDbDataReader_Unchecked(sink, setters, ordinal, metaData, (DbDataReader) value);
                    return;

                case ExtendedClrTypeCode.IEnumerableOfSqlDataRecord:
                    SetIEnumerableOfSqlDataRecord_Unchecked(sink, setters, ordinal, metaData, (IEnumerable<SqlDataRecord>) value, peekAhead);
                    return;

                case ExtendedClrTypeCode.TimeSpan:
                    SetTimeSpan_Checked(sink, setters, ordinal, metaData, (TimeSpan) value);
                    return;

                case ExtendedClrTypeCode.DateTimeOffset:
                    SetDateTimeOffset_Unchecked(sink, setters, ordinal, (DateTimeOffset) value);
                    return;
            }
            SetCompatibleValue(sink, setters, ordinal, metaData, value, typeCode, offset);
        }

        private static void SetDataTable_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DataTable value)
        {
            setters = setters.GetTypedGetterSetter(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            ExtendedClrTypeCode[] codeArray = new ExtendedClrTypeCode[metaData.FieldMetaData.Count];
            for (int i = 0; i < metaData.FieldMetaData.Count; i++)
            {
                codeArray[i] = ExtendedClrTypeCode.Invalid;
            }
            foreach (DataRow row in value.Rows)
            {
                setters.NewElement(sink);
                sink.ProcessMessagesAndThrow();
                for (int j = 0; j < metaData.FieldMetaData.Count; j++)
                {
                    SmiMetaData data = metaData.FieldMetaData[j];
                    if (row.IsNull(j))
                    {
                        SetDBNull_Unchecked(sink, setters, j);
                    }
                    else
                    {
                        object obj2 = row[j];
                        if (ExtendedClrTypeCode.Invalid == codeArray[j])
                        {
                            codeArray[j] = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(data.SqlDbType, data.IsMultiValued, obj2, data.Type, 210L);
                        }
                        SetCompatibleValueV200(sink, setters, j, data, obj2, codeArray[j], 0, -1, null);
                    }
                }
            }
            setters.EndElements(sink);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetDateTime(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, DateTime value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.DateTime);
            SetDateTime_Checked(sink, setters, ordinal, metaData, value);
        }

        private static void SetDateTime_Checked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, DateTime value)
        {
            VerifyDateTimeRange(metaData.SqlDbType, value);
            SetDateTime_Unchecked(sink, setters, ordinal, (SqlDbType.Date == metaData.SqlDbType) ? value.Date : value);
        }

        private static void SetDateTime_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, DateTime value)
        {
            setters.SetDateTime(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetDateTimeOffset(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, DateTimeOffset value, bool settersSupportKatmaiDateTime)
        {
            if (!settersSupportKatmaiDateTime)
            {
                throw ADP.InvalidCast();
            }
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.DateTimeOffset);
            SetDateTimeOffset_Unchecked(sink, (SmiTypedGetterSetter) setters, ordinal, value);
        }

        private static void SetDateTimeOffset_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, DateTimeOffset value)
        {
            setters.SetDateTimeOffset(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetDbDataReader_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DbDataReader value)
        {
            setters = setters.GetTypedGetterSetter(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            while (value.Read())
            {
                setters.NewElement(sink);
                sink.ProcessMessagesAndThrow();
                FillCompatibleSettersFromReader(sink, setters, metaData.FieldMetaData, value);
            }
            setters.EndElements(sink);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetDBNull(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, bool value)
        {
            SetDBNull_Unchecked(sink, setters, ordinal);
        }

        private static void SetDBNull_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal)
        {
            setters.SetDBNull(sink, ordinal);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetDecimal(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, decimal value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Decimal);
            SetDecimal_PossiblyMoney(sink, setters, ordinal, metaData, value);
        }

        private static void SetDecimal_PossiblyMoney(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, decimal value)
        {
            if ((SqlDbType.Decimal == metaData.SqlDbType) || (SqlDbType.Variant == metaData.SqlDbType))
            {
                SetDecimal_Unchecked(sink, setters, ordinal, value);
            }
            else
            {
                SetSqlMoney_Checked(sink, setters, ordinal, metaData, new SqlMoney(value));
            }
        }

        private static void SetDecimal_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, decimal value)
        {
            setters.SetSqlDecimal(sink, ordinal, new SqlDecimal(value));
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetDouble(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, double value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Double);
            SetDouble_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetDouble_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, double value)
        {
            setters.SetDouble(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetGuid(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, Guid value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Guid);
            SetGuid_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetGuid_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, Guid value)
        {
            setters.SetGuid(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetIEnumerableOfSqlDataRecord_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, IEnumerable<SqlDataRecord> value, ParameterPeekAheadValue peekAhead)
        {
            setters = setters.GetTypedGetterSetter(sink, ordinal);
            sink.ProcessMessagesAndThrow();
            IEnumerator<SqlDataRecord> enumerator = null;
            try
            {
                SmiExtendedMetaData[] array = new SmiExtendedMetaData[metaData.FieldMetaData.Count];
                metaData.FieldMetaData.CopyTo(array, 0);
                SmiDefaultFieldsProperty useDefaultValues = (SmiDefaultFieldsProperty) metaData.ExtendedProperties[SmiPropertySelector.DefaultFields];
                int recordNumber = 1;
                if ((peekAhead != null) && (peekAhead.FirstRecord != null))
                {
                    enumerator = peekAhead.Enumerator;
                    setters.NewElement(sink);
                    sink.ProcessMessagesAndThrow();
                    FillCompatibleSettersFromRecord(sink, setters, array, peekAhead.FirstRecord, useDefaultValues);
                    recordNumber++;
                }
                else
                {
                    enumerator = value.GetEnumerator();
                }
                using (enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        setters.NewElement(sink);
                        sink.ProcessMessagesAndThrow();
                        SqlDataRecord current = enumerator.Current;
                        if (current.FieldCount != array.Length)
                        {
                            throw SQL.EnumeratedRecordFieldCountChanged(recordNumber);
                        }
                        for (int i = 0; i < current.FieldCount; i++)
                        {
                            if (!MetaDataUtilsSmi.IsCompatible(metaData.FieldMetaData[i], current.GetSqlMetaData(i)))
                            {
                                throw SQL.EnumeratedRecordMetaDataChanged(current.GetName(i), recordNumber);
                            }
                        }
                        FillCompatibleSettersFromRecord(sink, setters, array, current, useDefaultValues);
                        recordNumber++;
                    }
                    setters.EndElements(sink);
                    sink.ProcessMessagesAndThrow();
                }
            }
            finally
            {
                IDisposable disposable = enumerator;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static void SetInt16(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, short value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Int16);
            SetInt16_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetInt16_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, short value)
        {
            setters.SetInt16(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetInt32(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, int value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Int32);
            SetInt32_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetInt32_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, int value)
        {
            setters.SetInt32(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetInt64(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, long value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Int64);
            SetInt64_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetInt64_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, long value)
        {
            setters.SetInt64(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSingle(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, float value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.Single);
            SetSingle_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSingle_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, float value)
        {
            setters.SetSingle(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlBinary(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlBinary value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlBinary);
            SetSqlBinary_LengthChecked(sink, setters, ordinal, metaData, value, 0);
        }

        private static void SetSqlBinary_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlBinary value, int offset)
        {
            int length = 0;
            if (!value.IsNull)
            {
                length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, value.Length, offset, value.Length - offset);
            }
            SetSqlBinary_Unchecked(sink, setters, ordinal, value, offset, length);
        }

        private static void SetSqlBinary_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlBinary value, int offset, int length)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                SetByteArray_Unchecked(sink, setters, ordinal, value.Value, offset, length);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlBoolean(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlBoolean value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlBoolean);
            SetSqlBoolean_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlBoolean_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlBoolean value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetBoolean(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlByte(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlByte value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlByte);
            SetSqlByte_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlByte_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlByte value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetByte(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlBytes(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlBytes value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlBytes);
            SetSqlBytes_LengthChecked(sink, setters, ordinal, metaData, value, 0);
        }

        private static void SetSqlBytes_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlBytes value, int offset)
        {
            int num2 = 0;
            if (!value.IsNull)
            {
                long length = value.Length;
                if (length > 0x7fffffffL)
                {
                    length = -1L;
                }
                num2 = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, (int) length, offset, (int) length);
            }
            SetSqlBytes_Unchecked(sink, setters, ordinal, value, 0, (long) num2);
        }

        private static void SetSqlBytes_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlBytes value, int offset, long length)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
                sink.ProcessMessagesAndThrow();
            }
            else
            {
                int num4;
                long num5;
                if ((length > 0x1f40L) || (length < 0L))
                {
                    num4 = 0x1f40;
                }
                else
                {
                    num4 = (int) length;
                }
                byte[] buffer = new byte[num4];
                long num2 = 1L;
                long num = offset;
                for (long i = 0L; ((length < 0L) || (i < length)) && ((0L != (num5 = value.Read(num, buffer, 0, num4))) && (0L != num2)); i += num2)
                {
                    num2 = setters.SetBytes(sink, ordinal, num, buffer, 0, (int) num5);
                    sink.ProcessMessagesAndThrow();
                    num += num2;
                }
                setters.SetBytesLength(sink, ordinal, num);
                sink.ProcessMessagesAndThrow();
            }
        }

        internal static void SetSqlChars(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlChars value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlChars);
            SetSqlChars_LengthChecked(sink, setters, ordinal, metaData, value, 0);
        }

        private static void SetSqlChars_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlChars value, int offset)
        {
            int length = 0;
            if (!value.IsNull)
            {
                long num = value.Length;
                if (num > 0x7fffffffL)
                {
                    num = -1L;
                }
                length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, (int) num, offset, ((int) num) - offset);
            }
            SetSqlChars_Unchecked(sink, setters, ordinal, value, 0, length);
        }

        private static void SetSqlChars_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlChars value, int offset, int length)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
                sink.ProcessMessagesAndThrow();
            }
            else
            {
                int num4;
                long num5;
                if ((length > 0xfa0) || (length < 0))
                {
                    num4 = 0xfa0;
                }
                else
                {
                    num4 = length;
                }
                char[] buffer = new char[num4];
                long num2 = 1L;
                long num = offset;
                for (long i = 0L; ((length < 0) || (i < length)) && ((0L != (num5 = value.Read(num, buffer, 0, num4))) && (0L != num2)); i += num2)
                {
                    num2 = setters.SetChars(sink, ordinal, num, buffer, 0, (int) num5);
                    sink.ProcessMessagesAndThrow();
                    num += num2;
                }
                setters.SetCharsLength(sink, ordinal, num);
                sink.ProcessMessagesAndThrow();
            }
        }

        internal static void SetSqlDateTime(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDateTime value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlDateTime);
            SetSqlDateTime_Checked(sink, setters, ordinal, metaData, value);
        }

        private static void SetSqlDateTime_Checked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDateTime value)
        {
            if (!value.IsNull)
            {
                VerifyDateTimeRange(metaData.SqlDbType, value.Value);
            }
            SetSqlDateTime_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlDateTime_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlDateTime value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetDateTime(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlDecimal(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDecimal value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlDecimal);
            SetSqlDecimal_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlDecimal_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlDecimal value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetSqlDecimal(sink, ordinal, value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlDouble(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlDouble value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlDouble);
            SetSqlDouble_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlDouble_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlDouble value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetDouble(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlGuid(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlGuid value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlGuid);
            SetSqlGuid_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlGuid_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlGuid value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetGuid(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlInt16(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlInt16 value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlInt16);
            SetSqlInt16_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlInt16_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlInt16 value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetInt16(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlInt32(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlInt32 value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlInt32);
            SetSqlInt32_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlInt32_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlInt32 value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetInt32(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlInt64(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlInt64 value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlInt64);
            SetSqlInt64_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlInt64_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlInt64 value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetInt64(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlMoney(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlMoney value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlMoney);
            SetSqlMoney_Checked(sink, setters, ordinal, metaData, value);
        }

        private static void SetSqlMoney_Checked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlMoney value)
        {
            if (!value.IsNull && (SqlDbType.SmallMoney == metaData.SqlDbType))
            {
                decimal num = value.Value;
                if ((TdsEnums.SQL_SMALL_MONEY_MIN > num) || (TdsEnums.SQL_SMALL_MONEY_MAX < num))
                {
                    throw SQL.MoneyOverflow(num.ToString(CultureInfo.InvariantCulture));
                }
            }
            SetSqlMoney_Unchecked(sink, setters, ordinal, metaData, value);
        }

        private static void SetSqlMoney_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlMoney value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                if (SqlDbType.Variant == metaData.SqlDbType)
                {
                    setters.SetVariantMetaData(sink, ordinal, SmiMetaData.DefaultMoney);
                    sink.ProcessMessagesAndThrow();
                }
                setters.SetInt64(sink, ordinal, value.ToSqlInternalRepresentation());
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlSingle(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlSingle value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlSingle);
            SetSqlSingle_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlSingle_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlSingle value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                setters.SetSingle(sink, ordinal, value.Value);
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetSqlString(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlString value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlString);
            SetSqlString_LengthChecked(sink, setters, ordinal, metaData, value, 0);
        }

        private static void SetSqlString_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlString value, int offset)
        {
            if (value.IsNull)
            {
                SetDBNull_Unchecked(sink, setters, ordinal);
            }
            else
            {
                string str = value.Value;
                int length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, str.Length, offset, str.Length - offset);
                SetSqlString_Unchecked(sink, setters, ordinal, metaData, value, offset, length);
            }
        }

        private static void SetSqlString_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlString value, int offset, int length)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
                sink.ProcessMessagesAndThrow();
            }
            else
            {
                if (SqlDbType.Variant == metaData.SqlDbType)
                {
                    metaData = new SmiMetaData(SqlDbType.NVarChar, 0xfa0L, 0, 0, (long) value.LCID, value.SqlCompareOptions, null);
                    setters.SetVariantMetaData(sink, ordinal, metaData);
                    sink.ProcessMessagesAndThrow();
                }
                SetString_Unchecked(sink, setters, ordinal, value.Value, offset, length);
            }
        }

        internal static void SetSqlXml(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, SqlXml value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.SqlXml);
            SetSqlXml_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetSqlXml_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SqlXml value)
        {
            if (value.IsNull)
            {
                setters.SetDBNull(sink, ordinal);
            }
            else
            {
                XmlReader reader = value.CreateReader();
                XmlWriterSettings settings = new XmlWriterSettings {
                    CloseOutput = false,
                    ConformanceLevel = ConformanceLevel.Fragment,
                    Encoding = Encoding.Unicode,
                    OmitXmlDeclaration = true
                };
                Stream output = new SmiSettersStream(sink, setters, ordinal, SmiMetaData.DefaultXml);
                XmlWriter writer = XmlWriter.Create(output, settings);
                reader.Read();
                while (!reader.EOF)
                {
                    writer.WriteNode(reader, true);
                }
                writer.Flush();
            }
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetString(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, string value)
        {
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.String);
            SetString_LengthChecked(sink, setters, ordinal, metaData, value, 0);
        }

        private static void SetString_FromReader(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, DbDataReader reader, int offset)
        {
            string str = reader.GetString(ordinal);
            int length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, (long) str.Length, 0L, -1, offset, -1);
            setters.SetString(sink, ordinal, str, offset, length);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetString_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, string value, int offset)
        {
            int length = CheckXetParameters(metaData.SqlDbType, metaData.MaxLength, -1L, 0L, value.Length, offset, value.Length - offset);
            SetString_Unchecked(sink, setters, ordinal, value, offset, length);
        }

        private static void SetString_Unchecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, string value, int offset, int length)
        {
            setters.SetString(sink, ordinal, value, offset, length);
            sink.ProcessMessagesAndThrow();
        }

        internal static void SetTimeSpan(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, TimeSpan value, bool settersSupportKatmaiDateTime)
        {
            if (!settersSupportKatmaiDateTime)
            {
                throw ADP.InvalidCast();
            }
            ThrowIfInvalidSetterAccess(metaData, ExtendedClrTypeCode.TimeSpan);
            SetTimeSpan_Checked(sink, (SmiTypedGetterSetter) setters, ordinal, metaData, value);
        }

        private static void SetTimeSpan_Checked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, SmiMetaData metaData, TimeSpan value)
        {
            VerifyTimeRange(metaData.SqlDbType, value);
            SetTimeSpan_Unchecked(sink, setters, ordinal, value);
        }

        private static void SetTimeSpan_Unchecked(SmiEventSink_Default sink, SmiTypedGetterSetter setters, int ordinal, TimeSpan value)
        {
            setters.SetTimeSpan(sink, ordinal, value);
            sink.ProcessMessagesAndThrow();
        }

        private static void SetUdt_LengthChecked(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData, object value)
        {
            if (ADP.IsNull(value))
            {
                setters.SetDBNull(sink, ordinal);
                sink.ProcessMessagesAndThrow();
            }
            else
            {
                Stream s = new SmiSettersStream(sink, setters, ordinal, metaData);
                SerializationHelperSql9.Serialize(s, value);
            }
        }

        private static void ThrowIfInvalidSetterAccess(SmiMetaData metaData, ExtendedClrTypeCode setterTypeCode)
        {
            if (!CanAccessSetterDirectly(metaData, setterTypeCode))
            {
                throw ADP.InvalidCast();
            }
        }

        private static void ThrowIfITypedGettersIsNull(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal)
        {
            if (IsDBNull_Unchecked(sink, getters, ordinal))
            {
                throw SQL.SqlNullValue();
            }
        }

        private static void VerifyDateTimeRange(SqlDbType dbType, DateTime value)
        {
            if ((SqlDbType.SmallDateTime == dbType) && ((x_dtSmallMax < value) || (x_dtSmallMin > value)))
            {
                throw ADP.InvalidMetaDataValue();
            }
        }

        private static void VerifyTimeRange(SqlDbType dbType, TimeSpan value)
        {
            if ((SqlDbType.Time == dbType) && ((x_timeMin > value) || (value > x_timeMax)))
            {
                throw ADP.InvalidMetaDataValue();
            }
        }
    }
}

