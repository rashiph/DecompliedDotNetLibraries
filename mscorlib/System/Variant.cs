namespace System
{
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct Variant
    {
        internal const int CV_EMPTY = 0;
        internal const int CV_VOID = 1;
        internal const int CV_BOOLEAN = 2;
        internal const int CV_CHAR = 3;
        internal const int CV_I1 = 4;
        internal const int CV_U1 = 5;
        internal const int CV_I2 = 6;
        internal const int CV_U2 = 7;
        internal const int CV_I4 = 8;
        internal const int CV_U4 = 9;
        internal const int CV_I8 = 10;
        internal const int CV_U8 = 11;
        internal const int CV_R4 = 12;
        internal const int CV_R8 = 13;
        internal const int CV_STRING = 14;
        internal const int CV_PTR = 15;
        internal const int CV_DATETIME = 0x10;
        internal const int CV_TIMESPAN = 0x11;
        internal const int CV_OBJECT = 0x12;
        internal const int CV_DECIMAL = 0x13;
        internal const int CV_ENUM = 0x15;
        internal const int CV_MISSING = 0x16;
        internal const int CV_NULL = 0x17;
        internal const int CV_LAST = 0x18;
        internal const int TypeCodeBitMask = 0xffff;
        internal const int VTBitMask = -16777216;
        internal const int VTBitShift = 0x18;
        internal const int ArrayBitMask = 0x10000;
        internal const int EnumI1 = 0x100000;
        internal const int EnumU1 = 0x200000;
        internal const int EnumI2 = 0x300000;
        internal const int EnumU2 = 0x400000;
        internal const int EnumI4 = 0x500000;
        internal const int EnumU4 = 0x600000;
        internal const int EnumI8 = 0x700000;
        internal const int EnumU8 = 0x800000;
        internal const int EnumMask = 0xf00000;
        private object m_objref;
        private int m_data1;
        private int m_data2;
        private int m_flags;
        internal static readonly Type[] ClassTypes;
        internal static readonly System.Variant Empty;
        internal static readonly System.Variant Missing;
        internal static readonly System.Variant DBNull;
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern double GetR8FromVar();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern float GetR4FromVar();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void SetFieldsR4(float val);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void SetFieldsR8(double val);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void SetFieldsObject(object val);
        internal long GetI8FromVar()
        {
            return ((this.m_data2 << 0x20) | (this.m_data1 & ((long) 0xffffffffL)));
        }

        internal Variant(int flags, object or, int data1, int data2)
        {
            this.m_flags = flags;
            this.m_objref = or;
            this.m_data1 = data1;
            this.m_data2 = data2;
        }

        public Variant(bool val)
        {
            this.m_objref = null;
            this.m_flags = 2;
            this.m_data1 = val ? 1 : 0;
            this.m_data2 = 0;
        }

        public Variant(sbyte val)
        {
            this.m_objref = null;
            this.m_flags = 4;
            this.m_data1 = val;
            this.m_data2 = val >> 0x20;
        }

        public Variant(byte val)
        {
            this.m_objref = null;
            this.m_flags = 5;
            this.m_data1 = val;
            this.m_data2 = 0;
        }

        public Variant(short val)
        {
            this.m_objref = null;
            this.m_flags = 6;
            this.m_data1 = val;
            this.m_data2 = val >> 0x20;
        }

        public Variant(ushort val)
        {
            this.m_objref = null;
            this.m_flags = 7;
            this.m_data1 = val;
            this.m_data2 = 0;
        }

        public Variant(char val)
        {
            this.m_objref = null;
            this.m_flags = 3;
            this.m_data1 = val;
            this.m_data2 = 0;
        }

        public Variant(int val)
        {
            this.m_objref = null;
            this.m_flags = 8;
            this.m_data1 = val;
            this.m_data2 = val >> 0x1f;
        }

        public Variant(uint val)
        {
            this.m_objref = null;
            this.m_flags = 9;
            this.m_data1 = (int) val;
            this.m_data2 = 0;
        }

        public Variant(long val)
        {
            this.m_objref = null;
            this.m_flags = 10;
            this.m_data1 = (int) val;
            this.m_data2 = (int) (val >> 0x20);
        }

        public Variant(ulong val)
        {
            this.m_objref = null;
            this.m_flags = 11;
            this.m_data1 = (int) val;
            this.m_data2 = (int) (val >> 0x20);
        }

        [SecuritySafeCritical]
        public Variant(float val)
        {
            this.m_objref = null;
            this.m_flags = 12;
            this.m_data1 = 0;
            this.m_data2 = 0;
            this.SetFieldsR4(val);
        }

        [SecurityCritical]
        public Variant(double val)
        {
            this.m_objref = null;
            this.m_flags = 13;
            this.m_data1 = 0;
            this.m_data2 = 0;
            this.SetFieldsR8(val);
        }

        public Variant(DateTime val)
        {
            this.m_objref = null;
            this.m_flags = 0x10;
            ulong ticks = (ulong) val.Ticks;
            this.m_data1 = (int) ticks;
            this.m_data2 = (int) (ticks >> 0x20);
        }

        public Variant(decimal val)
        {
            this.m_objref = val;
            this.m_flags = 0x13;
            this.m_data1 = 0;
            this.m_data2 = 0;
        }

        [SecuritySafeCritical]
        public Variant(object obj)
        {
            this.m_data1 = 0;
            this.m_data2 = 0;
            VarEnum enum2 = VarEnum.VT_EMPTY;
            if (obj is DateTime)
            {
                this.m_objref = null;
                this.m_flags = 0x10;
                DateTime time = (DateTime) obj;
                ulong ticks = (ulong) time.Ticks;
                this.m_data1 = (int) ticks;
                this.m_data2 = (int) (ticks >> 0x20);
            }
            else if (obj is string)
            {
                this.m_flags = 14;
                this.m_objref = obj;
            }
            else if (obj == null)
            {
                this = Empty;
            }
            else if (obj == System.DBNull.Value)
            {
                this = DBNull;
            }
            else if (obj == Type.Missing)
            {
                this = Missing;
            }
            else if (obj is Array)
            {
                this.m_flags = 0x10012;
                this.m_objref = obj;
            }
            else
            {
                this.m_flags = 0;
                this.m_objref = null;
                if (obj is UnknownWrapper)
                {
                    enum2 = VarEnum.VT_UNKNOWN;
                    obj = ((UnknownWrapper) obj).WrappedObject;
                }
                else if (obj is DispatchWrapper)
                {
                    enum2 = VarEnum.VT_DISPATCH;
                    obj = ((DispatchWrapper) obj).WrappedObject;
                }
                else if (obj is ErrorWrapper)
                {
                    enum2 = VarEnum.VT_ERROR;
                    obj = ((ErrorWrapper) obj).ErrorCode;
                }
                else if (obj is CurrencyWrapper)
                {
                    enum2 = VarEnum.VT_CY;
                    obj = ((CurrencyWrapper) obj).WrappedObject;
                }
                else if (obj is BStrWrapper)
                {
                    enum2 = VarEnum.VT_BSTR;
                    obj = ((BStrWrapper) obj).WrappedObject;
                }
                if (obj != null)
                {
                    this.SetFieldsObject(obj);
                }
                if (enum2 != VarEnum.VT_EMPTY)
                {
                    this.m_flags |= ((int) enum2) << 0x18;
                }
            }
        }

        [SecurityCritical]
        public unsafe Variant(void* voidPointer, Type pointerType)
        {
            if (pointerType == null)
            {
                throw new ArgumentNullException("pointerType");
            }
            if (!pointerType.IsPointer)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "pointerType");
            }
            this.m_objref = pointerType;
            this.m_flags = 15;
            this.m_data1 = (int) voidPointer;
            this.m_data2 = 0;
        }

        internal int CVType
        {
            get
            {
                return (this.m_flags & 0xffff);
            }
        }
        [SecuritySafeCritical]
        public object ToObject()
        {
            switch (this.CVType)
            {
                case 0:
                    return null;

                case 2:
                    return (this.m_data1 != 0);

                case 3:
                    return (char) this.m_data1;

                case 4:
                    return (sbyte) this.m_data1;

                case 5:
                    return (byte) this.m_data1;

                case 6:
                    return (short) this.m_data1;

                case 7:
                    return (ushort) this.m_data1;

                case 8:
                    return this.m_data1;

                case 9:
                    return (uint) this.m_data1;

                case 10:
                    return this.GetI8FromVar();

                case 11:
                    return (ulong) this.GetI8FromVar();

                case 12:
                    return this.GetR4FromVar();

                case 13:
                    return this.GetR8FromVar();

                case 0x10:
                    return new DateTime(this.GetI8FromVar());

                case 0x11:
                    return new TimeSpan(this.GetI8FromVar());

                case 0x15:
                    return this.BoxEnum();

                case 0x16:
                    return Type.Missing;

                case 0x17:
                    return System.DBNull.Value;
            }
            return this.m_objref;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object BoxEnum();
        [SecuritySafeCritical]
        internal static void MarshalHelperConvertObjectToVariant(object o, ref System.Variant v)
        {
            IConvertible convertible = RemotingServices.IsTransparentProxy(o) ? null : (o as IConvertible);
            if (o == null)
            {
                v = Empty;
            }
            else if (convertible == null)
            {
                v = new System.Variant(o);
            }
            else
            {
                IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Empty:
                        v = Empty;
                        return;

                    case TypeCode.Object:
                        v = new System.Variant(o);
                        return;

                    case TypeCode.DBNull:
                        v = DBNull;
                        return;

                    case TypeCode.Boolean:
                        v = new System.Variant(convertible.ToBoolean(invariantCulture));
                        return;

                    case TypeCode.Char:
                        v = new System.Variant(convertible.ToChar(invariantCulture));
                        return;

                    case TypeCode.SByte:
                        v = new System.Variant(convertible.ToSByte(invariantCulture));
                        return;

                    case TypeCode.Byte:
                        v = new System.Variant(convertible.ToByte(invariantCulture));
                        return;

                    case TypeCode.Int16:
                        v = new System.Variant(convertible.ToInt16(invariantCulture));
                        return;

                    case TypeCode.UInt16:
                        v = new System.Variant(convertible.ToUInt16(invariantCulture));
                        return;

                    case TypeCode.Int32:
                        v = new System.Variant(convertible.ToInt32(invariantCulture));
                        return;

                    case TypeCode.UInt32:
                        v = new System.Variant(convertible.ToUInt32(invariantCulture));
                        return;

                    case TypeCode.Int64:
                        v = new System.Variant(convertible.ToInt64(invariantCulture));
                        return;

                    case TypeCode.UInt64:
                        v = new System.Variant(convertible.ToUInt64(invariantCulture));
                        return;

                    case TypeCode.Single:
                        v = new System.Variant(convertible.ToSingle(invariantCulture));
                        return;

                    case TypeCode.Double:
                        v = new System.Variant(convertible.ToDouble(invariantCulture));
                        return;

                    case TypeCode.Decimal:
                        v = new System.Variant(convertible.ToDecimal(invariantCulture));
                        return;

                    case TypeCode.DateTime:
                        v = new System.Variant(convertible.ToDateTime(invariantCulture));
                        return;

                    case TypeCode.String:
                        v = new System.Variant(convertible.ToString(invariantCulture));
                        return;
                }
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnknownTypeCode", new object[] { convertible.GetTypeCode() }));
            }
        }

        internal static object MarshalHelperConvertVariantToObject(ref System.Variant v)
        {
            return v.ToObject();
        }

        [SecurityCritical]
        internal static void MarshalHelperCastVariant(object pValue, int vt, ref System.Variant v)
        {
            IConvertible convertible = pValue as IConvertible;
            if (convertible != null)
            {
                IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
                switch (vt)
                {
                    case 0:
                        v = Empty;
                        return;

                    case 1:
                        v = DBNull;
                        return;

                    case 2:
                        v = new System.Variant(convertible.ToInt16(invariantCulture));
                        return;

                    case 3:
                        v = new System.Variant(convertible.ToInt32(invariantCulture));
                        return;

                    case 4:
                        v = new System.Variant(convertible.ToSingle(invariantCulture));
                        return;

                    case 5:
                        v = new System.Variant(convertible.ToDouble(invariantCulture));
                        return;

                    case 6:
                        v = new System.Variant(new CurrencyWrapper(convertible.ToDecimal(invariantCulture)));
                        return;

                    case 7:
                        v = new System.Variant(convertible.ToDateTime(invariantCulture));
                        return;

                    case 8:
                        v = new System.Variant(convertible.ToString(invariantCulture));
                        return;

                    case 9:
                        v = new System.Variant(new DispatchWrapper(convertible));
                        return;

                    case 10:
                        v = new System.Variant(new ErrorWrapper(convertible.ToInt32(invariantCulture)));
                        return;

                    case 11:
                        v = new System.Variant(convertible.ToBoolean(invariantCulture));
                        return;

                    case 12:
                        v = new System.Variant(convertible);
                        return;

                    case 13:
                        v = new System.Variant(new UnknownWrapper(convertible));
                        return;

                    case 14:
                        v = new System.Variant(convertible.ToDecimal(invariantCulture));
                        return;

                    case 0x10:
                        v = new System.Variant(convertible.ToSByte(invariantCulture));
                        return;

                    case 0x11:
                        v = new System.Variant(convertible.ToByte(invariantCulture));
                        return;

                    case 0x12:
                        v = new System.Variant(convertible.ToUInt16(invariantCulture));
                        return;

                    case 0x13:
                        v = new System.Variant(convertible.ToUInt32(invariantCulture));
                        return;

                    case 20:
                        v = new System.Variant(convertible.ToInt64(invariantCulture));
                        return;

                    case 0x15:
                        v = new System.Variant(convertible.ToUInt64(invariantCulture));
                        return;

                    case 0x16:
                        v = new System.Variant(convertible.ToInt32(invariantCulture));
                        return;

                    case 0x17:
                        v = new System.Variant(convertible.ToUInt32(invariantCulture));
                        return;
                }
                throw new InvalidCastException(Environment.GetResourceString("InvalidCast_CannotCoerceByRefVariant"));
            }
            switch (vt)
            {
                case 8:
                    if (pValue != null)
                    {
                        throw new InvalidCastException(Environment.GetResourceString("InvalidCast_CannotCoerceByRefVariant"));
                    }
                    v = new System.Variant(null);
                    v.m_flags = 14;
                    return;

                case 9:
                    v = new System.Variant(new DispatchWrapper(pValue));
                    return;

                case 12:
                    v = new System.Variant(pValue);
                    return;

                case 13:
                    v = new System.Variant(new UnknownWrapper(pValue));
                    return;

                case 0x24:
                    v = new System.Variant(pValue);
                    return;
            }
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_CannotCoerceByRefVariant"));
        }

        static Variant()
        {
            ClassTypes = new Type[] { 
                typeof(System.Empty), typeof(void), typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(void), 
                typeof(DateTime), typeof(TimeSpan), typeof(object), typeof(decimal), typeof(object), typeof(System.Reflection.Missing), typeof(System.DBNull)
             };
            Empty = new System.Variant();
            Missing = new System.Variant(0x16, Type.Missing, 0, 0);
            DBNull = new System.Variant(0x17, System.DBNull.Value, 0, 0);
        }
    }
}

