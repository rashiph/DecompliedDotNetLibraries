namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    internal abstract class DataStorage
    {
        internal readonly DataColumn Column;
        internal readonly Type DataType;
        private BitArray dbNullBits;
        private readonly object DefaultValue;
        internal readonly bool IsCloneable;
        internal readonly bool IsCustomDefinedType;
        internal readonly bool IsStringType;
        internal readonly bool IsValueType;
        internal readonly object NullValue;
        private static readonly Type[] StorageClassType;
        internal readonly StorageType StorageTypeCode;
        internal readonly DataTable Table;

        static DataStorage()
        {
            Type[] typeArray = new Type[0x29];
            typeArray[1] = typeof(object);
            typeArray[2] = typeof(DBNull);
            typeArray[3] = typeof(bool);
            typeArray[4] = typeof(char);
            typeArray[5] = typeof(sbyte);
            typeArray[6] = typeof(byte);
            typeArray[7] = typeof(short);
            typeArray[8] = typeof(ushort);
            typeArray[9] = typeof(int);
            typeArray[10] = typeof(uint);
            typeArray[11] = typeof(long);
            typeArray[12] = typeof(ulong);
            typeArray[13] = typeof(float);
            typeArray[14] = typeof(double);
            typeArray[15] = typeof(decimal);
            typeArray[0x10] = typeof(DateTime);
            typeArray[0x11] = typeof(TimeSpan);
            typeArray[0x12] = typeof(string);
            typeArray[0x13] = typeof(Guid);
            typeArray[20] = typeof(byte[]);
            typeArray[0x15] = typeof(char[]);
            typeArray[0x16] = typeof(Type);
            typeArray[0x17] = typeof(DateTimeOffset);
            typeArray[0x18] = typeof(BigInteger);
            typeArray[0x19] = typeof(Uri);
            typeArray[0x1a] = typeof(SqlBinary);
            typeArray[0x1b] = typeof(SqlBoolean);
            typeArray[0x1c] = typeof(SqlByte);
            typeArray[0x1d] = typeof(SqlBytes);
            typeArray[30] = typeof(SqlChars);
            typeArray[0x1f] = typeof(SqlDateTime);
            typeArray[0x20] = typeof(SqlDecimal);
            typeArray[0x21] = typeof(SqlDouble);
            typeArray[0x22] = typeof(SqlGuid);
            typeArray[0x23] = typeof(SqlInt16);
            typeArray[0x24] = typeof(SqlInt32);
            typeArray[0x25] = typeof(SqlInt64);
            typeArray[0x26] = typeof(SqlMoney);
            typeArray[0x27] = typeof(SqlSingle);
            typeArray[40] = typeof(SqlString);
            StorageClassType = typeArray;
        }

        protected DataStorage(DataColumn column, Type type, object defaultValue) : this(column, type, defaultValue, DBNull.Value, false)
        {
        }

        protected DataStorage(DataColumn column, Type type, object defaultValue, object nullValue) : this(column, type, defaultValue, nullValue, false)
        {
        }

        protected DataStorage(DataColumn column, Type type, object defaultValue, object nullValue, bool isICloneable)
        {
            this.Column = column;
            this.Table = column.Table;
            this.DataType = type;
            this.StorageTypeCode = GetStorageType(type);
            this.DefaultValue = defaultValue;
            this.NullValue = nullValue;
            this.IsCloneable = isICloneable;
            this.IsCustomDefinedType = IsTypeCustomType(this.StorageTypeCode);
            this.IsStringType = (StorageType.String == this.StorageTypeCode) || (StorageType.SqlString == this.StorageTypeCode);
            this.IsValueType = DetermineIfValueType(this.StorageTypeCode, type);
        }

        public virtual object Aggregate(int[] recordNos, AggregateType kind)
        {
            if (AggregateType.Count == kind)
            {
                return this.AggregateCount(recordNos);
            }
            return null;
        }

        public object AggregateCount(int[] recordNos)
        {
            int num2 = 0;
            for (int i = 0; i < recordNos.Length; i++)
            {
                if (!this.dbNullBits.Get(recordNos[i]))
                {
                    num2++;
                }
            }
            return num2;
        }

        public abstract int Compare(int recordNo1, int recordNo2);
        protected int CompareBits(int recordNo1, int recordNo2)
        {
            bool flag = this.dbNullBits.Get(recordNo1);
            bool flag2 = this.dbNullBits.Get(recordNo2);
            if (!(flag ^ flag2))
            {
                return 0;
            }
            if (flag)
            {
                return -1;
            }
            return 1;
        }

        public abstract int CompareValueTo(int recordNo1, object value);
        public abstract string ConvertObjectToXml(object value);
        public virtual void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib)
        {
            xmlWriter.WriteString(this.ConvertObjectToXml(value));
        }

        public virtual object ConvertValue(object value)
        {
            return value;
        }

        public abstract object ConvertXmlToObject(string s);
        public virtual object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib)
        {
            return this.ConvertXmlToObject(xmlReader.Value);
        }

        public abstract void Copy(int recordNo1, int recordNo2);
        protected void CopyBits(int srcRecordNo, int dstRecordNo)
        {
            this.dbNullBits.Set(dstRecordNo, this.dbNullBits.Get(srcRecordNo));
        }

        protected abstract void CopyValue(int record, object store, BitArray nullbits, int storeIndex);
        internal void CopyValueInternal(int record, object store, BitArray nullbits, int storeIndex)
        {
            this.CopyValue(record, store, nullbits, storeIndex);
        }

        public static DataStorage CreateStorage(DataColumn column, Type dataType)
        {
            StorageType storageType = GetStorageType(dataType);
            if ((storageType == StorageType.Empty) && (null != dataType))
            {
                if (typeof(INullable).IsAssignableFrom(dataType))
                {
                    return new SqlUdtStorage(column, dataType);
                }
                return new ObjectStorage(column, dataType);
            }
            switch (storageType)
            {
                case StorageType.Empty:
                    throw ExceptionBuilder.InvalidStorageType(TypeCode.Empty);

                case StorageType.DBNull:
                    throw ExceptionBuilder.InvalidStorageType(TypeCode.DBNull);

                case StorageType.Boolean:
                    return new BooleanStorage(column);

                case StorageType.Char:
                    return new CharStorage(column);

                case StorageType.SByte:
                    return new SByteStorage(column);

                case StorageType.Byte:
                    return new ByteStorage(column);

                case StorageType.Int16:
                    return new Int16Storage(column);

                case StorageType.UInt16:
                    return new UInt16Storage(column);

                case StorageType.Int32:
                    return new Int32Storage(column);

                case StorageType.UInt32:
                    return new UInt32Storage(column);

                case StorageType.Int64:
                    return new Int64Storage(column);

                case StorageType.UInt64:
                    return new UInt64Storage(column);

                case StorageType.Single:
                    return new SingleStorage(column);

                case StorageType.Double:
                    return new DoubleStorage(column);

                case StorageType.Decimal:
                    return new DecimalStorage(column);

                case StorageType.DateTime:
                    return new DateTimeStorage(column);

                case StorageType.TimeSpan:
                    return new TimeSpanStorage(column);

                case StorageType.String:
                    return new StringStorage(column);

                case StorageType.Guid:
                    return new ObjectStorage(column, dataType);

                case StorageType.ByteArray:
                    return new ObjectStorage(column, dataType);

                case StorageType.CharArray:
                    return new ObjectStorage(column, dataType);

                case StorageType.Type:
                    return new ObjectStorage(column, dataType);

                case StorageType.DateTimeOffset:
                    return new DateTimeOffsetStorage(column);

                case StorageType.BigInteger:
                    return new BigIntegerStorage(column);

                case StorageType.Uri:
                    return new ObjectStorage(column, dataType);

                case StorageType.SqlBinary:
                    return new SqlBinaryStorage(column);

                case StorageType.SqlBoolean:
                    return new SqlBooleanStorage(column);

                case StorageType.SqlByte:
                    return new SqlByteStorage(column);

                case StorageType.SqlBytes:
                    return new SqlBytesStorage(column);

                case StorageType.SqlChars:
                    return new SqlCharsStorage(column);

                case StorageType.SqlDateTime:
                    return new SqlDateTimeStorage(column);

                case StorageType.SqlDecimal:
                    return new SqlDecimalStorage(column);

                case StorageType.SqlDouble:
                    return new SqlDoubleStorage(column);

                case StorageType.SqlGuid:
                    return new SqlGuidStorage(column);

                case StorageType.SqlInt16:
                    return new SqlInt16Storage(column);

                case StorageType.SqlInt32:
                    return new SqlInt32Storage(column);

                case StorageType.SqlInt64:
                    return new SqlInt64Storage(column);

                case StorageType.SqlMoney:
                    return new SqlMoneyStorage(column);

                case StorageType.SqlSingle:
                    return new SqlSingleStorage(column);

                case StorageType.SqlString:
                    return new SqlStringStorage(column);
            }
            return new ObjectStorage(column, dataType);
        }

        private static bool DetermineIfValueType(StorageType typeCode, Type dataType)
        {
            switch (typeCode)
            {
                case StorageType.Boolean:
                case StorageType.Char:
                case StorageType.SByte:
                case StorageType.Byte:
                case StorageType.Int16:
                case StorageType.UInt16:
                case StorageType.Int32:
                case StorageType.UInt32:
                case StorageType.Int64:
                case StorageType.UInt64:
                case StorageType.Single:
                case StorageType.Double:
                case StorageType.Decimal:
                case StorageType.DateTime:
                case StorageType.TimeSpan:
                case StorageType.Guid:
                case StorageType.DateTimeOffset:
                case StorageType.BigInteger:
                case StorageType.SqlBinary:
                case StorageType.SqlBoolean:
                case StorageType.SqlByte:
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
                    return true;

                case StorageType.String:
                case StorageType.ByteArray:
                case StorageType.CharArray:
                case StorageType.Type:
                case StorageType.Uri:
                case StorageType.SqlBytes:
                case StorageType.SqlChars:
                    return false;
            }
            return dataType.IsValueType;
        }

        public abstract object Get(int recordNo);
        protected object GetBits(int recordNo)
        {
            if (this.dbNullBits.Get(recordNo))
            {
                return this.NullValue;
            }
            return this.DefaultValue;
        }

        protected abstract object GetEmptyStorage(int recordCount);
        internal object GetEmptyStorageInternal(int recordCount)
        {
            return this.GetEmptyStorage(recordCount);
        }

        internal static string GetQualifiedName(Type type)
        {
            ObjectStorage.VerifyIDynamicMetaObjectProvider(type);
            return type.AssemblyQualifiedName;
        }

        internal static StorageType GetStorageType(Type dataType)
        {
            for (int i = 0; i < StorageClassType.Length; i++)
            {
                if (dataType == StorageClassType[i])
                {
                    return (StorageType) i;
                }
            }
            TypeCode typeCode = Type.GetTypeCode(dataType);
            if (TypeCode.Object != typeCode)
            {
                return (StorageType) typeCode;
            }
            return StorageType.Empty;
        }

        public virtual int GetStringLength(int record)
        {
            return 0x7fffffff;
        }

        internal static Type GetType(string value)
        {
            Type type = Type.GetType(value);
            if ((null == type) && ("System.Numerics.BigInteger" == value))
            {
                type = typeof(BigInteger);
            }
            ObjectStorage.VerifyIDynamicMetaObjectProvider(type);
            return type;
        }

        internal static Type GetTypeStorage(StorageType storageType)
        {
            return StorageClassType[(int) storageType];
        }

        protected bool HasValue(int recordNo)
        {
            return !this.dbNullBits.Get(recordNo);
        }

        internal static void ImplementsInterfaces(StorageType typeCode, Type dataType, out bool sqlType, out bool nullable, out bool xmlSerializable, out bool changeTracking, out bool revertibleChangeTracking)
        {
            if (IsSqlType(typeCode))
            {
                sqlType = true;
                nullable = true;
                changeTracking = false;
                revertibleChangeTracking = false;
                xmlSerializable = true;
            }
            else if (typeCode != StorageType.Empty)
            {
                sqlType = false;
                nullable = false;
                changeTracking = false;
                revertibleChangeTracking = false;
                xmlSerializable = false;
            }
            else
            {
                sqlType = false;
                nullable = typeof(INullable).IsAssignableFrom(dataType);
                changeTracking = typeof(IChangeTracking).IsAssignableFrom(dataType);
                revertibleChangeTracking = typeof(IRevertibleChangeTracking).IsAssignableFrom(dataType);
                xmlSerializable = typeof(IXmlSerializable).IsAssignableFrom(dataType);
            }
        }

        internal static bool ImplementsINullableValue(StorageType typeCode, Type dataType)
        {
            return (((typeCode == StorageType.Empty) && dataType.IsGenericType) && (dataType.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        public virtual bool IsNull(int recordNo)
        {
            return this.dbNullBits.Get(recordNo);
        }

        public static bool IsObjectNull(object value)
        {
            if ((value != null) && (DBNull.Value != value))
            {
                return IsObjectSqlNull(value);
            }
            return true;
        }

        public static bool IsObjectSqlNull(object value)
        {
            INullable nullable = value as INullable;
            return ((nullable != null) && nullable.IsNull);
        }

        internal static bool IsSqlType(StorageType storageType)
        {
            return (StorageType.SqlBinary <= storageType);
        }

        public static bool IsSqlType(Type dataType)
        {
            for (int i = 0x1a; i < StorageClassType.Length; i++)
            {
                if (dataType == StorageClassType[i])
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsTypeCustomType(StorageType typeCode)
        {
            if ((StorageType.Object != typeCode) && (typeCode != StorageType.Empty))
            {
                return (StorageType.CharArray == typeCode);
            }
            return true;
        }

        internal static bool IsTypeCustomType(Type type)
        {
            return IsTypeCustomType(GetStorageType(type));
        }

        public abstract void Set(int recordNo, object value);
        public virtual void SetCapacity(int capacity)
        {
            if (this.dbNullBits == null)
            {
                this.dbNullBits = new BitArray(capacity);
            }
            else
            {
                this.dbNullBits.Length = capacity;
            }
        }

        protected void SetNullBit(int recordNo, bool flag)
        {
            this.dbNullBits.Set(recordNo, flag);
        }

        protected void SetNullStorage(BitArray nullbits)
        {
            this.dbNullBits = nullbits;
        }

        protected abstract void SetStorage(object store, BitArray nullbits);
        internal void SetStorageInternal(object store, BitArray nullbits)
        {
            this.SetStorage(store, nullbits);
        }

        internal DataSetDateTime DateTimeMode
        {
            get
            {
                return this.Column.DateTimeMode;
            }
        }

        internal IFormatProvider FormatProvider
        {
            get
            {
                return this.Table.FormatProvider;
            }
        }
    }
}

