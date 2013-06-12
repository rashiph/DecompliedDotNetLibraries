namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class Converter
    {
        private static Type[] arrayTypeA;
        private static InternalPrimitiveTypeE[] codeA;
        private static int primitiveTypeEnumLength = 0x11;
        private static Type[] typeA;
        private static TypeCode[] typeCodeA;
        internal static Type typeofBoolean = typeof(bool);
        internal static Type typeofBooleanArray = typeof(bool[]);
        internal static Type typeofByte = typeof(byte);
        internal static Type typeofByteArray = typeof(byte[]);
        internal static Type typeofChar = typeof(char);
        internal static Type typeofCharArray = typeof(char[]);
        internal static Type typeofConverter = typeof(Converter);
        internal static Type typeofDateTime = typeof(DateTime);
        internal static Type typeofDateTimeArray = typeof(DateTime[]);
        internal static Type typeofDecimal = typeof(decimal);
        internal static Type typeofDecimalArray = typeof(decimal[]);
        internal static Type typeofDouble = typeof(double);
        internal static Type typeofDoubleArray = typeof(double[]);
        internal static Type typeofInt16 = typeof(short);
        internal static Type typeofInt16Array = typeof(short[]);
        internal static Type typeofInt32 = typeof(int);
        internal static Type typeofInt32Array = typeof(int[]);
        internal static Type typeofInt64 = typeof(long);
        internal static Type typeofInt64Array = typeof(long[]);
        internal static Type typeofISerializable = typeof(ISerializable);
        internal static Type typeofMarshalByRefObject = typeof(MarshalByRefObject);
        internal static Type typeofObject = typeof(object);
        internal static Type typeofObjectArray = typeof(object[]);
        internal static Type typeofSByte = typeof(sbyte);
        internal static Type typeofSByteArray = typeof(sbyte[]);
        internal static Type typeofSingle = typeof(float);
        internal static Type typeofSingleArray = typeof(float[]);
        internal static Type typeofString = typeof(string);
        internal static Type typeofStringArray = typeof(string[]);
        internal static Type typeofSystemVoid = typeof(void);
        internal static Type typeofTimeSpan = typeof(TimeSpan);
        internal static Type typeofTimeSpanArray = typeof(TimeSpan[]);
        internal static Type typeofTypeArray = typeof(Type[]);
        internal static Type typeofUInt16 = typeof(ushort);
        internal static Type typeofUInt16Array = typeof(ushort[]);
        internal static Type typeofUInt32 = typeof(uint);
        internal static Type typeofUInt32Array = typeof(uint[]);
        internal static Type typeofUInt64 = typeof(ulong);
        internal static Type typeofUInt64Array = typeof(ulong[]);
        internal static Assembly urtAssembly = Assembly.GetAssembly(typeofString);
        internal static string urtAssemblyString = urtAssembly.FullName;
        private static string[] valueA;

        private Converter()
        {
        }

        internal static Array CreatePrimitiveArray(InternalPrimitiveTypeE code, int length)
        {
            Array array = null;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    return new bool[length];

                case InternalPrimitiveTypeE.Byte:
                    return new byte[length];

                case InternalPrimitiveTypeE.Char:
                    return new char[length];

                case InternalPrimitiveTypeE.Currency:
                    return array;

                case InternalPrimitiveTypeE.Decimal:
                    return new decimal[length];

                case InternalPrimitiveTypeE.Double:
                    return new double[length];

                case InternalPrimitiveTypeE.Int16:
                    return new short[length];

                case InternalPrimitiveTypeE.Int32:
                    return new int[length];

                case InternalPrimitiveTypeE.Int64:
                    return new long[length];

                case InternalPrimitiveTypeE.SByte:
                    return new sbyte[length];

                case InternalPrimitiveTypeE.Single:
                    return new float[length];

                case InternalPrimitiveTypeE.TimeSpan:
                    return new TimeSpan[length];

                case InternalPrimitiveTypeE.DateTime:
                    return new DateTime[length];

                case InternalPrimitiveTypeE.UInt16:
                    return new ushort[length];

                case InternalPrimitiveTypeE.UInt32:
                    return new uint[length];

                case InternalPrimitiveTypeE.UInt64:
                    return new ulong[length];
            }
            return array;
        }

        internal static object FromString(string value, InternalPrimitiveTypeE code)
        {
            if (code != InternalPrimitiveTypeE.Invalid)
            {
                return Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture);
            }
            return value;
        }

        internal static InternalNameSpaceE GetNameSpaceEnum(InternalPrimitiveTypeE code, Type type, WriteObjectInfo objectInfo, out string typeName)
        {
            InternalNameSpaceE none = InternalNameSpaceE.None;
            typeName = null;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Char:
                case InternalPrimitiveTypeE.Double:
                case InternalPrimitiveTypeE.Int16:
                case InternalPrimitiveTypeE.Int32:
                case InternalPrimitiveTypeE.Int64:
                case InternalPrimitiveTypeE.SByte:
                case InternalPrimitiveTypeE.Single:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                case InternalPrimitiveTypeE.UInt16:
                case InternalPrimitiveTypeE.UInt32:
                case InternalPrimitiveTypeE.UInt64:
                    none = InternalNameSpaceE.XdrPrimitive;
                    typeName = "System." + ToComType(code);
                    break;

                case InternalPrimitiveTypeE.Decimal:
                    none = InternalNameSpaceE.UrtSystem;
                    typeName = "System." + ToComType(code);
                    break;
            }
            if ((none != InternalNameSpaceE.None) || (type == null))
            {
                return none;
            }
            if (object.ReferenceEquals(type, typeofString))
            {
                return InternalNameSpaceE.XdrString;
            }
            if (objectInfo == null)
            {
                typeName = type.FullName;
                if (type.Assembly == urtAssembly)
                {
                    return InternalNameSpaceE.UrtSystem;
                }
                return InternalNameSpaceE.UrtUser;
            }
            typeName = objectInfo.GetTypeFullName();
            if (objectInfo.GetAssemblyString().Equals(urtAssemblyString))
            {
                return InternalNameSpaceE.UrtSystem;
            }
            return InternalNameSpaceE.UrtUser;
        }

        private static void InitArrayTypeA()
        {
            Type[] typeArray = new Type[primitiveTypeEnumLength];
            typeArray[0] = null;
            typeArray[1] = typeofBooleanArray;
            typeArray[2] = typeofByteArray;
            typeArray[3] = typeofCharArray;
            typeArray[5] = typeofDecimalArray;
            typeArray[6] = typeofDoubleArray;
            typeArray[7] = typeofInt16Array;
            typeArray[8] = typeofInt32Array;
            typeArray[9] = typeofInt64Array;
            typeArray[10] = typeofSByteArray;
            typeArray[11] = typeofSingleArray;
            typeArray[12] = typeofTimeSpanArray;
            typeArray[13] = typeofDateTimeArray;
            typeArray[14] = typeofUInt16Array;
            typeArray[15] = typeofUInt32Array;
            typeArray[0x10] = typeofUInt64Array;
            arrayTypeA = typeArray;
        }

        private static void InitCodeA()
        {
            codeA = new InternalPrimitiveTypeE[] { 
                InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Boolean, InternalPrimitiveTypeE.Char, InternalPrimitiveTypeE.SByte, InternalPrimitiveTypeE.Byte, InternalPrimitiveTypeE.Int16, InternalPrimitiveTypeE.UInt16, InternalPrimitiveTypeE.Int32, InternalPrimitiveTypeE.UInt32, InternalPrimitiveTypeE.Int64, InternalPrimitiveTypeE.UInt64, InternalPrimitiveTypeE.Single, InternalPrimitiveTypeE.Double, InternalPrimitiveTypeE.Decimal, 
                InternalPrimitiveTypeE.DateTime, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid
             };
        }

        private static void InitTypeA()
        {
            Type[] typeArray = new Type[primitiveTypeEnumLength];
            typeArray[0] = null;
            typeArray[1] = typeofBoolean;
            typeArray[2] = typeofByte;
            typeArray[3] = typeofChar;
            typeArray[5] = typeofDecimal;
            typeArray[6] = typeofDouble;
            typeArray[7] = typeofInt16;
            typeArray[8] = typeofInt32;
            typeArray[9] = typeofInt64;
            typeArray[10] = typeofSByte;
            typeArray[11] = typeofSingle;
            typeArray[12] = typeofTimeSpan;
            typeArray[13] = typeofDateTime;
            typeArray[14] = typeofUInt16;
            typeArray[15] = typeofUInt32;
            typeArray[0x10] = typeofUInt64;
            typeA = typeArray;
        }

        private static void InitTypeCodeA()
        {
            TypeCode[] codeArray = new TypeCode[primitiveTypeEnumLength];
            codeArray[0] = TypeCode.Object;
            codeArray[1] = TypeCode.Boolean;
            codeArray[2] = TypeCode.Byte;
            codeArray[3] = TypeCode.Char;
            codeArray[5] = TypeCode.Decimal;
            codeArray[6] = TypeCode.Double;
            codeArray[7] = TypeCode.Int16;
            codeArray[8] = TypeCode.Int32;
            codeArray[9] = TypeCode.Int64;
            codeArray[10] = TypeCode.SByte;
            codeArray[11] = TypeCode.Single;
            codeArray[12] = TypeCode.Object;
            codeArray[13] = TypeCode.DateTime;
            codeArray[14] = TypeCode.UInt16;
            codeArray[15] = TypeCode.UInt32;
            codeArray[0x10] = TypeCode.UInt64;
            typeCodeA = codeArray;
        }

        private static void InitValueA()
        {
            string[] strArray = new string[primitiveTypeEnumLength];
            strArray[0] = null;
            strArray[1] = "Boolean";
            strArray[2] = "Byte";
            strArray[3] = "Char";
            strArray[5] = "Decimal";
            strArray[6] = "Double";
            strArray[7] = "Int16";
            strArray[8] = "Int32";
            strArray[9] = "Int64";
            strArray[10] = "SByte";
            strArray[11] = "Single";
            strArray[12] = "TimeSpan";
            strArray[13] = "DateTime";
            strArray[14] = "UInt16";
            strArray[15] = "UInt32";
            strArray[0x10] = "UInt64";
            valueA = strArray;
        }

        internal static bool IsPrimitiveArray(Type type, out object typeInformation)
        {
            typeInformation = null;
            bool flag = true;
            if (object.ReferenceEquals(type, typeofBooleanArray))
            {
                typeInformation = InternalPrimitiveTypeE.Boolean;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofByteArray))
            {
                typeInformation = InternalPrimitiveTypeE.Byte;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofCharArray))
            {
                typeInformation = InternalPrimitiveTypeE.Char;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofDoubleArray))
            {
                typeInformation = InternalPrimitiveTypeE.Double;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofInt16Array))
            {
                typeInformation = InternalPrimitiveTypeE.Int16;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofInt32Array))
            {
                typeInformation = InternalPrimitiveTypeE.Int32;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofInt64Array))
            {
                typeInformation = InternalPrimitiveTypeE.Int64;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofSByteArray))
            {
                typeInformation = InternalPrimitiveTypeE.SByte;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofSingleArray))
            {
                typeInformation = InternalPrimitiveTypeE.Single;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofUInt16Array))
            {
                typeInformation = InternalPrimitiveTypeE.UInt16;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofUInt32Array))
            {
                typeInformation = InternalPrimitiveTypeE.UInt32;
                return flag;
            }
            if (object.ReferenceEquals(type, typeofUInt64Array))
            {
                typeInformation = InternalPrimitiveTypeE.UInt64;
                return flag;
            }
            return false;
        }

        internal static bool IsWriteAsByteArray(InternalPrimitiveTypeE code)
        {
            bool flag = false;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Char:
                case InternalPrimitiveTypeE.Double:
                case InternalPrimitiveTypeE.Int16:
                case InternalPrimitiveTypeE.Int32:
                case InternalPrimitiveTypeE.Int64:
                case InternalPrimitiveTypeE.SByte:
                case InternalPrimitiveTypeE.Single:
                case InternalPrimitiveTypeE.UInt16:
                case InternalPrimitiveTypeE.UInt32:
                case InternalPrimitiveTypeE.UInt64:
                    return true;

                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return flag;
            }
            return flag;
        }

        internal static Type ToArrayType(InternalPrimitiveTypeE code)
        {
            if (arrayTypeA == null)
            {
                InitArrayTypeA();
            }
            return arrayTypeA[(int) code];
        }

        internal static InternalPrimitiveTypeE ToCode(Type type)
        {
            if ((type != null) && !type.IsPrimitive)
            {
                if (object.ReferenceEquals(type, typeofDateTime))
                {
                    return InternalPrimitiveTypeE.DateTime;
                }
                if (object.ReferenceEquals(type, typeofTimeSpan))
                {
                    return InternalPrimitiveTypeE.TimeSpan;
                }
                if (object.ReferenceEquals(type, typeofDecimal))
                {
                    return InternalPrimitiveTypeE.Decimal;
                }
                return InternalPrimitiveTypeE.Invalid;
            }
            return ToPrimitiveTypeEnum(Type.GetTypeCode(type));
        }

        internal static string ToComType(InternalPrimitiveTypeE code)
        {
            if (valueA == null)
            {
                InitValueA();
            }
            return valueA[(int) code];
        }

        internal static InternalPrimitiveTypeE ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (codeA == null)
            {
                InitCodeA();
            }
            return codeA[(int) typeCode];
        }

        internal static Type ToType(InternalPrimitiveTypeE code)
        {
            if (typeA == null)
            {
                InitTypeA();
            }
            return typeA[(int) code];
        }

        internal static TypeCode ToTypeCode(InternalPrimitiveTypeE code)
        {
            if (typeCodeA == null)
            {
                InitTypeCodeA();
            }
            return typeCodeA[(int) code];
        }

        internal static int TypeLength(InternalPrimitiveTypeE code)
        {
            int num = 0;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    return 1;

                case InternalPrimitiveTypeE.Byte:
                    return 1;

                case InternalPrimitiveTypeE.Char:
                    return 2;

                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return num;

                case InternalPrimitiveTypeE.Double:
                    return 8;

                case InternalPrimitiveTypeE.Int16:
                    return 2;

                case InternalPrimitiveTypeE.Int32:
                    return 4;

                case InternalPrimitiveTypeE.Int64:
                    return 8;

                case InternalPrimitiveTypeE.SByte:
                    return 1;

                case InternalPrimitiveTypeE.Single:
                    return 4;

                case InternalPrimitiveTypeE.UInt16:
                    return 2;

                case InternalPrimitiveTypeE.UInt32:
                    return 4;

                case InternalPrimitiveTypeE.UInt64:
                    return 8;
            }
            return num;
        }
    }
}

