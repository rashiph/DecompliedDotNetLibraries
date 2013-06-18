namespace System.Management
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public class PropertyData
    {
        private ManagementBaseObject parent;
        private int propertyFlavor;
        private string propertyName;
        private long propertyNullEnumValue;
        private int propertyType;
        private object propertyValue;
        private QualifierDataCollection qualifiers;

        internal PropertyData(ManagementBaseObject parent, string propName)
        {
            this.parent = parent;
            this.propertyName = propName;
            this.qualifiers = null;
            this.RefreshPropertyInfo();
        }

        internal static object MapValueToWmiValue(object val, CimType type, bool isArray)
        {
            object obj2 = DBNull.Value;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            if (val == null)
            {
                return obj2;
            }
            if (!isArray)
            {
                switch (type)
                {
                    case CimType.SInt16:
                        return Convert.ToInt16(val, (IFormatProvider) invariantCulture.GetFormat(typeof(short)));

                    case CimType.SInt32:
                        return Convert.ToInt32(val, (IFormatProvider) invariantCulture.GetFormat(typeof(int)));

                    case CimType.Real32:
                        return Convert.ToSingle(val, (IFormatProvider) invariantCulture.GetFormat(typeof(float)));

                    case CimType.Real64:
                        return Convert.ToDouble(val, (IFormatProvider) invariantCulture.GetFormat(typeof(double)));

                    case (CimType.Real32 | CimType.SInt16):
                    case (CimType.Real64 | CimType.SInt16):
                    case ((CimType) 9):
                    case (CimType.String | CimType.SInt16):
                    case (CimType.String | CimType.Real32):
                    case (CimType.String | CimType.Real32 | CimType.SInt16):
                    case (CimType.Object | CimType.SInt16):
                        return val;

                    case CimType.String:
                    case CimType.DateTime:
                    case CimType.Reference:
                        return val.ToString();

                    case CimType.Boolean:
                        return Convert.ToBoolean(val, (IFormatProvider) invariantCulture.GetFormat(typeof(bool)));

                    case CimType.Object:
                        if (val is ManagementBaseObject)
                        {
                            return Marshal.GetObjectForIUnknown((IntPtr) ((ManagementBaseObject) val).wbemObject);
                        }
                        return val;

                    case CimType.SInt8:
                        return (short) Convert.ToSByte(val, (IFormatProvider) invariantCulture.GetFormat(typeof(short)));

                    case CimType.UInt8:
                        return Convert.ToByte(val, (IFormatProvider) invariantCulture.GetFormat(typeof(byte)));

                    case CimType.UInt16:
                        return (int) Convert.ToUInt16(val, (IFormatProvider) invariantCulture.GetFormat(typeof(ushort)));

                    case CimType.UInt32:
                        return (int) Convert.ToUInt32(val, (IFormatProvider) invariantCulture.GetFormat(typeof(uint)));

                    case CimType.SInt64:
                        return Convert.ToInt64(val, (IFormatProvider) invariantCulture.GetFormat(typeof(long))).ToString((IFormatProvider) invariantCulture.GetFormat(typeof(long)));

                    case CimType.UInt64:
                        return Convert.ToUInt64(val, (IFormatProvider) invariantCulture.GetFormat(typeof(ulong))).ToString((IFormatProvider) invariantCulture.GetFormat(typeof(ulong)));

                    case CimType.Char16:
                        return (short) Convert.ToChar(val, (IFormatProvider) invariantCulture.GetFormat(typeof(char)));
                }
                return val;
            }
            Array array = (Array) val;
            int length = array.Length;
            switch (type)
            {
                case CimType.SInt16:
                    if (val is short[])
                    {
                        return val;
                    }
                    obj2 = new short[length];
                    for (int i = 0; i < length; i++)
                    {
                        ((short[]) obj2)[i] = Convert.ToInt16(array.GetValue(i), (IFormatProvider) invariantCulture.GetFormat(typeof(short)));
                    }
                    return obj2;

                case CimType.SInt32:
                    if (val is int[])
                    {
                        return val;
                    }
                    obj2 = new int[length];
                    for (int j = 0; j < length; j++)
                    {
                        ((int[]) obj2)[j] = Convert.ToInt32(array.GetValue(j), (IFormatProvider) invariantCulture.GetFormat(typeof(int)));
                    }
                    return obj2;

                case CimType.Real32:
                    if (val is float[])
                    {
                        return val;
                    }
                    obj2 = new float[length];
                    for (int k = 0; k < length; k++)
                    {
                        ((float[]) obj2)[k] = Convert.ToSingle(array.GetValue(k), (IFormatProvider) invariantCulture.GetFormat(typeof(float)));
                    }
                    return obj2;

                case CimType.Real64:
                    if (val is double[])
                    {
                        return val;
                    }
                    obj2 = new double[length];
                    for (int m = 0; m < length; m++)
                    {
                        ((double[]) obj2)[m] = Convert.ToDouble(array.GetValue(m), (IFormatProvider) invariantCulture.GetFormat(typeof(double)));
                    }
                    return obj2;

                case (CimType.Real32 | CimType.SInt16):
                case (CimType.Real64 | CimType.SInt16):
                case ((CimType) 9):
                case (CimType.String | CimType.SInt16):
                case (CimType.String | CimType.Real32):
                case (CimType.String | CimType.Real32 | CimType.SInt16):
                case (CimType.Object | CimType.SInt16):
                    return val;

                case CimType.String:
                case CimType.DateTime:
                case CimType.Reference:
                    if (val is string[])
                    {
                        return val;
                    }
                    obj2 = new string[length];
                    for (int n = 0; n < length; n++)
                    {
                        ((string[]) obj2)[n] = array.GetValue(n).ToString();
                    }
                    return obj2;

                case CimType.Boolean:
                    if (val is bool[])
                    {
                        return val;
                    }
                    obj2 = new bool[length];
                    for (int num14 = 0; num14 < length; num14++)
                    {
                        ((bool[]) obj2)[num14] = Convert.ToBoolean(array.GetValue(num14), (IFormatProvider) invariantCulture.GetFormat(typeof(bool)));
                    }
                    return obj2;

                case CimType.Object:
                    obj2 = new IWbemClassObject_DoNotMarshal[length];
                    for (int num15 = 0; num15 < length; num15++)
                    {
                        ((IWbemClassObject_DoNotMarshal[]) obj2)[num15] = (IWbemClassObject_DoNotMarshal) Marshal.GetObjectForIUnknown((IntPtr) ((ManagementBaseObject) array.GetValue(num15)).wbemObject);
                    }
                    return obj2;

                case CimType.SInt8:
                    obj2 = new short[length];
                    for (int num2 = 0; num2 < length; num2++)
                    {
                        ((short[]) obj2)[num2] = Convert.ToSByte(array.GetValue(num2), (IFormatProvider) invariantCulture.GetFormat(typeof(sbyte)));
                    }
                    return obj2;

                case CimType.UInt8:
                    if (val is byte[])
                    {
                        return val;
                    }
                    obj2 = new byte[length];
                    for (int num3 = 0; num3 < length; num3++)
                    {
                        ((byte[]) obj2)[num3] = Convert.ToByte(array.GetValue(num3), (IFormatProvider) invariantCulture.GetFormat(typeof(byte)));
                    }
                    return obj2;

                case CimType.UInt16:
                    obj2 = new int[length];
                    for (int num5 = 0; num5 < length; num5++)
                    {
                        ((int[]) obj2)[num5] = Convert.ToUInt16(array.GetValue(num5), (IFormatProvider) invariantCulture.GetFormat(typeof(ushort)));
                    }
                    return obj2;

                case CimType.UInt32:
                    obj2 = new int[length];
                    for (int num7 = 0; num7 < length; num7++)
                    {
                        ((int[]) obj2)[num7] = (int) Convert.ToUInt32(array.GetValue(num7), (IFormatProvider) invariantCulture.GetFormat(typeof(uint)));
                    }
                    return obj2;

                case CimType.SInt64:
                    obj2 = new string[length];
                    for (int num8 = 0; num8 < length; num8++)
                    {
                        ((string[]) obj2)[num8] = Convert.ToInt64(array.GetValue(num8), (IFormatProvider) invariantCulture.GetFormat(typeof(long))).ToString((IFormatProvider) invariantCulture.GetFormat(typeof(long)));
                    }
                    return obj2;

                case CimType.UInt64:
                    obj2 = new string[length];
                    for (int num9 = 0; num9 < length; num9++)
                    {
                        ((string[]) obj2)[num9] = Convert.ToUInt64(array.GetValue(num9), (IFormatProvider) invariantCulture.GetFormat(typeof(ulong))).ToString((IFormatProvider) invariantCulture.GetFormat(typeof(ulong)));
                    }
                    return obj2;

                case CimType.Char16:
                    obj2 = new short[length];
                    for (int num12 = 0; num12 < length; num12++)
                    {
                        ((short[]) obj2)[num12] = (short) Convert.ToChar(array.GetValue(num12), (IFormatProvider) invariantCulture.GetFormat(typeof(char)));
                    }
                    return obj2;
            }
            return val;
        }

        internal static object MapValueToWmiValue(object val, out bool isArray, out CimType type)
        {
            object objectForIUnknown = DBNull.Value;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            isArray = false;
            type = CimType.None;
            if (val != null)
            {
                isArray = val.GetType().IsArray;
                System.Type type2 = val.GetType();
                if (isArray)
                {
                    System.Type elementType = type2.GetElementType();
                    if (elementType.IsPrimitive)
                    {
                        if (elementType == typeof(byte))
                        {
                            byte[] buffer = (byte[]) val;
                            int length = buffer.Length;
                            type = CimType.UInt8;
                            objectForIUnknown = new short[length];
                            for (int i = 0; i < length; i++)
                            {
                                ((short[]) objectForIUnknown)[i] = ((IConvertible) buffer[i]).ToInt16(null);
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(sbyte))
                        {
                            sbyte[] numArray = (sbyte[]) val;
                            int num3 = numArray.Length;
                            type = CimType.SInt8;
                            objectForIUnknown = new short[num3];
                            for (int j = 0; j < num3; j++)
                            {
                                ((short[]) objectForIUnknown)[j] = ((IConvertible) numArray[j]).ToInt16(null);
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(bool))
                        {
                            type = CimType.Boolean;
                            return (bool[]) val;
                        }
                        if (elementType == typeof(ushort))
                        {
                            ushort[] numArray2 = (ushort[]) val;
                            int num5 = numArray2.Length;
                            type = CimType.UInt16;
                            objectForIUnknown = new int[num5];
                            for (int k = 0; k < num5; k++)
                            {
                                ((int[]) objectForIUnknown)[k] = ((IConvertible) numArray2[k]).ToInt32(null);
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(short))
                        {
                            type = CimType.SInt16;
                            return (short[]) val;
                        }
                        if (elementType == typeof(int))
                        {
                            type = CimType.SInt32;
                            return (int[]) val;
                        }
                        if (elementType == typeof(uint))
                        {
                            uint[] numArray3 = (uint[]) val;
                            int num7 = numArray3.Length;
                            type = CimType.UInt32;
                            objectForIUnknown = new string[num7];
                            for (int m = 0; m < num7; m++)
                            {
                                ((string[]) objectForIUnknown)[m] = numArray3[m].ToString((IFormatProvider) invariantCulture.GetFormat(typeof(uint)));
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(ulong))
                        {
                            ulong[] numArray4 = (ulong[]) val;
                            int num9 = numArray4.Length;
                            type = CimType.UInt64;
                            objectForIUnknown = new string[num9];
                            for (int n = 0; n < num9; n++)
                            {
                                ((string[]) objectForIUnknown)[n] = numArray4[n].ToString((IFormatProvider) invariantCulture.GetFormat(typeof(ulong)));
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(long))
                        {
                            long[] numArray5 = (long[]) val;
                            int num11 = numArray5.Length;
                            type = CimType.SInt64;
                            objectForIUnknown = new string[num11];
                            for (int num12 = 0; num12 < num11; num12++)
                            {
                                ((string[]) objectForIUnknown)[num12] = numArray5[num12].ToString((IFormatProvider) invariantCulture.GetFormat(typeof(long)));
                            }
                            return objectForIUnknown;
                        }
                        if (elementType == typeof(float))
                        {
                            type = CimType.Real32;
                            return (float[]) val;
                        }
                        if (elementType == typeof(double))
                        {
                            type = CimType.Real64;
                            return (double[]) val;
                        }
                        if (elementType == typeof(char))
                        {
                            char[] chArray = (char[]) val;
                            int num13 = chArray.Length;
                            type = CimType.Char16;
                            objectForIUnknown = new short[num13];
                            for (int num14 = 0; num14 < num13; num14++)
                            {
                                ((short[]) objectForIUnknown)[num14] = ((IConvertible) chArray[num14]).ToInt16(null);
                            }
                        }
                        return objectForIUnknown;
                    }
                    if (elementType == typeof(string))
                    {
                        type = CimType.String;
                        return (string[]) val;
                    }
                    if (val is ManagementBaseObject[])
                    {
                        Array array = (Array) val;
                        int num15 = array.Length;
                        type = CimType.Object;
                        objectForIUnknown = new IWbemClassObject_DoNotMarshal[num15];
                        for (int num16 = 0; num16 < num15; num16++)
                        {
                            ((IWbemClassObject_DoNotMarshal[]) objectForIUnknown)[num16] = (IWbemClassObject_DoNotMarshal) Marshal.GetObjectForIUnknown((IntPtr) ((ManagementBaseObject) array.GetValue(num16)).wbemObject);
                        }
                    }
                    return objectForIUnknown;
                }
                if (type2 == typeof(ushort))
                {
                    type = CimType.UInt16;
                    return ((IConvertible) ((ushort) val)).ToInt32(null);
                }
                if (type2 == typeof(uint))
                {
                    type = CimType.UInt32;
                    if ((((uint) val) & 0x80000000) != 0)
                    {
                        return Convert.ToString(val, (IFormatProvider) invariantCulture.GetFormat(typeof(uint)));
                    }
                    return Convert.ToInt32(val, (IFormatProvider) invariantCulture.GetFormat(typeof(int)));
                }
                if (type2 == typeof(ulong))
                {
                    type = CimType.UInt64;
                    ulong num17 = (ulong) val;
                    return num17.ToString((IFormatProvider) invariantCulture.GetFormat(typeof(ulong)));
                }
                if (type2 == typeof(sbyte))
                {
                    type = CimType.SInt8;
                    return ((IConvertible) ((sbyte) val)).ToInt16(null);
                }
                if (type2 == typeof(byte))
                {
                    type = CimType.UInt8;
                    return val;
                }
                if (type2 == typeof(short))
                {
                    type = CimType.SInt16;
                    return val;
                }
                if (type2 == typeof(int))
                {
                    type = CimType.SInt32;
                    return val;
                }
                if (type2 == typeof(long))
                {
                    type = CimType.SInt64;
                    return val.ToString();
                }
                if (type2 == typeof(bool))
                {
                    type = CimType.Boolean;
                    return val;
                }
                if (type2 == typeof(float))
                {
                    type = CimType.Real32;
                    return val;
                }
                if (type2 == typeof(double))
                {
                    type = CimType.Real64;
                    return val;
                }
                if (type2 == typeof(char))
                {
                    type = CimType.Char16;
                    return ((IConvertible) ((char) val)).ToInt16(null);
                }
                if (type2 == typeof(string))
                {
                    type = CimType.String;
                    return val;
                }
                if (val is ManagementBaseObject)
                {
                    type = CimType.Object;
                    objectForIUnknown = Marshal.GetObjectForIUnknown((IntPtr) ((ManagementBaseObject) val).wbemObject);
                }
            }
            return objectForIUnknown;
        }

        internal static object MapWmiValueToValue(object wmiValue, CimType type, bool isArray)
        {
            object obj2 = null;
            if ((DBNull.Value == wmiValue) || (wmiValue == null))
            {
                return obj2;
            }
            if (!isArray)
            {
                switch (type)
                {
                    case CimType.Object:
                        return new ManagementBaseObject(new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(wmiValue)));

                    case (CimType.String | CimType.Real32 | CimType.SInt16):
                    case (CimType.Object | CimType.SInt16):
                    case CimType.UInt8:
                        return wmiValue;

                    case CimType.SInt8:
                        return (sbyte) ((short) wmiValue);

                    case CimType.UInt16:
                        return (ushort) ((int) wmiValue);

                    case CimType.UInt32:
                        return (uint) ((int) wmiValue);

                    case CimType.SInt64:
                        return Convert.ToInt64((string) wmiValue, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(long)));

                    case CimType.UInt64:
                        return Convert.ToUInt64((string) wmiValue, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(ulong)));

                    case CimType.Char16:
                        return (char) ((ushort) ((short) wmiValue));
                }
                return wmiValue;
            }
            Array array = (Array) wmiValue;
            int length = array.Length;
            switch (type)
            {
                case CimType.Object:
                    obj2 = new ManagementBaseObject[length];
                    for (int i = 0; i < length; i++)
                    {
                        ((ManagementBaseObject[]) obj2)[i] = new ManagementBaseObject(new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(array.GetValue(i))));
                    }
                    return obj2;

                case (CimType.String | CimType.Real32 | CimType.SInt16):
                case (CimType.Object | CimType.SInt16):
                case CimType.UInt8:
                    return wmiValue;

                case CimType.SInt8:
                    obj2 = new sbyte[length];
                    for (int j = 0; j < length; j++)
                    {
                        ((sbyte[]) obj2)[j] = (sbyte) ((short) array.GetValue(j));
                    }
                    return obj2;

                case CimType.UInt16:
                    obj2 = new ushort[length];
                    for (int k = 0; k < length; k++)
                    {
                        ((ushort[]) obj2)[k] = (ushort) ((int) array.GetValue(k));
                    }
                    return obj2;

                case CimType.UInt32:
                    obj2 = new uint[length];
                    for (int m = 0; m < length; m++)
                    {
                        ((uint[]) obj2)[m] = (uint) ((int) array.GetValue(m));
                    }
                    return obj2;

                case CimType.SInt64:
                    obj2 = new long[length];
                    for (int n = 0; n < length; n++)
                    {
                        ((long[]) obj2)[n] = Convert.ToInt64((string) array.GetValue(n), (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(long)));
                    }
                    return obj2;

                case CimType.UInt64:
                    obj2 = new ulong[length];
                    for (int num4 = 0; num4 < length; num4++)
                    {
                        ((ulong[]) obj2)[num4] = Convert.ToUInt64((string) array.GetValue(num4), (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof(ulong)));
                    }
                    return obj2;

                case CimType.Char16:
                    obj2 = new char[length];
                    for (int num7 = 0; num7 < length; num7++)
                    {
                        ((char[]) obj2)[num7] = (char) ((ushort) ((short) array.GetValue(num7)));
                    }
                    return obj2;
            }
            return wmiValue;
        }

        private void RefreshPropertyInfo()
        {
            this.propertyValue = null;
            int errorCode = this.parent.wbemObject.Get_(this.propertyName, 0, ref this.propertyValue, ref this.propertyType, ref this.propertyFlavor);
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public bool IsArray
        {
            get
            {
                this.RefreshPropertyInfo();
                return ((this.propertyType & 0x2000) != 0);
            }
        }

        public bool IsLocal
        {
            get
            {
                this.RefreshPropertyInfo();
                return ((this.propertyFlavor & 0x20) == 0);
            }
        }

        public string Name
        {
            get
            {
                if (this.propertyName == null)
                {
                    return "";
                }
                return this.propertyName;
            }
        }

        internal long NullEnumValue
        {
            get
            {
                return this.propertyNullEnumValue;
            }
            set
            {
                this.propertyNullEnumValue = value;
            }
        }

        public string Origin
        {
            get
            {
                string pstrClassName = null;
                int errorCode = this.parent.wbemObject.GetPropertyOrigin_(this.propertyName, out pstrClassName);
                if (errorCode < 0)
                {
                    if (errorCode == -2147217393)
                    {
                        return string.Empty;
                    }
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        return pstrClassName;
                    }
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return pstrClassName;
            }
        }

        public QualifierDataCollection Qualifiers
        {
            get
            {
                if (this.qualifiers == null)
                {
                    this.qualifiers = new QualifierDataCollection(this.parent, this.propertyName, QualifierType.PropertyQualifier);
                }
                return this.qualifiers;
            }
        }

        public CimType Type
        {
            get
            {
                this.RefreshPropertyInfo();
                return (((CimType) this.propertyType) & ((CimType) (-8193)));
            }
        }

        public object Value
        {
            get
            {
                this.RefreshPropertyInfo();
                return ValueTypeSafety.GetSafeObject(MapWmiValueToValue(this.propertyValue, ((CimType) this.propertyType) & ((CimType) (-8193)), 0 != (this.propertyType & 0x2000)));
            }
            set
            {
                this.RefreshPropertyInfo();
                object pVal = MapValueToWmiValue(value, ((CimType) this.propertyType) & ((CimType) (-8193)), 0 != (this.propertyType & 0x2000));
                int errorCode = this.parent.wbemObject.Put_(this.propertyName, 0, ref pVal, 0);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                else if (this.parent.GetType() == typeof(ManagementObject))
                {
                    ((ManagementObject) this.parent).Path.UpdateRelativePath((string) this.parent["__RELPATH"]);
                }
            }
        }
    }
}

