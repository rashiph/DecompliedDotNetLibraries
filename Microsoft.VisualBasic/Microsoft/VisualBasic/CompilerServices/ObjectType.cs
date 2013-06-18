namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ObjectType
    {
        private static readonly CC[,] ConversionClassTable = new CC[,] { { CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err }, { CC.Err, CC.Same, CC.Narr, CC.Err, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Err, CC.Narr, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Same, CC.Err, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Err, CC.Narr, CC.Err, CC.Narr }, { CC.Err, CC.Err, CC.Err, CC.Same, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Same, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Err, CC.Narr, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Wide, CC.Same, CC.Narr, CC.Narr, CC.Narr, CC.Err, CC.Narr, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Wide, CC.Wide, CC.Same, CC.Narr, CC.Narr, CC.Err, CC.Narr, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Wide, CC.Wide, CC.Wide, CC.Same, CC.Narr, CC.Err, CC.Wide, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Wide, CC.Wide, CC.Wide, CC.Wide, CC.Same, CC.Err, CC.Wide, CC.Err, CC.Narr }, { CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Same, CC.Err, CC.Err, CC.Narr }, { CC.Err, CC.Narr, CC.Wide, CC.Err, CC.Wide, CC.Wide, CC.Wide, CC.Narr, CC.Narr, CC.Err, CC.Same, CC.Err, CC.Narr }, { CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err, CC.Err }, { CC.Err, CC.Narr, CC.Narr, CC.Wide, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Narr, CC.Err, CC.Same } };
        private const int TCMAX = 0x13;
        private static readonly VType[,] WiderType = new VType[,] { { VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad }, { VType.t_bad, VType.t_bool, VType.t_bool, VType.t_i2, VType.t_i4, VType.t_i8, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_bool, VType.t_ui1, VType.t_i2, VType.t_i4, VType.t_i8, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_i2, VType.t_i2, VType.t_i2, VType.t_i4, VType.t_i8, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_i4, VType.t_i4, VType.t_i4, VType.t_i4, VType.t_i8, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_i8, VType.t_i8, VType.t_i8, VType.t_i8, VType.t_i8, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_dec, VType.t_dec, VType.t_dec, VType.t_dec, VType.t_dec, VType.t_dec, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_r4, VType.t_r4, VType.t_r4, VType.t_r4, VType.t_r4, VType.t_r4, VType.t_r4, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_bad, VType.t_r8, VType.t_bad }, { VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_char, VType.t_str, VType.t_bad }, { VType.t_bad, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_r8, VType.t_str, VType.t_str, VType.t_date }, { VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_bad, VType.t_date, VType.t_date } };

        private static object AddByte(byte i1, byte i2)
        {
            short num = (short) (i1 + i2);
            if ((num >= 0) && (num <= 0xff))
            {
                return (byte) num;
            }
            return num;
        }

        private static object AddDecimal(IConvertible conv1, IConvertible conv2)
        {
            decimal num;
            if (conv1 != null)
            {
                num = conv1.ToDecimal(null);
            }
            decimal num2 = conv2.ToDecimal(null);
            try
            {
                return decimal.Add(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) + Convert.ToDouble(num2));
            }
        }

        private static object AddDouble(double d1, double d2)
        {
            return (d1 + d2);
        }

        private static object AddInt16(short i1, short i2)
        {
            int num = i1 + i2;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object AddInt32(int i1, int i2)
        {
            long num = i1 + i2;
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object AddInt64(long i1, long i2)
        {
            try
            {
                return (i1 + i2);
            }
            catch (OverflowException)
            {
                return decimal.Add(new decimal(i1), new decimal(i2));
            }
        }

        public static object AddObj(object o1, object o2)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = o1 as IConvertible;
            if (conv == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            if (((empty == TypeCode.Object) && (o1 is char[])) && (((typeCode == TypeCode.String) || (typeCode == TypeCode.Empty)) || ((typeCode == TypeCode.Object) && (o2 is char[]))))
            {
                o1 = new string(CharArrayType.FromObject(o1));
                conv = (IConvertible) o1;
                empty = TypeCode.String;
            }
            if (((typeCode == TypeCode.Object) && (o2 is char[])) && ((empty == TypeCode.String) || (empty == TypeCode.Empty)))
            {
                o2 = new string(CharArrayType.FromObject(o2));
                convertible2 = (IConvertible) o2;
                typeCode = TypeCode.String;
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return o2;

                case TypeCode.String:
                case ((TypeCode) 0x38):
                    return o2;

                case ((TypeCode) 0x39):
                case ((TypeCode) 0x72):
                case ((TypeCode) 0x85):
                case ((TypeCode) 0xab):
                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return o1;

                case ((TypeCode) 60):
                    return AddInt16((short) ToVBBool(conv), (short) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return AddInt16((short) ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x42):
                    return AddInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x44):
                    return AddInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 70):
                    return AddSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return AddDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x48):
                    return AddDecimal(ToVBBoolConv(conv), convertible2);

                case ((TypeCode) 0x4b):
                case ((TypeCode) 0x159):
                    return AddString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 80):
                case ((TypeCode) 0x5e):
                case ((TypeCode) 320):
                case ((TypeCode) 0x142):
                case ((TypeCode) 0x15a):
                case ((TypeCode) 0x166):
                case ((TypeCode) 360):
                    return (StringType.FromObject(o1) + StringType.FromObject(o2));

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return AddInt16(conv.ToInt16(null), (short) ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return AddByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return AddInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return AddInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                    return AddInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return AddSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return AddDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return AddDecimal(conv, convertible2);

                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return AddString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0xae):
                    return AddInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xd4):
                    return AddInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 250):
                    return AddSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return AddDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x120):
                    return AddDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0x156):
                case ((TypeCode) 0x158):
                    return o1;

                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return AddString(conv, empty, convertible2, typeCode);
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object AddSingle(float f1, float f2)
        {
            double d = f1 + f2;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(f1) && !float.IsInfinity(f2))))
            {
                return d;
            }
            return (float) d;
        }

        private static object AddString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return (num + num2);
        }

        public static object BitAndObj(object obj1, object obj2)
        {
            bool isEnum;
            bool flag2;
            if ((obj1 == null) && (obj2 == null))
            {
                return 0;
            }
            Type enumType = null;
            Type type = null;
            if (obj1 != null)
            {
                enumType = obj1.GetType();
                isEnum = enumType.IsEnum;
            }
            if (obj2 != null)
            {
                type = obj2.GetType();
                flag2 = type.IsEnum;
            }
            switch (GetWidestType(obj1, obj2, false))
            {
                case TypeCode.Boolean:
                    if (enumType != type)
                    {
                        return (short) (ShortType.FromObject(obj1) & ShortType.FromObject(obj2));
                    }
                    return (BooleanType.FromObject(obj1) & BooleanType.FromObject(obj2));

                case TypeCode.Byte:
                {
                    byte num = (byte) (ByteType.FromObject(obj1) & ByteType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num);
                    }
                    return num;
                }
                case TypeCode.Int16:
                {
                    short num2 = (short) (ShortType.FromObject(obj1) & ShortType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num2);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num2);
                    }
                    return num2;
                }
                case TypeCode.Int32:
                {
                    int num3 = IntegerType.FromObject(obj1) & IntegerType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num3);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num3);
                    }
                    return num3;
                }
                case TypeCode.Int64:
                {
                    long num4 = LongType.FromObject(obj1) & LongType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num4);
                        }
                        if (flag2)
                        {
                            return Enum.ToObject(type, num4);
                        }
                        break;
                    }
                    return num4;
                }
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return (LongType.FromObject(obj1) & LongType.FromObject(obj2));
            }
            throw GetNoValidOperatorException(obj1, obj2);
        }

        public static object BitOrObj(object obj1, object obj2)
        {
            bool isEnum;
            bool flag2;
            if ((obj1 == null) && (obj2 == null))
            {
                return 0;
            }
            Type enumType = null;
            Type type = null;
            if (obj1 != null)
            {
                enumType = obj1.GetType();
                isEnum = enumType.IsEnum;
            }
            if (obj2 != null)
            {
                type = obj2.GetType();
                flag2 = type.IsEnum;
            }
            switch (GetWidestType(obj1, obj2, false))
            {
                case TypeCode.Boolean:
                    if (enumType != type)
                    {
                        return (short) (ShortType.FromObject(obj1) | ShortType.FromObject(obj2));
                    }
                    return (BooleanType.FromObject(obj1) | BooleanType.FromObject(obj2));

                case TypeCode.Byte:
                {
                    byte num = (byte) (ByteType.FromObject(obj1) | ByteType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num);
                    }
                    return num;
                }
                case TypeCode.Int16:
                {
                    short num2 = (short) (ShortType.FromObject(obj1) | ShortType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num2);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num2);
                    }
                    return num2;
                }
                case TypeCode.Int32:
                {
                    int num3 = IntegerType.FromObject(obj1) | IntegerType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num3);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num3);
                    }
                    return num3;
                }
                case TypeCode.Int64:
                {
                    long num4 = LongType.FromObject(obj1) | LongType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num4);
                        }
                        if (flag2)
                        {
                            return Enum.ToObject(type, num4);
                        }
                        break;
                    }
                    return num4;
                }
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return (LongType.FromObject(obj1) | LongType.FromObject(obj2));
            }
            throw GetNoValidOperatorException(obj1, obj2);
        }

        public static object BitXorObj(object obj1, object obj2)
        {
            bool isEnum;
            bool flag2;
            if ((obj1 == null) && (obj2 == null))
            {
                return 0;
            }
            Type enumType = null;
            Type type = null;
            if (obj1 != null)
            {
                enumType = obj1.GetType();
                isEnum = enumType.IsEnum;
            }
            if (obj2 != null)
            {
                type = obj2.GetType();
                flag2 = type.IsEnum;
            }
            switch (GetWidestType(obj1, obj2, false))
            {
                case TypeCode.Boolean:
                    if (enumType != type)
                    {
                        return (short) (ShortType.FromObject(obj1) ^ ShortType.FromObject(obj2));
                    }
                    return (BooleanType.FromObject(obj1) ^ BooleanType.FromObject(obj2));

                case TypeCode.Byte:
                {
                    byte num = (byte) (ByteType.FromObject(obj1) ^ ByteType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num);
                    }
                    return num;
                }
                case TypeCode.Int16:
                {
                    short num2 = (short) (ShortType.FromObject(obj1) ^ ShortType.FromObject(obj2));
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num2);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num2);
                    }
                    return num2;
                }
                case TypeCode.Int32:
                {
                    int num3 = IntegerType.FromObject(obj1) ^ IntegerType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num3);
                        }
                        if (!flag2)
                        {
                            break;
                        }
                        return Enum.ToObject(type, num3);
                    }
                    return num3;
                }
                case TypeCode.Int64:
                {
                    long num4 = LongType.FromObject(obj1) ^ LongType.FromObject(obj2);
                    if (((!isEnum || !flag2) || (enumType == type)) && (isEnum && flag2))
                    {
                        if (isEnum)
                        {
                            return Enum.ToObject(enumType, num4);
                        }
                        if (flag2)
                        {
                            return Enum.ToObject(type, num4);
                        }
                        break;
                    }
                    return num4;
                }
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return (LongType.FromObject(obj1) ^ LongType.FromObject(obj2));
            }
            throw GetNoValidOperatorException(obj1, obj2);
        }

        internal static object CTypeHelper(object obj, Type toType)
        {
            bool flag;
            object objectValuePrimitive;
            if (obj == null)
            {
                return null;
            }
            if (toType == typeof(object))
            {
                return obj;
            }
            Type typ = obj.GetType();
            if (toType.IsByRef)
            {
                toType = toType.GetElementType();
                flag = true;
            }
            if (typ.IsByRef)
            {
                typ = typ.GetElementType();
            }
            if ((typ == toType) || (toType == typeof(object)))
            {
                if (!flag)
                {
                    return obj;
                }
                objectValuePrimitive = GetObjectValuePrimitive(obj);
            }
            else
            {
                TypeCode typeCode = Type.GetTypeCode(toType);
                if (typeCode == TypeCode.Object)
                {
                    if ((toType == typeof(object)) || toType.IsInstanceOfType(obj))
                    {
                        return obj;
                    }
                    string str = obj as string;
                    if ((str == null) || (toType != typeof(char[])))
                    {
                        throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(typ), Utils.VBFriendlyName(toType) }));
                    }
                    return CharArrayType.FromString(str);
                }
                objectValuePrimitive = CTypeHelper(obj, typeCode);
            }
            if (toType.IsEnum)
            {
                return Enum.ToObject(toType, objectValuePrimitive);
            }
            return objectValuePrimitive;
        }

        internal static object CTypeHelper(object obj, TypeCode toType)
        {
            if (obj == null)
            {
                return null;
            }
            switch (toType)
            {
                case TypeCode.Boolean:
                    return BooleanType.FromObject(obj);

                case TypeCode.Char:
                    return Microsoft.VisualBasic.CompilerServices.CharType.FromObject(obj);

                case TypeCode.Byte:
                    return ByteType.FromObject(obj);

                case TypeCode.Int16:
                    return ShortType.FromObject(obj);

                case TypeCode.Int32:
                    return IntegerType.FromObject(obj);

                case TypeCode.Int64:
                    return LongType.FromObject(obj);

                case TypeCode.Single:
                    return SingleType.FromObject(obj);

                case TypeCode.Double:
                    return DoubleType.FromObject(obj);

                case TypeCode.Decimal:
                    return DecimalType.FromObject(obj);

                case TypeCode.DateTime:
                    return DateType.FromObject(obj);

                case TypeCode.String:
                    return StringType.FromObject(obj);
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(obj), Utils.VBFriendlyName(TypeFromTypeCode(toType)) }));
        }

        private static object DivDecimal(IConvertible conv1, IConvertible conv2)
        {
            decimal num;
            decimal num2;
            if (conv1 != null)
            {
                num = conv1.ToDecimal(null);
            }
            if (conv2 != null)
            {
                num2 = conv2.ToDecimal(null);
            }
            try
            {
                return decimal.Divide(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToSingle(num) / Convert.ToSingle(num2));
            }
        }

        private static object DivDouble(double d1, double d2)
        {
            return (d1 / d2);
        }

        public static object DivObj(object o1, object o2)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = o1 as IConvertible;
            if (convertible == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = convertible.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return DivDouble(0.0, 0.0);

                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return DivDouble(0.0, convertible2.ToDouble(null));

                case TypeCode.String:
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 0x39):
                    return DivDouble((double) ToVBBool(convertible), 0.0);

                case ((TypeCode) 60):
                    return DivDouble((double) ToVBBool(convertible), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                case ((TypeCode) 0x42):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x47):
                    return DivDouble((double) ToVBBool(convertible), convertible2.ToDouble(null));

                case ((TypeCode) 70):
                    return DivSingle((float) ToVBBool(convertible), convertible2.ToSingle(null));

                case ((TypeCode) 0x48):
                    return DivDecimal(ToVBBoolConv(convertible), (IConvertible) convertible2.ToDecimal(null));

                case ((TypeCode) 0x4b):
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 0x72):
                case ((TypeCode) 0x85):
                case ((TypeCode) 0xab):
                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return DivDouble(convertible.ToDouble(null), 0.0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                case ((TypeCode) 0xae):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0x10d):
                    return DivDouble(convertible.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 120):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return DivDouble(convertible.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return DivSingle(convertible.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return DivDecimal(convertible, convertible2);

                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 250):
                    return DivSingle(convertible.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x120):
                    return DivDecimal(convertible, ToVBBoolConv(convertible2));

                case ((TypeCode) 0x156):
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 0x159):
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return DivString(convertible, empty, convertible2, typeCode);

                case ((TypeCode) 360):
                    return DivStringString(convertible.ToString(null), convertible2.ToString(null));
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object DivSingle(float sng1, float sng2)
        {
            float f = sng1 / sng2;
            if (float.IsInfinity(f) && (!float.IsInfinity(sng1) && !float.IsInfinity(sng2)))
            {
                return (((double) sng1) / ((double) sng2));
            }
            return f;
        }

        private static object DivString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return (num / num2);
        }

        private static object DivStringString(string s1, string s2)
        {
            double num;
            double num2;
            if (s1 != null)
            {
                num = DoubleType.FromString(s1);
            }
            if (s2 != null)
            {
                num2 = DoubleType.FromString(s2);
            }
            return (num / num2);
        }

        private static Exception GetNoValidOperatorException(object Operand)
        {
            return new InvalidCastException(Utils.GetResourceString("NoValidOperator_OneOperand", new string[] { Utils.VBFriendlyName(Operand) }));
        }

        private static Exception GetNoValidOperatorException(object Left, object Right)
        {
            string resourceString;
            string str2;
            if (Left == null)
            {
                resourceString = "'Nothing'";
            }
            else
            {
                string str3 = Left as string;
                if (str3 != null)
                {
                    resourceString = Utils.GetResourceString("NoValidOperator_StringType1", new string[] { Strings.Left(str3, 0x20) });
                }
                else
                {
                    resourceString = Utils.GetResourceString("NoValidOperator_NonStringType1", new string[] { Utils.VBFriendlyName(Left) });
                }
            }
            if (Right == null)
            {
                str2 = "'Nothing'";
            }
            else
            {
                string str = Right as string;
                if (str != null)
                {
                    str2 = Utils.GetResourceString("NoValidOperator_StringType1", new string[] { Strings.Left(str, 0x20) });
                }
                else
                {
                    str2 = Utils.GetResourceString("NoValidOperator_NonStringType1", new string[] { Utils.VBFriendlyName(Right) });
                }
            }
            return new InvalidCastException(Utils.GetResourceString("NoValidOperator_TwoOperands", new string[] { resourceString, str2 }));
        }

        public static object GetObjectValuePrimitive(object o)
        {
            if (o == null)
            {
                return null;
            }
            IConvertible convertible = o as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return convertible.ToBoolean(null);

                    case TypeCode.Char:
                        return convertible.ToChar(null);

                    case TypeCode.SByte:
                        return convertible.ToSByte(null);

                    case TypeCode.Byte:
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        return convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        return convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        return convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        return convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        return convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        return convertible.ToUInt64(null);

                    case TypeCode.Single:
                        return convertible.ToSingle(null);

                    case TypeCode.Double:
                        return convertible.ToDouble(null);

                    case TypeCode.Decimal:
                        return convertible.ToDecimal(null);

                    case TypeCode.DateTime:
                        return convertible.ToDateTime(null);

                    case (TypeCode.DateTime | TypeCode.Object):
                        return o;

                    case TypeCode.String:
                        return o;
                }
            }
            return o;
        }

        internal static TypeCode GetWidestType(object obj1, TypeCode type2)
        {
            TypeCode typeCode;
            IConvertible convertible = obj1 as IConvertible;
            if (convertible != null)
            {
                typeCode = convertible.GetTypeCode();
            }
            else if (obj1 == null)
            {
                typeCode = TypeCode.Empty;
            }
            else if ((obj1 is char[]) && (((Array) obj1).Rank == 1))
            {
                typeCode = TypeCode.String;
            }
            else
            {
                typeCode = TypeCode.Object;
            }
            if (obj1 == null)
            {
                return type2;
            }
            return TypeCodeFromVType(WiderType[(int) VTypeFromTypeCode(typeCode), (int) VTypeFromTypeCode(type2)]);
        }

        internal static TypeCode GetWidestType(object obj1, object obj2, bool IsAdd = false)
        {
            TypeCode typeCode;
            TypeCode empty;
            IConvertible convertible = obj1 as IConvertible;
            IConvertible convertible2 = obj2 as IConvertible;
            if (convertible != null)
            {
                typeCode = convertible.GetTypeCode();
            }
            else if (obj1 == null)
            {
                typeCode = TypeCode.Empty;
            }
            else if ((obj1 is char[]) && (((Array) obj1).Rank == 1))
            {
                typeCode = TypeCode.String;
            }
            else
            {
                typeCode = TypeCode.Object;
            }
            if (convertible2 != null)
            {
                empty = convertible2.GetTypeCode();
            }
            else if (obj2 == null)
            {
                empty = TypeCode.Empty;
            }
            else if ((obj2 is char[]) && (((Array) obj2).Rank == 1))
            {
                empty = TypeCode.String;
            }
            else
            {
                empty = TypeCode.Object;
            }
            if (obj1 == null)
            {
                return empty;
            }
            if (obj2 == null)
            {
                return typeCode;
            }
            if (!IsAdd || (((typeCode != TypeCode.DBNull) || (empty != TypeCode.String)) && ((typeCode != TypeCode.String) || (empty != TypeCode.DBNull))))
            {
                return TypeCodeFromVType(WiderType[(int) VTypeFromTypeCode(typeCode), (int) VTypeFromTypeCode(empty)]);
            }
            return TypeCode.DBNull;
        }

        private static object IDivideByte(byte d1, byte d2)
        {
            return (byte) (d1 / d2);
        }

        private static object IDivideInt16(short d1, short d2)
        {
            return (short) (d1 / d2);
        }

        private static object IDivideInt32(int d1, int d2)
        {
            return (d1 / d2);
        }

        private static object IDivideInt64(long d1, long d2)
        {
            return (d1 / d2);
        }

        private static object IDivideString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            long num;
            long num2;
            if (tc1 == TypeCode.String)
            {
                try
                {
                    num = LongType.FromString(conv1.ToString(null));
                    goto Label_0040;
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw GetNoValidOperatorException(conv1, conv2);
                }
            }
            if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToInt64(null);
            }
        Label_0040:
            if (tc2 == TypeCode.String)
            {
                try
                {
                    num2 = LongType.FromString(conv2.ToString(null));
                    goto Label_0082;
                }
                catch (StackOverflowException exception4)
                {
                    throw exception4;
                }
                catch (OutOfMemoryException exception5)
                {
                    throw exception5;
                }
                catch (ThreadAbortException exception6)
                {
                    throw exception6;
                }
                catch (Exception)
                {
                    throw GetNoValidOperatorException(conv1, conv2);
                }
            }
            if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToInt64(null);
            }
        Label_0082:
            return (num / num2);
        }

        private static object IDivideStringString(string s1, string s2)
        {
            long num;
            long num2;
            if (s1 != null)
            {
                num = LongType.FromString(s1);
            }
            if (s2 != null)
            {
                num2 = LongType.FromString(s2);
            }
            return (num / num2);
        }

        public static object IDivObj(object o1, object o2)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = o1 as IConvertible;
            if (conv == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return IDivideInt32(0, 0);

                case TypeCode.Boolean:
                    return IDivideInt64(0L, (long) ToVBBool(convertible2));

                case TypeCode.Byte:
                    return IDivideByte(0, convertible2.ToByte(null));

                case TypeCode.Int16:
                    return IDivideInt16(0, convertible2.ToInt16(null));

                case TypeCode.Int32:
                    return IDivideInt32(0, convertible2.ToInt32(null));

                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return IDivideInt64(0L, convertible2.ToInt64(null));

                case TypeCode.String:
                    return IDivideInt64(0L, LongType.FromString(convertible2.ToString(null)));

                case ((TypeCode) 0x39):
                    return IDivideInt16((short) ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return IDivideInt16((short) ToVBBool(conv), (short) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return IDivideInt16((short) ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x42):
                    return IDivideInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x44):
                case ((TypeCode) 70):
                case ((TypeCode) 0x47):
                case ((TypeCode) 0x48):
                    return IDivideInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x4b):
                    return IDivideInt64((long) ToVBBool(conv), LongType.FromString(convertible2.ToString(null)));

                case ((TypeCode) 0x72):
                    return IDivideByte(conv.ToByte(null), 0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return IDivideInt16(conv.ToInt16(null), (short) ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return IDivideByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return IDivideInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return IDivideInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x12a):
                case ((TypeCode) 0x12b):
                case ((TypeCode) 300):
                    return IDivideInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return IDivideString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0x85):
                    return IDivideInt16(conv.ToInt16(null), 0);

                case ((TypeCode) 0xab):
                    return IDivideInt32(conv.ToInt32(null), 0);

                case ((TypeCode) 0xae):
                    return IDivideInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return IDivideInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0xd4):
                case ((TypeCode) 250):
                case ((TypeCode) 0x10d):
                case ((TypeCode) 0x120):
                    return IDivideInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0x156):
                    return IDivideInt64(LongType.FromString(conv.ToString(null)), 0L);

                case ((TypeCode) 0x159):
                    return IDivideInt64(LongType.FromString(conv.ToString(null)), (long) ToVBBool(convertible2));

                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return IDivideInt64(LongType.FromString(conv.ToString(null)), convertible2.ToInt64(null));

                case ((TypeCode) 360):
                    return IDivideStringString(conv.ToString(null), convertible2.ToString(null));
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object InternalNegObj(object obj, IConvertible conv, TypeCode tc)
        {
            switch (tc)
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Boolean:
                    if (obj is bool)
                    {
                        return -((short) -(((bool) obj) > false));
                    }
                    return -((short) -(conv.ToBoolean(null) > false));

                case TypeCode.Byte:
                    if (obj is byte)
                    {
                        return (short) -((byte) obj);
                    }
                    return (short) -conv.ToByte(null);

                case TypeCode.Int16:
                    int num4;
                    if (obj is short)
                    {
                        num4 = 0 - ((short) obj);
                    }
                    else
                    {
                        num4 = 0 - conv.ToInt16(null);
                    }
                    if ((num4 >= -32768) && (num4 <= 0x7fff))
                    {
                        return (short) num4;
                    }
                    return num4;

                case TypeCode.Int32:
                    long num5;
                    if (obj is int)
                    {
                        num5 = 0L - ((int) obj);
                    }
                    else
                    {
                        num5 = 0L - conv.ToInt32(null);
                    }
                    if ((num5 >= -2147483648L) && (num5 <= 0x7fffffffL))
                    {
                        return (int) num5;
                    }
                    return num5;

                case TypeCode.Int64:
                    try
                    {
                        if (obj is long)
                        {
                            return (0L - ((long) obj));
                        }
                        return (0L - conv.ToInt64(null));
                    }
                    catch (StackOverflowException exception)
                    {
                        throw exception;
                    }
                    catch (OutOfMemoryException exception2)
                    {
                        throw exception2;
                    }
                    catch (ThreadAbortException exception3)
                    {
                        throw exception3;
                    }
                    catch (Exception)
                    {
                        return decimal.Negate(conv.ToDecimal(null));
                    }
                    break;

                case TypeCode.Single:
                    goto Label_01B9;

                case TypeCode.Double:
                    if (obj is double)
                    {
                        return -((double) obj);
                    }
                    return -conv.ToDouble(null);

                case TypeCode.Decimal:
                    break;

                case TypeCode.String:
                {
                    string str = obj as string;
                    if (str == null)
                    {
                        return -DoubleType.FromString(conv.ToString(null));
                    }
                    return -DoubleType.FromString(str);
                }
                default:
                    throw GetNoValidOperatorException(obj);
            }
            try
            {
                if (obj is decimal)
                {
                    return decimal.Negate((decimal) obj);
                }
                return decimal.Negate(conv.ToDecimal(null));
            }
            catch (StackOverflowException exception4)
            {
                throw exception4;
            }
            catch (OutOfMemoryException exception5)
            {
                throw exception5;
            }
            catch (ThreadAbortException exception6)
            {
                throw exception6;
            }
            catch (Exception)
            {
                return -conv.ToDouble(null);
            }
        Label_01B9:
            if (obj is float)
            {
                return -((float) obj);
            }
            return -conv.ToSingle(null);
        }

        internal static bool IsWideningConversion(Type FromType, Type ToType)
        {
            TypeCode typeCode = Type.GetTypeCode(FromType);
            TypeCode typ = Type.GetTypeCode(ToType);
            if (typeCode == TypeCode.Object)
            {
                if ((FromType == typeof(char[])) && ((typ == TypeCode.String) || (ToType == typeof(char[]))))
                {
                    return true;
                }
                if (typ == TypeCode.Object)
                {
                    if (!FromType.IsArray || !ToType.IsArray)
                    {
                        return ToType.IsAssignableFrom(FromType);
                    }
                    if (FromType.GetArrayRank() == ToType.GetArrayRank())
                    {
                        return ToType.GetElementType().IsAssignableFrom(FromType.GetElementType());
                    }
                }
                return false;
            }
            if (typ == TypeCode.Object)
            {
                if ((ToType == typeof(char[])) && (typeCode == TypeCode.String))
                {
                    return false;
                }
                return ToType.IsAssignableFrom(FromType);
            }
            if (ToType.IsEnum)
            {
                return false;
            }
            CC cc = ConversionClassTable[(int) VType2FromTypeCode(typ), (int) VType2FromTypeCode(typeCode)];
            if ((cc != CC.Wide) && (cc != CC.Same))
            {
                return false;
            }
            return true;
        }

        internal static bool IsWiderNumeric(Type Type1, Type Type2)
        {
            TypeCode typeCode = Type.GetTypeCode(Type1);
            TypeCode typCode = Type.GetTypeCode(Type2);
            if (!Information.IsOldNumericTypeCode(typeCode) || !Information.IsOldNumericTypeCode(typCode))
            {
                return false;
            }
            if ((typeCode == TypeCode.Boolean) || (typCode == TypeCode.Boolean))
            {
                return false;
            }
            if (Type1.IsEnum)
            {
                return false;
            }
            return (WiderType[(int) VTypeFromTypeCode(typeCode), (int) VTypeFromTypeCode(typCode)] == VTypeFromTypeCode(typeCode));
        }

        public static bool LikeObj(object vLeft, object vRight, CompareMethod CompareOption)
        {
            return StringType.StrLike(StringType.FromObject(vLeft), StringType.FromObject(vRight), CompareOption);
        }

        private static object ModByte(byte i1, byte i2)
        {
            return (byte) (i1 % i2);
        }

        private static object ModDecimal(IConvertible conv1, IConvertible conv2)
        {
            decimal num;
            decimal num2;
            if (conv1 != null)
            {
                num = conv1.ToDecimal(null);
            }
            if (conv2 != null)
            {
                num2 = conv2.ToDecimal(null);
            }
            return decimal.Remainder(num, num2);
        }

        private static object ModDouble(double d1, double d2)
        {
            return (d1 % d2);
        }

        private static object ModInt16(short i1, short i2)
        {
            int num = i1 % i2;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object ModInt32(int i1, int i2)
        {
            long num = ((long) i1) % ((long) i2);
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object ModInt64(long i1, long i2)
        {
            try
            {
                return (i1 % i2);
            }
            catch (OverflowException)
            {
                decimal num = decimal.Remainder(new decimal(i1), new decimal(i2));
                if ((decimal.Compare(num, -9223372036854775808M) < 0) || (decimal.Compare(num, 9223372036854775807M) > 0))
                {
                    return num;
                }
                return Convert.ToInt64(num);
            }
        }

        public static object ModObj(object o1, object o2)
        {
            TypeCode typeCode;
            TypeCode empty;
            IConvertible convertible = o1 as IConvertible;
            IConvertible conv = o2 as IConvertible;
            if (convertible != null)
            {
                typeCode = convertible.GetTypeCode();
            }
            else if (o1 == null)
            {
                typeCode = TypeCode.Empty;
            }
            else
            {
                typeCode = TypeCode.Object;
            }
            if (conv != null)
            {
                empty = conv.GetTypeCode();
            }
            else
            {
                conv = null;
                if (o2 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            switch (((typeCode * (TypeCode.String | TypeCode.Object)) + empty))
            {
                case TypeCode.Empty:
                    return ModInt32(0, 0);

                case TypeCode.Boolean:
                    return ModInt16(0, (short) ToVBBool(conv));

                case TypeCode.Byte:
                    return ModByte(0, conv.ToByte(null));

                case TypeCode.Int16:
                    return ModInt16(0, (short) ToVBBool(conv));

                case TypeCode.Int32:
                    return ModInt32(0, conv.ToInt32(null));

                case TypeCode.Int64:
                    return ModInt64(0L, conv.ToInt64(null));

                case TypeCode.Single:
                    return ModSingle(0f, conv.ToSingle(null));

                case TypeCode.Double:
                    return ModDouble(0.0, conv.ToDouble(null));

                case TypeCode.Decimal:
                    return ModDecimal(null, conv);

                case TypeCode.String:
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 0x39):
                    return ModInt16((short) ToVBBool(convertible), 0);

                case ((TypeCode) 60):
                    return ModInt16((short) ToVBBool(convertible), (short) ToVBBool(conv));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return ModInt16((short) ToVBBool(convertible), conv.ToInt16(null));

                case ((TypeCode) 0x42):
                    return ModInt32(ToVBBool(convertible), conv.ToInt32(null));

                case ((TypeCode) 0x44):
                    return ModInt64((long) ToVBBool(convertible), conv.ToInt64(null));

                case ((TypeCode) 70):
                    return ModSingle((float) ToVBBool(convertible), conv.ToSingle(null));

                case ((TypeCode) 0x47):
                    return ModDouble((double) ToVBBool(convertible), conv.ToDouble(null));

                case ((TypeCode) 0x48):
                    return ModDecimal(ToVBBoolConv(convertible), conv);

                case ((TypeCode) 0x4b):
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 0x72):
                    return ModByte(convertible.ToByte(null), 0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return ModInt16(convertible.ToInt16(null), (short) ToVBBool(conv));

                case ((TypeCode) 120):
                    return ModByte(convertible.ToByte(null), conv.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return ModInt16(convertible.ToInt16(null), conv.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return ModInt32(convertible.ToInt32(null), conv.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                    return ModInt64(convertible.ToInt64(null), conv.ToInt64(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return ModSingle(convertible.ToSingle(null), conv.ToSingle(null));

                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return ModDouble(convertible.ToDouble(null), conv.ToDouble(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return ModDecimal(convertible, conv);

                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 0x85):
                    return ModInt16(convertible.ToInt16(null), 0);

                case ((TypeCode) 0xab):
                    return ModInt32(convertible.ToInt32(null), 0);

                case ((TypeCode) 0xae):
                    return ModInt32(convertible.ToInt32(null), ToVBBool(conv));

                case ((TypeCode) 0xd1):
                    return ModInt64(convertible.ToInt64(null), 0L);

                case ((TypeCode) 0xd4):
                    return ModInt64(convertible.ToInt64(null), (long) ToVBBool(conv));

                case ((TypeCode) 0xf7):
                    return ModSingle(convertible.ToSingle(null), 0f);

                case ((TypeCode) 250):
                    return ModSingle(convertible.ToSingle(null), (float) ToVBBool(conv));

                case ((TypeCode) 0x10a):
                    return ModDouble(convertible.ToDouble(null), 0.0);

                case ((TypeCode) 0x10d):
                    return ModDouble(convertible.ToDouble(null), (double) ToVBBool(conv));

                case ((TypeCode) 0x11d):
                    return ModDecimal(convertible, null);

                case ((TypeCode) 0x120):
                    return ModDecimal(convertible, ToVBBoolConv(conv));

                case ((TypeCode) 0x156):
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 0x159):
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return ModString(convertible, typeCode, conv, empty);

                case ((TypeCode) 360):
                    return ModStringString(convertible.ToString(null), conv.ToString(null));
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object ModSingle(float sng1, float sng2)
        {
            return (sng1 % sng2);
        }

        private static object ModString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return (num % num2);
        }

        private static object ModStringString(string s1, string s2)
        {
            double num;
            double num2;
            if (s1 != null)
            {
                num = DoubleType.FromString(s1);
            }
            if (s2 != null)
            {
                num2 = DoubleType.FromString(s2);
            }
            return (num % num2);
        }

        private static object MulByte(byte i1, byte i2)
        {
            int num = i1 * i2;
            if ((num >= 0) && (num <= 0xff))
            {
                return (byte) num;
            }
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object MulDecimal(IConvertible conv1, IConvertible conv2)
        {
            decimal num = conv1.ToDecimal(null);
            decimal num2 = conv2.ToDecimal(null);
            try
            {
                return decimal.Multiply(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) * Convert.ToDouble(num2));
            }
        }

        private static object MulDouble(double d1, double d2)
        {
            return (d1 * d2);
        }

        private static object MulInt16(short i1, short i2)
        {
            int num = i1 * i2;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object MulInt32(int i1, int i2)
        {
            long num = i1 * i2;
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object MulInt64(long i1, long i2)
        {
            object obj2;
            try
            {
                obj2 = i1 * i2;
            }
            catch (OverflowException)
            {
                try
                {
                    obj2 = decimal.Multiply(new decimal(i1), new decimal(i2));
                }
                catch (OverflowException)
                {
                    obj2 = i1 * i2;
                }
            }
            return obj2;
        }

        public static object MulObj(object o1, object o2)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = o1 as IConvertible;
            if (conv == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                case TypeCode.Int32:
                case ((TypeCode) 0xab):
                    return 0;

                case TypeCode.Boolean:
                case TypeCode.Int16:
                case ((TypeCode) 0x39):
                case ((TypeCode) 0x85):
                    return (short) 0;

                case TypeCode.Byte:
                case ((TypeCode) 0x72):
                    return (byte) 0;

                case TypeCode.Int64:
                case ((TypeCode) 0xd1):
                    return 0L;

                case TypeCode.Single:
                case ((TypeCode) 0xf7):
                    return 0f;

                case TypeCode.Double:
                case ((TypeCode) 0x10a):
                    return 0.0;

                case TypeCode.Decimal:
                case ((TypeCode) 0x11d):
                    return decimal.Zero;

                case TypeCode.String:
                case ((TypeCode) 0x156):
                    return 0.0;

                case ((TypeCode) 60):
                    return MulInt16((short) ToVBBool(conv), (short) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return MulInt16((short) ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x42):
                    return MulInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x44):
                    return MulInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 70):
                    return MulSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return MulDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x48):
                    return MulDecimal(ToVBBoolConv(conv), convertible2);

                case ((TypeCode) 0x4b):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                case ((TypeCode) 0x159):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return MulString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return MulInt16(conv.ToInt16(null), (short) ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return MulByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return MulInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return MulInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                    return MulInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return MulSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return MulDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return MulDecimal(conv, convertible2);

                case ((TypeCode) 0xae):
                    return MulInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xd4):
                    return MulInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 250):
                    return MulSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return MulDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x120):
                    return MulDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 360):
                    return MulStringString(conv.ToString(null), convertible2.ToString(null));
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object MulSingle(float f1, float f2)
        {
            double d = f1 * f2;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(f1) && !float.IsInfinity(f2))))
            {
                return d;
            }
            return (float) d;
        }

        private static object MulString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return (num * num2);
        }

        private static object MulStringString(string s1, string s2)
        {
            double num;
            double num2;
            if (s1 != null)
            {
                num = DoubleType.FromString(s1);
            }
            if (s2 != null)
            {
                num2 = DoubleType.FromString(s2);
            }
            return (num * num2);
        }

        public static object NegObj(object obj)
        {
            TypeCode empty;
            IConvertible conv = obj as IConvertible;
            if (conv == null)
            {
                if (obj == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            return InternalNegObj(obj, conv, empty);
        }

        public static object NotObj(object obj)
        {
            Type type;
            TypeCode typeCode;
            if (obj == null)
            {
                return -1;
            }
            IConvertible convertible = obj as IConvertible;
            if (convertible != null)
            {
                typeCode = convertible.GetTypeCode();
            }
            else
            {
                typeCode = TypeCode.Object;
            }
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return !convertible.ToBoolean(null);

                case TypeCode.Byte:
                {
                    type = obj.GetType();
                    byte num = ~convertible.ToByte(null);
                    if (!type.IsEnum)
                    {
                        return num;
                    }
                    return Enum.ToObject(type, num);
                }
                case TypeCode.Int16:
                {
                    type = obj.GetType();
                    short num2 = ~convertible.ToInt16(null);
                    if (!type.IsEnum)
                    {
                        return num2;
                    }
                    return Enum.ToObject(type, num2);
                }
                case TypeCode.Int32:
                {
                    type = obj.GetType();
                    int num3 = ~convertible.ToInt32(null);
                    if (!type.IsEnum)
                    {
                        return num3;
                    }
                    return Enum.ToObject(type, num3);
                }
                case TypeCode.Int64:
                {
                    type = obj.GetType();
                    long num4 = ~convertible.ToInt64(null);
                    if (!type.IsEnum)
                    {
                        return num4;
                    }
                    return Enum.ToObject(type, num4);
                }
                case TypeCode.Single:
                    return ~Convert.ToInt64(convertible.ToDecimal(null));

                case TypeCode.Double:
                    return ~Convert.ToInt64(convertible.ToDecimal(null));

                case TypeCode.Decimal:
                    return ~Convert.ToInt64(convertible.ToDecimal(null));

                case TypeCode.String:
                    return ~LongType.FromString(convertible.ToString(null));
            }
            throw GetNoValidOperatorException(obj);
        }

        public static int ObjTst(object o1, object o2, bool TextCompare)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = o1 as IConvertible;
            if (conv == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            if (((empty == TypeCode.Object) && (o1 is char[])) && (((typeCode == TypeCode.String) || (typeCode == TypeCode.Empty)) || ((typeCode == TypeCode.Object) && (o2 is char[]))))
            {
                o1 = new string(CharArrayType.FromObject(o1));
                conv = (IConvertible) o1;
                empty = TypeCode.String;
            }
            if (((typeCode == TypeCode.Object) && (o2 is char[])) && ((empty == TypeCode.String) || (empty == TypeCode.Empty)))
            {
                o2 = new string(CharArrayType.FromObject(o2));
                convertible2 = (IConvertible) o2;
                typeCode = TypeCode.String;
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Boolean:
                    return ObjTstInt32(0, ToVBBool(convertible2));

                case TypeCode.Char:
                    return ObjTstChar('\0', convertible2.ToChar(null));

                case TypeCode.Byte:
                    return ObjTstByte(0, convertible2.ToByte(null));

                case TypeCode.Int16:
                    return ObjTstInt16(0, convertible2.ToInt16(null));

                case TypeCode.Int32:
                    return ObjTstInt32(0, convertible2.ToInt32(null));

                case TypeCode.Int64:
                    return ObjTstInt64(0L, convertible2.ToInt64(null));

                case TypeCode.Single:
                    return ObjTstSingle(0f, convertible2.ToSingle(null));

                case TypeCode.Double:
                    return ObjTstDouble(0.0, convertible2.ToDouble(null));

                case TypeCode.Decimal:
                    return ObjTstDecimal((IConvertible) 0, convertible2);

                case TypeCode.DateTime:
                    return ObjTstDateTime(DateType.FromObject(null), convertible2.ToDateTime(null));

                case TypeCode.String:
                    return ObjTstStringString(null, o2.ToString(), TextCompare);

                case ((TypeCode) 0x39):
                    return ObjTstInt32(ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return ObjTstInt16((short) ToVBBool(conv), (short) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return ObjTstInt16((short) ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x42):
                    return ObjTstInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x44):
                    return ObjTstInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 70):
                    return ObjTstSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return ObjTstDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x48):
                    return ObjTstDecimal((IConvertible) ToVBBool(conv), convertible2);

                case ((TypeCode) 0x4b):
                    return ObjTstBoolean(conv.ToBoolean(null), BooleanType.FromString(convertible2.ToString(null)));

                case ((TypeCode) 0x4c):
                    return ObjTstChar(conv.ToChar(null), '\0');

                case ((TypeCode) 80):
                    return ObjTstChar(conv.ToChar(null), convertible2.ToChar(null));

                case ((TypeCode) 0x5e):
                case ((TypeCode) 0x15a):
                    return ObjTstStringString(conv.ToString(null), convertible2.ToString(null), TextCompare);

                case ((TypeCode) 0x72):
                    return ObjTstByte(conv.ToByte(null), 0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return ObjTstInt16(conv.ToInt16(null), (short) ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return ObjTstByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return ObjTstInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return ObjTstInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                    return ObjTstInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return ObjTstSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return ObjTstDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return ObjTstDecimal(conv, convertible2);

                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return ObjTstString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0x85):
                    return ObjTstInt16(conv.ToInt16(null), 0);

                case ((TypeCode) 0xab):
                    return ObjTstInt32(conv.ToInt32(null), 0);

                case ((TypeCode) 0xae):
                    return ObjTstInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                    return ObjTstInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0xd4):
                    return ObjTstInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xf7):
                    return ObjTstSingle(conv.ToSingle(null), 0f);

                case ((TypeCode) 250):
                    return ObjTstSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10a):
                    return ObjTstDouble(conv.ToDouble(null), 0.0);

                case ((TypeCode) 0x10d):
                    return ObjTstDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x11d):
                    return ObjTstDecimal(conv, (IConvertible) 0);

                case ((TypeCode) 0x120):
                    return ObjTstDecimal(conv, (IConvertible) ToVBBool(convertible2));

                case ((TypeCode) 0x130):
                    return ObjTstDateTime(conv.ToDateTime(null), DateType.FromObject(null));

                case ((TypeCode) 320):
                    return ObjTstDateTime(conv.ToDateTime(null), convertible2.ToDateTime(null));

                case ((TypeCode) 0x142):
                    return ObjTstDateTime(conv.ToDateTime(null), DateType.FromString(convertible2.ToString(null), Utils.GetCultureInfo()));

                case ((TypeCode) 0x156):
                    return ObjTstStringString(o1.ToString(), null, TextCompare);

                case ((TypeCode) 0x159):
                    return ObjTstBoolean(BooleanType.FromString(conv.ToString(null)), convertible2.ToBoolean(null));

                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return ObjTstString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0x166):
                    return ObjTstDateTime(DateType.FromString(conv.ToString(null), Utils.GetCultureInfo()), convertible2.ToDateTime(null));

                case ((TypeCode) 360):
                    return ObjTstStringString(conv.ToString(null), convertible2.ToString(null), TextCompare);
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static int ObjTstBoolean(bool b1, bool b2)
        {
            if (b1 == b2)
            {
                return 0;
            }
            if (b1 < b2)
            {
                return 1;
            }
            return -1;
        }

        private static int ObjTstByte(byte by1, byte by2)
        {
            if (by1 < by2)
            {
                return -1;
            }
            if (by1 > by2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstChar(char ch1, char ch2)
        {
            if (ch1 < ch2)
            {
                return -1;
            }
            if (ch1 > ch2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstDateTime(DateTime var1, DateTime var2)
        {
            long ticks = var1.Ticks;
            long num3 = var2.Ticks;
            if (ticks < num3)
            {
                return -1;
            }
            if (ticks > num3)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstDecimal(IConvertible i1, IConvertible i2)
        {
            decimal num = i1.ToDecimal(null);
            decimal num2 = i2.ToDecimal(null);
            if (decimal.Compare(num, num2) < 0)
            {
                return -1;
            }
            if (decimal.Compare(num, num2) > 0)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstDouble(double d1, double d2)
        {
            if (d1 < d2)
            {
                return -1;
            }
            if (d1 > d2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstInt16(short d1, short d2)
        {
            if (d1 < d2)
            {
                return -1;
            }
            if (d1 > d2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstInt32(int d1, int d2)
        {
            if (d1 < d2)
            {
                return -1;
            }
            if (d1 > d2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstInt64(long d1, long d2)
        {
            if (d1 < d2)
            {
                return -1;
            }
            if (d1 > d2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstSingle(float d1, float d2)
        {
            if (d1 < d2)
            {
                return -1;
            }
            if (d1 > d2)
            {
                return 1;
            }
            return 0;
        }

        private static int ObjTstString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return ObjTstDouble(num, num2);
        }

        private static int ObjTstStringString(string s1, string s2, bool TextCompare)
        {
            if (s1 == null)
            {
                if (s2.Length > 0)
                {
                    return -1;
                }
                return 0;
            }
            if (s2 == null)
            {
                if (s1.Length > 0)
                {
                    return 1;
                }
                return 0;
            }
            if (TextCompare)
            {
                return Utils.GetCultureInfo().CompareInfo.Compare(s1, s2, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return string.CompareOrdinal(s1, s2);
        }

        public static object PlusObj(object obj)
        {
            TypeCode empty;
            if (obj == null)
            {
                return 0;
            }
            IConvertible convertible = obj as IConvertible;
            if (convertible == null)
            {
                if (obj == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = convertible.GetTypeCode();
            }
            switch (empty)
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Boolean:
                    if (obj is bool)
                    {
                        return (short) -(((bool) obj) > false);
                    }
                    return (short) -(convertible.ToBoolean(null) > false);

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return obj;

                case TypeCode.String:
                    return DoubleType.FromObject(obj);
            }
            throw GetNoValidOperatorException(obj);
        }

        public static object PowObj(object obj1, object obj2)
        {
            if ((obj1 == null) && (obj2 == null))
            {
                return 1.0;
            }
            switch (GetWidestType(obj1, obj2, false))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return Math.Pow(DoubleType.FromObject(obj1), DoubleType.FromObject(obj2));
            }
            throw GetNoValidOperatorException(obj1, obj2);
        }

        public static object ShiftLeftObj(object o1, int amount)
        {
            TypeCode empty;
            IConvertible convertible = o1 as IConvertible;
            if (convertible == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = convertible.GetTypeCode();
            }
            switch (empty)
            {
                case TypeCode.Empty:
                    return (((int) 0) << amount);

                case TypeCode.Boolean:
                    return (short) (((short) -(convertible.ToBoolean(null) > 0)) << (amount & 15));

                case TypeCode.Byte:
                    return (byte) (convertible.ToByte(null) << (amount & 7));

                case TypeCode.Int16:
                    return (short) (convertible.ToInt16(null) << (amount & 15));

                case TypeCode.Int32:
                    return (convertible.ToInt32(null) << amount);

                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (convertible.ToInt64(null) << amount);

                case TypeCode.String:
                    return (LongType.FromString(convertible.ToString(null)) << amount);
            }
            throw GetNoValidOperatorException(o1);
        }

        public static object ShiftRightObj(object o1, int amount)
        {
            TypeCode empty;
            IConvertible convertible = o1 as IConvertible;
            if (convertible == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = convertible.GetTypeCode();
            }
            switch (empty)
            {
                case TypeCode.Empty:
                    return (((int) 0) >> amount);

                case TypeCode.Boolean:
                    return (short) (((short) -(convertible.ToBoolean(null) > 0)) >> (amount & 15));

                case TypeCode.Byte:
                    return (byte) (convertible.ToByte(null) >> (amount & 7));

                case TypeCode.Int16:
                    return (short) (convertible.ToInt16(null) >> (amount & 15));

                case TypeCode.Int32:
                    return (convertible.ToInt32(null) >> amount);

                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (convertible.ToInt64(null) >> amount);

                case TypeCode.String:
                    return (LongType.FromString(convertible.ToString(null)) >> amount);
            }
            throw GetNoValidOperatorException(o1);
        }

        public static object StrCatObj(object vLeft, object vRight)
        {
            bool flag = vLeft is DBNull;
            bool flag2 = vRight is DBNull;
            if (flag & flag2)
            {
                return vLeft;
            }
            if (flag & !flag2)
            {
                vLeft = "";
            }
            else if (flag2 & !flag)
            {
                vRight = "";
            }
            return (StringType.FromObject(vLeft) + StringType.FromObject(vRight));
        }

        private static object SubByte(byte i1, byte i2)
        {
            short num = (short) (i1 - i2);
            if ((num >= 0) && (num <= 0xff))
            {
                return (byte) num;
            }
            return num;
        }

        private static object SubDecimal(IConvertible conv1, IConvertible conv2)
        {
            decimal num = conv1.ToDecimal(null);
            decimal num2 = conv2.ToDecimal(null);
            try
            {
                return decimal.Subtract(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) - Convert.ToDouble(num2));
            }
        }

        private static object SubDouble(double d1, double d2)
        {
            return (d1 - d2);
        }

        private static object SubInt16(short i1, short i2)
        {
            int num = i1 - i2;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object SubInt32(int i1, int i2)
        {
            long num = i1 - i2;
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object SubInt64(long i1, long i2)
        {
            object obj2;
            try
            {
                obj2 = i1 - i2;
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                obj2 = decimal.Subtract(new decimal(i1), new decimal(i2));
            }
            return obj2;
        }

        public static object SubObj(object o1, object o2)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = o1 as IConvertible;
            if (conv == null)
            {
                if (o1 == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = conv.GetTypeCode();
            }
            IConvertible convertible2 = o2 as IConvertible;
            if (convertible2 == null)
            {
                if (o2 == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return InternalNegObj(o2, convertible2, typeCode);

                case TypeCode.String:
                    return SubStringString(null, convertible2.ToString(null));

                case ((TypeCode) 0x39):
                case ((TypeCode) 0x72):
                case ((TypeCode) 0x85):
                case ((TypeCode) 0xab):
                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return o1;

                case ((TypeCode) 60):
                    return SubInt16((short) ToVBBool(conv), (short) ToVBBool(convertible2));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return SubInt16((short) ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x42):
                    return SubInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x44):
                    return SubInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 70):
                    return SubSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return SubDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x48):
                    return SubDecimal(ToVBBoolConv(conv), convertible2);

                case ((TypeCode) 0x4b):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                case ((TypeCode) 0x159):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return SubString(conv, empty, convertible2, typeCode);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return SubInt16(conv.ToInt16(null), (short) ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return SubByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return SubInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 180):
                    return SubInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xda):
                case ((TypeCode) 220):
                    return SubInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x102):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return SubSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return SubDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x128):
                case ((TypeCode) 300):
                    return SubDecimal(conv, convertible2);

                case ((TypeCode) 0xae):
                    return SubInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xd4):
                    return SubInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 250):
                    return SubSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return SubDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x120):
                    return SubDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0x156):
                    return SubStringString(conv.ToString(null), null);

                case ((TypeCode) 360):
                    return SubStringString(conv.ToString(null), convertible2.ToString(null));
            }
            throw GetNoValidOperatorException(o1, o2);
        }

        private static object SubSingle(float f1, float f2)
        {
            double d = f1 - f2;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(f1) && !float.IsInfinity(f2))))
            {
                return d;
            }
            return (float) d;
        }

        private static object SubString(IConvertible conv1, TypeCode tc1, IConvertible conv2, TypeCode tc2)
        {
            double num;
            double num2;
            if (tc1 == TypeCode.String)
            {
                num = DoubleType.FromString(conv1.ToString(null));
            }
            else if (tc1 == TypeCode.Boolean)
            {
                num = ToVBBool(conv1);
            }
            else
            {
                num = conv1.ToDouble(null);
            }
            if (tc2 == TypeCode.String)
            {
                num2 = DoubleType.FromString(conv2.ToString(null));
            }
            else if (tc2 == TypeCode.Boolean)
            {
                num2 = ToVBBool(conv2);
            }
            else
            {
                num2 = conv2.ToDouble(null);
            }
            return (num - num2);
        }

        private static object SubStringString(string s1, string s2)
        {
            double num;
            double num2;
            if (s1 != null)
            {
                num = DoubleType.FromString(s1);
            }
            if (s2 != null)
            {
                num2 = DoubleType.FromString(s2);
            }
            return (num - num2);
        }

        private static int ToVBBool(IConvertible conv)
        {
            if (conv.ToBoolean(null))
            {
                return -1;
            }
            return 0;
        }

        private static IConvertible ToVBBoolConv(IConvertible conv)
        {
            if (conv.ToBoolean(null))
            {
                return (IConvertible) (-1);
            }
            return (IConvertible) 0;
        }

        private static TypeCode TypeCodeFromVType(VType vartyp)
        {
            switch (vartyp)
            {
                case VType.t_bool:
                    return TypeCode.Boolean;

                case VType.t_ui1:
                    return TypeCode.Byte;

                case VType.t_i2:
                    return TypeCode.Int16;

                case VType.t_i4:
                    return TypeCode.Int32;

                case VType.t_i8:
                    return TypeCode.Int64;

                case VType.t_dec:
                    return TypeCode.Decimal;

                case VType.t_r4:
                    return TypeCode.Single;

                case VType.t_r8:
                    return TypeCode.Double;

                case VType.t_char:
                    return TypeCode.Char;

                case VType.t_str:
                    return TypeCode.String;

                case VType.t_date:
                    return TypeCode.DateTime;
            }
            return TypeCode.Object;
        }

        internal static Type TypeFromTypeCode(TypeCode vartyp)
        {
            switch (vartyp)
            {
                case TypeCode.Object:
                    return typeof(object);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Boolean:
                    return typeof(bool);

                case TypeCode.Char:
                    return typeof(char);

                case TypeCode.SByte:
                    return typeof(sbyte);

                case TypeCode.Byte:
                    return typeof(byte);

                case TypeCode.Int16:
                    return typeof(short);

                case TypeCode.UInt16:
                    return typeof(ushort);

                case TypeCode.Int32:
                    return typeof(int);

                case TypeCode.UInt32:
                    return typeof(uint);

                case TypeCode.Int64:
                    return typeof(long);

                case TypeCode.UInt64:
                    return typeof(ulong);

                case TypeCode.Single:
                    return typeof(float);

                case TypeCode.Double:
                    return typeof(double);

                case TypeCode.Decimal:
                    return typeof(decimal);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.String:
                    return typeof(string);
            }
            return null;
        }

        private static VType2 VType2FromTypeCode(TypeCode typ)
        {
            switch (typ)
            {
                case TypeCode.Boolean:
                    return VType2.t_bool;

                case TypeCode.Char:
                    return VType2.t_char;

                case TypeCode.Byte:
                    return VType2.t_ui1;

                case TypeCode.Int16:
                    return VType2.t_i2;

                case TypeCode.Int32:
                    return VType2.t_i4;

                case TypeCode.Int64:
                    return VType2.t_i8;

                case TypeCode.Single:
                    return VType2.t_r4;

                case TypeCode.Double:
                    return VType2.t_r8;

                case TypeCode.Decimal:
                    return VType2.t_dec;

                case TypeCode.DateTime:
                    return VType2.t_date;

                case TypeCode.String:
                    return VType2.t_str;
            }
            return VType2.t_bad;
        }

        private static VType VTypeFromTypeCode(TypeCode typ)
        {
            switch (typ)
            {
                case TypeCode.Boolean:
                    return VType.t_bool;

                case TypeCode.Char:
                    return VType.t_char;

                case TypeCode.Byte:
                    return VType.t_ui1;

                case TypeCode.Int16:
                    return VType.t_i2;

                case TypeCode.Int32:
                    return VType.t_i4;

                case TypeCode.Int64:
                    return VType.t_i8;

                case TypeCode.Single:
                    return VType.t_r4;

                case TypeCode.Double:
                    return VType.t_r8;

                case TypeCode.Decimal:
                    return VType.t_dec;

                case TypeCode.DateTime:
                    return VType.t_date;

                case TypeCode.String:
                    return VType.t_str;
            }
            return VType.t_bad;
        }

        public static object XorObj(object obj1, object obj2)
        {
            if ((obj1 == null) && (obj2 == null))
            {
                return false;
            }
            switch (GetWidestType(obj1, obj2, false))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return (BooleanType.FromObject(obj1) ^ BooleanType.FromObject(obj2));
            }
            throw GetNoValidOperatorException(obj1, obj2);
        }

        private enum CC : byte
        {
            Err = 0,
            Narr = 2,
            Same = 1,
            Wide = 3
        }

        private enum VType
        {
            t_bad,
            t_bool,
            t_ui1,
            t_i2,
            t_i4,
            t_i8,
            t_dec,
            t_r4,
            t_r8,
            t_char,
            t_str,
            t_date
        }

        private enum VType2
        {
            t_bad,
            t_bool,
            t_ui1,
            t_char,
            t_i2,
            t_i4,
            t_i8,
            t_r4,
            t_r8,
            t_date,
            t_dec,
            t_ref,
            t_str
        }
    }
}

