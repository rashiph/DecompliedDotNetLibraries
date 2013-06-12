namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class ColumnBinding
    {
        private readonly System.Data.OleDb.Bindings _bindings;
        private readonly OleDbDataReader _dataReader;
        private readonly bool _ifIRowsetElseIRow;
        private readonly int _index;
        private readonly int _indexForAccessor;
        private readonly int _indexWithinAccessor;
        private readonly int _maxLen;
        private readonly int _offsetLength;
        private readonly int _offsetStatus;
        private readonly int _offsetValue;
        private readonly int _ordinal;
        private readonly OleDbParameter _parameter;
        private readonly int _parameterChangeID;
        private GCHandle _pinnedBuffer;
        private readonly byte _precision;
        private readonly System.Data.OleDb.RowBinding _rowbinding;
        internal StringMemHandle _sptr;
        private object _value;
        private int _valueBindingOffset;
        private int _valueBindingSize;
        private readonly short _wType;

        internal ColumnBinding(OleDbDataReader dataReader, int index, int indexForAccessor, int indexWithinAccessor, OleDbParameter parameter, System.Data.OleDb.RowBinding rowbinding, System.Data.OleDb.Bindings bindings, tagDBBINDING binding, int offset, bool ifIRowsetElseIRow)
        {
            this._dataReader = dataReader;
            this._rowbinding = rowbinding;
            this._bindings = bindings;
            this._index = index;
            this._indexForAccessor = indexForAccessor;
            this._indexWithinAccessor = indexWithinAccessor;
            if (parameter != null)
            {
                this._parameter = parameter;
                this._parameterChangeID = parameter.ChangeID;
            }
            this._offsetStatus = binding.obStatus.ToInt32() + offset;
            this._offsetLength = binding.obLength.ToInt32() + offset;
            this._offsetValue = binding.obValue.ToInt32() + offset;
            this._ordinal = binding.iOrdinal.ToInt32();
            this._maxLen = binding.cbMaxLen.ToInt32();
            this._wType = binding.wType;
            this._precision = binding.bPrecision;
            this._ifIRowsetElseIRow = ifIRowsetElseIRow;
            this.SetSize(this.Bindings.ParamSize.ToInt32());
        }

        private Exception CheckTypeValueStatusValue()
        {
            return this.CheckTypeValueStatusValue(this.ExpectedType);
        }

        private Exception CheckTypeValueStatusValue(Type expectedType)
        {
            switch (this.StatusValue())
            {
                case DBStatus.S_OK:
                case DBStatus.E_CANTCONVERTVALUE:
                case DBStatus.S_TRUNCATED:
                    return ODB.CantConvertValue();

                case DBStatus.E_BADACCESSOR:
                    return ODB.BadAccessor();

                case DBStatus.S_ISNULL:
                    return ADP.InvalidCast();

                case DBStatus.E_SIGNMISMATCH:
                    return ODB.SignMismatch(expectedType);

                case DBStatus.E_DATAOVERFLOW:
                    return ODB.DataOverflow(expectedType);

                case DBStatus.E_CANTCREATE:
                    return ODB.CantCreate(expectedType);

                case DBStatus.E_UNAVAILABLE:
                    return ODB.Unavailable(expectedType);
            }
            return ODB.UnexpectedStatusValue(this.StatusValue());
        }

        private OleDbDataReader DataReader()
        {
            return this._dataReader;
        }

        internal bool IsParameterBindingInvalid(OleDbParameter parameter)
        {
            if (this._parameter.ChangeID == this._parameterChangeID)
            {
                return (this._parameter != parameter);
            }
            return true;
        }

        internal bool IsValueNull()
        {
            if (DBStatus.S_ISNULL == this.StatusValue())
            {
                return true;
            }
            if ((12 != this.DbType) && (0x8a != this.DbType))
            {
                return false;
            }
            return Convert.IsDBNull(this.ValueVariant());
        }

        private int LengthValue()
        {
            int num;
            if (this._ifIRowsetElseIRow)
            {
                num = this.RowBinding.ReadIntPtr(this._offsetLength).ToInt32();
            }
            else
            {
                num = this.Bindings.DBColumnAccess[this.IndexWithinAccessor].cbDataLen.ToInt32();
            }
            return Math.Max(num, 0);
        }

        private void LengthValue(int value)
        {
            this.RowBinding.WriteIntPtr(this._offsetLength, (IntPtr) value);
        }

        internal OleDbParameter Parameter()
        {
            return this._parameter;
        }

        internal void ResetValue()
        {
            this._value = null;
            StringMemHandle handle = this._sptr;
            this._sptr = null;
            if (handle != null)
            {
                handle.Dispose();
            }
            if (this._pinnedBuffer.IsAllocated)
            {
                this._pinnedBuffer.Free();
            }
        }

        internal void SetOffset(int offset)
        {
            if (0 > offset)
            {
                throw ADP.InvalidOffsetValue(offset);
            }
            this._valueBindingOffset = Math.Max(offset, 0);
        }

        internal void SetSize(int size)
        {
            this._valueBindingSize = Math.Max(size, 0);
        }

        private void SetValueDBNull()
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_ISNULL);
            this.RowBinding.WriteInt64(this.ValueOffset, 0L);
        }

        private void SetValueEmpty()
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_DEFAULT);
            this.RowBinding.WriteInt64(this.ValueOffset, 0L);
        }

        internal DBStatus StatusValue()
        {
            if (this._ifIRowsetElseIRow)
            {
                return (DBStatus) this.RowBinding.ReadInt32(this._offsetStatus);
            }
            return (DBStatus) this.Bindings.DBColumnAccess[this.IndexWithinAccessor].dwStatus;
        }

        internal void StatusValue(DBStatus value)
        {
            this.RowBinding.WriteInt32(this._offsetStatus, (int) value);
        }

        internal object Value()
        {
            object obj2 = this._value;
            if (obj2 != null)
            {
                return obj2;
            }
            switch (this.StatusValue())
            {
                case DBStatus.S_OK:
                {
                    short dbType = this.DbType;
                    if (dbType > 0x40)
                    {
                        switch (dbType)
                        {
                            case 0x80:
                                obj2 = this.Value_BYTES();
                                goto Label_0381;

                            case 130:
                                obj2 = this.Value_WSTR();
                                goto Label_0381;

                            case 0x83:
                                obj2 = this.Value_NUMERIC();
                                goto Label_0381;

                            case 0x85:
                                obj2 = this.Value_DBDATE();
                                goto Label_0381;

                            case 0x86:
                                obj2 = this.Value_DBTIME();
                                goto Label_0381;

                            case 0x87:
                                obj2 = this.Value_DBTIMESTAMP();
                                goto Label_0381;

                            case 0x88:
                                obj2 = this.Value_HCHAPTER();
                                goto Label_0381;

                            case 0x8a:
                                obj2 = this.Value_VARIANT();
                                goto Label_0381;

                            case 0x48:
                                obj2 = this.Value_GUID();
                                goto Label_0381;

                            case 0x4080:
                                obj2 = this.Value_ByRefBYTES();
                                goto Label_0381;

                            case 0x4082:
                                obj2 = this.Value_ByRefWSTR();
                                goto Label_0381;
                        }
                        break;
                    }
                    switch (dbType)
                    {
                        case 0:
                        case 1:
                            obj2 = DBNull.Value;
                            goto Label_0381;

                        case 2:
                            obj2 = this.Value_I2();
                            goto Label_0381;

                        case 3:
                            obj2 = this.Value_I4();
                            goto Label_0381;

                        case 4:
                            obj2 = this.Value_R4();
                            goto Label_0381;

                        case 5:
                            obj2 = this.Value_R8();
                            goto Label_0381;

                        case 6:
                            obj2 = this.Value_CY();
                            goto Label_0381;

                        case 7:
                            obj2 = this.Value_DATE();
                            goto Label_0381;

                        case 8:
                            obj2 = this.Value_BSTR();
                            goto Label_0381;

                        case 9:
                            obj2 = this.Value_IDISPATCH();
                            goto Label_0381;

                        case 10:
                            obj2 = this.Value_ERROR();
                            goto Label_0381;

                        case 11:
                            obj2 = this.Value_BOOL();
                            goto Label_0381;

                        case 12:
                            obj2 = this.Value_VARIANT();
                            goto Label_0381;

                        case 13:
                            obj2 = this.Value_IUNKNOWN();
                            goto Label_0381;

                        case 14:
                            obj2 = this.Value_DECIMAL();
                            goto Label_0381;

                        case 0x10:
                            obj2 = this.Value_I1();
                            goto Label_0381;

                        case 0x11:
                            obj2 = this.Value_UI1();
                            goto Label_0381;

                        case 0x12:
                            obj2 = this.Value_UI2();
                            goto Label_0381;

                        case 0x13:
                            obj2 = this.Value_UI4();
                            goto Label_0381;

                        case 20:
                            obj2 = this.Value_I8();
                            goto Label_0381;

                        case 0x15:
                            obj2 = this.Value_UI8();
                            goto Label_0381;

                        case 0x40:
                            obj2 = this.Value_FILETIME();
                            goto Label_0381;
                    }
                    break;
                }
                case DBStatus.S_ISNULL:
                case DBStatus.S_DEFAULT:
                    obj2 = DBNull.Value;
                    goto Label_0381;

                case DBStatus.S_TRUNCATED:
                    switch (this.DbType)
                    {
                        case 0x80:
                            obj2 = this.Value_BYTES();
                            goto Label_0381;

                        case 130:
                            obj2 = this.Value_WSTR();
                            goto Label_0381;

                        case 0x4080:
                            obj2 = this.Value_ByRefBYTES();
                            goto Label_0381;

                        case 0x4082:
                            obj2 = this.Value_ByRefWSTR();
                            goto Label_0381;
                    }
                    throw ODB.GVtUnknown(this.DbType);

                default:
                    throw this.CheckTypeValueStatusValue();
            }
            throw ODB.GVtUnknown(this.DbType);
        Label_0381:
            this._value = obj2;
            return obj2;
        }

        internal void Value(object value)
        {
            if (value == null)
            {
                this.SetValueEmpty();
            }
            else if (Convert.IsDBNull(value))
            {
                this.SetValueDBNull();
            }
            else
            {
                switch (this.DbType)
                {
                    case 0:
                        this.SetValueEmpty();
                        return;

                    case 1:
                        this.SetValueDBNull();
                        return;

                    case 2:
                        this.Value_I2((short) value);
                        return;

                    case 3:
                        this.Value_I4((int) value);
                        return;

                    case 4:
                        this.Value_R4((float) value);
                        return;

                    case 5:
                        this.Value_R8((double) value);
                        return;

                    case 6:
                        this.Value_CY((decimal) value);
                        return;

                    case 7:
                        this.Value_DATE((DateTime) value);
                        return;

                    case 8:
                        this.Value_BSTR((string) value);
                        return;

                    case 9:
                        this.Value_IDISPATCH(value);
                        return;

                    case 10:
                        this.Value_ERROR((int) value);
                        return;

                    case 11:
                        this.Value_BOOL((bool) value);
                        return;

                    case 12:
                        this.Value_VARIANT(value);
                        return;

                    case 13:
                        this.Value_IUNKNOWN(value);
                        return;

                    case 14:
                        this.Value_DECIMAL((decimal) value);
                        return;

                    case 0x10:
                        if (!(value is short))
                        {
                            this.Value_I1((sbyte) value);
                            return;
                        }
                        this.Value_I1(Convert.ToSByte((short) value, CultureInfo.InvariantCulture));
                        return;

                    case 0x11:
                        this.Value_UI1((byte) value);
                        return;

                    case 0x12:
                        if (!(value is int))
                        {
                            this.Value_UI2((ushort) value);
                            return;
                        }
                        this.Value_UI2(Convert.ToUInt16((int) value, CultureInfo.InvariantCulture));
                        return;

                    case 0x13:
                        if (!(value is long))
                        {
                            this.Value_UI4((uint) value);
                            return;
                        }
                        this.Value_UI4(Convert.ToUInt32((long) value, CultureInfo.InvariantCulture));
                        return;

                    case 20:
                        this.Value_I8((long) value);
                        return;

                    case 0x15:
                        if (!(value is decimal))
                        {
                            this.Value_UI8((ulong) value);
                            return;
                        }
                        this.Value_UI8(Convert.ToUInt64((decimal) value, CultureInfo.InvariantCulture));
                        return;

                    case 0x40:
                        this.Value_FILETIME((DateTime) value);
                        return;

                    case 0x80:
                        this.Value_BYTES((byte[]) value);
                        return;

                    case 130:
                        if (!(value is string))
                        {
                            this.Value_WSTR((char[]) value);
                            return;
                        }
                        this.Value_WSTR((string) value);
                        return;

                    case 0x83:
                        this.Value_NUMERIC((decimal) value);
                        return;

                    case 0x85:
                        this.Value_DBDATE((DateTime) value);
                        return;

                    case 0x86:
                        this.Value_DBTIME((TimeSpan) value);
                        return;

                    case 0x87:
                        this.Value_DBTIMESTAMP((DateTime) value);
                        return;

                    case 0x8a:
                        this.Value_VARIANT(value);
                        return;

                    case 0x48:
                        this.Value_GUID((Guid) value);
                        return;

                    case 0x4080:
                        this.Value_ByRefBYTES((byte[]) value);
                        return;

                    case 0x4082:
                        if (!(value is string))
                        {
                            this.Value_ByRefWSTR((char[]) value);
                            return;
                        }
                        this.Value_ByRefWSTR((string) value);
                        return;
                }
                throw ODB.SVtUnknown(this.DbType);
            }
        }

        internal bool Value_BOOL()
        {
            short num = this.RowBinding.ReadInt16(this.ValueOffset);
            return (0 != num);
        }

        private void Value_BOOL(bool value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt16(this.ValueOffset, value ? ((short) (-1)) : ((short) 0));
        }

        private string Value_BSTR()
        {
            string str = "";
            System.Data.OleDb.RowBinding rowBinding = this.RowBinding;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                rowBinding.DangerousAddRef(ref success);
                IntPtr ptr = rowBinding.ReadIntPtr(this.ValueOffset);
                if (ADP.PtrZero != ptr)
                {
                    str = Marshal.PtrToStringBSTR(ptr);
                }
            }
            finally
            {
                if (success)
                {
                    rowBinding.DangerousRelease();
                }
            }
            return str;
        }

        private void Value_BSTR(string value)
        {
            this.LengthValue(value.Length * 2);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.SetBstrValue(this.ValueOffset, value);
        }

        private byte[] Value_ByRefBYTES()
        {
            byte[] destination = null;
            System.Data.OleDb.RowBinding rowBinding = this.RowBinding;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                rowBinding.DangerousAddRef(ref success);
                IntPtr source = rowBinding.ReadIntPtr(this.ValueOffset);
                if (ADP.PtrZero != source)
                {
                    destination = new byte[this.LengthValue()];
                    Marshal.Copy(source, destination, 0, destination.Length);
                }
            }
            finally
            {
                if (success)
                {
                    rowBinding.DangerousRelease();
                }
            }
            if (destination == null)
            {
                return new byte[0];
            }
            return destination;
        }

        private void Value_ByRefBYTES(byte[] value)
        {
            int num = (this.ValueBindingOffset < value.Length) ? (value.Length - this.ValueBindingOffset) : 0;
            this.LengthValue((0 < this.ValueBindingSize) ? Math.Min(this.ValueBindingSize, num) : num);
            this.StatusValue(DBStatus.S_OK);
            IntPtr ptrZero = ADP.PtrZero;
            if (0 < num)
            {
                this._pinnedBuffer = GCHandle.Alloc(value, GCHandleType.Pinned);
                ptrZero = ADP.IntPtrOffset(this._pinnedBuffer.AddrOfPinnedObject(), this.ValueBindingOffset);
            }
            this.RowBinding.SetByRefValue(this.ValueOffset, ptrZero);
        }

        private string Value_ByRefWSTR()
        {
            string str = "";
            System.Data.OleDb.RowBinding rowBinding = this.RowBinding;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                rowBinding.DangerousAddRef(ref success);
                IntPtr ptr = rowBinding.ReadIntPtr(this.ValueOffset);
                if (ADP.PtrZero != ptr)
                {
                    int len = this.LengthValue() / 2;
                    str = Marshal.PtrToStringUni(ptr, len);
                }
            }
            finally
            {
                if (success)
                {
                    rowBinding.DangerousRelease();
                }
            }
            return str;
        }

        private void Value_ByRefWSTR(string value)
        {
            int num = (this.ValueBindingOffset < value.Length) ? (value.Length - this.ValueBindingOffset) : 0;
            this.LengthValue(((0 < this.ValueBindingSize) ? Math.Min(this.ValueBindingSize, num) : num) * 2);
            this.StatusValue(DBStatus.S_OK);
            IntPtr ptrZero = ADP.PtrZero;
            if (0 < num)
            {
                this._pinnedBuffer = GCHandle.Alloc(value, GCHandleType.Pinned);
                ptrZero = ADP.IntPtrOffset(this._pinnedBuffer.AddrOfPinnedObject(), this.ValueBindingOffset);
            }
            this.RowBinding.SetByRefValue(this.ValueOffset, ptrZero);
        }

        private void Value_ByRefWSTR(char[] value)
        {
            int num = (this.ValueBindingOffset < value.Length) ? (value.Length - this.ValueBindingOffset) : 0;
            this.LengthValue(((0 < this.ValueBindingSize) ? Math.Min(this.ValueBindingSize, num) : num) * 2);
            this.StatusValue(DBStatus.S_OK);
            IntPtr ptrZero = ADP.PtrZero;
            if (0 < num)
            {
                this._pinnedBuffer = GCHandle.Alloc(value, GCHandleType.Pinned);
                ptrZero = ADP.IntPtrOffset(this._pinnedBuffer.AddrOfPinnedObject(), this.ValueBindingOffset);
            }
            this.RowBinding.SetByRefValue(this.ValueOffset, ptrZero);
        }

        private byte[] Value_BYTES()
        {
            int length = Math.Min(this.LengthValue(), this.ColumnBindingMaxLen);
            byte[] destination = new byte[length];
            this.RowBinding.ReadBytes(this.ValueOffset, destination, 0, length);
            return destination;
        }

        private void Value_BYTES(byte[] value)
        {
            int num = (this.ValueBindingOffset < value.Length) ? Math.Min(value.Length - this.ValueBindingOffset, this.ColumnBindingMaxLen) : 0;
            this.LengthValue(num);
            this.StatusValue(DBStatus.S_OK);
            if (0 < num)
            {
                this.RowBinding.WriteBytes(this.ValueOffset, value, this.ValueBindingOffset, num);
            }
        }

        private decimal Value_CY()
        {
            return decimal.FromOACurrency(this.RowBinding.ReadInt64(this.ValueOffset));
        }

        private void Value_CY(decimal value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt64(this.ValueOffset, decimal.ToOACurrency(value));
        }

        private DateTime Value_DATE()
        {
            return DateTime.FromOADate(this.RowBinding.ReadDouble(this.ValueOffset));
        }

        private void Value_DATE(DateTime value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteDouble(this.ValueOffset, value.ToOADate());
        }

        private DateTime Value_DBDATE()
        {
            return this.RowBinding.ReadDate(this.ValueOffset);
        }

        private void Value_DBDATE(DateTime value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteDate(this.ValueOffset, value);
        }

        private TimeSpan Value_DBTIME()
        {
            return this.RowBinding.ReadTime(this.ValueOffset);
        }

        private void Value_DBTIME(TimeSpan value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteTime(this.ValueOffset, value);
        }

        private DateTime Value_DBTIMESTAMP()
        {
            return this.RowBinding.ReadDateTime(this.ValueOffset);
        }

        private void Value_DBTIMESTAMP(DateTime value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteDateTime(this.ValueOffset, value);
        }

        private decimal Value_DECIMAL()
        {
            int[] destination = new int[4];
            this.RowBinding.ReadInt32Array(this.ValueOffset, destination, 0, 4);
            return new decimal(destination[2], destination[3], destination[1], 0 != (destination[0] & -2147483648), (byte) ((destination[0] & 0xff0000) >> 0x10));
        }

        private void Value_DECIMAL(decimal value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            int[] bits = decimal.GetBits(value);
            int[] source = new int[] { bits[3], bits[2], bits[0], bits[1] };
            this.RowBinding.WriteInt32Array(this.ValueOffset, source, 0, 4);
        }

        private int Value_ERROR()
        {
            return this.RowBinding.ReadInt32(this.ValueOffset);
        }

        private void Value_ERROR(int value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt32(this.ValueOffset, value);
        }

        private DateTime Value_FILETIME()
        {
            return DateTime.FromFileTime(this.RowBinding.ReadInt64(this.ValueOffset));
        }

        private void Value_FILETIME(DateTime value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            long num = value.ToFileTime();
            this.RowBinding.WriteInt64(this.ValueOffset, num);
        }

        internal Guid Value_GUID()
        {
            return this.RowBinding.ReadGuid(this.ValueOffset);
        }

        private void Value_GUID(Guid value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteGuid(this.ValueOffset, value);
        }

        internal OleDbDataReader Value_HCHAPTER()
        {
            return this.DataReader().ResetChapter(this.IndexForAccessor, this.IndexWithinAccessor, this.RowBinding, this.ValueOffset);
        }

        private sbyte Value_I1()
        {
            return (sbyte) this.RowBinding.ReadByte(this.ValueOffset);
        }

        private void Value_I1(sbyte value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteByte(this.ValueOffset, (byte) value);
        }

        internal short Value_I2()
        {
            return this.RowBinding.ReadInt16(this.ValueOffset);
        }

        private void Value_I2(short value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt16(this.ValueOffset, value);
        }

        private int Value_I4()
        {
            return this.RowBinding.ReadInt32(this.ValueOffset);
        }

        private void Value_I4(int value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt32(this.ValueOffset, value);
        }

        private long Value_I8()
        {
            return this.RowBinding.ReadInt64(this.ValueOffset);
        }

        private void Value_I8(long value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt64(this.ValueOffset, value);
        }

        private object Value_IDISPATCH()
        {
            object objectForIUnknown;
            System.Data.OleDb.RowBinding rowBinding = this.RowBinding;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                rowBinding.DangerousAddRef(ref success);
                objectForIUnknown = Marshal.GetObjectForIUnknown(rowBinding.ReadIntPtr(this.ValueOffset));
            }
            finally
            {
                if (success)
                {
                    rowBinding.DangerousRelease();
                }
            }
            return objectForIUnknown;
        }

        private void Value_IDISPATCH(object value)
        {
            new NamedPermissionSet("FullTrust").Demand();
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            IntPtr iDispatchForObject = Marshal.GetIDispatchForObject(value);
            this.RowBinding.WriteIntPtr(this.ValueOffset, iDispatchForObject);
        }

        private object Value_IUNKNOWN()
        {
            object objectForIUnknown;
            System.Data.OleDb.RowBinding rowBinding = this.RowBinding;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                rowBinding.DangerousAddRef(ref success);
                objectForIUnknown = Marshal.GetObjectForIUnknown(rowBinding.ReadIntPtr(this.ValueOffset));
            }
            finally
            {
                if (success)
                {
                    rowBinding.DangerousRelease();
                }
            }
            return objectForIUnknown;
        }

        private void Value_IUNKNOWN(object value)
        {
            new NamedPermissionSet("FullTrust").Demand();
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(value);
            this.RowBinding.WriteIntPtr(this.ValueOffset, iUnknownForObject);
        }

        private decimal Value_NUMERIC()
        {
            return this.RowBinding.ReadNumeric(this.ValueOffset);
        }

        private void Value_NUMERIC(decimal value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteNumeric(this.ValueOffset, value, this.ColumnBindingPrecision);
        }

        private float Value_R4()
        {
            return this.RowBinding.ReadSingle(this.ValueOffset);
        }

        private void Value_R4(float value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteSingle(this.ValueOffset, value);
        }

        private double Value_R8()
        {
            return this.RowBinding.ReadDouble(this.ValueOffset);
        }

        private void Value_R8(double value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteDouble(this.ValueOffset, value);
        }

        private byte Value_UI1()
        {
            return this.RowBinding.ReadByte(this.ValueOffset);
        }

        private void Value_UI1(byte value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteByte(this.ValueOffset, value);
        }

        internal ushort Value_UI2()
        {
            return (ushort) this.RowBinding.ReadInt16(this.ValueOffset);
        }

        private void Value_UI2(ushort value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt16(this.ValueOffset, (short) value);
        }

        internal uint Value_UI4()
        {
            return (uint) this.RowBinding.ReadInt32(this.ValueOffset);
        }

        private void Value_UI4(uint value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt32(this.ValueOffset, (int) value);
        }

        internal ulong Value_UI8()
        {
            return (ulong) this.RowBinding.ReadInt64(this.ValueOffset);
        }

        private void Value_UI8(ulong value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.WriteInt64(this.ValueOffset, (long) value);
        }

        private object Value_VARIANT()
        {
            return this.RowBinding.GetVariantValue(this.ValueOffset);
        }

        private void Value_VARIANT(object value)
        {
            this.LengthValue(0);
            this.StatusValue(DBStatus.S_OK);
            this.RowBinding.SetVariantValue(this.ValueOffset, value);
        }

        private string Value_WSTR()
        {
            int num = Math.Min(this.LengthValue(), this.ColumnBindingMaxLen - 2);
            return this.RowBinding.PtrToStringUni(this.ValueOffset, num / 2);
        }

        private void Value_WSTR(string value)
        {
            int length = (this.ValueBindingOffset < value.Length) ? Math.Min((int) (value.Length - this.ValueBindingOffset), (int) ((this.ColumnBindingMaxLen - 2) / 2)) : 0;
            this.LengthValue(length * 2);
            this.StatusValue(DBStatus.S_OK);
            if (0 < length)
            {
                char[] source = value.ToCharArray(this.ValueBindingOffset, length);
                this.RowBinding.WriteCharArray(this.ValueOffset, source, this.ValueBindingOffset, length);
            }
        }

        private void Value_WSTR(char[] value)
        {
            int length = (this.ValueBindingOffset < value.Length) ? Math.Min((int) (value.Length - this.ValueBindingOffset), (int) ((this.ColumnBindingMaxLen - 2) / 2)) : 0;
            this.LengthValue(length * 2);
            this.StatusValue(DBStatus.S_OK);
            if (0 < length)
            {
                this.RowBinding.WriteCharArray(this.ValueOffset, value, this.ValueBindingOffset, length);
            }
        }

        internal bool ValueBoolean()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(bool));
            }
            switch (this.DbType)
            {
                case 11:
                    return this.Value_BOOL();

                case 12:
                    return (bool) this.ValueVariant();
            }
            throw ODB.ConversionRequired();
        }

        internal byte ValueByte()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(byte));
            }
            short dbType = this.DbType;
            if (dbType == 12)
            {
                return (byte) this.ValueVariant();
            }
            if (dbType != 0x11)
            {
                throw ODB.ConversionRequired();
            }
            return this.Value_UI1();
        }

        internal byte[] ValueByteArray()
        {
            byte[] buffer = (byte[]) this._value;
            if (buffer != null)
            {
                return buffer;
            }
            DBStatus status = this.StatusValue();
            if (status != DBStatus.S_OK)
            {
                if (status != DBStatus.S_TRUNCATED)
                {
                    throw this.CheckTypeValueStatusValue(typeof(byte[]));
                }
            }
            else
            {
                switch (this.DbType)
                {
                    case 12:
                        buffer = (byte[]) this.ValueVariant();
                        goto Label_00AC;

                    case 0x80:
                        buffer = this.Value_BYTES();
                        goto Label_00AC;

                    case 0x4080:
                        buffer = this.Value_ByRefBYTES();
                        goto Label_00AC;
                }
                throw ODB.ConversionRequired();
            }
            switch (this.DbType)
            {
                case 0x80:
                    buffer = this.Value_BYTES();
                    break;

                case 0x4080:
                    buffer = this.Value_ByRefBYTES();
                    break;

                default:
                    throw ODB.ConversionRequired();
            }
        Label_00AC:
            this._value = buffer;
            return buffer;
        }

        internal OleDbDataReader ValueChapter()
        {
            OleDbDataReader reader = (OleDbDataReader) this._value;
            if (reader == null)
            {
                if (this.StatusValue() != DBStatus.S_OK)
                {
                    throw this.CheckTypeValueStatusValue(typeof(string));
                }
                if (this.DbType != 0x88)
                {
                    throw ODB.ConversionRequired();
                }
                reader = this.Value_HCHAPTER();
                this._value = reader;
            }
            return reader;
        }

        internal DateTime ValueDateTime()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(short));
            }
            switch (this.DbType)
            {
                case 0x85:
                    return this.Value_DBDATE();

                case 0x87:
                    return this.Value_DBTIMESTAMP();

                case 0x40:
                    return this.Value_FILETIME();

                case 7:
                    return this.Value_DATE();

                case 12:
                    return (DateTime) this.ValueVariant();
            }
            throw ODB.ConversionRequired();
        }

        internal decimal ValueDecimal()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(short));
            }
            switch (this.DbType)
            {
                case 0x15:
                    return this.Value_UI8();

                case 0x83:
                    return this.Value_NUMERIC();

                case 12:
                    return (decimal) this.ValueVariant();

                case 14:
                    return this.Value_DECIMAL();

                case 6:
                    return this.Value_CY();
            }
            throw ODB.ConversionRequired();
        }

        internal double ValueDouble()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(double));
            }
            switch (this.DbType)
            {
                case 5:
                    return this.Value_R8();

                case 12:
                    return (double) this.ValueVariant();
            }
            throw ODB.ConversionRequired();
        }

        internal Guid ValueGuid()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(short));
            }
            if (this.DbType != 0x48)
            {
                throw ODB.ConversionRequired();
            }
            return this.Value_GUID();
        }

        internal short ValueInt16()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(short));
            }
            switch (this.DbType)
            {
                case 2:
                    return this.Value_I2();

                case 12:
                {
                    object obj2 = this.ValueVariant();
                    if (obj2 is sbyte)
                    {
                        return (sbyte) obj2;
                    }
                    return (short) obj2;
                }
                case 0x10:
                    return this.Value_I1();
            }
            throw ODB.ConversionRequired();
        }

        internal int ValueInt32()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(int));
            }
            switch (this.DbType)
            {
                case 3:
                    return this.Value_I4();

                case 12:
                {
                    object obj2 = this.ValueVariant();
                    if (obj2 is ushort)
                    {
                        return (ushort) obj2;
                    }
                    return (int) obj2;
                }
                case 0x12:
                    return this.Value_UI2();
            }
            throw ODB.ConversionRequired();
        }

        internal long ValueInt64()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(long));
            }
            switch (this.DbType)
            {
                case 0x13:
                    return (long) this.Value_UI4();

                case 20:
                    return this.Value_I8();

                case 12:
                {
                    object obj2 = this.ValueVariant();
                    if (obj2 is uint)
                    {
                        return (long) ((uint) obj2);
                    }
                    return (long) obj2;
                }
            }
            throw ODB.ConversionRequired();
        }

        internal float ValueSingle()
        {
            if (this.StatusValue() != DBStatus.S_OK)
            {
                throw this.CheckTypeValueStatusValue(typeof(float));
            }
            switch (this.DbType)
            {
                case 4:
                    return this.Value_R4();

                case 12:
                    return (float) this.ValueVariant();
            }
            throw ODB.ConversionRequired();
        }

        internal string ValueString()
        {
            string str = (string) this._value;
            if (str != null)
            {
                return str;
            }
            DBStatus status = this.StatusValue();
            if (status != DBStatus.S_OK)
            {
                if (status != DBStatus.S_TRUNCATED)
                {
                    throw this.CheckTypeValueStatusValue(typeof(string));
                }
            }
            else
            {
                switch (this.DbType)
                {
                    case 130:
                        str = this.Value_WSTR();
                        goto Label_00C0;

                    case 0x4082:
                        str = this.Value_ByRefWSTR();
                        goto Label_00C0;

                    case 8:
                        str = this.Value_BSTR();
                        goto Label_00C0;

                    case 12:
                        str = (string) this.ValueVariant();
                        goto Label_00C0;
                }
                throw ODB.ConversionRequired();
            }
            switch (this.DbType)
            {
                case 130:
                    str = this.Value_WSTR();
                    break;

                case 0x4082:
                    str = this.Value_ByRefWSTR();
                    break;

                default:
                    throw ODB.ConversionRequired();
            }
        Label_00C0:
            this._value = str;
            return str;
        }

        private object ValueVariant()
        {
            object obj2 = this._value;
            if (obj2 == null)
            {
                obj2 = this.Value_VARIANT();
                this._value = obj2;
            }
            return obj2;
        }

        internal System.Data.OleDb.Bindings Bindings
        {
            get
            {
                this._bindings.CurrentIndex = this.IndexWithinAccessor;
                return this._bindings;
            }
        }

        private int ColumnBindingMaxLen
        {
            get
            {
                return this._maxLen;
            }
        }

        internal int ColumnBindingOrdinal
        {
            get
            {
                return this._ordinal;
            }
        }

        private byte ColumnBindingPrecision
        {
            get
            {
                return this._precision;
            }
        }

        private short DbType
        {
            get
            {
                return this._wType;
            }
        }

        private Type ExpectedType
        {
            get
            {
                return NativeDBType.FromDBType(this.DbType, false, false).dataType;
            }
        }

        internal int Index
        {
            get
            {
                return this._index;
            }
        }

        internal int IndexForAccessor
        {
            get
            {
                return this._indexForAccessor;
            }
        }

        internal int IndexWithinAccessor
        {
            get
            {
                return this._indexWithinAccessor;
            }
        }

        internal System.Data.OleDb.RowBinding RowBinding
        {
            get
            {
                return this._rowbinding;
            }
        }

        private int ValueBindingOffset
        {
            get
            {
                return this._valueBindingOffset;
            }
        }

        private int ValueBindingSize
        {
            get
            {
                return this._valueBindingSize;
            }
        }

        internal int ValueOffset
        {
            get
            {
                return this._offsetValue;
            }
        }
    }
}

