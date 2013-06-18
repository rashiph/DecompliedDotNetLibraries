namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class SqlRecordBuffer
    {
        private bool _isMetaSet;
        private bool _isNull = true;
        private SmiMetaData _metadata;
        private object _object;
        private StorageType _type;
        private Storage _value;

        internal SqlRecordBuffer(SmiMetaData metaData)
        {
        }

        private void ConvertXmlStringToByteArray()
        {
            string s = (string) this._object;
            byte[] bytes = new byte[2 + Encoding.Unicode.GetByteCount(s)];
            bytes[0] = 0xff;
            bytes[1] = 0xfe;
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 2);
            this._object = bytes;
            this._value._int64 = bytes.Length;
            this._type = StorageType.ByteArray;
        }

        internal int GetBytes(long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            int srcOffset = (int) fieldOffset;
            if (StorageType.String == this._type)
            {
                this.ConvertXmlStringToByteArray();
            }
            Buffer.BlockCopy((byte[]) this._object, srcOffset, buffer, bufferOffset, length);
            return length;
        }

        internal int GetChars(long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            int sourceIndex = (int) fieldOffset;
            if (StorageType.CharArray == this._type)
            {
                Array.Copy((char[]) this._object, sourceIndex, buffer, bufferOffset, length);
                return length;
            }
            ((string) this._object).CopyTo(sourceIndex, buffer, bufferOffset, length);
            return length;
        }

        internal int SetBytes(long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            int dstOffset = (int) fieldOffset;
            if (this.IsNull || (StorageType.ByteArray != this._type))
            {
                if (dstOffset != 0)
                {
                    throw ADP.ArgumentOutOfRange("fieldOffset");
                }
                this._object = new byte[length];
                this._type = StorageType.ByteArray;
                this._isNull = false;
                this.BytesLength = length;
            }
            else
            {
                if (dstOffset > this.BytesLength)
                {
                    throw ADP.ArgumentOutOfRange("fieldOffset");
                }
                if ((dstOffset + length) > this.BytesLength)
                {
                    int num2 = ((byte[]) this._object).Length;
                    if ((dstOffset + length) > num2)
                    {
                        byte[] dst = new byte[Math.Max((int) (dstOffset + length), (int) (2 * num2))];
                        Buffer.BlockCopy((byte[]) this._object, 0, dst, 0, (int) this.BytesLength);
                        this._object = dst;
                    }
                    this.BytesLength = dstOffset + length;
                }
            }
            Buffer.BlockCopy(buffer, bufferOffset, (byte[]) this._object, dstOffset, length);
            return length;
        }

        internal int SetChars(long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            int destinationIndex = (int) fieldOffset;
            if (this.IsNull || ((StorageType.CharArray != this._type) && (StorageType.String != this._type)))
            {
                if (destinationIndex != 0)
                {
                    throw ADP.ArgumentOutOfRange("fieldOffset");
                }
                this._object = new char[length];
                this._type = StorageType.CharArray;
                this._isNull = false;
                this.CharsLength = length;
            }
            else
            {
                if (destinationIndex > this.CharsLength)
                {
                    throw ADP.ArgumentOutOfRange("fieldOffset");
                }
                if (StorageType.String == this._type)
                {
                    this._object = ((string) this._object).ToCharArray();
                    this._type = StorageType.CharArray;
                }
                if ((destinationIndex + length) > this.CharsLength)
                {
                    int num2 = ((char[]) this._object).Length;
                    if ((destinationIndex + length) > num2)
                    {
                        char[] destinationArray = new char[Math.Max((int) (destinationIndex + length), (int) (2 * num2))];
                        Array.Copy((char[]) this._object, 0L, destinationArray, 0L, this.CharsLength);
                        this._object = destinationArray;
                    }
                    this.CharsLength = destinationIndex + length;
                }
            }
            Array.Copy(buffer, bufferOffset, (char[]) this._object, destinationIndex, length);
            return length;
        }

        internal void SetNull()
        {
            this._isNull = true;
        }

        internal bool Boolean
        {
            get
            {
                return this._value._boolean;
            }
            set
            {
                this._value._boolean = value;
                this._type = StorageType.Boolean;
                this._isNull = false;
            }
        }

        internal byte Byte
        {
            get
            {
                return this._value._byte;
            }
            set
            {
                this._value._byte = value;
                this._type = StorageType.Byte;
                this._isNull = false;
            }
        }

        internal long BytesLength
        {
            get
            {
                if (StorageType.String == this._type)
                {
                    this.ConvertXmlStringToByteArray();
                }
                return this._value._int64;
            }
            set
            {
                if (0L == value)
                {
                    this._value._int64 = value;
                    this._object = new byte[0];
                    this._type = StorageType.ByteArray;
                    this._isNull = false;
                }
                else
                {
                    this._value._int64 = value;
                }
            }
        }

        internal long CharsLength
        {
            get
            {
                return this._value._int64;
            }
            set
            {
                if (0L == value)
                {
                    this._value._int64 = value;
                    this._object = new char[0];
                    this._type = StorageType.CharArray;
                    this._isNull = false;
                }
                else
                {
                    this._value._int64 = value;
                }
            }
        }

        internal System.DateTime DateTime
        {
            get
            {
                return this._value._dateTime;
            }
            set
            {
                this._value._dateTime = value;
                this._type = StorageType.DateTime;
                this._isNull = false;
            }
        }

        internal System.DateTimeOffset DateTimeOffset
        {
            get
            {
                return this._value._dateTimeOffset;
            }
            set
            {
                this._value._dateTimeOffset = value;
                this._type = StorageType.DateTimeOffset;
                this._isNull = false;
            }
        }

        internal double Double
        {
            get
            {
                return this._value._double;
            }
            set
            {
                this._value._double = value;
                this._type = StorageType.Double;
                this._isNull = false;
            }
        }

        internal System.Guid Guid
        {
            get
            {
                return this._value._guid;
            }
            set
            {
                this._value._guid = value;
                this._type = StorageType.Guid;
                this._isNull = false;
            }
        }

        internal short Int16
        {
            get
            {
                return this._value._int16;
            }
            set
            {
                this._value._int16 = value;
                this._type = StorageType.Int16;
                this._isNull = false;
            }
        }

        internal int Int32
        {
            get
            {
                return this._value._int32;
            }
            set
            {
                this._value._int32 = value;
                this._type = StorageType.Int32;
                this._isNull = false;
            }
        }

        internal long Int64
        {
            get
            {
                return this._value._int64;
            }
            set
            {
                this._value._int64 = value;
                this._type = StorageType.Int64;
                this._isNull = false;
                if (this._isMetaSet)
                {
                    this._isMetaSet = false;
                }
                else
                {
                    this._metadata = null;
                }
            }
        }

        internal bool IsNull
        {
            get
            {
                return this._isNull;
            }
        }

        internal float Single
        {
            get
            {
                return this._value._single;
            }
            set
            {
                this._value._single = value;
                this._type = StorageType.Single;
                this._isNull = false;
            }
        }

        internal System.Data.SqlTypes.SqlDecimal SqlDecimal
        {
            get
            {
                return (System.Data.SqlTypes.SqlDecimal) this._object;
            }
            set
            {
                this._object = value;
                this._type = StorageType.SqlDecimal;
                this._isNull = false;
            }
        }

        internal string String
        {
            get
            {
                if (StorageType.String == this._type)
                {
                    return (string) this._object;
                }
                if (StorageType.CharArray == this._type)
                {
                    return new string((char[]) this._object, 0, (int) this.CharsLength);
                }
                Stream stream = new MemoryStream((byte[]) this._object, false);
                return new SqlXml(stream).Value;
            }
            set
            {
                this._object = value;
                this._value._int64 = value.Length;
                this._type = StorageType.String;
                this._isNull = false;
                if (this._isMetaSet)
                {
                    this._isMetaSet = false;
                }
                else
                {
                    this._metadata = null;
                }
            }
        }

        internal System.TimeSpan TimeSpan
        {
            get
            {
                return this._value._timeSpan;
            }
            set
            {
                this._value._timeSpan = value;
                this._type = StorageType.TimeSpan;
                this._isNull = false;
            }
        }

        internal SmiMetaData VariantType
        {
            get
            {
                switch (this._type)
                {
                    case StorageType.Boolean:
                        return SmiMetaData.DefaultBit;

                    case StorageType.Byte:
                        return SmiMetaData.DefaultTinyInt;

                    case StorageType.ByteArray:
                        return SmiMetaData.DefaultVarBinary;

                    case StorageType.CharArray:
                        return SmiMetaData.DefaultNVarChar;

                    case StorageType.DateTime:
                        return SmiMetaData.DefaultDateTime;

                    case StorageType.DateTimeOffset:
                        return SmiMetaData.DefaultDateTimeOffset;

                    case StorageType.Double:
                        return SmiMetaData.DefaultFloat;

                    case StorageType.Guid:
                        return SmiMetaData.DefaultUniqueIdentifier;

                    case StorageType.Int16:
                        return SmiMetaData.DefaultSmallInt;

                    case StorageType.Int32:
                        return SmiMetaData.DefaultInt;

                    case StorageType.Int64:
                        return (this._metadata ?? SmiMetaData.DefaultBigInt);

                    case StorageType.Single:
                        return SmiMetaData.DefaultReal;

                    case StorageType.String:
                        return (this._metadata ?? SmiMetaData.DefaultNVarChar);

                    case StorageType.SqlDecimal:
                    {
                        System.Data.SqlTypes.SqlDecimal num2 = (System.Data.SqlTypes.SqlDecimal) this._object;
                        System.Data.SqlTypes.SqlDecimal num = (System.Data.SqlTypes.SqlDecimal) this._object;
                        return new SmiMetaData(SqlDbType.Decimal, 0x11L, num2.Precision, num.Scale, 0L, SqlCompareOptions.None, null);
                    }
                    case StorageType.TimeSpan:
                        return SmiMetaData.DefaultTime;
                }
                return null;
            }
            set
            {
                this._metadata = value;
                this._isMetaSet = true;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Storage
        {
            [FieldOffset(0)]
            internal bool _boolean;
            [FieldOffset(0)]
            internal byte _byte;
            [FieldOffset(0)]
            internal DateTime _dateTime;
            [FieldOffset(0)]
            internal DateTimeOffset _dateTimeOffset;
            [FieldOffset(0)]
            internal double _double;
            [FieldOffset(0)]
            internal Guid _guid;
            [FieldOffset(0)]
            internal short _int16;
            [FieldOffset(0)]
            internal int _int32;
            [FieldOffset(0)]
            internal long _int64;
            [FieldOffset(0)]
            internal float _single;
            [FieldOffset(0)]
            internal TimeSpan _timeSpan;
        }

        internal enum StorageType
        {
            Boolean,
            Byte,
            ByteArray,
            CharArray,
            DateTime,
            DateTimeOffset,
            Double,
            Guid,
            Int16,
            Int32,
            Int64,
            Single,
            String,
            SqlDecimal,
            TimeSpan
        }
    }
}

