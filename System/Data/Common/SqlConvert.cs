namespace System.Data.Common
{
    using System;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Numerics;
    using System.Xml;

    internal static class SqlConvert
    {
        public static object ChangeType2(object value, StorageType stype, Type type, IFormatProvider formatProvider)
        {
            switch (stype)
            {
                case StorageType.SqlBinary:
                    return ConvertToSqlBinary(value);

                case StorageType.SqlBoolean:
                    return ConvertToSqlBoolean(value);

                case StorageType.SqlByte:
                    return ConvertToSqlByte(value);

                case StorageType.SqlBytes:
                    return ConvertToSqlBytes(value);

                case StorageType.SqlChars:
                    return ConvertToSqlChars(value);

                case StorageType.SqlDateTime:
                    return ConvertToSqlDateTime(value);

                case StorageType.SqlDecimal:
                    return ConvertToSqlDecimal(value);

                case StorageType.SqlDouble:
                    return ConvertToSqlDouble(value);

                case StorageType.SqlGuid:
                    return ConvertToSqlGuid(value);

                case StorageType.SqlInt16:
                    return ConvertToSqlInt16(value);

                case StorageType.SqlInt32:
                    return ConvertToSqlInt32(value);

                case StorageType.SqlInt64:
                    return ConvertToSqlInt64(value);

                case StorageType.SqlMoney:
                    return ConvertToSqlMoney(value);

                case StorageType.SqlSingle:
                    return ConvertToSqlSingle(value);

                case StorageType.SqlString:
                    return ConvertToSqlString(value);

                default:
                {
                    if ((DBNull.Value == value) || (value == null))
                    {
                        return DBNull.Value;
                    }
                    Type dataType = value.GetType();
                    StorageType storageType = DataStorage.GetStorageType(dataType);
                    switch (storageType)
                    {
                        case StorageType.SqlBinary:
                        case StorageType.SqlBoolean:
                        case StorageType.SqlByte:
                        case StorageType.SqlBytes:
                        case StorageType.SqlChars:
                        case StorageType.SqlDateTime:
                        case StorageType.SqlDecimal:
                        case StorageType.SqlDouble:
                        case StorageType.SqlGuid:
                        case StorageType.SqlInt16:
                        case StorageType.SqlInt32:
                        case StorageType.SqlInt64:
                        case StorageType.SqlMoney:
                        case StorageType.SqlSingle:
                        case StorageType.SqlString:
                            throw ExceptionBuilder.ConvertFailed(dataType, type);
                    }
                    if (StorageType.String != stype)
                    {
                        if (StorageType.TimeSpan == stype)
                        {
                            switch (storageType)
                            {
                                case StorageType.Int32:
                                    return new TimeSpan((long) ((int) value));

                                case StorageType.Int64:
                                    return new TimeSpan((long) value);

                                case StorageType.String:
                                    return XmlConvert.ToTimeSpan((string) value);
                            }
                            return (TimeSpan) value;
                        }
                        if (StorageType.DateTimeOffset == stype)
                        {
                            return (DateTimeOffset) value;
                        }
                        if (StorageType.String == storageType)
                        {
                            switch (stype)
                            {
                                case StorageType.Boolean:
                                    if (!("1" == ((string) value)))
                                    {
                                        if ("0" == ((string) value))
                                        {
                                            return false;
                                        }
                                        break;
                                    }
                                    return true;

                                case StorageType.Char:
                                    return ((IConvertible) ((string) value)).ToChar(formatProvider);

                                case StorageType.SByte:
                                    return ((IConvertible) ((string) value)).ToSByte(formatProvider);

                                case StorageType.Byte:
                                    return ((IConvertible) ((string) value)).ToByte(formatProvider);

                                case StorageType.Int16:
                                    return ((IConvertible) ((string) value)).ToInt16(formatProvider);

                                case StorageType.UInt16:
                                    return ((IConvertible) ((string) value)).ToUInt16(formatProvider);

                                case StorageType.Int32:
                                    return ((IConvertible) ((string) value)).ToInt32(formatProvider);

                                case StorageType.UInt32:
                                    return ((IConvertible) ((string) value)).ToUInt32(formatProvider);

                                case StorageType.Int64:
                                    return ((IConvertible) ((string) value)).ToInt64(formatProvider);

                                case StorageType.UInt64:
                                    return ((IConvertible) ((string) value)).ToUInt64(formatProvider);

                                case StorageType.Single:
                                    return ((IConvertible) ((string) value)).ToSingle(formatProvider);

                                case StorageType.Double:
                                    return ((IConvertible) ((string) value)).ToDouble(formatProvider);

                                case StorageType.Decimal:
                                    return ((IConvertible) ((string) value)).ToDecimal(formatProvider);

                                case StorageType.DateTime:
                                    return ((IConvertible) ((string) value)).ToDateTime(formatProvider);

                                case StorageType.TimeSpan:
                                    return XmlConvert.ToTimeSpan((string) value);

                                case StorageType.String:
                                    return (string) value;

                                case StorageType.Guid:
                                    return XmlConvert.ToGuid((string) value);

                                case StorageType.Uri:
                                    return new Uri((string) value);
                            }
                        }
                        goto Label_057D;
                    }
                    switch (storageType)
                    {
                        case StorageType.Boolean:
                            return ((bool) value).ToString(formatProvider);

                        case StorageType.Char:
                            return ((char) value).ToString(formatProvider);

                        case StorageType.SByte:
                            return ((sbyte) value).ToString(formatProvider);

                        case StorageType.Byte:
                            return ((byte) value).ToString(formatProvider);

                        case StorageType.Int16:
                            return ((short) value).ToString(formatProvider);

                        case StorageType.UInt16:
                            return ((ushort) value).ToString(formatProvider);

                        case StorageType.Int32:
                            return ((int) value).ToString(formatProvider);

                        case StorageType.UInt32:
                            return ((uint) value).ToString(formatProvider);

                        case StorageType.Int64:
                            return ((long) value).ToString(formatProvider);

                        case StorageType.UInt64:
                            return ((ulong) value).ToString(formatProvider);

                        case StorageType.Single:
                            return ((float) value).ToString(formatProvider);

                        case StorageType.Double:
                            return ((double) value).ToString(formatProvider);

                        case StorageType.Decimal:
                            return ((decimal) value).ToString(formatProvider);

                        case StorageType.DateTime:
                            return ((DateTime) value).ToString(formatProvider);

                        case StorageType.TimeSpan:
                            return XmlConvert.ToString((TimeSpan) value);

                        case StorageType.String:
                            return (string) value;

                        case StorageType.Guid:
                            return XmlConvert.ToString((Guid) value);

                        case StorageType.CharArray:
                            return new string((char[]) value);

                        case StorageType.DateTimeOffset:
                        {
                            DateTimeOffset offset = (DateTimeOffset) value;
                            return offset.ToString(formatProvider);
                        }
                        case StorageType.BigInteger:
                            goto Label_057D;
                    }
                    break;
                }
            }
            IConvertible convertible = value as IConvertible;
            if (convertible != null)
            {
                return convertible.ToString(formatProvider);
            }
            IFormattable formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, formatProvider);
            }
            return value.ToString();
        Label_057D:
            return Convert.ChangeType(value, type, formatProvider);
        }

        public static object ChangeTypeForDefaultValue(object value, Type type, IFormatProvider formatProvider)
        {
            if (type == typeof(BigInteger))
            {
                if ((DBNull.Value != value) && (value != null))
                {
                    return BigIntegerStorage.ConvertToBigInteger(value, formatProvider);
                }
                return DBNull.Value;
            }
            if (value is BigInteger)
            {
                return BigIntegerStorage.ConvertFromBigInteger((BigInteger) value, type, formatProvider);
            }
            return ChangeType2(value, DataStorage.GetStorageType(type), type, formatProvider);
        }

        public static object ChangeTypeForXML(object value, Type type)
        {
            StorageType storageType = DataStorage.GetStorageType(type);
            StorageType type3 = DataStorage.GetStorageType(value.GetType());
            switch (storageType)
            {
                case StorageType.Boolean:
                    if (!("1" == ((string) value)))
                    {
                        if ("0" == ((string) value))
                        {
                            return false;
                        }
                        return XmlConvert.ToBoolean((string) value);
                    }
                    return true;

                case StorageType.Char:
                    return XmlConvert.ToChar((string) value);

                case StorageType.SByte:
                    return XmlConvert.ToSByte((string) value);

                case StorageType.Byte:
                    return XmlConvert.ToByte((string) value);

                case StorageType.Int16:
                    return XmlConvert.ToInt16((string) value);

                case StorageType.UInt16:
                    return XmlConvert.ToUInt16((string) value);

                case StorageType.Int32:
                    return XmlConvert.ToInt32((string) value);

                case StorageType.UInt32:
                    return XmlConvert.ToUInt32((string) value);

                case StorageType.Int64:
                    return XmlConvert.ToInt64((string) value);

                case StorageType.UInt64:
                    return XmlConvert.ToUInt64((string) value);

                case StorageType.Single:
                    return XmlConvert.ToSingle((string) value);

                case StorageType.Double:
                    return XmlConvert.ToDouble((string) value);

                case StorageType.Decimal:
                    return XmlConvert.ToDecimal((string) value);

                case StorageType.DateTime:
                    return XmlConvert.ToDateTime((string) value, XmlDateTimeSerializationMode.RoundtripKind);

                case StorageType.TimeSpan:
                {
                    StorageType type2 = type3;
                    switch (type2)
                    {
                        case StorageType.Int32:
                            return new TimeSpan((long) ((int) value));

                        case StorageType.Int64:
                            return new TimeSpan((long) value);
                    }
                    if (type2 != StorageType.String)
                    {
                        break;
                    }
                    return XmlConvert.ToTimeSpan((string) value);
                }
                case StorageType.Guid:
                    return XmlConvert.ToGuid((string) value);

                case StorageType.DateTimeOffset:
                    return XmlConvert.ToDateTimeOffset((string) value);

                case StorageType.Uri:
                    return new Uri((string) value);

                case StorageType.SqlBinary:
                    return new SqlBinary(Convert.FromBase64String((string) value));

                case StorageType.SqlBoolean:
                    return new SqlBoolean(XmlConvert.ToBoolean((string) value));

                case StorageType.SqlByte:
                    return new SqlByte(XmlConvert.ToByte((string) value));

                case StorageType.SqlBytes:
                    return new SqlBytes(Convert.FromBase64String((string) value));

                case StorageType.SqlChars:
                    return new SqlChars(((string) value).ToCharArray());

                case StorageType.SqlDateTime:
                    return new SqlDateTime(XmlConvert.ToDateTime((string) value, XmlDateTimeSerializationMode.RoundtripKind));

                case StorageType.SqlDecimal:
                    return SqlDecimal.Parse((string) value);

                case StorageType.SqlDouble:
                    return new SqlDouble(XmlConvert.ToDouble((string) value));

                case StorageType.SqlGuid:
                    return new SqlGuid(XmlConvert.ToGuid((string) value));

                case StorageType.SqlInt16:
                    return new SqlInt16(XmlConvert.ToInt16((string) value));

                case StorageType.SqlInt32:
                    return new SqlInt32(XmlConvert.ToInt32((string) value));

                case StorageType.SqlInt64:
                    return new SqlInt64(XmlConvert.ToInt64((string) value));

                case StorageType.SqlMoney:
                    return new SqlMoney(XmlConvert.ToDecimal((string) value));

                case StorageType.SqlSingle:
                    return new SqlSingle(XmlConvert.ToSingle((string) value));

                case StorageType.SqlString:
                    return new SqlString((string) value);

                default:
                {
                    if ((DBNull.Value == value) || (value == null))
                    {
                        return DBNull.Value;
                    }
                    switch (type3)
                    {
                        case StorageType.Boolean:
                            return XmlConvert.ToString((bool) value);

                        case StorageType.Char:
                            return XmlConvert.ToString((char) value);

                        case StorageType.SByte:
                            return XmlConvert.ToString((sbyte) value);

                        case StorageType.Byte:
                            return XmlConvert.ToString((byte) value);

                        case StorageType.Int16:
                            return XmlConvert.ToString((short) value);

                        case StorageType.UInt16:
                            return XmlConvert.ToString((ushort) value);

                        case StorageType.Int32:
                            return XmlConvert.ToString((int) value);

                        case StorageType.UInt32:
                            return XmlConvert.ToString((uint) value);

                        case StorageType.Int64:
                            return XmlConvert.ToString((long) value);

                        case StorageType.UInt64:
                            return XmlConvert.ToString((ulong) value);

                        case StorageType.Single:
                            return XmlConvert.ToString((float) value);

                        case StorageType.Double:
                            return XmlConvert.ToString((double) value);

                        case StorageType.Decimal:
                            return XmlConvert.ToString((decimal) value);

                        case StorageType.DateTime:
                            return XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.RoundtripKind);

                        case StorageType.TimeSpan:
                            return XmlConvert.ToString((TimeSpan) value);

                        case StorageType.String:
                            return (string) value;

                        case StorageType.Guid:
                            return XmlConvert.ToString((Guid) value);

                        case StorageType.CharArray:
                            return new string((char[]) value);

                        case StorageType.DateTimeOffset:
                            return XmlConvert.ToString((DateTimeOffset) value);

                        case StorageType.SqlBinary:
                        {
                            SqlBinary binary = (SqlBinary) value;
                            return Convert.ToBase64String(binary.Value);
                        }
                        case StorageType.SqlBoolean:
                        {
                            SqlBoolean flag = (SqlBoolean) value;
                            return XmlConvert.ToString(flag.Value);
                        }
                        case StorageType.SqlByte:
                        {
                            SqlByte num7 = (SqlByte) value;
                            return XmlConvert.ToString(num7.Value);
                        }
                        case StorageType.SqlBytes:
                            return Convert.ToBase64String(((SqlBytes) value).Value);

                        case StorageType.SqlChars:
                            return new string(((SqlChars) value).Value);

                        case StorageType.SqlDateTime:
                        {
                            SqlDateTime time = (SqlDateTime) value;
                            return XmlConvert.ToString(time.Value, XmlDateTimeSerializationMode.RoundtripKind);
                        }
                        case StorageType.SqlDecimal:
                        {
                            SqlDecimal num6 = (SqlDecimal) value;
                            return num6.ToString();
                        }
                        case StorageType.SqlDouble:
                        {
                            SqlDouble num5 = (SqlDouble) value;
                            return XmlConvert.ToString(num5.Value);
                        }
                        case StorageType.SqlGuid:
                        {
                            SqlGuid guid = (SqlGuid) value;
                            return XmlConvert.ToString(guid.Value);
                        }
                        case StorageType.SqlInt16:
                        {
                            SqlInt16 num4 = (SqlInt16) value;
                            return XmlConvert.ToString(num4.Value);
                        }
                        case StorageType.SqlInt32:
                        {
                            SqlInt32 num3 = (SqlInt32) value;
                            return XmlConvert.ToString(num3.Value);
                        }
                        case StorageType.SqlInt64:
                        {
                            SqlInt64 num2 = (SqlInt64) value;
                            return XmlConvert.ToString(num2.Value);
                        }
                        case StorageType.SqlMoney:
                        {
                            SqlMoney money = (SqlMoney) value;
                            return XmlConvert.ToString(money.Value);
                        }
                        case StorageType.SqlSingle:
                        {
                            SqlSingle num = (SqlSingle) value;
                            return XmlConvert.ToString(num.Value);
                        }
                        case StorageType.SqlString:
                        {
                            SqlString str = (SqlString) value;
                            return str.Value;
                        }
                    }
                    IConvertible convertible = value as IConvertible;
                    if (convertible != null)
                    {
                        return convertible.ToString(CultureInfo.InvariantCulture);
                    }
                    IFormattable formattable = value as IFormattable;
                    if (formattable != null)
                    {
                        return formattable.ToString(null, CultureInfo.InvariantCulture);
                    }
                    return value.ToString();
                }
            }
            return (TimeSpan) value;
        }

        public static DateTimeOffset ConvertStringToDateTimeOffset(string value, IFormatProvider formatProvider)
        {
            return DateTimeOffset.Parse(value, formatProvider);
        }

        public static SqlBinary ConvertToSqlBinary(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlBinary.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.ByteArray)
            {
                if (storageType != StorageType.SqlBinary)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlBinary));
                }
                return (SqlBinary) value;
            }
            return (byte[]) value;
        }

        public static SqlBoolean ConvertToSqlBoolean(object value)
        {
            if ((value == DBNull.Value) || (value == null))
            {
                return SqlBoolean.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.Boolean)
            {
                if (storageType != StorageType.SqlBoolean)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlBoolean));
                }
                return (SqlBoolean) value;
            }
            return (bool) value;
        }

        public static SqlByte ConvertToSqlByte(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlByte.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.Byte)
            {
                if (storageType != StorageType.SqlByte)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlByte));
                }
                return (SqlByte) value;
            }
            return (byte) value;
        }

        public static SqlBytes ConvertToSqlBytes(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlBytes.Null;
            }
            Type dataType = value.GetType();
            if (DataStorage.GetStorageType(dataType) != StorageType.SqlBytes)
            {
                throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlBytes));
            }
            return (SqlBytes) value;
        }

        public static SqlChars ConvertToSqlChars(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlChars.Null;
            }
            Type dataType = value.GetType();
            if (DataStorage.GetStorageType(dataType) != StorageType.SqlChars)
            {
                throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlChars));
            }
            return (SqlChars) value;
        }

        public static SqlDateTime ConvertToSqlDateTime(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlDateTime.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.DateTime)
            {
                if (storageType != StorageType.SqlDateTime)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlDateTime));
                }
                return (SqlDateTime) value;
            }
            return (DateTime) value;
        }

        public static SqlDecimal ConvertToSqlDecimal(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlDecimal.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (SqlDecimal) ((byte) value);

                case StorageType.Int16:
                    return (long) ((short) value);

                case StorageType.UInt16:
                    return (SqlDecimal) ((ushort) value);

                case StorageType.Int32:
                    return (long) ((int) value);

                case StorageType.UInt32:
                    return (SqlDecimal) ((uint) value);

                case StorageType.Int64:
                    return (long) value;

                case StorageType.UInt64:
                    return (SqlDecimal) ((ulong) value);

                case StorageType.Decimal:
                    return (decimal) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlDecimal:
                    return (SqlDecimal) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;

                case StorageType.SqlInt64:
                    return (SqlInt64) value;

                case StorageType.SqlMoney:
                    return (SqlMoney) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlDecimal));
        }

        public static SqlDouble ConvertToSqlDouble(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlDouble.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (double) ((byte) value);

                case StorageType.Int16:
                    return (double) ((short) value);

                case StorageType.UInt16:
                    return (double) ((ushort) value);

                case StorageType.Int32:
                    return (double) ((int) value);

                case StorageType.UInt32:
                    return (double) ((uint) value);

                case StorageType.Int64:
                    return (double) ((long) value);

                case StorageType.UInt64:
                    return (double) ((ulong) value);

                case StorageType.Single:
                    return (double) ((float) value);

                case StorageType.Double:
                    return (double) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlDecimal:
                    return (SqlDecimal) value;

                case StorageType.SqlDouble:
                    return (SqlDouble) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;

                case StorageType.SqlInt64:
                    return (SqlInt64) value;

                case StorageType.SqlMoney:
                    return (SqlMoney) value;

                case StorageType.SqlSingle:
                    return (SqlSingle) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlDouble));
        }

        public static SqlGuid ConvertToSqlGuid(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlGuid.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.Guid)
            {
                if (storageType != StorageType.SqlGuid)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlGuid));
                }
                return (SqlGuid) value;
            }
            return (Guid) value;
        }

        public static SqlInt16 ConvertToSqlInt16(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlInt16.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (SqlInt16) ((byte) value);

                case StorageType.Int16:
                    return (short) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlInt16));
        }

        public static SqlInt32 ConvertToSqlInt32(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlInt32.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (SqlInt32) ((byte) value);

                case StorageType.Int16:
                    return (SqlInt32) ((short) value);

                case StorageType.UInt16:
                    return (SqlInt32) ((ushort) value);

                case StorageType.Int32:
                    return (int) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlInt32));
        }

        public static SqlInt64 ConvertToSqlInt64(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlInt32.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (SqlInt64) ((byte) value);

                case StorageType.Int16:
                    return (long) ((short) value);

                case StorageType.UInt16:
                    return (SqlInt64) ((ushort) value);

                case StorageType.Int32:
                    return (long) ((int) value);

                case StorageType.UInt32:
                    return (SqlInt64) ((uint) value);

                case StorageType.Int64:
                    return (long) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;

                case StorageType.SqlInt64:
                    return (SqlInt64) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlInt64));
        }

        public static SqlMoney ConvertToSqlMoney(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlMoney.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (SqlMoney) ((byte) value);

                case StorageType.Int16:
                    return (long) ((short) value);

                case StorageType.UInt16:
                    return (SqlMoney) ((ushort) value);

                case StorageType.Int32:
                    return (long) ((int) value);

                case StorageType.UInt32:
                    return (SqlMoney) ((uint) value);

                case StorageType.Int64:
                    return (long) value;

                case StorageType.UInt64:
                    return (SqlMoney) ((ulong) value);

                case StorageType.Decimal:
                    return (decimal) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;

                case StorageType.SqlInt64:
                    return (SqlInt64) value;

                case StorageType.SqlMoney:
                    return (SqlMoney) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlMoney));
        }

        public static SqlSingle ConvertToSqlSingle(object value)
        {
            if (value == DBNull.Value)
            {
                return SqlSingle.Null;
            }
            Type type = value.GetType();
            switch (DataStorage.GetStorageType(type))
            {
                case StorageType.Byte:
                    return (float) ((byte) value);

                case StorageType.Int16:
                    return (float) ((short) value);

                case StorageType.UInt16:
                    return (float) ((ushort) value);

                case StorageType.Int32:
                    return (float) ((int) value);

                case StorageType.UInt32:
                    return (float) ((uint) value);

                case StorageType.Int64:
                    return (float) ((long) value);

                case StorageType.UInt64:
                    return (float) ((ulong) value);

                case StorageType.Single:
                    return (float) value;

                case StorageType.SqlByte:
                    return (SqlByte) value;

                case StorageType.SqlDecimal:
                    return (SqlDecimal) value;

                case StorageType.SqlInt16:
                    return (SqlInt16) value;

                case StorageType.SqlInt32:
                    return (SqlInt32) value;

                case StorageType.SqlInt64:
                    return (SqlInt64) value;

                case StorageType.SqlMoney:
                    return (SqlMoney) value;

                case StorageType.SqlSingle:
                    return (SqlSingle) value;
            }
            throw ExceptionBuilder.ConvertFailed(type, typeof(SqlSingle));
        }

        public static SqlString ConvertToSqlString(object value)
        {
            if ((value == DBNull.Value) || (value == null))
            {
                return SqlString.Null;
            }
            Type dataType = value.GetType();
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (storageType != StorageType.String)
            {
                if (storageType != StorageType.SqlString)
                {
                    throw ExceptionBuilder.ConvertFailed(dataType, typeof(SqlString));
                }
                return (SqlString) value;
            }
            return (string) value;
        }
    }
}

