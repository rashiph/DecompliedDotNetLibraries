namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Operators
    {
        internal static readonly object Boxed_ZeroByte = ((byte) 0);
        internal static readonly object Boxed_ZeroDecimal = decimal.Zero;
        internal static readonly object Boxed_ZeroDouble = 0.0;
        internal static readonly object Boxed_ZeroInteger = 0;
        internal static readonly object Boxed_ZeroLong = 0L;
        internal static readonly object Boxed_ZeroSByte = ((sbyte) 0);
        internal static readonly object Boxed_ZeroShort = ((short) 0);
        internal static readonly object Boxed_ZeroSinge = 0f;
        internal static readonly object Boxed_ZeroUInteger = 0;
        internal static readonly object Boxed_ZeroULong = ((ulong) 0L);
        internal static readonly object Boxed_ZeroUShort = ((ushort) 0);
        private const int TCMAX = 0x13;

        private Operators()
        {
        }

        private static object AddByte(byte Left, byte Right)
        {
            short num = (short) (Left + Right);
            if (num > 0xff)
            {
                return num;
            }
            return (byte) num;
        }

        private static object AddDecimal(IConvertible Left, IConvertible Right)
        {
            decimal num = Left.ToDecimal(null);
            decimal num2 = Right.ToDecimal(null);
            try
            {
                return decimal.Add(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) + Convert.ToDouble(num2));
            }
        }

        private static object AddDouble(double Left, double Right)
        {
            return (Left + Right);
        }

        private static object AddInt16(short Left, short Right)
        {
            int num = Left + Right;
            if ((num <= 0x7fff) && (num >= -32768))
            {
                return (short) num;
            }
            return num;
        }

        private static object AddInt32(int Left, int Right)
        {
            long num = Left + Right;
            if ((num <= 0x7fffffffL) && (num >= -2147483648L))
            {
                return (int) num;
            }
            return num;
        }

        private static object AddInt64(long Left, long Right)
        {
            try
            {
                return (Left + Right);
            }
            catch (OverflowException)
            {
                return decimal.Add(new decimal(Left), new decimal(Right));
            }
        }

        public static object AddObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
            if (empty == TypeCode.Object)
            {
                char[] chArray = Left as char[];
                if ((chArray != null) && (((typeCode == TypeCode.String) || (typeCode == TypeCode.Empty)) || ((typeCode == TypeCode.Object) && (Right is char[]))))
                {
                    Left = new string(chArray);
                    conv = (IConvertible) Left;
                    empty = TypeCode.String;
                }
            }
            if (typeCode == TypeCode.Object)
            {
                char[] chArray2 = Right as char[];
                if ((chArray2 != null) && ((empty == TypeCode.String) || (empty == TypeCode.Empty)))
                {
                    Right = new string(chArray2);
                    convertible2 = (IConvertible) Right;
                    typeCode = TypeCode.String;
                }
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                    return AddInt16(0, ToVBBool(convertible2));

                case TypeCode.Char:
                    return AddString("\0", convertible2.ToString(null));

                case TypeCode.SByte:
                    return convertible2.ToSByte(null);

                case TypeCode.Byte:
                    return convertible2.ToByte(null);

                case TypeCode.Int16:
                    return convertible2.ToInt16(null);

                case TypeCode.UInt16:
                    return convertible2.ToUInt16(null);

                case TypeCode.Int32:
                    return convertible2.ToInt32(null);

                case TypeCode.UInt32:
                    return convertible2.ToUInt32(null);

                case TypeCode.Int64:
                    return convertible2.ToInt64(null);

                case TypeCode.UInt64:
                    return convertible2.ToUInt64(null);

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case ((TypeCode) 0x38):
                    return Right;

                case TypeCode.DateTime:
                    return AddString(Conversions.ToString(DateTime.MinValue), Conversions.ToString(convertible2.ToDateTime(null)));

                case ((TypeCode) 0x39):
                    return AddInt16(ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return AddInt16(ToVBBool(conv), ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                    return AddSByte(ToVBBool(conv), convertible2.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return AddInt16(ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return AddInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                    return AddInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x45):
                case ((TypeCode) 0x48):
                    return AddDecimal(ToVBBoolConv(conv), (IConvertible) convertible2.ToDecimal(null));

                case ((TypeCode) 70):
                    return AddSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return AddDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x4b):
                    return AddDouble((double) ToVBBool(conv), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x4c):
                    return AddString(conv.ToString(null), "\0");

                case ((TypeCode) 80):
                case ((TypeCode) 0x5e):
                case ((TypeCode) 0x15a):
                    return AddString(conv.ToString(null), convertible2.ToString(null));

                case ((TypeCode) 0x5f):
                    return conv.ToSByte(null);

                case ((TypeCode) 0x62):
                    return AddSByte(conv.ToSByte(null), ToVBBool(convertible2));

                case ((TypeCode) 100):
                    return AddSByte(conv.ToSByte(null), convertible2.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return AddInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return AddInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                    return AddInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x6b):
                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return AddDecimal(conv, convertible2);

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return AddSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return AddDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return AddDouble(conv.ToDouble(null), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x72):
                    return conv.ToByte(null);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return AddInt16(conv.ToInt16(null), ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return AddByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return AddUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return AddUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return AddUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null));

                case ((TypeCode) 0x85):
                    return conv.ToInt16(null);

                case ((TypeCode) 0x98):
                    return conv.ToUInt16(null);

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return AddInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xab):
                    return conv.ToInt32(null);

                case ((TypeCode) 190):
                    return conv.ToUInt32(null);

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                    return AddInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                    return conv.ToInt64(null);

                case ((TypeCode) 0xe4):
                    return conv.ToUInt64(null);

                case ((TypeCode) 0xe7):
                case ((TypeCode) 0x120):
                    return AddDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                case ((TypeCode) 0x156):
                case ((TypeCode) 0x158):
                    return Left;

                case ((TypeCode) 250):
                    return AddSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return AddDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x130):
                    return AddString(Conversions.ToString(conv.ToDateTime(null)), Conversions.ToString(DateTime.MinValue));

                case ((TypeCode) 320):
                    return AddString(Conversions.ToString(conv.ToDateTime(null)), Conversions.ToString(convertible2.ToDateTime(null)));

                case ((TypeCode) 0x142):
                    return AddString(Conversions.ToString(conv.ToDateTime(null)), convertible2.ToString(null));

                case ((TypeCode) 0x159):
                    return AddDouble(Conversions.ToDouble(conv.ToString(null)), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return AddDouble(Conversions.ToDouble(conv.ToString(null)), convertible2.ToDouble(null));

                case ((TypeCode) 0x166):
                    return AddString(conv.ToString(null), Conversions.ToString(convertible2.ToDateTime(null)));

                case ((TypeCode) 360):
                    return AddString(conv.ToString(null), convertible2.ToString(null));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Plus, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Plus, new object[] { Left, Right });
        }

        private static object AddSByte(sbyte Left, sbyte Right)
        {
            short num = (short) (Left + Right);
            if ((num <= 0x7f) && (num >= -128))
            {
                return (sbyte) num;
            }
            return num;
        }

        private static object AddSingle(float Left, float Right)
        {
            double d = Left + Right;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(Left) && !float.IsInfinity(Right))))
            {
                return d;
            }
            return (float) d;
        }

        private static object AddString(string Left, string Right)
        {
            return (Left + Right);
        }

        private static object AddUInt16(ushort Left, ushort Right)
        {
            int num = Left + Right;
            if (num > 0xffff)
            {
                return num;
            }
            return (ushort) num;
        }

        private static object AddUInt32(uint Left, uint Right)
        {
            long num = Left + Right;
            if (num > 0xffffffffL)
            {
                return num;
            }
            return (uint) num;
        }

        private static object AddUInt64(ulong Left, ulong Right)
        {
            try
            {
                return (Left + Right);
            }
            catch (OverflowException)
            {
                return decimal.Add(new decimal(Left), new decimal(Right));
            }
        }

        private static object AndBoolean(bool Left, bool Right)
        {
            return (Left & Right);
        }

        private static object AndByte(byte Left, byte Right, Type EnumType = null)
        {
            byte num = (byte) (Left & Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndInt16(short Left, short Right, Type EnumType = null)
        {
            short num = (short) (Left & Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndInt32(int Left, int Right, Type EnumType = null)
        {
            int num = Left & Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndInt64(long Left, long Right, Type EnumType = null)
        {
            long num = Left & Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        public static object AndObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                case ((TypeCode) 0x39):
                    return false;

                case TypeCode.SByte:
                case ((TypeCode) 0x5f):
                    return AndSByte(0, 0, GetEnumResult(Left, Right));

                case TypeCode.Byte:
                case ((TypeCode) 0x72):
                    return AndByte(0, 0, GetEnumResult(Left, Right));

                case TypeCode.Int16:
                case ((TypeCode) 0x85):
                    return AndInt16(0, 0, GetEnumResult(Left, Right));

                case TypeCode.UInt16:
                case ((TypeCode) 0x98):
                    return AndUInt16(0, 0, GetEnumResult(Left, Right));

                case TypeCode.Int32:
                case ((TypeCode) 0xab):
                    return AndInt32(0, 0, GetEnumResult(Left, Right));

                case TypeCode.UInt32:
                case ((TypeCode) 190):
                    return AndUInt32(0, 0, GetEnumResult(Left, Right));

                case TypeCode.Int64:
                case ((TypeCode) 0xd1):
                    return AndInt64(0L, 0L, GetEnumResult(Left, Right));

                case TypeCode.UInt64:
                case ((TypeCode) 0xe4):
                    return AndUInt64(0L, 0L, GetEnumResult(Left, Right));

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return AndInt64(0L, convertible2.ToInt64(null), null);

                case TypeCode.String:
                    return AndInt64(0L, Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 60):
                    return AndBoolean(conv.ToBoolean(null), convertible2.ToBoolean(null));

                case ((TypeCode) 0x3e):
                    return AndSByte(ToVBBool(conv), convertible2.ToSByte(null), null);

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return AndInt16(ToVBBool(conv), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return AndInt32(ToVBBool(conv), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x45):
                case ((TypeCode) 70):
                case ((TypeCode) 0x47):
                case ((TypeCode) 0x48):
                    return AndInt64((long) ToVBBool(conv), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x4b):
                    return AndBoolean(conv.ToBoolean(null), Conversions.ToBoolean(convertible2.ToString(null)));

                case ((TypeCode) 0x62):
                    return AndSByte(conv.ToSByte(null), ToVBBool(convertible2), null);

                case ((TypeCode) 100):
                    return AndSByte(conv.ToSByte(null), convertible2.ToSByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                    return AndInt16(conv.ToInt16(null), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                    return AndInt32(conv.ToInt32(null), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x6b):
                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x6d):
                case ((TypeCode) 110):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 0x12a):
                case ((TypeCode) 0x12b):
                case ((TypeCode) 300):
                    return AndInt64(conv.ToInt64(null), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return AndInt64(conv.ToInt64(null), Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return AndInt16(conv.ToInt16(null), ToVBBool(convertible2), null);

                case ((TypeCode) 120):
                    return AndByte(conv.ToByte(null), convertible2.ToByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                    return AndUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), null);

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                    return AndUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), null);

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                    return AndUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), null);

                case ((TypeCode) 140):
                    return AndInt16(conv.ToInt16(null), convertible2.ToInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return AndInt32(conv.ToInt32(null), ToVBBool(convertible2), null);

                case ((TypeCode) 160):
                    return AndUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 180):
                    return AndInt32(conv.ToInt32(null), convertible2.ToInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0xe7):
                case ((TypeCode) 250):
                case ((TypeCode) 0x10d):
                case ((TypeCode) 0x120):
                    return AndInt64(conv.ToInt64(null), (long) ToVBBool(convertible2), null);

                case ((TypeCode) 200):
                    return AndUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 220):
                    return AndInt64(conv.ToInt64(null), convertible2.ToInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 240):
                    return AndUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return AndInt64(conv.ToInt64(null), 0L, null);

                case ((TypeCode) 0x156):
                    return AndInt64(Conversions.ToLong(conv.ToString(null)), 0L, null);

                case ((TypeCode) 0x159):
                    return AndBoolean(Conversions.ToBoolean(conv.ToString(null)), convertible2.ToBoolean(null));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return AndInt64(Conversions.ToLong(conv.ToString(null)), convertible2.ToInt64(null), null);

                case ((TypeCode) 360):
                    return AndInt64(Conversions.ToLong(conv.ToString(null)), Conversions.ToLong(convertible2.ToString(null)), null);
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.And, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.And, new object[] { Left, Right });
        }

        private static object AndSByte(sbyte Left, sbyte Right, Type EnumType = null)
        {
            sbyte num = (sbyte) (Left & Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndUInt16(ushort Left, ushort Right, Type EnumType = null)
        {
            ushort num = (ushort) (Left & Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndUInt32(uint Left, uint Right, Type EnumType = null)
        {
            uint num = Left & Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object AndUInt64(ulong Left, ulong Right, Type EnumType = null)
        {
            ulong num = Left & Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static int AsteriskSkip(string Pattern, string Source, int SourceEndIndex, CompareMethod CompareOption, CompareInfo ci)
        {
            int num2;
            int num4;
            int num3 = Strings.Len(Pattern);
            while (num4 < num3)
            {
                bool flag;
                bool flag2;
                bool flag3;
                switch (Pattern[num4])
                {
                    case '-':
                        if (Pattern[num4 + 1] == ']')
                        {
                            flag2 = true;
                        }
                        break;

                    case '!':
                        if (Pattern[num4 + 1] == ']')
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag3 = true;
                        }
                        break;

                    case '[':
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag = true;
                        }
                        break;

                    case ']':
                        if (flag2 || !flag)
                        {
                            num2++;
                            flag3 = true;
                        }
                        flag2 = false;
                        flag = false;
                        break;

                    case '*':
                        if (num2 > 0)
                        {
                            CompareOptions ordinal;
                            if (flag3)
                            {
                                num2 = MultipleAsteriskSkip(Pattern, Source, num2, CompareOption);
                                return (SourceEndIndex - num2);
                            }
                            string str = Pattern.Substring(0, num4);
                            if (CompareOption == CompareMethod.Binary)
                            {
                                ordinal = CompareOptions.Ordinal;
                            }
                            else
                            {
                                ordinal = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
                            }
                            return ci.LastIndexOf(Source, str, ordinal);
                        }
                        break;

                    case '?':
                    case '#':
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            num2++;
                            flag3 = true;
                        }
                        break;

                    default:
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            num2++;
                        }
                        break;
                }
                num4++;
            }
            return (SourceEndIndex - num2);
        }

        internal static List<Symbols.Method> CollectOperators(Symbols.UserDefinedOperator Op, Type Type1, Type Type2, ref bool FoundType1Operators, ref bool FoundType2Operators)
        {
            List<Symbols.Method> list2;
            int num;
            int num2;
            bool flag = Type2 != null;
            if (!Symbols.IsRootObjectType(Type1) && Symbols.IsClassOrValueType(Type1))
            {
                Symbols.Container container = new Symbols.Container(Type1);
                num = 0;
                num2 = 0;
                list2 = OverloadResolution.CollectOverloadCandidates(container.LookupNamedMembers(Symbols.OperatorCLSNames[(int) Op]), null, Interaction.IIf<int>(Symbols.IsUnaryOperator(Op), 1, 2), null, null, true, null, ref num, ref num2);
                if (list2.Count > 0)
                {
                    FoundType1Operators = true;
                }
            }
            else
            {
                list2 = new List<Symbols.Method>();
            }
            if ((flag && !Symbols.IsRootObjectType(Type2)) && Symbols.IsClassOrValueType(Type2))
            {
                Type baseType = Type1;
                while (baseType != null)
                {
                    if (Symbols.IsOrInheritsFrom(Type2, baseType))
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                Symbols.Container container2 = new Symbols.Container(Type2);
                num2 = 0;
                num = 0;
                List<Symbols.Method> collection = OverloadResolution.CollectOverloadCandidates(container2.LookupNamedMembers(Symbols.OperatorCLSNames[(int) Op]), null, Interaction.IIf<int>(Symbols.IsUnaryOperator(Op), 1, 2), null, null, true, baseType, ref num2, ref num);
                if (collection.Count > 0)
                {
                    FoundType2Operators = true;
                }
                list2.AddRange(collection);
            }
            return list2;
        }

        private static CompareClass CompareBoolean(bool Left, bool Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left < Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareChar(char Left, char Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareDate(DateTime Left, DateTime Right)
        {
            int num = DateTime.Compare(Left, Right);
            if (num == 0)
            {
                return CompareClass.Equal;
            }
            if (num > 0)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareDecimal(IConvertible Left, IConvertible Right)
        {
            int num = decimal.Compare(Left.ToDecimal(null), Right.ToDecimal(null));
            if (num == 0)
            {
                return CompareClass.Equal;
            }
            if (num > 0)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareDouble(double Left, double Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left < Right)
            {
                return CompareClass.Less;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Unordered;
        }

        private static CompareClass CompareInt32(int Left, int Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareInt64(long Left, long Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        public static int CompareObject(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return 0;

                case CompareClass.UserDefined:
                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.IsTrue, Left, Right);
            }
            return (int) class2;
        }

        private static CompareClass CompareObject2(object Left, object Right, bool TextCompare)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible right = Right as IConvertible;
            if (right == null)
            {
                if (Right == null)
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
                typeCode = right.GetTypeCode();
            }
            if (empty == TypeCode.Object)
            {
                char[] chArray = Left as char[];
                if ((chArray != null) && (((typeCode == TypeCode.String) || (typeCode == TypeCode.Empty)) || ((typeCode == TypeCode.Object) && (Right is char[]))))
                {
                    Left = new string(chArray);
                    conv = (IConvertible) Left;
                    empty = TypeCode.String;
                }
            }
            if (typeCode == TypeCode.Object)
            {
                char[] chArray2 = Right as char[];
                if ((chArray2 != null) && ((empty == TypeCode.String) || (empty == TypeCode.Empty)))
                {
                    Right = new string(chArray2);
                    right = (IConvertible) Right;
                    typeCode = TypeCode.String;
                }
            }
            switch (((empty * (TypeCode.String | TypeCode.Object)) + typeCode))
            {
                case TypeCode.Empty:
                    return CompareClass.Equal;

                case TypeCode.Boolean:
                    return CompareBoolean(false, right.ToBoolean(null));

                case TypeCode.Char:
                    return CompareChar('\0', right.ToChar(null));

                case TypeCode.SByte:
                    return CompareInt32(0, right.ToSByte(null));

                case TypeCode.Byte:
                    return CompareInt32(0, right.ToByte(null));

                case TypeCode.Int16:
                    return CompareInt32(0, right.ToInt16(null));

                case TypeCode.UInt16:
                    return CompareInt32(0, right.ToUInt16(null));

                case TypeCode.Int32:
                    return CompareInt32(0, right.ToInt32(null));

                case TypeCode.UInt32:
                    return CompareUInt32(0, right.ToUInt32(null));

                case TypeCode.Int64:
                    return CompareInt64(0L, right.ToInt64(null));

                case TypeCode.UInt64:
                    return CompareUInt64(0L, right.ToUInt64(null));

                case TypeCode.Single:
                    return CompareSingle(0f, right.ToSingle(null));

                case TypeCode.Double:
                    return CompareDouble(0.0, right.ToDouble(null));

                case TypeCode.Decimal:
                    return CompareDecimal((IConvertible) decimal.Zero, right);

                case TypeCode.DateTime:
                    return CompareDate(DateTime.MinValue, right.ToDateTime(null));

                case TypeCode.String:
                    return (CompareClass) CompareString(null, right.ToString(null), TextCompare);

                case ((TypeCode) 0x39):
                    return CompareBoolean(conv.ToBoolean(null), false);

                case ((TypeCode) 60):
                    return CompareBoolean(conv.ToBoolean(null), right.ToBoolean(null));

                case ((TypeCode) 0x3e):
                    return CompareInt32(ToVBBool(conv), right.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return CompareInt32(ToVBBool(conv), right.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return CompareInt32(ToVBBool(conv), right.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                    return CompareInt64((long) ToVBBool(conv), right.ToInt64(null));

                case ((TypeCode) 0x45):
                case ((TypeCode) 0x48):
                    return CompareDecimal(ToVBBoolConv(conv), right);

                case ((TypeCode) 70):
                    return CompareSingle((float) ToVBBool(conv), right.ToSingle(null));

                case ((TypeCode) 0x47):
                    return CompareDouble((double) ToVBBool(conv), right.ToDouble(null));

                case ((TypeCode) 0x4b):
                    return CompareBoolean(conv.ToBoolean(null), Conversions.ToBoolean(right.ToString(null)));

                case ((TypeCode) 0x4c):
                    return CompareChar(conv.ToChar(null), '\0');

                case ((TypeCode) 80):
                    return CompareChar(conv.ToChar(null), right.ToChar(null));

                case ((TypeCode) 0x5e):
                case ((TypeCode) 0x15a):
                case ((TypeCode) 360):
                    return (CompareClass) CompareString(conv.ToString(null), right.ToString(null), TextCompare);

                case ((TypeCode) 0x5f):
                    return CompareInt32(conv.ToSByte(null), 0);

                case ((TypeCode) 0x62):
                    return CompareInt32(conv.ToSByte(null), ToVBBool(right));

                case ((TypeCode) 100):
                    return CompareInt32(conv.ToSByte(null), right.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return CompareInt32(conv.ToInt16(null), right.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return CompareInt32(conv.ToInt32(null), right.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                    return CompareInt64(conv.ToInt64(null), right.ToInt64(null));

                case ((TypeCode) 0x6b):
                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return CompareDecimal(conv, right);

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return CompareSingle(conv.ToSingle(null), right.ToSingle(null));

                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return CompareDouble(conv.ToDouble(null), right.ToDouble(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return CompareDouble(conv.ToDouble(null), Conversions.ToDouble(right.ToString(null)));

                case ((TypeCode) 0x72):
                    return CompareInt32(conv.ToByte(null), 0);

                case ((TypeCode) 0x75):
                    return CompareInt32(conv.ToInt16(null), ToVBBool(right));

                case ((TypeCode) 120):
                    return CompareInt32(conv.ToByte(null), right.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return CompareInt32(conv.ToUInt16(null), right.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return CompareUInt32(conv.ToUInt32(null), right.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return CompareUInt64(conv.ToUInt64(null), right.ToUInt64(null));

                case ((TypeCode) 0x85):
                    return CompareInt32(conv.ToInt16(null), 0);

                case ((TypeCode) 0x88):
                    return CompareInt32(conv.ToInt16(null), ToVBBool(right));

                case ((TypeCode) 0x98):
                    return CompareInt32(conv.ToUInt16(null), 0);

                case ((TypeCode) 0x9b):
                    return CompareInt32(conv.ToInt32(null), ToVBBool(right));

                case ((TypeCode) 0xab):
                    return CompareInt32(conv.ToInt32(null), 0);

                case ((TypeCode) 0xae):
                    return CompareInt32(conv.ToInt32(null), ToVBBool(right));

                case ((TypeCode) 190):
                    return CompareUInt32(conv.ToUInt32(null), 0);

                case ((TypeCode) 0xc1):
                    return CompareInt64(conv.ToInt64(null), (long) ToVBBool(right));

                case ((TypeCode) 0xd1):
                    return CompareInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0xd4):
                    return CompareInt64(conv.ToInt64(null), (long) ToVBBool(right));

                case ((TypeCode) 0xe4):
                    return CompareUInt64(conv.ToUInt64(null), 0L);

                case ((TypeCode) 0xe7):
                    return CompareDecimal(conv, ToVBBoolConv(right));

                case ((TypeCode) 0xf7):
                    return CompareSingle(conv.ToSingle(null), 0f);

                case ((TypeCode) 250):
                    return CompareSingle(conv.ToSingle(null), (float) ToVBBool(right));

                case ((TypeCode) 0x10a):
                    return CompareDouble(conv.ToDouble(null), 0.0);

                case ((TypeCode) 0x10d):
                    return CompareDouble(conv.ToDouble(null), (double) ToVBBool(right));

                case ((TypeCode) 0x11d):
                    return CompareDecimal(conv, (IConvertible) decimal.Zero);

                case ((TypeCode) 0x120):
                    return CompareDecimal(conv, ToVBBoolConv(right));

                case ((TypeCode) 0x130):
                    return CompareDate(conv.ToDateTime(null), DateTime.MinValue);

                case ((TypeCode) 320):
                    return CompareDate(conv.ToDateTime(null), right.ToDateTime(null));

                case ((TypeCode) 0x142):
                    return CompareDate(conv.ToDateTime(null), Conversions.ToDate(right.ToString(null)));

                case ((TypeCode) 0x156):
                    return (CompareClass) CompareString(conv.ToString(null), null, TextCompare);

                case ((TypeCode) 0x159):
                    return CompareBoolean(Conversions.ToBoolean(conv.ToString(null)), right.ToBoolean(null));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return CompareDouble(Conversions.ToDouble(conv.ToString(null)), right.ToDouble(null));

                case ((TypeCode) 0x166):
                    return CompareDate(Conversions.ToDate(conv.ToString(null)), right.ToDateTime(null));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                return CompareClass.Undefined;
            }
            return CompareClass.UserDefined;
        }

        public static object CompareObjectEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Equal, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Equal, Left, Right);
            }
            return (class2 == CompareClass.Equal);
        }

        public static object CompareObjectGreater(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Greater, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Greater, Left, Right);
            }
            return (class2 > CompareClass.Equal);
        }

        public static object CompareObjectGreaterEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.GreaterEqual, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.GreaterEqual, Left, Right);
            }
            return (class2 >= CompareClass.Equal);
        }

        public static object CompareObjectLess(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Less, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Less, Left, Right);
            }
            return (class2 < CompareClass.Equal);
        }

        public static object CompareObjectLessEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.LessEqual, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.LessEqual, Left, Right);
            }
            return (class2 <= CompareClass.Equal);
        }

        public static object CompareObjectNotEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return true;

                case CompareClass.UserDefined:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.NotEqual, new object[] { Left, Right });

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.NotEqual, Left, Right);
            }
            return (class2 != CompareClass.Equal);
        }

        private static CompareClass CompareSingle(float Left, float Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left < Right)
            {
                return CompareClass.Less;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Unordered;
        }

        public static int CompareString(string Left, string Right, bool TextCompare)
        {
            int num2;
            if (Left == Right)
            {
                return 0;
            }
            if (Left == null)
            {
                if (Right.Length == 0)
                {
                    return 0;
                }
                return -1;
            }
            if (Right == null)
            {
                if (Left.Length == 0)
                {
                    return 0;
                }
                return 1;
            }
            if (TextCompare)
            {
                num2 = Utils.GetCultureInfo().CompareInfo.Compare(Left, Right, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            else
            {
                num2 = string.CompareOrdinal(Left, Right);
            }
            if (num2 == 0)
            {
                return 0;
            }
            if (num2 > 0)
            {
                return 1;
            }
            return -1;
        }

        private static CompareClass CompareUInt32(uint Left, uint Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        private static CompareClass CompareUInt64(ulong Left, ulong Right)
        {
            if (Left == Right)
            {
                return CompareClass.Equal;
            }
            if (Left > Right)
            {
                return CompareClass.Greater;
            }
            return CompareClass.Less;
        }

        public static object ConcatenateObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = Left as IConvertible;
            if (convertible == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
            if ((empty == TypeCode.Object) && (Left is char[]))
            {
                empty = TypeCode.String;
            }
            if ((typeCode == TypeCode.Object) && (Right is char[]))
            {
                typeCode = TypeCode.String;
            }
            if ((empty == TypeCode.Object) || (typeCode == TypeCode.Object))
            {
                return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Concatenate, new object[] { Left, Right });
            }
            bool flag = empty == TypeCode.DBNull;
            bool flag2 = typeCode == TypeCode.DBNull;
            if (flag & flag2)
            {
                return Left;
            }
            if (flag & !flag2)
            {
                Left = "";
            }
            else if (flag2 & !flag)
            {
                Right = "";
            }
            return (Conversions.ToString(Left) + Conversions.ToString(Right));
        }

        public static bool ConditionalCompareObjectEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Equal, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Equal, Left, Right);
            }
            return (class2 == CompareClass.Equal);
        }

        public static bool ConditionalCompareObjectGreater(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Greater, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Greater, Left, Right);
            }
            return (class2 > CompareClass.Equal);
        }

        public static bool ConditionalCompareObjectGreaterEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.GreaterEqual, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.GreaterEqual, Left, Right);
            }
            return (class2 >= CompareClass.Equal);
        }

        public static bool ConditionalCompareObjectLess(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Less, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Less, Left, Right);
            }
            return (class2 < CompareClass.Equal);
        }

        public static bool ConditionalCompareObjectLessEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return false;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.LessEqual, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.LessEqual, Left, Right);
            }
            return (class2 <= CompareClass.Equal);
        }

        public static bool ConditionalCompareObjectNotEqual(object Left, object Right, bool TextCompare)
        {
            CompareClass class2 = CompareObject2(Left, Right, TextCompare);
            switch (class2)
            {
                case CompareClass.Unordered:
                    return true;

                case CompareClass.UserDefined:
                    return Conversions.ToBoolean(InvokeUserDefinedOperator(Symbols.UserDefinedOperator.NotEqual, new object[] { Left, Right }));

                case CompareClass.Undefined:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.NotEqual, Left, Right);
            }
            return (class2 != CompareClass.Equal);
        }

        private static object DivideDecimal(IConvertible Left, IConvertible Right)
        {
            decimal num = Left.ToDecimal(null);
            decimal num2 = Right.ToDecimal(null);
            try
            {
                return decimal.Divide(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToSingle(num) / Convert.ToSingle(num2));
            }
        }

        private static object DivideDouble(double Left, double Right)
        {
            return (Left / Right);
        }

        public static object DivideObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return DivideDouble(0.0, 0.0);

                case TypeCode.Boolean:
                    return DivideDouble(0.0, (double) ToVBBool(convertible2));

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Double:
                    return DivideDouble(0.0, convertible2.ToDouble(null));

                case TypeCode.Single:
                    return DivideSingle(0f, convertible2.ToSingle(null));

                case TypeCode.Decimal:
                    return DivideDecimal((IConvertible) decimal.Zero, convertible2);

                case TypeCode.String:
                    return DivideDouble(0.0, Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x39):
                    return DivideDouble((double) ToVBBool(conv), 0.0);

                case ((TypeCode) 60):
                    return DivideDouble((double) ToVBBool(conv), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x45):
                case ((TypeCode) 0x47):
                    return DivideDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 70):
                    return DivideSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x48):
                    return DivideDecimal(ToVBBoolConv(conv), convertible2);

                case ((TypeCode) 0x4b):
                    return DivideDouble((double) ToVBBool(conv), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                case ((TypeCode) 0x72):
                case ((TypeCode) 0x85):
                case ((TypeCode) 0x98):
                case ((TypeCode) 0xab):
                case ((TypeCode) 190):
                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xe4):
                case ((TypeCode) 0x10a):
                    return DivideDouble(conv.ToDouble(null), 0.0);

                case ((TypeCode) 0x62):
                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0xe7):
                case ((TypeCode) 0x10d):
                    return DivideDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 100):
                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x6b):
                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x77):
                case ((TypeCode) 120):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x7c):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7e):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 160):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 200):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xee):
                case ((TypeCode) 0xef):
                case ((TypeCode) 240):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return DivideDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return DivideSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return DivideDecimal(conv, convertible2);

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return DivideDouble(conv.ToDouble(null), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0xf7):
                    return DivideSingle(conv.ToSingle(null), 0f);

                case ((TypeCode) 250):
                    return DivideSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x11d):
                    return DivideDecimal(conv, (IConvertible) decimal.Zero);

                case ((TypeCode) 0x120):
                    return DivideDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0x156):
                    return DivideDouble(Conversions.ToDouble(conv.ToString(null)), 0.0);

                case ((TypeCode) 0x159):
                    return DivideDouble(Conversions.ToDouble(conv.ToString(null)), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return DivideDouble(Conversions.ToDouble(conv.ToString(null)), convertible2.ToDouble(null));

                case ((TypeCode) 360):
                    return DivideDouble(Conversions.ToDouble(conv.ToString(null)), Conversions.ToDouble(convertible2.ToString(null)));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Divide, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Divide, new object[] { Left, Right });
        }

        private static object DivideSingle(float Left, float Right)
        {
            float f = Left / Right;
            if (float.IsInfinity(f) && (!float.IsInfinity(Left) && !float.IsInfinity(Right)))
            {
                return (((double) Left) / ((double) Right));
            }
            return f;
        }

        public static object ExponentObject(object Left, object Right)
        {
            double num;
            double num2;
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
            switch (empty)
            {
                case TypeCode.Empty:
                    num = 0.0;
                    break;

                case TypeCode.Object:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Power, new object[] { Left, Right });

                case TypeCode.Boolean:
                    num = ToVBBool(conv);
                    break;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    num = conv.ToDouble(null);
                    break;

                case TypeCode.String:
                    num = Conversions.ToDouble(conv.ToString(null));
                    break;

                default:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Power, Left, Right);
            }
            switch (typeCode)
            {
                case TypeCode.Empty:
                    num2 = 0.0;
                    break;

                case TypeCode.Object:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Power, new object[] { Left, Right });

                case TypeCode.Boolean:
                    num2 = ToVBBool(convertible2);
                    break;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    num2 = convertible2.ToDouble(null);
                    break;

                case TypeCode.String:
                    num2 = Conversions.ToDouble(convertible2.ToString(null));
                    break;

                default:
                    throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Power, Left, Right);
            }
            return Math.Pow(num, num2);
        }

        [DebuggerHidden, DebuggerStepThrough, Obsolete("do not use this method", true)]
        public static object FallbackInvokeUserDefinedOperator(object vbOp, object[] Arguments)
        {
            return InvokeObjectUserDefinedOperator((Symbols.UserDefinedOperator) Conversions.ToSByte(vbOp), Arguments);
        }

        internal static Symbols.Method GetCallableUserDefinedOperator(Symbols.UserDefinedOperator Op, params object[] Arguments)
        {
            Symbols.Method targetProcedure = ResolveUserDefinedOperator(Op, Arguments, false);
            if (((targetProcedure != null) && !targetProcedure.ArgumentsValidated) && !OverloadResolution.CanMatchArguments(targetProcedure, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, false, null))
            {
                return null;
            }
            return targetProcedure;
        }

        private static Type GetEnumResult(object Left, object Right)
        {
            if (Left != null)
            {
                if (Left is Enum)
                {
                    if (Right == null)
                    {
                        return Left.GetType();
                    }
                    if (Right is Enum)
                    {
                        Type type = Left.GetType();
                        if (type == Right.GetType())
                        {
                            return type;
                        }
                    }
                }
            }
            else if (Right is Enum)
            {
                return Right.GetType();
            }
            return null;
        }

        private static Exception GetNoValidOperatorException(Symbols.UserDefinedOperator Op, object Operand)
        {
            return new InvalidCastException(Utils.GetResourceString("UnaryOperand2", new string[] { Symbols.OperatorNames[(int) Op], Utils.VBFriendlyName(Operand) }));
        }

        private static Exception GetNoValidOperatorException(Symbols.UserDefinedOperator Op, object Left, object Right)
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
            return new InvalidCastException(Utils.GetResourceString("BinaryOperands3", new string[] { Symbols.OperatorNames[(int) Op], resourceString, str2 }));
        }

        private static object IntDivideByte(byte Left, byte Right)
        {
            return (byte) (Left / Right);
        }

        private static object IntDivideInt16(short Left, short Right)
        {
            if ((Left == -32768) && (Right == -1))
            {
                return 0x8000;
            }
            return (short) (Left / Right);
        }

        private static object IntDivideInt32(int Left, int Right)
        {
            if ((Left == -2147483648) && (Right == -1))
            {
                return (long) 0x80000000L;
            }
            return (Left / Right);
        }

        private static object IntDivideInt64(long Left, long Right)
        {
            return (Left / Right);
        }

        public static object IntDivideObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return IntDivideInt32(0, 0);

                case TypeCode.Boolean:
                    return IntDivideInt16(0, ToVBBool(convertible2));

                case TypeCode.SByte:
                    return IntDivideSByte(0, convertible2.ToSByte(null));

                case TypeCode.Byte:
                    return IntDivideByte(0, convertible2.ToByte(null));

                case TypeCode.Int16:
                    return IntDivideInt16(0, convertible2.ToInt16(null));

                case TypeCode.UInt16:
                    return IntDivideUInt16(0, convertible2.ToUInt16(null));

                case TypeCode.Int32:
                    return IntDivideInt32(0, convertible2.ToInt32(null));

                case TypeCode.UInt32:
                    return IntDivideUInt32(0, convertible2.ToUInt32(null));

                case TypeCode.Int64:
                    return IntDivideInt64(0L, convertible2.ToInt64(null));

                case TypeCode.UInt64:
                    return IntDivideUInt64(0L, convertible2.ToUInt64(null));

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return IntDivideInt64(0L, convertible2.ToInt64(null));

                case TypeCode.String:
                    return IntDivideInt64(0L, Conversions.ToLong(convertible2.ToString(null)));

                case ((TypeCode) 0x39):
                    return IntDivideInt16(ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return IntDivideInt16(ToVBBool(conv), ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                    return IntDivideSByte(ToVBBool(conv), convertible2.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return IntDivideInt16(ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return IntDivideInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x45):
                case ((TypeCode) 70):
                case ((TypeCode) 0x47):
                case ((TypeCode) 0x48):
                    return IntDivideInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x4b):
                    return IntDivideInt64((long) ToVBBool(conv), Conversions.ToLong(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                    return IntDivideSByte(conv.ToSByte(null), 0);

                case ((TypeCode) 0x62):
                    return IntDivideSByte(conv.ToSByte(null), ToVBBool(convertible2));

                case ((TypeCode) 100):
                    return IntDivideSByte(conv.ToSByte(null), convertible2.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return IntDivideInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return IntDivideInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x6b):
                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x6d):
                case ((TypeCode) 110):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 0x12a):
                case ((TypeCode) 0x12b):
                case ((TypeCode) 300):
                    return IntDivideInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return IntDivideInt64(conv.ToInt64(null), Conversions.ToLong(convertible2.ToString(null)));

                case ((TypeCode) 0x72):
                    return IntDivideByte(conv.ToByte(null), 0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return IntDivideInt16(conv.ToInt16(null), ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return IntDivideByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return IntDivideUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return IntDivideUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return IntDivideUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null));

                case ((TypeCode) 0x85):
                    return IntDivideInt16(conv.ToInt16(null), 0);

                case ((TypeCode) 0x98):
                    return IntDivideUInt16(conv.ToUInt16(null), 0);

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return IntDivideInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xab):
                    return IntDivideInt32(conv.ToInt32(null), 0);

                case ((TypeCode) 190):
                    return IntDivideUInt32(conv.ToUInt32(null), 0);

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0xe7):
                case ((TypeCode) 250):
                case ((TypeCode) 0x10d):
                case ((TypeCode) 0x120):
                    return IntDivideInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                    return IntDivideInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0xe4):
                    return IntDivideUInt64(conv.ToUInt64(null), 0L);

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return IntDivideInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0x156):
                    return IntDivideInt64(Conversions.ToLong(conv.ToString(null)), 0L);

                case ((TypeCode) 0x159):
                    return IntDivideInt64(Conversions.ToLong(conv.ToString(null)), (long) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return IntDivideInt64(Conversions.ToLong(conv.ToString(null)), convertible2.ToInt64(null));

                case ((TypeCode) 360):
                    return IntDivideInt64(Conversions.ToLong(conv.ToString(null)), Conversions.ToLong(convertible2.ToString(null)));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.IntegralDivide, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.IntegralDivide, new object[] { Left, Right });
        }

        private static object IntDivideSByte(sbyte Left, sbyte Right)
        {
            if ((Left == -128) && (Right == -1))
            {
                return (short) 0x80;
            }
            return (sbyte) (Left / Right);
        }

        private static object IntDivideUInt16(ushort Left, ushort Right)
        {
            return (ushort) (Left / Right);
        }

        private static object IntDivideUInt32(uint Left, uint Right)
        {
            return (Left / Right);
        }

        private static object IntDivideUInt64(ulong Left, ulong Right)
        {
            return (Left / Right);
        }

        internal static object InvokeObjectUserDefinedOperator(Symbols.UserDefinedOperator Op, object[] Arguments)
        {
            Symbols.Method operatorMethod = ResolveUserDefinedOperator(Op, Arguments, true);
            if (operatorMethod != null)
            {
                return InvokeUserDefinedOperator(operatorMethod, false, Arguments);
            }
            if (Arguments.Length > 1)
            {
                throw GetNoValidOperatorException(Op, Arguments[0], Arguments[1]);
            }
            throw GetNoValidOperatorException(Op, Arguments[0]);
        }

        internal static object InvokeUserDefinedOperator(Symbols.UserDefinedOperator Op, params object[] Arguments)
        {
            if (IDOUtils.TryCastToIDMOP(Arguments[0]) != null)
            {
                return IDOBinder.InvokeUserDefinedOperator(Op, Arguments);
            }
            return InvokeObjectUserDefinedOperator(Op, Arguments);
        }

        internal static object InvokeUserDefinedOperator(Symbols.Method OperatorMethod, bool ForceArgumentValidation, params object[] Arguments)
        {
            if ((!OperatorMethod.ArgumentsValidated || ForceArgumentValidation) && !OverloadResolution.CanMatchArguments(OperatorMethod, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, false, null))
            {
                string str = "";
                List<string> errors = new List<string>();
                bool flag = OverloadResolution.CanMatchArguments(OperatorMethod, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, false, errors);
                foreach (string str2 in errors)
                {
                    str = str + "\r\n    " + str2;
                }
                throw new InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", new string[] { OperatorMethod.ToString(), str }));
            }
            Symbols.Container container = new Symbols.Container(OperatorMethod.DeclaringType);
            return container.InvokeMethod(OperatorMethod, Arguments, null, BindingFlags.InvokeMethod);
        }

        public static object LeftShiftObject(object Operand, object Amount)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = Operand as IConvertible;
            if (convertible == null)
            {
                if (Operand == null)
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
            IConvertible convertible2 = Amount as IConvertible;
            if (convertible2 == null)
            {
                if (Amount == null)
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
            if ((empty == TypeCode.Object) || (typeCode == TypeCode.Object))
            {
                return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.ShiftLeft, new object[] { Operand, Amount });
            }
            switch (empty)
            {
                case TypeCode.Empty:
                    return (((int) 0) << Conversions.ToInteger(Amount));

                case TypeCode.Boolean:
                    return (short) (((short) -(convertible.ToBoolean(null) > 0)) << (Conversions.ToInteger(Amount) & 15));

                case TypeCode.SByte:
                    return (sbyte) (convertible.ToSByte(null) << (Conversions.ToInteger(Amount) & 7));

                case TypeCode.Byte:
                    return (byte) (convertible.ToByte(null) << (Conversions.ToInteger(Amount) & 7));

                case TypeCode.Int16:
                    return (short) (convertible.ToInt16(null) << (Conversions.ToInteger(Amount) & 15));

                case TypeCode.UInt16:
                    return (ushort) (convertible.ToUInt16(null) << (Conversions.ToInteger(Amount) & 15));

                case TypeCode.Int32:
                    return (convertible.ToInt32(null) << Conversions.ToInteger(Amount));

                case TypeCode.UInt32:
                    return (convertible.ToUInt32(null) << Conversions.ToInteger(Amount));

                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (convertible.ToInt64(null) << Conversions.ToInteger(Amount));

                case TypeCode.UInt64:
                    return (convertible.ToUInt64(null) << Conversions.ToInteger(Amount));

                case TypeCode.String:
                    return (Conversions.ToLong(convertible.ToString(null)) << Conversions.ToInteger(Amount));
            }
            throw GetNoValidOperatorException(Symbols.UserDefinedOperator.ShiftLeft, Operand);
        }

        public static object LikeObject(object Source, object Pattern, CompareMethod CompareOption)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = Source as IConvertible;
            if (convertible == null)
            {
                if (Source == null)
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
            IConvertible convertible2 = Pattern as IConvertible;
            if (convertible2 == null)
            {
                if (Pattern == null)
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
            if ((empty == TypeCode.Object) && (Source is char[]))
            {
                empty = TypeCode.String;
            }
            if ((typeCode == TypeCode.Object) && (Pattern is char[]))
            {
                typeCode = TypeCode.String;
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                return LikeString(Conversions.ToString(Source), Conversions.ToString(Pattern), CompareOption);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Like, new object[] { Source, Pattern });
        }

        public static bool LikeString(string Source, string Pattern, CompareMethod CompareOption)
        {
            if (CompareOption == CompareMethod.Binary)
            {
                return LikeStringBinary(Source, Pattern);
            }
            return LikeStringText(Source, Pattern);
        }

        private static bool LikeStringBinary(string Source, string Pattern)
        {
            bool flag;
            int length;
            int num2;
            char ch3;
            int num4;
            int num5;
            bool flag3 = false;
            if (Pattern == null)
            {
                length = 0;
            }
            else
            {
                length = Pattern.Length;
            }
            if (Source == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = Source.Length;
            }
            if (num5 < num4)
            {
                ch3 = Source[num5];
            }
            while (num2 < length)
            {
                char p = Pattern[num2];
                if ((p == '*') && !flag)
                {
                    int num3 = AsteriskSkip(Pattern.Substring(num2 + 1), Source.Substring(num5), num4 - num5, CompareMethod.Binary, Strings.m_InvariantCompareInfo);
                    if (num3 < 0)
                    {
                        return false;
                    }
                    if (num3 > 0)
                    {
                        num5 += num3;
                        if (num5 < num4)
                        {
                            ch3 = Source[num5];
                        }
                    }
                }
                else if ((p == '?') && !flag)
                {
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else if ((p == '#') && !flag)
                {
                    if (!char.IsDigit(ch3))
                    {
                        break;
                    }
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else
                {
                    bool flag5;
                    bool flag6;
                    if ((((p == '-') && flag) && (flag6 && !flag3)) && (!flag5 && (((num2 + 1) >= length) || (Pattern[num2 + 1] != ']'))))
                    {
                        flag5 = true;
                    }
                    else
                    {
                        bool flag4;
                        bool flag7;
                        if (((p == '!') && flag) && !flag7)
                        {
                            flag7 = true;
                            flag4 = true;
                        }
                        else
                        {
                            char ch;
                            char ch4;
                            if ((p == '[') && !flag)
                            {
                                flag = true;
                                ch4 = '\0';
                                ch = '\0';
                                flag6 = false;
                            }
                            else if ((p == ']') && flag)
                            {
                                flag = false;
                                if (flag6)
                                {
                                    if (!flag4)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                else if (flag5)
                                {
                                    if (!flag4)
                                    {
                                        break;
                                    }
                                }
                                else if (flag7)
                                {
                                    if ('!' != ch3)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                flag4 = false;
                                flag6 = false;
                                flag7 = false;
                                flag5 = false;
                            }
                            else
                            {
                                flag6 = true;
                                flag3 = false;
                                if (flag)
                                {
                                    if (flag5)
                                    {
                                        flag5 = false;
                                        flag3 = true;
                                        ch = p;
                                        if (ch4 > ch)
                                        {
                                            throw ExceptionUtils.VbMakeException(0x5d);
                                        }
                                        if ((flag7 && flag4) || (!flag7 && !flag4))
                                        {
                                            flag4 = (ch3 > ch4) && (ch3 <= ch);
                                            if (flag7)
                                            {
                                                flag4 = !flag4;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ch4 = p;
                                        flag4 = LikeStringCompareBinary(flag7, flag4, p, ch3);
                                    }
                                }
                                else
                                {
                                    if ((p != ch3) && !flag7)
                                    {
                                        break;
                                    }
                                    flag7 = false;
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                    else if (num5 > num4)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                num2++;
            }
            if (flag)
            {
                if (num4 != 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                }
                return false;
            }
            return ((num2 == length) && (num5 == num4));
        }

        private static bool LikeStringCompare(CompareInfo ci, bool SeenNot, bool Match, char p, char s, CompareOptions Options)
        {
            if (SeenNot && Match)
            {
                if (Options == CompareOptions.Ordinal)
                {
                    return (p != s);
                }
                return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) != 0);
            }
            if (SeenNot || Match)
            {
                return Match;
            }
            if (Options == CompareOptions.Ordinal)
            {
                return (p == s);
            }
            return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) == 0);
        }

        private static bool LikeStringCompareBinary(bool SeenNot, bool Match, char p, char s)
        {
            if (SeenNot && Match)
            {
                return (p != s);
            }
            if (!SeenNot && !Match)
            {
                return (p == s);
            }
            return Match;
        }

        private static bool LikeStringText(string Source, string Pattern)
        {
            bool flag;
            int length;
            int num2;
            char ch3;
            int num4;
            int num5;
            bool flag3 = false;
            if (Pattern == null)
            {
                length = 0;
            }
            else
            {
                length = Pattern.Length;
            }
            if (Source == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = Source.Length;
            }
            if (num5 < num4)
            {
                ch3 = Source[num5];
            }
            CompareInfo compareInfo = Utils.GetCultureInfo().CompareInfo;
            CompareOptions options = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
            while (num2 < length)
            {
                char p = Pattern[num2];
                if ((p == '*') && !flag)
                {
                    int num3 = AsteriskSkip(Pattern.Substring(num2 + 1), Source.Substring(num5), num4 - num5, CompareMethod.Text, compareInfo);
                    if (num3 < 0)
                    {
                        return false;
                    }
                    if (num3 > 0)
                    {
                        num5 += num3;
                        if (num5 < num4)
                        {
                            ch3 = Source[num5];
                        }
                    }
                }
                else if ((p == '?') && !flag)
                {
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else if ((p == '#') && !flag)
                {
                    if (!char.IsDigit(ch3))
                    {
                        break;
                    }
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else
                {
                    bool flag5;
                    bool flag6;
                    if ((((p == '-') && flag) && (flag6 && !flag3)) && (!flag5 && (((num2 + 1) >= length) || (Pattern[num2 + 1] != ']'))))
                    {
                        flag5 = true;
                    }
                    else
                    {
                        bool flag4;
                        bool flag7;
                        if (((p == '!') && flag) && !flag7)
                        {
                            flag7 = true;
                            flag4 = true;
                        }
                        else
                        {
                            char ch;
                            char ch4;
                            if ((p == '[') && !flag)
                            {
                                flag = true;
                                ch4 = '\0';
                                ch = '\0';
                                flag6 = false;
                            }
                            else if ((p == ']') && flag)
                            {
                                flag = false;
                                if (flag6)
                                {
                                    if (!flag4)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                else if (flag5)
                                {
                                    if (!flag4)
                                    {
                                        break;
                                    }
                                }
                                else if (flag7)
                                {
                                    if (compareInfo.Compare("!", Conversions.ToString(ch3)) != 0)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                flag4 = false;
                                flag6 = false;
                                flag7 = false;
                                flag5 = false;
                            }
                            else
                            {
                                flag6 = true;
                                flag3 = false;
                                if (flag)
                                {
                                    if (flag5)
                                    {
                                        flag5 = false;
                                        flag3 = true;
                                        ch = p;
                                        if (ch4 > ch)
                                        {
                                            throw ExceptionUtils.VbMakeException(0x5d);
                                        }
                                        if ((flag7 && flag4) || (!flag7 && !flag4))
                                        {
                                            if (options == CompareOptions.Ordinal)
                                            {
                                                flag4 = (ch3 > ch4) && (ch3 <= ch);
                                            }
                                            else
                                            {
                                                flag4 = (compareInfo.Compare(Conversions.ToString(ch4), Conversions.ToString(ch3), options) < 0) && (compareInfo.Compare(Conversions.ToString(ch), Conversions.ToString(ch3), options) >= 0);
                                            }
                                            if (flag7)
                                            {
                                                flag4 = !flag4;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ch4 = p;
                                        flag4 = LikeStringCompare(compareInfo, flag7, flag4, p, ch3, options);
                                    }
                                }
                                else
                                {
                                    if (options == CompareOptions.Ordinal)
                                    {
                                        if ((p != ch3) && !flag7)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        string str = Conversions.ToString(p);
                                        string str2 = Conversions.ToString(ch3);
                                        while (((num2 + 1) < length) && ((UnicodeCategory.ModifierSymbol == char.GetUnicodeCategory(Pattern[num2 + 1])) || (UnicodeCategory.NonSpacingMark == char.GetUnicodeCategory(Pattern[num2 + 1]))))
                                        {
                                            str = str + Conversions.ToString(Pattern[num2 + 1]);
                                            num2++;
                                        }
                                        while (((num5 + 1) < num4) && ((UnicodeCategory.ModifierSymbol == char.GetUnicodeCategory(Source[num5 + 1])) || (UnicodeCategory.NonSpacingMark == char.GetUnicodeCategory(Source[num5 + 1]))))
                                        {
                                            str2 = str2 + Conversions.ToString(Source[num5 + 1]);
                                            num5++;
                                        }
                                        if ((compareInfo.Compare(str, str2, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase) != 0) && !flag7)
                                        {
                                            break;
                                        }
                                    }
                                    flag7 = false;
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                    else if (num5 > num4)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                num2++;
            }
            if (flag)
            {
                if (num4 != 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                }
                return false;
            }
            return ((num2 == length) && (num5 == num4));
        }

        private static object ModByte(byte Left, byte Right)
        {
            return (byte) (Left % Right);
        }

        private static object ModDecimal(IConvertible Left, IConvertible Right)
        {
            decimal num = Left.ToDecimal(null);
            decimal num2 = Right.ToDecimal(null);
            return decimal.Remainder(num, num2);
        }

        private static object ModDouble(double Left, double Right)
        {
            return (Left % Right);
        }

        private static object ModInt16(short Left, short Right)
        {
            int num = Left % Right;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object ModInt32(int Left, int Right)
        {
            long num = ((long) Left) % ((long) Right);
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object ModInt64(long Left, long Right)
        {
            if ((Left == -9223372036854775808L) && (Right == -1L))
            {
                return 0L;
            }
            return (Left % Right);
        }

        public static object ModObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return ModInt32(0, 0);

                case TypeCode.Boolean:
                    return ModInt16(0, ToVBBool(convertible2));

                case TypeCode.SByte:
                    return ModSByte(0, convertible2.ToSByte(null));

                case TypeCode.Byte:
                    return ModByte(0, convertible2.ToByte(null));

                case TypeCode.Int16:
                    return ModInt16(0, convertible2.ToInt16(null));

                case TypeCode.UInt16:
                    return ModUInt16(0, convertible2.ToUInt16(null));

                case TypeCode.Int32:
                    return ModInt32(0, convertible2.ToInt32(null));

                case TypeCode.UInt32:
                    return ModUInt32(0, convertible2.ToUInt32(null));

                case TypeCode.Int64:
                    return ModInt64(0L, convertible2.ToInt64(null));

                case TypeCode.UInt64:
                    return ModUInt64(0L, convertible2.ToUInt64(null));

                case TypeCode.Single:
                    return ModSingle(0f, convertible2.ToSingle(null));

                case TypeCode.Double:
                    return ModDouble(0.0, convertible2.ToDouble(null));

                case TypeCode.Decimal:
                    return ModDecimal((IConvertible) decimal.Zero, (IConvertible) convertible2.ToDecimal(null));

                case TypeCode.String:
                    return ModDouble(0.0, Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x39):
                    return ModInt16(ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return ModInt16(ToVBBool(conv), ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                    return ModSByte(ToVBBool(conv), convertible2.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return ModInt16(ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return ModInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                    return ModInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x45):
                case ((TypeCode) 0x48):
                    return ModDecimal(ToVBBoolConv(conv), (IConvertible) convertible2.ToDecimal(null));

                case ((TypeCode) 70):
                    return ModSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return ModDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x4b):
                    return ModDouble((double) ToVBBool(conv), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                    return ModSByte(conv.ToSByte(null), 0);

                case ((TypeCode) 0x62):
                    return ModSByte(conv.ToSByte(null), ToVBBool(convertible2));

                case ((TypeCode) 100):
                    return ModSByte(conv.ToSByte(null), convertible2.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return ModInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return ModInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                    return ModInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x6b):
                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return ModDecimal(conv, convertible2);

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return ModSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return ModDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return ModDouble(conv.ToDouble(null), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x72):
                    return ModByte(conv.ToByte(null), 0);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return ModInt16(conv.ToInt16(null), ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return ModByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return ModUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return ModUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return ModUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null));

                case ((TypeCode) 0x85):
                    return ModInt16(conv.ToInt16(null), 0);

                case ((TypeCode) 0x98):
                    return ModUInt16(conv.ToUInt16(null), 0);

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return ModInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xab):
                    return ModInt32(conv.ToInt32(null), 0);

                case ((TypeCode) 190):
                    return ModUInt32(conv.ToUInt32(null), 0);

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                    return ModInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                    return ModInt64(conv.ToInt64(null), 0L);

                case ((TypeCode) 0xe4):
                    return ModUInt64(conv.ToUInt64(null), 0L);

                case ((TypeCode) 0xe7):
                case ((TypeCode) 0x120):
                    return ModDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0xf7):
                    return ModSingle(conv.ToSingle(null), 0f);

                case ((TypeCode) 250):
                    return ModSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10a):
                    return ModDouble(conv.ToDouble(null), 0.0);

                case ((TypeCode) 0x10d):
                    return ModDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x11d):
                    return ModDecimal(conv, (IConvertible) decimal.Zero);

                case ((TypeCode) 0x156):
                    return ModDouble(Conversions.ToDouble(conv.ToString(null)), 0.0);

                case ((TypeCode) 0x159):
                    return ModDouble(Conversions.ToDouble(conv.ToString(null)), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return ModDouble(Conversions.ToDouble(conv.ToString(null)), convertible2.ToDouble(null));

                case ((TypeCode) 360):
                    return ModDouble(Conversions.ToDouble(conv.ToString(null)), Conversions.ToDouble(convertible2.ToString(null)));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Modulus, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Modulus, new object[] { Left, Right });
        }

        private static object ModSByte(sbyte Left, sbyte Right)
        {
            return (sbyte) (Left % Right);
        }

        private static object ModSingle(float Left, float Right)
        {
            return (Left % Right);
        }

        private static object ModUInt16(ushort Left, ushort Right)
        {
            return (ushort) (Left % Right);
        }

        private static object ModUInt32(uint Left, uint Right)
        {
            return (Left % Right);
        }

        private static object ModUInt64(ulong Left, ulong Right)
        {
            return (Left % Right);
        }

        private static int MultipleAsteriskSkip(string Pattern, string Source, int Count, CompareMethod CompareOption)
        {
            int num2 = Strings.Len(Source);
            while (Count < num2)
            {
                bool flag;
                string source = Source.Substring(num2 - Count);
                try
                {
                    flag = LikeString(source, Pattern, CompareOption);
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
                    flag = false;
                }
                if (flag)
                {
                    return Count;
                }
                Count++;
            }
            return Count;
        }

        private static object MultiplyByte(byte Left, byte Right)
        {
            int num = Left * Right;
            if (num <= 0xff)
            {
                return (byte) num;
            }
            if (num > 0x7fff)
            {
                return num;
            }
            return (short) num;
        }

        private static object MultiplyDecimal(IConvertible Left, IConvertible Right)
        {
            decimal num = Left.ToDecimal(null);
            decimal num2 = Right.ToDecimal(null);
            try
            {
                return decimal.Multiply(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) * Convert.ToDouble(num2));
            }
        }

        private static object MultiplyDouble(double Left, double Right)
        {
            return (Left * Right);
        }

        private static object MultiplyInt16(short Left, short Right)
        {
            int num = Left * Right;
            if ((num <= 0x7fff) && (num >= -32768))
            {
                return (short) num;
            }
            return num;
        }

        private static object MultiplyInt32(int Left, int Right)
        {
            long num = Left * Right;
            if ((num <= 0x7fffffffL) && (num >= -2147483648L))
            {
                return (int) num;
            }
            return num;
        }

        private static object MultiplyInt64(long Left, long Right)
        {
            try
            {
                return (Left * Right);
            }
            catch (OverflowException)
            {
            }
            try
            {
                return decimal.Multiply(new decimal(Left), new decimal(Right));
            }
            catch (OverflowException)
            {
                return (Left * Right);
            }
        }

        public static object MultiplyObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                case TypeCode.Int16:
                case ((TypeCode) 0x39):
                case ((TypeCode) 0x85):
                    return Boxed_ZeroShort;

                case TypeCode.SByte:
                case ((TypeCode) 0x5f):
                    return Boxed_ZeroSByte;

                case TypeCode.Byte:
                case ((TypeCode) 0x72):
                    return Boxed_ZeroByte;

                case TypeCode.UInt16:
                case ((TypeCode) 0x98):
                    return Boxed_ZeroUShort;

                case TypeCode.UInt32:
                case ((TypeCode) 190):
                    return Boxed_ZeroUInteger;

                case TypeCode.Int64:
                case ((TypeCode) 0xd1):
                    return Boxed_ZeroLong;

                case TypeCode.UInt64:
                case ((TypeCode) 0xe4):
                    return Boxed_ZeroULong;

                case TypeCode.Single:
                case ((TypeCode) 0xf7):
                    return Boxed_ZeroSinge;

                case TypeCode.Double:
                case ((TypeCode) 0x10a):
                    return Boxed_ZeroDouble;

                case TypeCode.Decimal:
                case ((TypeCode) 0x11d):
                    return Boxed_ZeroDecimal;

                case TypeCode.String:
                    return MultiplyDouble(0.0, Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 60):
                    return MultiplyInt16(ToVBBool(conv), ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                    return MultiplySByte(ToVBBool(conv), convertible2.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return MultiplyInt16(ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return MultiplyInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                    return MultiplyInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x45):
                case ((TypeCode) 0x48):
                    return MultiplyDecimal(ToVBBoolConv(conv), (IConvertible) convertible2.ToDecimal(null));

                case ((TypeCode) 70):
                    return MultiplySingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return MultiplyDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x4b):
                    return MultiplyDouble((double) ToVBBool(conv), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x62):
                    return MultiplySByte(conv.ToSByte(null), ToVBBool(convertible2));

                case ((TypeCode) 100):
                    return MultiplySByte(conv.ToSByte(null), convertible2.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return MultiplyInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return MultiplyInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                    return MultiplyInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x6b):
                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return MultiplyDecimal(conv, convertible2);

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return MultiplySingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return MultiplyDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return MultiplyDouble(conv.ToDouble(null), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return MultiplyInt16(conv.ToInt16(null), ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return MultiplyByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return MultiplyUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return MultiplyUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return MultiplyUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null));

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return MultiplyInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                    return MultiplyInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xe7):
                case ((TypeCode) 0x120):
                    return MultiplyDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 250):
                    return MultiplySingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return MultiplyDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x156):
                    return MultiplyDouble(Conversions.ToDouble(conv.ToString(null)), 0.0);

                case ((TypeCode) 0x159):
                    return MultiplyDouble(Conversions.ToDouble(conv.ToString(null)), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return MultiplyDouble(Conversions.ToDouble(conv.ToString(null)), convertible2.ToDouble(null));

                case ((TypeCode) 360):
                    return MultiplyDouble(Conversions.ToDouble(conv.ToString(null)), Conversions.ToDouble(convertible2.ToString(null)));
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Multiply, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Multiply, new object[] { Left, Right });
        }

        private static object MultiplySByte(sbyte Left, sbyte Right)
        {
            short num = (short) (Left * Right);
            if ((num <= 0x7f) && (num >= -128))
            {
                return (sbyte) num;
            }
            return num;
        }

        private static object MultiplySingle(float Left, float Right)
        {
            double d = Left * Right;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(Left) && !float.IsInfinity(Right))))
            {
                return d;
            }
            return (float) d;
        }

        private static object MultiplyUInt16(ushort Left, ushort Right)
        {
            long num = Left * Right;
            if (num <= 0xffffL)
            {
                return (ushort) num;
            }
            if (num > 0x7fffffffL)
            {
                return num;
            }
            return (int) num;
        }

        private static object MultiplyUInt32(uint Left, uint Right)
        {
            ulong num = Left * Right;
            if (num <= 0xffffffffL)
            {
                return (uint) num;
            }
            if (decimal.Compare(new decimal(num), 9223372036854775807M) > 0)
            {
                return new decimal(num);
            }
            return (long) num;
        }

        private static object MultiplyUInt64(ulong Left, ulong Right)
        {
            try
            {
                return (Left * Right);
            }
            catch (OverflowException)
            {
            }
            try
            {
                return decimal.Multiply(new decimal(Left), new decimal(Right));
            }
            catch (OverflowException)
            {
                return (Left * Right);
            }
        }

        private static object NegateBoolean(bool Operand)
        {
            return -((short) -(Operand > false));
        }

        private static object NegateByte(byte Operand)
        {
            return (short) -Operand;
        }

        private static object NegateDecimal(decimal Operand)
        {
            try
            {
                return decimal.Negate(Operand);
            }
            catch (OverflowException)
            {
                return -Convert.ToDouble(Operand);
            }
        }

        private static object NegateDouble(double Operand)
        {
            return -Operand;
        }

        private static object NegateInt16(short Operand)
        {
            if (Operand == -32768)
            {
                return 0x8000;
            }
            return -Operand;
        }

        private static object NegateInt32(int Operand)
        {
            if (Operand == -2147483648)
            {
                return (long) 0x80000000L;
            }
            return (0 - Operand);
        }

        private static object NegateInt64(long Operand)
        {
            if (Operand == -9223372036854775808L)
            {
                return 9223372036854775808M;
            }
            return (0L - Operand);
        }

        public static object NegateObject(object Operand)
        {
            TypeCode empty;
            IConvertible convertible = Operand as IConvertible;
            if (convertible == null)
            {
                if (Operand == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Object:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Negate, new object[] { Operand });

                case TypeCode.Boolean:
                    if (Operand is bool)
                    {
                        return NegateBoolean((bool) Operand);
                    }
                    return NegateBoolean(convertible.ToBoolean(null));

                case TypeCode.SByte:
                    if (Operand is sbyte)
                    {
                        return NegateSByte((sbyte) Operand);
                    }
                    return NegateSByte(convertible.ToSByte(null));

                case TypeCode.Byte:
                    if (Operand is byte)
                    {
                        return NegateByte((byte) Operand);
                    }
                    return NegateByte(convertible.ToByte(null));

                case TypeCode.Int16:
                    if (Operand is short)
                    {
                        return NegateInt16((short) Operand);
                    }
                    return NegateInt16(convertible.ToInt16(null));

                case TypeCode.UInt16:
                    if (Operand is ushort)
                    {
                        return NegateUInt16((ushort) Operand);
                    }
                    return NegateUInt16(convertible.ToUInt16(null));

                case TypeCode.Int32:
                    if (Operand is int)
                    {
                        return NegateInt32((int) Operand);
                    }
                    return NegateInt32(convertible.ToInt32(null));

                case TypeCode.UInt32:
                    if (Operand is uint)
                    {
                        return NegateUInt32((uint) Operand);
                    }
                    return NegateUInt32(convertible.ToUInt32(null));

                case TypeCode.Int64:
                    if (Operand is long)
                    {
                        return NegateInt64((long) Operand);
                    }
                    return NegateInt64(convertible.ToInt64(null));

                case TypeCode.UInt64:
                    if (Operand is ulong)
                    {
                        return NegateUInt64((ulong) Operand);
                    }
                    return NegateUInt64(convertible.ToUInt64(null));

                case TypeCode.Single:
                    if (Operand is float)
                    {
                        return NegateSingle((float) Operand);
                    }
                    return NegateSingle(convertible.ToSingle(null));

                case TypeCode.Double:
                    if (Operand is double)
                    {
                        return NegateDouble((double) Operand);
                    }
                    return NegateDouble(convertible.ToDouble(null));

                case TypeCode.Decimal:
                    if (Operand is decimal)
                    {
                        return NegateDecimal((decimal) Operand);
                    }
                    return NegateDecimal(convertible.ToDecimal(null));

                case TypeCode.String:
                {
                    string operand = Operand as string;
                    if (operand != null)
                    {
                        return NegateString(operand);
                    }
                    return NegateString(convertible.ToString(null));
                }
            }
            throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Negate, Operand);
        }

        private static object NegateSByte(sbyte Operand)
        {
            if (Operand == -128)
            {
                return (short) 0x80;
            }
            return -Operand;
        }

        private static object NegateSingle(float Operand)
        {
            return -Operand;
        }

        private static object NegateString(string Operand)
        {
            return -Conversions.ToDouble(Operand);
        }

        private static object NegateUInt16(ushort Operand)
        {
            return (0 - Operand);
        }

        private static object NegateUInt32(uint Operand)
        {
            return (0L - Operand);
        }

        private static object NegateUInt64(ulong Operand)
        {
            return decimal.Negate(new decimal(Operand));
        }

        private static object NotBoolean(bool Operand)
        {
            return !Operand;
        }

        private static object NotByte(byte Operand, Type OperandType)
        {
            byte num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotInt16(short Operand, Type OperandType)
        {
            short num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotInt32(int Operand, Type OperandType)
        {
            int num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotInt64(long Operand)
        {
            return ~Operand;
        }

        private static object NotInt64(long Operand, Type OperandType)
        {
            long num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        public static object NotObject(object Operand)
        {
            TypeCode empty;
            IConvertible convertible = Operand as IConvertible;
            if (convertible == null)
            {
                if (Operand == null)
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
                    return -1;

                case TypeCode.Object:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Not, new object[] { Operand });

                case TypeCode.Boolean:
                    return NotBoolean(convertible.ToBoolean(null));

                case TypeCode.SByte:
                    return NotSByte(convertible.ToSByte(null), Operand.GetType());

                case TypeCode.Byte:
                    return NotByte(convertible.ToByte(null), Operand.GetType());

                case TypeCode.Int16:
                    return NotInt16(convertible.ToInt16(null), Operand.GetType());

                case TypeCode.UInt16:
                    return NotUInt16(convertible.ToUInt16(null), Operand.GetType());

                case TypeCode.Int32:
                    return NotInt32(convertible.ToInt32(null), Operand.GetType());

                case TypeCode.UInt32:
                    return NotUInt32(convertible.ToUInt32(null), Operand.GetType());

                case TypeCode.Int64:
                    return NotInt64(convertible.ToInt64(null), Operand.GetType());

                case TypeCode.UInt64:
                    return NotUInt64(convertible.ToUInt64(null), Operand.GetType());

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return NotInt64(convertible.ToInt64(null));

                case TypeCode.String:
                    return NotInt64(Conversions.ToLong(convertible.ToString(null)));
            }
            throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Not, Operand);
        }

        private static object NotSByte(sbyte Operand, Type OperandType)
        {
            sbyte num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotUInt16(ushort Operand, Type OperandType)
        {
            ushort num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotUInt32(uint Operand, Type OperandType)
        {
            uint num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object NotUInt64(ulong Operand, Type OperandType)
        {
            ulong num = ~Operand;
            if (OperandType.IsEnum)
            {
                return Enum.ToObject(OperandType, num);
            }
            return num;
        }

        private static object OrBoolean(bool Left, bool Right)
        {
            return (Left | Right);
        }

        private static object OrByte(byte Left, byte Right, Type EnumType = null)
        {
            byte num = (byte) (Left | Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrInt16(short Left, short Right, Type EnumType = null)
        {
            short num = (short) (Left | Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrInt32(int Left, int Right, Type EnumType = null)
        {
            int num = Left | Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrInt64(long Left, long Right, Type EnumType = null)
        {
            long num = Left | Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        public static object OrObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                    return OrBoolean(false, convertible2.ToBoolean(null));

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return Right;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return OrInt64(0L, convertible2.ToInt64(null), null);

                case TypeCode.String:
                    return OrInt64(0L, Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 0x39):
                    return OrBoolean(conv.ToBoolean(null), false);

                case ((TypeCode) 60):
                    return OrBoolean(conv.ToBoolean(null), convertible2.ToBoolean(null));

                case ((TypeCode) 0x3e):
                    return OrSByte(ToVBBool(conv), convertible2.ToSByte(null), null);

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return OrInt16(ToVBBool(conv), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return OrInt32(ToVBBool(conv), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x45):
                case ((TypeCode) 70):
                case ((TypeCode) 0x47):
                case ((TypeCode) 0x48):
                    return OrInt64((long) ToVBBool(conv), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x4b):
                    return OrBoolean(conv.ToBoolean(null), Conversions.ToBoolean(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                case ((TypeCode) 0x72):
                case ((TypeCode) 0x85):
                case ((TypeCode) 0x98):
                case ((TypeCode) 0xab):
                case ((TypeCode) 190):
                case ((TypeCode) 0xd1):
                case ((TypeCode) 0xe4):
                    return Left;

                case ((TypeCode) 0x62):
                    return OrSByte(conv.ToSByte(null), ToVBBool(convertible2), null);

                case ((TypeCode) 100):
                    return OrSByte(conv.ToSByte(null), convertible2.ToSByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                    return OrInt16(conv.ToInt16(null), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                    return OrInt32(conv.ToInt32(null), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x6b):
                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x6d):
                case ((TypeCode) 110):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 0x12a):
                case ((TypeCode) 0x12b):
                case ((TypeCode) 300):
                    return OrInt64(conv.ToInt64(null), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return OrInt64(conv.ToInt64(null), Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return OrInt16(conv.ToInt16(null), ToVBBool(convertible2), null);

                case ((TypeCode) 120):
                    return OrByte(conv.ToByte(null), convertible2.ToByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                    return OrUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), null);

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                    return OrUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), null);

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                    return OrUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), null);

                case ((TypeCode) 140):
                    return OrInt16(conv.ToInt16(null), convertible2.ToInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return OrInt32(conv.ToInt32(null), ToVBBool(convertible2), null);

                case ((TypeCode) 160):
                    return OrUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 180):
                    return OrInt32(conv.ToInt32(null), convertible2.ToInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0xe7):
                case ((TypeCode) 250):
                case ((TypeCode) 0x10d):
                case ((TypeCode) 0x120):
                    return OrInt64(conv.ToInt64(null), (long) ToVBBool(convertible2), null);

                case ((TypeCode) 200):
                    return OrUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 220):
                    return OrInt64(conv.ToInt64(null), convertible2.ToInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 240):
                    return OrUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return OrInt64(conv.ToInt64(null), 0L, null);

                case ((TypeCode) 0x156):
                    return OrInt64(Conversions.ToLong(conv.ToString(null)), 0L, null);

                case ((TypeCode) 0x159):
                    return OrBoolean(Conversions.ToBoolean(conv.ToString(null)), convertible2.ToBoolean(null));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return OrInt64(Conversions.ToLong(conv.ToString(null)), convertible2.ToInt64(null), null);

                case ((TypeCode) 360):
                    return OrInt64(Conversions.ToLong(conv.ToString(null)), Conversions.ToLong(convertible2.ToString(null)), null);
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Or, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Or, new object[] { Left, Right });
        }

        private static object OrSByte(sbyte Left, sbyte Right, Type EnumType = null)
        {
            sbyte num = (sbyte) (Left | Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrUInt16(ushort Left, ushort Right, Type EnumType = null)
        {
            ushort num = (ushort) (Left | Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrUInt32(uint Left, uint Right, Type EnumType = null)
        {
            uint num = Left | Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object OrUInt64(ulong Left, ulong Right, Type EnumType = null)
        {
            ulong num = Left | Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        public static object PlusObject(object Operand)
        {
            TypeCode empty;
            if (Operand == null)
            {
                return Boxed_ZeroInteger;
            }
            IConvertible convertible = Operand as IConvertible;
            if (convertible == null)
            {
                if (Operand == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Object:
                    return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.UnaryPlus, new object[] { Operand });

                case TypeCode.Boolean:
                    return (short) -(convertible.ToBoolean(null) > false);

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
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Operand;

                case TypeCode.String:
                    return Conversions.ToDouble(convertible.ToString(null));
            }
            throw GetNoValidOperatorException(Symbols.UserDefinedOperator.UnaryPlus, Operand);
        }

        internal static Symbols.Method ResolveUserDefinedOperator(Symbols.UserDefinedOperator Op, object[] Arguments, bool ReportErrors)
        {
            bool flag;
            bool flag2;
            Type type;
            Arguments = (object[]) Arguments.Clone();
            Type type2 = null;
            if (Arguments[0] == null)
            {
                type2 = Arguments[1].GetType();
                type = type2;
                Arguments[0] = new Symbols.TypedNothing(type);
            }
            else
            {
                type = Arguments[0].GetType();
                if (Arguments.Length > 1)
                {
                    if (Arguments[1] != null)
                    {
                        type2 = Arguments[1].GetType();
                    }
                    else
                    {
                        type2 = type;
                        Arguments[1] = new Symbols.TypedNothing(type2);
                    }
                }
            }
            List<Symbols.Method> candidates = CollectOperators(Op, type, type2, ref flag, ref flag2);
            if (candidates.Count > 0)
            {
                OverloadResolution.ResolutionFailure failure;
                return OverloadResolution.ResolveOverloadedCall(Symbols.OperatorNames[(int) Op], candidates, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, BindingFlags.InvokeMethod, ReportErrors, ref failure);
            }
            return null;
        }

        public static object RightShiftObject(object Operand, object Amount)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = Operand as IConvertible;
            if (convertible == null)
            {
                if (Operand == null)
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
            IConvertible convertible2 = Amount as IConvertible;
            if (convertible2 == null)
            {
                if (Amount == null)
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
            if ((empty == TypeCode.Object) || (typeCode == TypeCode.Object))
            {
                return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.ShiftRight, new object[] { Operand, Amount });
            }
            switch (empty)
            {
                case TypeCode.Empty:
                    return (((int) 0) >> Conversions.ToInteger(Amount));

                case TypeCode.Boolean:
                    return (short) (((short) -(convertible.ToBoolean(null) > 0)) >> (Conversions.ToInteger(Amount) & 15));

                case TypeCode.SByte:
                    return (sbyte) (convertible.ToSByte(null) >> (Conversions.ToInteger(Amount) & 7));

                case TypeCode.Byte:
                    return (byte) (convertible.ToByte(null) >> (Conversions.ToInteger(Amount) & 7));

                case TypeCode.Int16:
                    return (short) (convertible.ToInt16(null) >> (Conversions.ToInteger(Amount) & 15));

                case TypeCode.UInt16:
                    return (ushort) (convertible.ToUInt16(null) >> (Conversions.ToInteger(Amount) & 15));

                case TypeCode.Int32:
                    return (convertible.ToInt32(null) >> Conversions.ToInteger(Amount));

                case TypeCode.UInt32:
                    return (convertible.ToUInt32(null) >> Conversions.ToInteger(Amount));

                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (convertible.ToInt64(null) >> Conversions.ToInteger(Amount));

                case TypeCode.UInt64:
                    return (convertible.ToUInt64(null) >> Conversions.ToInteger(Amount));

                case TypeCode.String:
                    return (Conversions.ToLong(convertible.ToString(null)) >> Conversions.ToInteger(Amount));
            }
            throw GetNoValidOperatorException(Symbols.UserDefinedOperator.ShiftRight, Operand);
        }

        private static object SubtractByte(byte Left, byte Right)
        {
            short num = (short) (Left - Right);
            if (num < 0)
            {
                return num;
            }
            return (byte) num;
        }

        private static object SubtractDecimal(IConvertible Left, IConvertible Right)
        {
            decimal num = Left.ToDecimal(null);
            decimal num2 = Right.ToDecimal(null);
            try
            {
                return decimal.Subtract(num, num2);
            }
            catch (OverflowException)
            {
                return (Convert.ToDouble(num) - Convert.ToDouble(num2));
            }
        }

        private static object SubtractDouble(double Left, double Right)
        {
            return (Left - Right);
        }

        private static object SubtractInt16(short Left, short Right)
        {
            int num = Left - Right;
            if ((num >= -32768) && (num <= 0x7fff))
            {
                return (short) num;
            }
            return num;
        }

        private static object SubtractInt32(int Left, int Right)
        {
            long num = Left - Right;
            if ((num >= -2147483648L) && (num <= 0x7fffffffL))
            {
                return (int) num;
            }
            return num;
        }

        private static object SubtractInt64(long Left, long Right)
        {
            try
            {
                return (Left - Right);
            }
            catch (OverflowException)
            {
                return decimal.Subtract(new decimal(Left), new decimal(Right));
            }
        }

        public static object SubtractObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                    return SubtractInt16(0, ToVBBool(convertible2));

                case TypeCode.SByte:
                    return SubtractSByte(0, convertible2.ToSByte(null));

                case TypeCode.Byte:
                    return SubtractByte(0, convertible2.ToByte(null));

                case TypeCode.Int16:
                    return SubtractInt16(0, convertible2.ToInt16(null));

                case TypeCode.UInt16:
                    return SubtractUInt16(0, convertible2.ToUInt16(null));

                case TypeCode.Int32:
                    return SubtractInt32(0, convertible2.ToInt32(null));

                case TypeCode.UInt32:
                    return SubtractUInt32(0, convertible2.ToUInt32(null));

                case TypeCode.Int64:
                    return SubtractInt64(0L, convertible2.ToInt64(null));

                case TypeCode.UInt64:
                    return SubtractUInt64(0L, convertible2.ToUInt64(null));

                case TypeCode.Single:
                    return SubtractSingle(0f, convertible2.ToSingle(null));

                case TypeCode.Double:
                    return SubtractDouble(0.0, convertible2.ToDouble(null));

                case TypeCode.Decimal:
                    return SubtractDecimal((IConvertible) decimal.Zero, convertible2);

                case TypeCode.String:
                    return SubtractDouble(0.0, Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x39):
                    return SubtractInt16(ToVBBool(conv), 0);

                case ((TypeCode) 60):
                    return SubtractInt16(ToVBBool(conv), ToVBBool(convertible2));

                case ((TypeCode) 0x3e):
                    return SubtractSByte(ToVBBool(conv), convertible2.ToSByte(null));

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return SubtractInt16(ToVBBool(conv), convertible2.ToInt16(null));

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return SubtractInt32(ToVBBool(conv), convertible2.ToInt32(null));

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                    return SubtractInt64((long) ToVBBool(conv), convertible2.ToInt64(null));

                case ((TypeCode) 0x45):
                case ((TypeCode) 0x48):
                    return SubtractDecimal(ToVBBoolConv(conv), (IConvertible) convertible2.ToDecimal(null));

                case ((TypeCode) 70):
                    return SubtractSingle((float) ToVBBool(conv), convertible2.ToSingle(null));

                case ((TypeCode) 0x47):
                    return SubtractDouble((double) ToVBBool(conv), convertible2.ToDouble(null));

                case ((TypeCode) 0x4b):
                    return SubtractDouble((double) ToVBBool(conv), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                    return conv.ToSByte(null);

                case ((TypeCode) 0x62):
                    return SubtractSByte(conv.ToSByte(null), ToVBBool(convertible2));

                case ((TypeCode) 100):
                    return SubtractSByte(conv.ToSByte(null), convertible2.ToSByte(null));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                case ((TypeCode) 140):
                    return SubtractInt16(conv.ToInt16(null), convertible2.ToInt16(null));

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                case ((TypeCode) 180):
                    return SubtractInt32(conv.ToInt32(null), convertible2.ToInt32(null));

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 220):
                    return SubtractInt64(conv.ToInt64(null), convertible2.ToInt64(null));

                case ((TypeCode) 0x6b):
                case ((TypeCode) 110):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 300):
                    return SubtractDecimal(conv, convertible2);

                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x12a):
                    return SubtractSingle(conv.ToSingle(null), convertible2.ToSingle(null));

                case ((TypeCode) 0x6d):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 0x12b):
                    return SubtractDouble(conv.ToDouble(null), convertible2.ToDouble(null));

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return SubtractDouble(conv.ToDouble(null), Conversions.ToDouble(convertible2.ToString(null)));

                case ((TypeCode) 0x72):
                    return conv.ToByte(null);

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return SubtractInt16(conv.ToInt16(null), ToVBBool(convertible2));

                case ((TypeCode) 120):
                    return SubtractByte(conv.ToByte(null), convertible2.ToByte(null));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                case ((TypeCode) 160):
                    return SubtractUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null));

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                case ((TypeCode) 200):
                    return SubtractUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null));

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                case ((TypeCode) 240):
                    return SubtractUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null));

                case ((TypeCode) 0x85):
                    return conv.ToInt16(null);

                case ((TypeCode) 0x98):
                    return conv.ToUInt16(null);

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return SubtractInt32(conv.ToInt32(null), ToVBBool(convertible2));

                case ((TypeCode) 0xab):
                    return conv.ToInt32(null);

                case ((TypeCode) 190):
                    return conv.ToUInt32(null);

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                    return SubtractInt64(conv.ToInt64(null), (long) ToVBBool(convertible2));

                case ((TypeCode) 0xd1):
                    return conv.ToInt64(null);

                case ((TypeCode) 0xe4):
                    return conv.ToUInt64(null);

                case ((TypeCode) 0xe7):
                case ((TypeCode) 0x120):
                    return SubtractDecimal(conv, ToVBBoolConv(convertible2));

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return Left;

                case ((TypeCode) 250):
                    return SubtractSingle(conv.ToSingle(null), (float) ToVBBool(convertible2));

                case ((TypeCode) 0x10d):
                    return SubtractDouble(conv.ToDouble(null), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x156):
                    return Conversions.ToDouble(conv.ToString(null));

                case ((TypeCode) 0x159):
                    return SubtractDouble(Conversions.ToDouble(conv.ToString(null)), (double) ToVBBool(convertible2));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return SubtractDouble(Conversions.ToDouble(conv.ToString(null)), convertible2.ToDouble(null));

                case ((TypeCode) 360):
                    return SubtractDouble(Conversions.ToDouble(conv.ToString(null)), Conversions.ToDouble(convertible2.ToString(null)));
            }
            if (((((empty != TypeCode.Object) && (typeCode != TypeCode.Object)) && ((empty != TypeCode.DateTime) || (typeCode != TypeCode.DateTime))) && ((empty != TypeCode.DateTime) || (typeCode != TypeCode.Empty))) && ((empty != TypeCode.Empty) || (typeCode != TypeCode.DateTime)))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Minus, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Minus, new object[] { Left, Right });
        }

        private static object SubtractSByte(sbyte Left, sbyte Right)
        {
            short num = (short) (Left - Right);
            if ((num >= -128) && (num <= 0x7f))
            {
                return (sbyte) num;
            }
            return num;
        }

        private static object SubtractSingle(float Left, float Right)
        {
            double d = Left - Right;
            if (((d > 3.4028234663852886E+38) || (d < -3.4028234663852886E+38)) && (!double.IsInfinity(d) || (!float.IsInfinity(Left) && !float.IsInfinity(Right))))
            {
                return d;
            }
            return (float) d;
        }

        private static object SubtractUInt16(ushort Left, ushort Right)
        {
            int num = Left - Right;
            if (num < 0)
            {
                return num;
            }
            return (ushort) num;
        }

        private static object SubtractUInt32(uint Left, uint Right)
        {
            long num = Left - Right;
            if (num < 0L)
            {
                return num;
            }
            return (uint) num;
        }

        private static object SubtractUInt64(ulong Left, ulong Right)
        {
            try
            {
                return (Left - Right);
            }
            catch (OverflowException)
            {
                return decimal.Subtract(new decimal(Left), new decimal(Right));
            }
        }

        private static sbyte ToVBBool(IConvertible conv)
        {
            return (sbyte) -(conv.ToBoolean(null) > false);
        }

        private static IConvertible ToVBBoolConv(IConvertible conv)
        {
            return (IConvertible) ((sbyte) -(conv.ToBoolean(null) > false));
        }

        private static object XorBoolean(bool Left, bool Right)
        {
            return (Left ^ Right);
        }

        private static object XorByte(byte Left, byte Right, Type EnumType = null)
        {
            byte num = (byte) (Left ^ Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorInt16(short Left, short Right, Type EnumType = null)
        {
            short num = (short) (Left ^ Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorInt32(int Left, int Right, Type EnumType = null)
        {
            int num = Left ^ Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorInt64(long Left, long Right, Type EnumType = null)
        {
            long num = Left ^ Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        public static object XorObject(object Left, object Right)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible conv = Left as IConvertible;
            if (conv == null)
            {
                if (Left == null)
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
            IConvertible convertible2 = Right as IConvertible;
            if (convertible2 == null)
            {
                if (Right == null)
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
                    return Boxed_ZeroInteger;

                case TypeCode.Boolean:
                    return XorBoolean(false, convertible2.ToBoolean(null));

                case TypeCode.SByte:
                    return XorSByte(0, convertible2.ToSByte(null), GetEnumResult(Left, Right));

                case TypeCode.Byte:
                    return XorByte(0, convertible2.ToByte(null), GetEnumResult(Left, Right));

                case TypeCode.Int16:
                    return XorInt16(0, convertible2.ToInt16(null), GetEnumResult(Left, Right));

                case TypeCode.UInt16:
                    return XorUInt16(0, convertible2.ToUInt16(null), GetEnumResult(Left, Right));

                case TypeCode.Int32:
                    return XorInt32(0, convertible2.ToInt32(null), GetEnumResult(Left, Right));

                case TypeCode.UInt32:
                    return XorUInt32(0, convertible2.ToUInt32(null), GetEnumResult(Left, Right));

                case TypeCode.Int64:
                    return XorInt64(0L, convertible2.ToInt64(null), GetEnumResult(Left, Right));

                case TypeCode.UInt64:
                    return XorUInt64(0L, convertible2.ToUInt64(null), GetEnumResult(Left, Right));

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return XorInt64(0L, convertible2.ToInt64(null), null);

                case TypeCode.String:
                    return XorInt64(0L, Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 0x39):
                    return XorBoolean(conv.ToBoolean(null), false);

                case ((TypeCode) 60):
                    return XorBoolean(conv.ToBoolean(null), convertible2.ToBoolean(null));

                case ((TypeCode) 0x3e):
                    return XorSByte(ToVBBool(conv), convertible2.ToSByte(null), null);

                case ((TypeCode) 0x3f):
                case ((TypeCode) 0x40):
                    return XorInt16(ToVBBool(conv), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x41):
                case ((TypeCode) 0x42):
                    return XorInt32(ToVBBool(conv), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x43):
                case ((TypeCode) 0x44):
                case ((TypeCode) 0x45):
                case ((TypeCode) 70):
                case ((TypeCode) 0x47):
                case ((TypeCode) 0x48):
                    return XorInt64((long) ToVBBool(conv), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x4b):
                    return XorBoolean(conv.ToBoolean(null), Conversions.ToBoolean(convertible2.ToString(null)));

                case ((TypeCode) 0x5f):
                    return XorSByte(conv.ToSByte(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 0x62):
                    return XorSByte(conv.ToSByte(null), ToVBBool(convertible2), null);

                case ((TypeCode) 100):
                    return XorSByte(conv.ToSByte(null), convertible2.ToSByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x65):
                case ((TypeCode) 0x66):
                case ((TypeCode) 0x77):
                case ((TypeCode) 0x79):
                case ((TypeCode) 0x8a):
                case ((TypeCode) 0x8b):
                    return XorInt16(conv.ToInt16(null), convertible2.ToInt16(null), null);

                case ((TypeCode) 0x67):
                case ((TypeCode) 0x68):
                case ((TypeCode) 0x7b):
                case ((TypeCode) 0x8d):
                case ((TypeCode) 0x8e):
                case ((TypeCode) 0x9d):
                case ((TypeCode) 0x9f):
                case ((TypeCode) 0xa1):
                case ((TypeCode) 0xb0):
                case ((TypeCode) 0xb1):
                case ((TypeCode) 0xb2):
                case ((TypeCode) 0xb3):
                    return XorInt32(conv.ToInt32(null), convertible2.ToInt32(null), null);

                case ((TypeCode) 0x69):
                case ((TypeCode) 0x6a):
                case ((TypeCode) 0x6b):
                case ((TypeCode) 0x6c):
                case ((TypeCode) 0x6d):
                case ((TypeCode) 110):
                case ((TypeCode) 0x7d):
                case ((TypeCode) 0x7f):
                case ((TypeCode) 0x80):
                case ((TypeCode) 0x81):
                case ((TypeCode) 0x8f):
                case ((TypeCode) 0x90):
                case ((TypeCode) 0x91):
                case ((TypeCode) 0x92):
                case ((TypeCode) 0x93):
                case ((TypeCode) 0x94):
                case ((TypeCode) 0xa3):
                case ((TypeCode) 0xa5):
                case ((TypeCode) 0xa6):
                case ((TypeCode) 0xa7):
                case ((TypeCode) 0xb5):
                case ((TypeCode) 0xb6):
                case ((TypeCode) 0xb7):
                case ((TypeCode) 0xb8):
                case ((TypeCode) 0xb9):
                case ((TypeCode) 0xba):
                case ((TypeCode) 0xc3):
                case ((TypeCode) 0xc5):
                case ((TypeCode) 0xc7):
                case ((TypeCode) 0xc9):
                case ((TypeCode) 0xcb):
                case ((TypeCode) 0xcc):
                case ((TypeCode) 0xcd):
                case ((TypeCode) 0xd6):
                case ((TypeCode) 0xd7):
                case ((TypeCode) 0xd8):
                case ((TypeCode) 0xd9):
                case ((TypeCode) 0xda):
                case ((TypeCode) 0xdb):
                case ((TypeCode) 0xdd):
                case ((TypeCode) 0xde):
                case ((TypeCode) 0xdf):
                case ((TypeCode) 0xe0):
                case ((TypeCode) 0xe9):
                case ((TypeCode) 0xeb):
                case ((TypeCode) 0xed):
                case ((TypeCode) 0xef):
                case ((TypeCode) 0xf1):
                case ((TypeCode) 0xf2):
                case ((TypeCode) 0xf3):
                case ((TypeCode) 0xfc):
                case ((TypeCode) 0xfd):
                case ((TypeCode) 0xfe):
                case ((TypeCode) 0xff):
                case ((TypeCode) 0x100):
                case ((TypeCode) 0x101):
                case ((TypeCode) 0x102):
                case ((TypeCode) 0x103):
                case ((TypeCode) 260):
                case ((TypeCode) 0x105):
                case ((TypeCode) 0x106):
                case ((TypeCode) 0x10f):
                case ((TypeCode) 0x110):
                case ((TypeCode) 0x111):
                case ((TypeCode) 0x112):
                case ((TypeCode) 0x113):
                case ((TypeCode) 0x114):
                case ((TypeCode) 0x115):
                case ((TypeCode) 0x116):
                case ((TypeCode) 0x117):
                case ((TypeCode) 280):
                case ((TypeCode) 0x119):
                case ((TypeCode) 290):
                case ((TypeCode) 0x123):
                case ((TypeCode) 0x124):
                case ((TypeCode) 0x125):
                case ((TypeCode) 0x126):
                case ((TypeCode) 0x127):
                case ((TypeCode) 0x128):
                case ((TypeCode) 0x129):
                case ((TypeCode) 0x12a):
                case ((TypeCode) 0x12b):
                case ((TypeCode) 300):
                    return XorInt64(conv.ToInt64(null), convertible2.ToInt64(null), null);

                case ((TypeCode) 0x71):
                case ((TypeCode) 0x84):
                case ((TypeCode) 0x97):
                case ((TypeCode) 170):
                case ((TypeCode) 0xbd):
                case ((TypeCode) 0xd0):
                case ((TypeCode) 0xe3):
                case ((TypeCode) 0xf6):
                case ((TypeCode) 0x109):
                case ((TypeCode) 0x11c):
                case ((TypeCode) 0x12f):
                    return XorInt64(conv.ToInt64(null), Conversions.ToLong(convertible2.ToString(null)), null);

                case ((TypeCode) 0x72):
                    return XorByte(conv.ToByte(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 0x75):
                case ((TypeCode) 0x88):
                    return XorInt16(conv.ToInt16(null), ToVBBool(convertible2), null);

                case ((TypeCode) 120):
                    return XorByte(conv.ToByte(null), convertible2.ToByte(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x7a):
                case ((TypeCode) 0x9e):
                    return XorUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), null);

                case ((TypeCode) 0x7c):
                case ((TypeCode) 0xa2):
                case ((TypeCode) 0xc4):
                case ((TypeCode) 0xc6):
                    return XorUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), null);

                case ((TypeCode) 0x7e):
                case ((TypeCode) 0xa4):
                case ((TypeCode) 0xca):
                case ((TypeCode) 0xea):
                case ((TypeCode) 0xec):
                case ((TypeCode) 0xee):
                    return XorUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), null);

                case ((TypeCode) 0x85):
                    return XorInt16(conv.ToInt16(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 140):
                    return XorInt16(conv.ToInt16(null), convertible2.ToInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0x98):
                    return XorUInt16(conv.ToUInt16(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 0x9b):
                case ((TypeCode) 0xae):
                    return XorInt32(conv.ToInt32(null), ToVBBool(convertible2), null);

                case ((TypeCode) 160):
                    return XorUInt16(conv.ToUInt16(null), convertible2.ToUInt16(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xab):
                    return XorInt32(conv.ToInt32(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 180):
                    return XorInt32(conv.ToInt32(null), convertible2.ToInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 190):
                    return XorUInt32(conv.ToUInt32(null), 0, GetEnumResult(Left, Right));

                case ((TypeCode) 0xc1):
                case ((TypeCode) 0xd4):
                case ((TypeCode) 0xe7):
                case ((TypeCode) 250):
                case ((TypeCode) 0x10d):
                case ((TypeCode) 0x120):
                    return XorInt64(conv.ToInt64(null), (long) ToVBBool(convertible2), null);

                case ((TypeCode) 200):
                    return XorUInt32(conv.ToUInt32(null), convertible2.ToUInt32(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xd1):
                    return XorInt64(conv.ToInt64(null), 0L, GetEnumResult(Left, Right));

                case ((TypeCode) 220):
                    return XorInt64(conv.ToInt64(null), convertible2.ToInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xe4):
                    return XorUInt64(conv.ToUInt64(null), 0L, GetEnumResult(Left, Right));

                case ((TypeCode) 240):
                    return XorUInt64(conv.ToUInt64(null), convertible2.ToUInt64(null), GetEnumResult(Left, Right));

                case ((TypeCode) 0xf7):
                case ((TypeCode) 0x10a):
                case ((TypeCode) 0x11d):
                    return XorInt64(conv.ToInt64(null), 0L, null);

                case ((TypeCode) 0x156):
                    return XorInt64(Conversions.ToLong(conv.ToString(null)), 0L, null);

                case ((TypeCode) 0x159):
                    return XorBoolean(Conversions.ToBoolean(conv.ToString(null)), convertible2.ToBoolean(null));

                case ((TypeCode) 0x15b):
                case ((TypeCode) 0x15c):
                case ((TypeCode) 0x15d):
                case ((TypeCode) 350):
                case ((TypeCode) 0x15f):
                case ((TypeCode) 0x160):
                case ((TypeCode) 0x161):
                case ((TypeCode) 0x162):
                case ((TypeCode) 0x163):
                case ((TypeCode) 0x164):
                case ((TypeCode) 0x165):
                    return XorInt64(Conversions.ToLong(conv.ToString(null)), convertible2.ToInt64(null), null);

                case ((TypeCode) 360):
                    return XorInt64(Conversions.ToLong(conv.ToString(null)), Conversions.ToLong(convertible2.ToString(null)), null);
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                throw GetNoValidOperatorException(Symbols.UserDefinedOperator.Xor, Left, Right);
            }
            return InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Xor, new object[] { Left, Right });
        }

        private static object XorSByte(sbyte Left, sbyte Right, Type EnumType = null)
        {
            sbyte num = (sbyte) (Left ^ Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorUInt16(ushort Left, ushort Right, Type EnumType = null)
        {
            ushort num = (ushort) (Left ^ Right);
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorUInt32(uint Left, uint Right, Type EnumType = null)
        {
            uint num = Left ^ Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private static object XorUInt64(ulong Left, ulong Right, Type EnumType = null)
        {
            ulong num = Left ^ Right;
            if (EnumType != null)
            {
                return Enum.ToObject(EnumType, num);
            }
            return num;
        }

        private enum CompareClass
        {
            Equal = 0,
            Greater = 1,
            Less = -1,
            Undefined = 4,
            Unordered = 2,
            UserDefined = 3
        }
    }
}

