namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;

    [StructLayout(LayoutKind.Explicit), SecurityCritical]
    internal struct Variant
    {
        [FieldOffset(0)]
        private decimal _decimal;
        [FieldOffset(0)]
        private TypeUnion _typeUnion;

        public unsafe void Clear()
        {
            VarEnum variantType = this.VariantType;
            if ((variantType & VarEnum.VT_BYREF) != VarEnum.VT_EMPTY)
            {
                this.VariantType = VarEnum.VT_EMPTY;
            }
            else if (((((variantType & VarEnum.VT_ARRAY) != VarEnum.VT_EMPTY) || (variantType == VarEnum.VT_BSTR)) || ((variantType == VarEnum.VT_UNKNOWN) || (variantType == VarEnum.VT_DISPATCH))) || ((variantType == VarEnum.VT_VARIANT) || (variantType == VarEnum.VT_RECORD)))
            {
                fixed (IntPtr* ptrRef = ((IntPtr*) this))
                {
                    NativeMethods.VariantClear((IntPtr) ptrRef);
                }
            }
            else
            {
                this.VariantType = VarEnum.VT_EMPTY;
            }
        }

        public void CopyFromIndirect(object value)
        {
            VarEnum enum2 = this.VariantType & ~VarEnum.VT_BYREF;
            if (value == null)
            {
                switch (enum2)
                {
                    case VarEnum.VT_DISPATCH:
                    case VarEnum.VT_UNKNOWN:
                    case VarEnum.VT_BSTR:
                        this._typeUnion._unionTypes._byref[0] = IntPtr.Zero;
                        break;
                }
            }
            else
            {
                switch (enum2)
                {
                    case VarEnum.VT_I2:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((short) value);
                        return;

                    case VarEnum.VT_I4:
                    case VarEnum.VT_INT:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((int) value);
                        return;

                    case VarEnum.VT_R4:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((float) value);
                        return;

                    case VarEnum.VT_R8:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((double) value);
                        return;

                    case VarEnum.VT_CY:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) decimal.ToOACurrency((decimal) value);
                        return;

                    case VarEnum.VT_DATE:
                    {
                        DateTime time = (DateTime) value;
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) time.ToOADate();
                        return;
                    }
                    case VarEnum.VT_BSTR:
                        this._typeUnion._unionTypes._byref[0] = Marshal.StringToBSTR((string) value);
                        return;

                    case VarEnum.VT_DISPATCH:
                        this._typeUnion._unionTypes._byref[0] = Marshal.GetIDispatchForObject(value);
                        return;

                    case VarEnum.VT_ERROR:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((ErrorWrapper) value).ErrorCode;
                        return;

                    case VarEnum.VT_BOOL:
                        this._typeUnion._unionTypes._byref[0] = ((bool) value) ? ((IntPtr) (-1)) : IntPtr.Zero;
                        return;

                    case VarEnum.VT_UNKNOWN:
                        this._typeUnion._unionTypes._byref[0] = Marshal.GetIUnknownForObject(value);
                        return;

                    case VarEnum.VT_DECIMAL:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((decimal) value);
                        return;

                    case VarEnum.VT_I1:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((sbyte) value);
                        return;

                    case VarEnum.VT_UI1:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((byte) value);
                        return;

                    case VarEnum.VT_UI2:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((ushort) value);
                        return;

                    case VarEnum.VT_UI4:
                    case VarEnum.VT_UINT:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((uint) value);
                        return;

                    case VarEnum.VT_I8:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((long) value);
                        return;

                    case VarEnum.VT_UI8:
                        this._typeUnion._unionTypes._byref[0] = (IntPtr) ((ulong) value);
                        return;
                }
                throw new ArgumentException("invalid argument type");
            }
        }

        internal static bool IsPrimitiveType(VarEnum varEnum)
        {
            switch (varEnum)
            {
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                case VarEnum.VT_R4:
                case VarEnum.VT_R8:
                case VarEnum.VT_DATE:
                case VarEnum.VT_BSTR:
                case VarEnum.VT_BOOL:
                case VarEnum.VT_DECIMAL:
                case VarEnum.VT_I1:
                case VarEnum.VT_UI1:
                case VarEnum.VT_UI2:
                case VarEnum.VT_UI4:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI8:
                case VarEnum.VT_INT:
                case VarEnum.VT_UINT:
                    return true;
            }
            return false;
        }

        public void SetAsNULL()
        {
            this.VariantType = VarEnum.VT_NULL;
        }

        public unsafe object ToObject()
        {
            object objectForNativeVariant;
            if (this.IsEmpty)
            {
                return null;
            }
            switch (this.VariantType)
            {
                case VarEnum.VT_NULL:
                    return DBNull.Value;

                case VarEnum.VT_I2:
                    return this.AsI2;

                case VarEnum.VT_I4:
                    return this.AsI4;

                case VarEnum.VT_R4:
                    return this.AsR4;

                case VarEnum.VT_R8:
                    return this.AsR8;

                case VarEnum.VT_CY:
                    return this.AsCy;

                case VarEnum.VT_DATE:
                    return this.AsDate;

                case VarEnum.VT_BSTR:
                    return this.AsBstr;

                case VarEnum.VT_DISPATCH:
                    return this.AsDispatch;

                case VarEnum.VT_ERROR:
                    return this.AsError;

                case VarEnum.VT_BOOL:
                    return this.AsBool;

                case VarEnum.VT_UNKNOWN:
                    return this.AsUnknown;

                case VarEnum.VT_DECIMAL:
                    return this.AsDecimal;

                case VarEnum.VT_I1:
                    return this.AsI1;

                case VarEnum.VT_UI1:
                    return this.AsUi1;

                case VarEnum.VT_UI2:
                    return this.AsUi2;

                case VarEnum.VT_UI4:
                    return this.AsUi4;

                case VarEnum.VT_I8:
                    return this.AsI8;

                case VarEnum.VT_UI8:
                    return this.AsUi8;

                case VarEnum.VT_INT:
                    return this.AsInt;

                case VarEnum.VT_UINT:
                    return this.AsUint;
            }
            try
            {
                fixed (IntPtr* ptrRef = ((IntPtr*) this))
                {
                    objectForNativeVariant = Marshal.GetObjectForNativeVariant((IntPtr) ptrRef);
                }
            }
            catch (Exception exception)
            {
                throw new NotImplementedException("Variant.ToObject cannot handle" + this.VariantType, exception);
            }
            return objectForNativeVariant;
        }

        public bool AsBool
        {
            get
            {
                return (this._typeUnion._unionTypes._bool != 0);
            }
            set
            {
                this.VariantType = VarEnum.VT_BOOL;
                this._typeUnion._unionTypes._bool = value ? ((short) (-1)) : ((short) 0);
            }
        }

        public string AsBstr
        {
            get
            {
                return Marshal.PtrToStringBSTR(this._typeUnion._unionTypes._bstr);
            }
            set
            {
                this.VariantType = VarEnum.VT_BSTR;
                this._typeUnion._unionTypes._bstr = Marshal.StringToBSTR(value);
            }
        }

        public decimal AsCy
        {
            get
            {
                return decimal.FromOACurrency(this._typeUnion._unionTypes._cy);
            }
            set
            {
                this.VariantType = VarEnum.VT_CY;
                this._typeUnion._unionTypes._cy = decimal.ToOACurrency(value);
            }
        }

        public DateTime AsDate
        {
            get
            {
                return DateTime.FromOADate(this._typeUnion._unionTypes._date);
            }
            set
            {
                this.VariantType = VarEnum.VT_DATE;
                this._typeUnion._unionTypes._date = value.ToOADate();
            }
        }

        public decimal AsDecimal
        {
            get
            {
                System.Runtime.InteropServices.Variant variant = this;
                variant._typeUnion._vt = 0;
                return variant._decimal;
            }
            set
            {
                this.VariantType = VarEnum.VT_DECIMAL;
                this._decimal = value;
                this._typeUnion._vt = 14;
            }
        }

        public object AsDispatch
        {
            get
            {
                if (this._typeUnion._unionTypes._dispatch == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.GetObjectForIUnknown(this._typeUnion._unionTypes._dispatch);
            }
            set
            {
                this.VariantType = VarEnum.VT_DISPATCH;
                if (value == null)
                {
                    this._typeUnion._unionTypes._dispatch = IntPtr.Zero;
                }
                else
                {
                    this._typeUnion._unionTypes._dispatch = Marshal.GetIDispatchForObject(value);
                }
            }
        }

        public int AsError
        {
            get
            {
                return this._typeUnion._unionTypes._error;
            }
            set
            {
                this.VariantType = VarEnum.VT_ERROR;
                this._typeUnion._unionTypes._error = value;
            }
        }

        public sbyte AsI1
        {
            get
            {
                return this._typeUnion._unionTypes._i1;
            }
            set
            {
                this.VariantType = VarEnum.VT_I1;
                this._typeUnion._unionTypes._i1 = value;
            }
        }

        public short AsI2
        {
            get
            {
                return this._typeUnion._unionTypes._i2;
            }
            set
            {
                this.VariantType = VarEnum.VT_I2;
                this._typeUnion._unionTypes._i2 = value;
            }
        }

        public int AsI4
        {
            get
            {
                return this._typeUnion._unionTypes._i4;
            }
            set
            {
                this.VariantType = VarEnum.VT_I4;
                this._typeUnion._unionTypes._i4 = value;
            }
        }

        public long AsI8
        {
            get
            {
                return this._typeUnion._unionTypes._i8;
            }
            set
            {
                this.VariantType = VarEnum.VT_I8;
                this._typeUnion._unionTypes._i8 = value;
            }
        }

        public IntPtr AsInt
        {
            get
            {
                return this._typeUnion._unionTypes._int;
            }
            set
            {
                this.VariantType = VarEnum.VT_INT;
                this._typeUnion._unionTypes._int = value;
            }
        }

        public float AsR4
        {
            get
            {
                return this._typeUnion._unionTypes._r4;
            }
            set
            {
                this.VariantType = VarEnum.VT_R4;
                this._typeUnion._unionTypes._r4 = value;
            }
        }

        public double AsR8
        {
            get
            {
                return this._typeUnion._unionTypes._r8;
            }
            set
            {
                this.VariantType = VarEnum.VT_R8;
                this._typeUnion._unionTypes._r8 = value;
            }
        }

        public byte AsUi1
        {
            get
            {
                return this._typeUnion._unionTypes._ui1;
            }
            set
            {
                this.VariantType = VarEnum.VT_UI1;
                this._typeUnion._unionTypes._ui1 = value;
            }
        }

        public ushort AsUi2
        {
            get
            {
                return this._typeUnion._unionTypes._ui2;
            }
            set
            {
                this.VariantType = VarEnum.VT_UI2;
                this._typeUnion._unionTypes._ui2 = value;
            }
        }

        public uint AsUi4
        {
            get
            {
                return this._typeUnion._unionTypes._ui4;
            }
            set
            {
                this.VariantType = VarEnum.VT_UI4;
                this._typeUnion._unionTypes._ui4 = value;
            }
        }

        public ulong AsUi8
        {
            get
            {
                return this._typeUnion._unionTypes._ui8;
            }
            set
            {
                this.VariantType = VarEnum.VT_UI8;
                this._typeUnion._unionTypes._ui8 = value;
            }
        }

        public UIntPtr AsUint
        {
            get
            {
                return this._typeUnion._unionTypes._uint;
            }
            set
            {
                this.VariantType = VarEnum.VT_UINT;
                this._typeUnion._unionTypes._uint = value;
            }
        }

        public object AsUnknown
        {
            get
            {
                if (this._typeUnion._unionTypes._unknown == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.GetObjectForIUnknown(this._typeUnion._unionTypes._unknown);
            }
            set
            {
                this.VariantType = VarEnum.VT_UNKNOWN;
                if (value == null)
                {
                    this._typeUnion._unionTypes._unknown = IntPtr.Zero;
                }
                else
                {
                    this._typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);
                }
            }
        }

        internal bool IsByRef
        {
            get
            {
                return ((this._typeUnion._vt & 0x4000) != 0);
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (this._typeUnion._vt == 0);
            }
        }

        public VarEnum VariantType
        {
            get
            {
                return (VarEnum) this._typeUnion._vt;
            }
            set
            {
                this._typeUnion._vt = (ushort) value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            private IntPtr _record;
            private IntPtr _recordInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TypeUnion
        {
            internal ushort _vt;
            internal ushort _wReserved1;
            internal ushort _wReserved2;
            internal ushort _wReserved3;
            internal System.Runtime.InteropServices.Variant.UnionTypes _unionTypes;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UnionTypes
        {
            [FieldOffset(0)]
            internal short _bool;
            [FieldOffset(0)]
            internal IntPtr _bstr;
            [FieldOffset(0)]
            internal IntPtr _byref;
            [FieldOffset(0)]
            internal long _cy;
            [FieldOffset(0)]
            internal double _date;
            [FieldOffset(0)]
            internal IntPtr _dispatch;
            [FieldOffset(0)]
            internal int _error;
            [FieldOffset(0)]
            internal sbyte _i1;
            [FieldOffset(0)]
            internal short _i2;
            [FieldOffset(0)]
            internal int _i4;
            [FieldOffset(0)]
            internal long _i8;
            [FieldOffset(0)]
            internal IntPtr _int;
            [FieldOffset(0)]
            internal float _r4;
            [FieldOffset(0)]
            internal double _r8;
            [FieldOffset(0)]
            internal System.Runtime.InteropServices.Variant.Record _record;
            [FieldOffset(0)]
            internal byte _ui1;
            [FieldOffset(0)]
            internal ushort _ui2;
            [FieldOffset(0)]
            internal uint _ui4;
            [FieldOffset(0)]
            internal ulong _ui8;
            [FieldOffset(0)]
            internal UIntPtr _uint;
            [FieldOffset(0)]
            internal IntPtr _unknown;
        }
    }
}

