namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.InteropServices;

    internal sealed class CNativeBuffer : DbBuffer
    {
        internal CNativeBuffer(int initialSize) : base(initialSize)
        {
        }

        internal object MarshalToManaged(int offset, ODBC32.SQL_C sqlctype, int cb)
        {
            switch (sqlctype)
            {
                case ODBC32.SQL_C.SLONG:
                    return base.ReadInt32(offset);

                case ODBC32.SQL_C.SSHORT:
                    return base.ReadInt16(offset);

                case ODBC32.SQL_C.SBIGINT:
                    return base.ReadInt64(offset);

                case ODBC32.SQL_C.UTINYINT:
                    return base.ReadByte(offset);

                case ODBC32.SQL_C.GUID:
                    return base.ReadGuid(offset);

                case ODBC32.SQL_C.WCHAR:
                    if (cb != -3)
                    {
                        cb = Math.Min((int) (cb / 2), (int) ((base.Length - 2) / 2));
                        return base.PtrToStringUni(offset, cb);
                    }
                    return base.PtrToStringUni(offset);

                case ODBC32.SQL_C.BIT:
                    return (base.ReadByte(offset) != 0);

                case ODBC32.SQL_C.BINARY:
                case ODBC32.SQL_C.CHAR:
                    cb = Math.Min(cb, base.Length);
                    return base.ReadBytes(offset, cb);

                case ODBC32.SQL_C.NUMERIC:
                    return base.ReadNumeric(offset);

                case ODBC32.SQL_C.REAL:
                    return base.ReadSingle(offset);

                case ODBC32.SQL_C.DOUBLE:
                    return base.ReadDouble(offset);

                case ODBC32.SQL_C.TYPE_DATE:
                    return base.ReadDate(offset);

                case ODBC32.SQL_C.TYPE_TIME:
                    return base.ReadTime(offset);

                case ODBC32.SQL_C.TYPE_TIMESTAMP:
                    return base.ReadDateTime(offset);
            }
            return null;
        }

        internal void MarshalToNative(int offset, object value, ODBC32.SQL_C sqlctype, int sizeorprecision, int valueOffset)
        {
            ODBC32.SQL_C sql_c = sqlctype;
            if (sql_c <= ODBC32.SQL_C.SSHORT)
            {
                if (sql_c != ODBC32.SQL_C.UTINYINT)
                {
                    switch (sql_c)
                    {
                        case ODBC32.SQL_C.SLONG:
                            base.WriteInt32(offset, (int) value);
                            return;

                        case ODBC32.SQL_C.SSHORT:
                            base.WriteInt16(offset, (short) value);
                            return;

                        case ODBC32.SQL_C.SBIGINT:
                            base.WriteInt64(offset, (long) value);
                            return;
                    }
                }
                else
                {
                    base.WriteByte(offset, (byte) value);
                }
            }
            else if (sql_c <= ODBC32.SQL_C.NUMERIC)
            {
                switch (sql_c)
                {
                    case ODBC32.SQL_C.GUID:
                        base.WriteGuid(offset, (Guid) value);
                        return;

                    case ~(ODBC32.SQL_C.CHAR | ODBC32.SQL_C.DOUBLE):
                    case ~ODBC32.SQL_C.DOUBLE:
                    case ((ODBC32.SQL_C) (-1)):
                    case ((ODBC32.SQL_C) 0):
                        return;

                    case ODBC32.SQL_C.WCHAR:
                        int num;
                        char[] chArray;
                        if (value is string)
                        {
                            num = Math.Max(0, ((string) value).Length - valueOffset);
                            if ((sizeorprecision > 0) && (sizeorprecision < num))
                            {
                                num = sizeorprecision;
                            }
                            chArray = ((string) value).ToCharArray(valueOffset, num);
                            base.WriteCharArray(offset, chArray, 0, chArray.Length);
                            base.WriteInt16(offset + (chArray.Length * 2), 0);
                            return;
                        }
                        num = Math.Max(0, ((char[]) value).Length - valueOffset);
                        if ((sizeorprecision > 0) && (sizeorprecision < num))
                        {
                            num = sizeorprecision;
                        }
                        chArray = (char[]) value;
                        base.WriteCharArray(offset, chArray, valueOffset, num);
                        base.WriteInt16(offset + (chArray.Length * 2), 0);
                        return;

                    case ODBC32.SQL_C.BIT:
                        base.WriteByte(offset, ((bool) value) ? ((byte) 1) : ((byte) 0));
                        return;

                    case ODBC32.SQL_C.BINARY:
                    case ODBC32.SQL_C.CHAR:
                    {
                        byte[] source = (byte[]) value;
                        int length = source.Length;
                        length -= valueOffset;
                        if ((sizeorprecision > 0) && (sizeorprecision < length))
                        {
                            length = sizeorprecision;
                        }
                        base.WriteBytes(offset, source, valueOffset, length);
                        return;
                    }
                    case ODBC32.SQL_C.NUMERIC:
                        base.WriteNumeric(offset, (decimal) value, (byte) sizeorprecision);
                        return;
                }
            }
            else
            {
                switch (sql_c)
                {
                    case ODBC32.SQL_C.REAL:
                        base.WriteSingle(offset, (float) value);
                        return;

                    case ODBC32.SQL_C.DOUBLE:
                        base.WriteDouble(offset, (double) value);
                        return;

                    case ODBC32.SQL_C.TYPE_DATE:
                        base.WriteDate(offset, (DateTime) value);
                        return;

                    case ODBC32.SQL_C.TYPE_TIME:
                        base.WriteTime(offset, (TimeSpan) value);
                        return;

                    case ODBC32.SQL_C.TYPE_TIMESTAMP:
                        this.WriteODBCDateTime(offset, (DateTime) value);
                        return;

                    default:
                        return;
                }
            }
        }

        internal HandleRef PtrOffset(int offset, int length)
        {
            base.Validate(offset, length);
            return new HandleRef(this, ADP.IntPtrOffset(base.DangerousGetHandle(), offset));
        }

        internal void WriteODBCDateTime(int offset, DateTime value)
        {
            short[] source = new short[] { (short) value.Year, (short) value.Month, (short) value.Day, (short) value.Hour, (short) value.Minute, (short) value.Second };
            base.WriteInt16Array(offset, source, 0, 6);
            base.WriteInt32(offset + 12, value.Millisecond * 0xf4240);
        }

        internal short ShortLength
        {
            get
            {
                return (short) base.Length;
            }
        }
    }
}

