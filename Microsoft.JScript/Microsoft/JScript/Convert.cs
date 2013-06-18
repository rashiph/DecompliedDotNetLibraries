namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public sealed class Convert
    {
        private static bool[,] promotable = new bool[,] { 
            { 
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
                true, true, true
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false, false, false
             }, { 
                true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
                true, true, true
             }, { 
                false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, true, false, false, false, true, true, true, true, true, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, false, true, false, true, false, true, false, true, false, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, true, false, true, true, true, true, true, true, true, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, false, true, false, true, false, true, false, true, false, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, true, false, false, false, true, true, true, true, true, true, true, true, 
                true, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, true, false, true, false, false, true, true, 
                true, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, true, true, true, false, true, true, 
                true, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, true, 
                true, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, true, 
                true, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, 
                false, false, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, 
                false, false, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, 
                false, false, false
             }, 
            { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                true, false, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false, true, false
             }, { 
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
                false, false, true
             }
         };
        private static int[] rgcchSig = new int[] { 
            0x35, 0x22, 0x1b, 0x18, 0x16, 20, 0x13, 0x12, 0x11, 0x11, 0x10, 0x10, 15, 15, 14, 14, 
            14, 14, 14, 13, 13, 13, 13, 13, 13, 12, 12, 12, 12, 12, 12, 12, 
            12, 12, 12
         };

        public static double CheckIfDoubleIsInteger(double d)
        {
            if (d != Math.Round(d))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            return d;
        }

        public static float CheckIfSingleIsInteger(float s)
        {
            if (s != Math.Round((double) s))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            return s;
        }

        public static object Coerce(object value, object type)
        {
            return Coerce(value, type, false);
        }

        internal static object Coerce(object value, object type, bool explicitOK)
        {
            TypeExpression expression = type as TypeExpression;
            if (expression != null)
            {
                type = expression.ToIReflect();
            }
            TypedArray array = type as TypedArray;
            if (array != null)
            {
                IReflect elementType = array.elementType;
                int rank = array.rank;
                Type type2 = (elementType is Type) ? ((Type) elementType) : ((elementType is ClassScope) ? ((ClassScope) elementType).GetBakedSuperType() : typeof(object));
                ArrayObject obj2 = value as ArrayObject;
                if (obj2 != null)
                {
                    return obj2.ToNativeArray(type2);
                }
                Array array2 = value as Array;
                if ((array2 != null) && (array2.Rank == rank))
                {
                    type = ToType(TypedArray.ToRankString(rank), type2);
                }
                if ((value == null) || (value is DBNull))
                {
                    return null;
                }
            }
            ClassScope classScope = type as ClassScope;
            if (classScope != null)
            {
                if (classScope.HasInstance(value))
                {
                    return value;
                }
                EnumDeclaration owner = classScope.owner as EnumDeclaration;
                if (owner != null)
                {
                    EnumWrapper wrapper = value as EnumWrapper;
                    if (wrapper == null)
                    {
                        return new DeclaredEnumValue(Coerce(value, owner.baseType), null, classScope);
                    }
                    if (wrapper.classScopeOrType != classScope)
                    {
                        throw new JScriptException(JSError.TypeMismatch);
                    }
                    return value;
                }
                if ((value != null) && !(value is DBNull))
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
                return null;
            }
            if (type is Type)
            {
                if ((type == typeof(Type)) && (value is ClassScope))
                {
                    return value;
                }
                if (((Type) type).IsEnum)
                {
                    EnumWrapper wrapper2 = value as EnumWrapper;
                    if (wrapper2 != null)
                    {
                        if (wrapper2.classScopeOrType != type)
                        {
                            throw new JScriptException(JSError.TypeMismatch);
                        }
                        return value;
                    }
                    Type type3 = type as Type;
                    return MetadataEnumValue.GetEnumValue(type3, CoerceT(value, GetUnderlyingType(type3), explicitOK));
                }
            }
            else
            {
                type = ToType(Runtime.TypeRefs, (IReflect) type);
            }
            return CoerceT(value, (Type) type, explicitOK);
        }

        public static object Coerce2(object value, TypeCode target, bool truncationPermitted)
        {
            if (truncationPermitted)
            {
                return Coerce2WithTruncationPermitted(value, target);
            }
            return Coerce2WithNoTrunctation(value, target);
        }

        private static object Coerce2WithNoTrunctation(object value, TypeCode target)
        {
            if (value is EnumWrapper)
            {
                value = ((EnumWrapper) value).value;
            }
            if (value is ConstantWrapper)
            {
                value = ((ConstantWrapper) value).value;
            }
            try
            {
                ushort num2;
                float num10;
                double num11;
                string str;
                IConvertible iConvertible = GetIConvertible(value);
                switch (GetTypeCode(value, iConvertible))
                {
                    case TypeCode.Empty:
                        break;

                    case TypeCode.Object:
                        if ((value is System.Reflection.Missing) || ((value is Microsoft.JScript.Missing) && (target != TypeCode.Object)))
                        {
                            break;
                        }
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return ToBoolean(value, false);

                            case TypeCode.Char:
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
                                return Coerce2WithNoTrunctation(ToNumber(value, iConvertible), target);

                            case TypeCode.DateTime:
                                goto Label_0261;

                            case TypeCode.String:
                                return ToString(value, iConvertible);
                        }
                        goto Label_1731;

                    case TypeCode.DBNull:
                        switch (target)
                        {
                            case TypeCode.DBNull:
                                return DBNull.Value;

                            case TypeCode.Boolean:
                                return false;

                            case TypeCode.Char:
                                return '\0';

                            case TypeCode.SByte:
                                return (sbyte) 0;

                            case TypeCode.Byte:
                                return (byte) 0;

                            case TypeCode.Int16:
                                return (short) 0;

                            case TypeCode.UInt16:
                                return (ushort) 0;

                            case TypeCode.Int32:
                                return 0;

                            case TypeCode.UInt32:
                                return 0;

                            case TypeCode.Int64:
                                return 0L;

                            case TypeCode.UInt64:
                                return (ulong) 0L;

                            case TypeCode.Single:
                                return 0f;

                            case TypeCode.Double:
                                return 0.0;

                            case TypeCode.Decimal:
                                return 0M;

                            case TypeCode.DateTime:
                                return new DateTime(0L);

                            case TypeCode.String:
                                return null;
                        }
                        goto Label_1731;

                    case TypeCode.Boolean:
                    {
                        bool flag = iConvertible.ToBoolean(null);
                        int num = flag ? 1 : 0;
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return flag;

                            case TypeCode.Char:
                                return (char) num;

                            case TypeCode.SByte:
                                return (sbyte) num;

                            case TypeCode.Byte:
                                return (byte) num;

                            case TypeCode.Int16:
                                return (short) num;

                            case TypeCode.UInt16:
                                return (ushort) num;

                            case TypeCode.Int32:
                                return num;

                            case TypeCode.UInt32:
                                return (uint) num;

                            case TypeCode.Int64:
                                return (long) num;

                            case TypeCode.UInt64:
                                return (ulong) num;

                            case TypeCode.Single:
                                return (float) num;

                            case TypeCode.Double:
                                return (double) num;

                            case TypeCode.Decimal:
                                return (decimal) num;

                            case TypeCode.DateTime:
                                return new DateTime((long) num);

                            case TypeCode.String:
                                return (flag ? "true" : "false");
                        }
                        goto Label_1731;
                    }
                    case TypeCode.Char:
                    {
                        char c = iConvertible.ToChar(null);
                        num2 = c;
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num2 != 0);

                            case TypeCode.Char:
                                return c;

                            case TypeCode.SByte:
                                return (sbyte) num2;

                            case TypeCode.Byte:
                                return (byte) num2;

                            case TypeCode.Int16:
                                return (short) num2;

                            case TypeCode.UInt16:
                                return num2;

                            case TypeCode.Int32:
                                return (int) num2;

                            case TypeCode.UInt32:
                                return (uint) num2;

                            case TypeCode.Int64:
                                return (long) num2;

                            case TypeCode.UInt64:
                                return (ulong) num2;

                            case TypeCode.Single:
                                return (float) num2;

                            case TypeCode.Double:
                                return (double) num2;

                            case TypeCode.Decimal:
                                return (decimal) num2;

                            case TypeCode.DateTime:
                                return new DateTime((long) num2);

                            case TypeCode.String:
                                return char.ToString(c);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.SByte:
                    {
                        sbyte num3 = iConvertible.ToSByte(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num3 != 0);

                            case TypeCode.Char:
                                return (char) ((ushort) num3);

                            case TypeCode.SByte:
                                return num3;

                            case TypeCode.Byte:
                                return (byte) num3;

                            case TypeCode.Int16:
                                return (short) num3;

                            case TypeCode.UInt16:
                                return (ushort) num3;

                            case TypeCode.Int32:
                                return (int) num3;

                            case TypeCode.UInt32:
                                return (uint) num3;

                            case TypeCode.Int64:
                                return (long) num3;

                            case TypeCode.UInt64:
                                return (ulong) num3;

                            case TypeCode.Single:
                                return (float) num3;

                            case TypeCode.Double:
                                return (double) num3;

                            case TypeCode.Decimal:
                                return (decimal) num3;

                            case TypeCode.DateTime:
                                return new DateTime((long) num3);

                            case TypeCode.String:
                                return num3.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.Byte:
                    {
                        byte num4 = iConvertible.ToByte(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num4 != 0);

                            case TypeCode.Char:
                                return (char) num4;

                            case TypeCode.SByte:
                                return (sbyte) num4;

                            case TypeCode.Byte:
                                return num4;

                            case TypeCode.Int16:
                                return (short) num4;

                            case TypeCode.UInt16:
                                return (ushort) num4;

                            case TypeCode.Int32:
                                return (int) num4;

                            case TypeCode.UInt32:
                                return (uint) num4;

                            case TypeCode.Int64:
                                return (long) num4;

                            case TypeCode.UInt64:
                                return (ulong) num4;

                            case TypeCode.Single:
                                return (float) num4;

                            case TypeCode.Double:
                                return (double) num4;

                            case TypeCode.Decimal:
                                return (decimal) num4;

                            case TypeCode.DateTime:
                                return new DateTime((long) num4);

                            case TypeCode.String:
                                return num4.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.Int16:
                    {
                        short num5 = iConvertible.ToInt16(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num5 != 0);

                            case TypeCode.Char:
                                return (char) ((ushort) num5);

                            case TypeCode.SByte:
                                return (sbyte) num5;

                            case TypeCode.Byte:
                                return (byte) num5;

                            case TypeCode.Int16:
                                return num5;

                            case TypeCode.UInt16:
                                return (ushort) num5;

                            case TypeCode.Int32:
                                return (int) num5;

                            case TypeCode.UInt32:
                                return (uint) num5;

                            case TypeCode.Int64:
                                return (long) num5;

                            case TypeCode.UInt64:
                                return (ulong) num5;

                            case TypeCode.Single:
                                return (float) num5;

                            case TypeCode.Double:
                                return (double) num5;

                            case TypeCode.Decimal:
                                return (decimal) num5;

                            case TypeCode.DateTime:
                                return new DateTime((long) num5);

                            case TypeCode.String:
                                return num5.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.UInt16:
                        num2 = iConvertible.ToUInt16(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num2 != 0);

                            case TypeCode.Char:
                                return (char) num2;

                            case TypeCode.SByte:
                                return (sbyte) num2;

                            case TypeCode.Byte:
                                return (byte) num2;

                            case TypeCode.Int16:
                                return (short) num2;

                            case TypeCode.UInt16:
                                return num2;

                            case TypeCode.Int32:
                                return (int) num2;

                            case TypeCode.UInt32:
                                return (uint) num2;

                            case TypeCode.Int64:
                                return (long) num2;

                            case TypeCode.UInt64:
                                return (ulong) num2;

                            case TypeCode.Single:
                                return (float) num2;

                            case TypeCode.Double:
                                return (double) num2;

                            case TypeCode.Decimal:
                                return (decimal) num2;

                            case TypeCode.DateTime:
                                return new DateTime((long) num2);

                            case TypeCode.String:
                                return num2.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;

                    case TypeCode.Int32:
                    {
                        int num6 = iConvertible.ToInt32(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num6 != 0);

                            case TypeCode.Char:
                                return (char) num6;

                            case TypeCode.SByte:
                                return (sbyte) num6;

                            case TypeCode.Byte:
                                return (byte) num6;

                            case TypeCode.Int16:
                                return (short) num6;

                            case TypeCode.UInt16:
                                return (ushort) num6;

                            case TypeCode.Int32:
                                return num6;

                            case TypeCode.UInt32:
                                return (uint) num6;

                            case TypeCode.Int64:
                                return (long) num6;

                            case TypeCode.UInt64:
                                return (ulong) num6;

                            case TypeCode.Single:
                                return (float) num6;

                            case TypeCode.Double:
                                return (double) num6;

                            case TypeCode.Decimal:
                                return (decimal) num6;

                            case TypeCode.DateTime:
                                return new DateTime((long) num6);

                            case TypeCode.String:
                                return num6.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.UInt32:
                    {
                        uint num7 = iConvertible.ToUInt32(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num7 != 0);

                            case TypeCode.Char:
                                return (char) num7;

                            case TypeCode.SByte:
                                return (sbyte) num7;

                            case TypeCode.Byte:
                                return (byte) num7;

                            case TypeCode.Int16:
                                return (short) num7;

                            case TypeCode.UInt16:
                                return (ushort) num7;

                            case TypeCode.Int32:
                                return (int) num7;

                            case TypeCode.UInt32:
                                return num7;

                            case TypeCode.Int64:
                                return (long) num7;

                            case TypeCode.UInt64:
                                return (ulong) num7;

                            case TypeCode.Single:
                                return (float) num7;

                            case TypeCode.Double:
                                return (double) num7;

                            case TypeCode.Decimal:
                                return (decimal) num7;

                            case TypeCode.DateTime:
                                return new DateTime((long) num7);

                            case TypeCode.String:
                                return num7.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.Int64:
                    {
                        long ticks = iConvertible.ToInt64(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (ticks != 0L);

                            case TypeCode.Char:
                                return (char) ((ushort) ticks);

                            case TypeCode.SByte:
                                return (sbyte) ticks;

                            case TypeCode.Byte:
                                return (byte) ticks;

                            case TypeCode.Int16:
                                return (short) ticks;

                            case TypeCode.UInt16:
                                return (ushort) ticks;

                            case TypeCode.Int32:
                                return (int) ticks;

                            case TypeCode.UInt32:
                                return (uint) ticks;

                            case TypeCode.Int64:
                                return ticks;

                            case TypeCode.UInt64:
                                return (ulong) ticks;

                            case TypeCode.Single:
                                return (float) ticks;

                            case TypeCode.Double:
                                return (double) ticks;

                            case TypeCode.Decimal:
                                return (decimal) ticks;

                            case TypeCode.DateTime:
                                return new DateTime(ticks);

                            case TypeCode.String:
                                return ticks.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.UInt64:
                    {
                        ulong num9 = iConvertible.ToUInt64(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num9 != 0L);

                            case TypeCode.Char:
                                return (char) num9;

                            case TypeCode.SByte:
                                return (sbyte) num9;

                            case TypeCode.Byte:
                                return (byte) num9;

                            case TypeCode.Int16:
                                return (short) num9;

                            case TypeCode.UInt16:
                                return (ushort) num9;

                            case TypeCode.Int32:
                                return (int) num9;

                            case TypeCode.UInt32:
                                return (uint) num9;

                            case TypeCode.Int64:
                                return (long) num9;

                            case TypeCode.UInt64:
                                return num9;

                            case TypeCode.Single:
                                return (float) num9;

                            case TypeCode.Double:
                                return (double) num9;

                            case TypeCode.Decimal:
                                return (decimal) num9;

                            case TypeCode.DateTime:
                                return new DateTime((long) num9);

                            case TypeCode.String:
                                return num9.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.Single:
                        num10 = iConvertible.ToSingle(null);
                        switch (target)
                        {
                            case TypeCode.Single:
                                return num10;

                            case TypeCode.Double:
                                return (double) num10;

                            case TypeCode.Decimal:
                                return (decimal) num10;

                            case TypeCode.String:
                                return ToString((double) num10);

                            case TypeCode.Boolean:
                                goto Label_10EF;
                        }
                        goto Label_115A;

                    case TypeCode.Double:
                        num11 = iConvertible.ToDouble(null);
                        switch (target)
                        {
                            case TypeCode.Single:
                                return (float) num11;

                            case TypeCode.Double:
                                return num11;

                            case TypeCode.Decimal:
                                return (decimal) num11;

                            case TypeCode.String:
                                return ToString(num11);

                            case TypeCode.Boolean:
                                return ToBoolean(num11);
                        }
                        goto Label_12D1;

                    case TypeCode.Decimal:
                    {
                        decimal num12 = iConvertible.ToDecimal(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return (num12 != 0M);

                            case TypeCode.Char:
                                return (char) decimal.ToUInt16(num12);

                            case TypeCode.SByte:
                                return decimal.ToSByte(num12);

                            case TypeCode.Byte:
                                return decimal.ToByte(num12);

                            case TypeCode.Int16:
                                return decimal.ToInt16(num12);

                            case TypeCode.UInt16:
                                return decimal.ToUInt16(num12);

                            case TypeCode.Int32:
                                return decimal.ToInt32(num12);

                            case TypeCode.UInt32:
                                return decimal.ToUInt32(num12);

                            case TypeCode.Int64:
                                return decimal.ToInt64(num12);

                            case TypeCode.UInt64:
                                return decimal.ToUInt64(num12);

                            case TypeCode.Single:
                                return decimal.ToSingle(num12);

                            case TypeCode.Double:
                                return decimal.ToDouble(num12);

                            case TypeCode.Decimal:
                                return num12;

                            case TypeCode.DateTime:
                                return new DateTime(decimal.ToInt64(num12));

                            case TypeCode.String:
                                return num12.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.DateTime:
                    {
                        DateTime time = iConvertible.ToDateTime(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                            case TypeCode.Char:
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
                                return Coerce2WithNoTrunctation(time.Ticks, target);

                            case TypeCode.DateTime:
                                return time;

                            case TypeCode.String:
                                return time.ToString(CultureInfo.InvariantCulture);
                        }
                        goto Label_1731;
                    }
                    case TypeCode.String:
                        str = iConvertible.ToString(null);
                        switch (target)
                        {
                            case TypeCode.Boolean:
                                return ToBoolean(str, false);

                            case TypeCode.Char:
                                goto Label_163E;

                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Double:
                                goto Label_1664;

                            case TypeCode.Int64:
                                goto Label_1698;

                            case TypeCode.UInt64:
                                goto Label_16B3;

                            case TypeCode.Single:
                                goto Label_167D;

                            case TypeCode.Decimal:
                                goto Label_16CB;

                            case TypeCode.DateTime:
                                goto Label_16E3;

                            case TypeCode.String:
                                return str;
                        }
                        goto Label_1731;

                    default:
                        goto Label_1731;
                }
                switch (target)
                {
                    case TypeCode.DBNull:
                        return DBNull.Value;

                    case TypeCode.Boolean:
                        return false;

                    case TypeCode.Char:
                        return '\0';

                    case TypeCode.SByte:
                        return (sbyte) 0;

                    case TypeCode.Byte:
                        return (byte) 0;

                    case TypeCode.Int16:
                        return (short) 0;

                    case TypeCode.UInt16:
                        return (ushort) 0;

                    case TypeCode.Int32:
                        return 0;

                    case TypeCode.UInt32:
                        return 0;

                    case TypeCode.Int64:
                        return 0L;

                    case TypeCode.UInt64:
                        return (ulong) 0L;

                    case TypeCode.Single:
                        return (float) 1.0 / (float) 0.0;

                    case TypeCode.Double:
                        return (double) 1.0 / (double) 0.0;

                    case TypeCode.Decimal:
                        return 0M;

                    case TypeCode.DateTime:
                        return new DateTime(0L);

                    case TypeCode.String:
                        return null;

                    default:
                        goto Label_1731;
                }
            Label_0261:
                if (value is DateObject)
                {
                    return DatePrototype.getVarDate((DateObject) value);
                }
                return Coerce2WithNoTrunctation(ToNumber(value, iConvertible), target);
            Label_10EF:
                if (num10 != num10)
                {
                    return false;
                }
                return !(num10 == 0f);
            Label_115A:
                if (Math.Round((double) num10) == num10)
                {
                    switch (target)
                    {
                        case TypeCode.Char:
                            return (char) ((ushort) num10);

                        case TypeCode.SByte:
                            return (sbyte) num10;

                        case TypeCode.Byte:
                            return (byte) num10;

                        case TypeCode.Int16:
                            return (short) num10;

                        case TypeCode.UInt16:
                            return (ushort) num10;

                        case TypeCode.Int32:
                            return (int) num10;

                        case TypeCode.UInt32:
                            return (uint) num10;

                        case TypeCode.Int64:
                            return (long) num10;

                        case TypeCode.UInt64:
                            return (ulong) num10;

                        case TypeCode.DateTime:
                            return new DateTime((long) num10);
                    }
                }
                goto Label_1731;
            Label_12D1:
                if (Math.Round(num11) == num11)
                {
                    switch (target)
                    {
                        case TypeCode.Char:
                            return (char) ((ushort) num11);

                        case TypeCode.SByte:
                            return (sbyte) num11;

                        case TypeCode.Byte:
                            return (byte) num11;

                        case TypeCode.Int16:
                            return (short) num11;

                        case TypeCode.UInt16:
                            return (ushort) num11;

                        case TypeCode.Int32:
                            return (int) num11;

                        case TypeCode.UInt32:
                            return (uint) num11;

                        case TypeCode.Int64:
                            return (long) num11;

                        case TypeCode.UInt64:
                            return (ulong) num11;

                        case TypeCode.DateTime:
                            return new DateTime((long) num11);
                    }
                }
                goto Label_1731;
            Label_163E:
                if (str.Length == 1)
                {
                    return str[0];
                }
                throw new JScriptException(JSError.TypeMismatch);
            Label_1664:
                return Coerce2WithNoTrunctation(ToNumber(str), target);
            Label_167D:
                try
                {
                    return float.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    goto Label_1664;
                }
            Label_1698:
                try
                {
                    return long.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    goto Label_1664;
                }
            Label_16B3:
                try
                {
                    return ulong.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    goto Label_1664;
                }
            Label_16CB:
                try
                {
                    return decimal.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    goto Label_1664;
                }
            Label_16E3:
                try
                {
                    return DateTime.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return DatePrototype.getVarDate(DateConstructor.ob.CreateInstance(new object[] { DatePrototype.ParseDate(str) }));
                }
                return str;
            }
            catch (OverflowException)
            {
            }
        Label_1731:
            throw new JScriptException(JSError.TypeMismatch);
        }

        private static object Coerce2WithTruncationPermitted(object value, TypeCode target)
        {
            ushort num2;
            long num8;
            float num10;
            double num11;
            string str;
            if (value is EnumWrapper)
            {
                value = ((EnumWrapper) value).value;
            }
            if (value is ConstantWrapper)
            {
                value = ((ConstantWrapper) value).value;
            }
            IConvertible iConvertible = GetIConvertible(value);
            switch (GetTypeCode(value, iConvertible))
            {
                case TypeCode.Empty:
                    break;

                case TypeCode.Object:
                    if ((value is System.Reflection.Missing) || ((value is Microsoft.JScript.Missing) && (target != TypeCode.Object)))
                    {
                        break;
                    }
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return ToBoolean(value, iConvertible);

                        case TypeCode.Char:
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
                            return Coerce2WithTruncationPermitted(ToNumber(value, iConvertible), target);

                        case TypeCode.DateTime:
                            if (value is DateObject)
                            {
                                return DatePrototype.getVarDate((DateObject) value);
                            }
                            return Coerce2WithTruncationPermitted(ToNumber(value, iConvertible), target);

                        case TypeCode.String:
                            return ToString(value, iConvertible);
                    }
                    goto Label_1100;

                case TypeCode.DBNull:
                    switch (target)
                    {
                        case TypeCode.DBNull:
                            return DBNull.Value;

                        case TypeCode.Boolean:
                            return false;

                        case TypeCode.Char:
                            return '\0';

                        case TypeCode.SByte:
                            return (sbyte) 0;

                        case TypeCode.Byte:
                            return (byte) 0;

                        case TypeCode.Int16:
                            return (short) 0;

                        case TypeCode.UInt16:
                            return (ushort) 0;

                        case TypeCode.Int32:
                            return 0;

                        case TypeCode.UInt32:
                            return 0;

                        case TypeCode.Int64:
                            return 0L;

                        case TypeCode.UInt64:
                            return (ulong) 0L;

                        case TypeCode.Single:
                            return 0f;

                        case TypeCode.Double:
                            return 0.0;

                        case TypeCode.Decimal:
                            return 0M;

                        case TypeCode.DateTime:
                            return new DateTime(0L);

                        case TypeCode.String:
                            return "null";
                    }
                    goto Label_1100;

                case TypeCode.Boolean:
                {
                    bool flag = iConvertible.ToBoolean(null);
                    int num = flag ? 1 : 0;
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return flag;

                        case TypeCode.Char:
                            return (char) num;

                        case TypeCode.SByte:
                            return (sbyte) num;

                        case TypeCode.Byte:
                            return (byte) num;

                        case TypeCode.Int16:
                            return (short) num;

                        case TypeCode.UInt16:
                            return (ushort) num;

                        case TypeCode.Int32:
                            return num;

                        case TypeCode.UInt32:
                            return (uint) num;

                        case TypeCode.Int64:
                            return (long) num;

                        case TypeCode.UInt64:
                            return (ulong) num;

                        case TypeCode.Single:
                            return (float) num;

                        case TypeCode.Double:
                            return (double) num;

                        case TypeCode.Decimal:
                            return (decimal) num;

                        case TypeCode.DateTime:
                            return new DateTime((long) num);

                        case TypeCode.String:
                            if (!flag)
                            {
                                return "false";
                            }
                            return "true";
                    }
                    goto Label_1100;
                }
                case TypeCode.Char:
                {
                    char c = iConvertible.ToChar(null);
                    num2 = c;
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num2 != 0);

                        case TypeCode.Char:
                            return c;

                        case TypeCode.SByte:
                            return (sbyte) num2;

                        case TypeCode.Byte:
                            return (byte) num2;

                        case TypeCode.Int16:
                            return (short) num2;

                        case TypeCode.UInt16:
                            return num2;

                        case TypeCode.Int32:
                            return (int) num2;

                        case TypeCode.UInt32:
                            return (uint) num2;

                        case TypeCode.Int64:
                            return (long) num2;

                        case TypeCode.UInt64:
                            return (ulong) num2;

                        case TypeCode.Single:
                            return (float) num2;

                        case TypeCode.Double:
                            return (double) num2;

                        case TypeCode.Decimal:
                            return (decimal) num2;

                        case TypeCode.DateTime:
                            return new DateTime((long) num2);

                        case TypeCode.String:
                            return char.ToString(c);
                    }
                    goto Label_1100;
                }
                case TypeCode.SByte:
                {
                    sbyte num3 = iConvertible.ToSByte(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num3 != 0);

                        case TypeCode.Char:
                            return (char) ((ushort) num3);

                        case TypeCode.SByte:
                            return num3;

                        case TypeCode.Byte:
                            return (byte) num3;

                        case TypeCode.Int16:
                            return (short) num3;

                        case TypeCode.UInt16:
                            return (ushort) num3;

                        case TypeCode.Int32:
                            return (int) num3;

                        case TypeCode.UInt32:
                            return (uint) num3;

                        case TypeCode.Int64:
                            return (long) num3;

                        case TypeCode.UInt64:
                            return (ulong) num3;

                        case TypeCode.Single:
                            return (float) num3;

                        case TypeCode.Double:
                            return (double) num3;

                        case TypeCode.Decimal:
                            return (decimal) num3;

                        case TypeCode.DateTime:
                            return new DateTime((long) num3);

                        case TypeCode.String:
                            return num3.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.Byte:
                {
                    byte num4 = iConvertible.ToByte(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num4 != 0);

                        case TypeCode.Char:
                            return (char) num4;

                        case TypeCode.SByte:
                            return (sbyte) num4;

                        case TypeCode.Byte:
                            return num4;

                        case TypeCode.Int16:
                            return (short) num4;

                        case TypeCode.UInt16:
                            return (ushort) num4;

                        case TypeCode.Int32:
                            return (int) num4;

                        case TypeCode.UInt32:
                            return (uint) num4;

                        case TypeCode.Int64:
                            return (long) num4;

                        case TypeCode.UInt64:
                            return (ulong) num4;

                        case TypeCode.Single:
                            return (float) num4;

                        case TypeCode.Double:
                            return (double) num4;

                        case TypeCode.Decimal:
                            return (decimal) num4;

                        case TypeCode.DateTime:
                            return new DateTime((long) num4);

                        case TypeCode.String:
                            return num4.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.Int16:
                {
                    short num5 = iConvertible.ToInt16(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num5 != 0);

                        case TypeCode.Char:
                            return (char) ((ushort) num5);

                        case TypeCode.SByte:
                            return (sbyte) num5;

                        case TypeCode.Byte:
                            return (byte) num5;

                        case TypeCode.Int16:
                            return num5;

                        case TypeCode.UInt16:
                            return (ushort) num5;

                        case TypeCode.Int32:
                            return (int) num5;

                        case TypeCode.UInt32:
                            return (uint) num5;

                        case TypeCode.Int64:
                            return (long) num5;

                        case TypeCode.UInt64:
                            return (ulong) num5;

                        case TypeCode.Single:
                            return (float) num5;

                        case TypeCode.Double:
                            return (double) num5;

                        case TypeCode.Decimal:
                            return (decimal) num5;

                        case TypeCode.DateTime:
                            return new DateTime((long) num5);

                        case TypeCode.String:
                            return num5.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.UInt16:
                    num2 = iConvertible.ToUInt16(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num2 != 0);

                        case TypeCode.Char:
                            return (char) num2;

                        case TypeCode.SByte:
                            return (sbyte) num2;

                        case TypeCode.Byte:
                            return (byte) num2;

                        case TypeCode.Int16:
                            return (short) num2;

                        case TypeCode.UInt16:
                            return num2;

                        case TypeCode.Int32:
                            return (int) num2;

                        case TypeCode.UInt32:
                            return (uint) num2;

                        case TypeCode.Int64:
                            return (long) num2;

                        case TypeCode.UInt64:
                            return (ulong) num2;

                        case TypeCode.Single:
                            return (float) num2;

                        case TypeCode.Double:
                            return (double) num2;

                        case TypeCode.Decimal:
                            return (decimal) num2;

                        case TypeCode.DateTime:
                            return new DateTime((long) num2);

                        case TypeCode.String:
                            return num2.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;

                case TypeCode.Int32:
                {
                    int num6 = iConvertible.ToInt32(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num6 != 0);

                        case TypeCode.Char:
                            return (char) num6;

                        case TypeCode.SByte:
                            return (sbyte) num6;

                        case TypeCode.Byte:
                            return (byte) num6;

                        case TypeCode.Int16:
                            return (short) num6;

                        case TypeCode.UInt16:
                            return (ushort) num6;

                        case TypeCode.Int32:
                            return num6;

                        case TypeCode.UInt32:
                            return (uint) num6;

                        case TypeCode.Int64:
                            return (long) num6;

                        case TypeCode.UInt64:
                            return (ulong) num6;

                        case TypeCode.Single:
                            return (float) num6;

                        case TypeCode.Double:
                            return (double) num6;

                        case TypeCode.Decimal:
                            return (decimal) num6;

                        case TypeCode.DateTime:
                            return new DateTime((long) num6);

                        case TypeCode.String:
                            return num6.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.UInt32:
                {
                    uint num7 = iConvertible.ToUInt32(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num7 != 0);

                        case TypeCode.Char:
                            return (char) num7;

                        case TypeCode.SByte:
                            return (sbyte) num7;

                        case TypeCode.Byte:
                            return (byte) num7;

                        case TypeCode.Int16:
                            return (short) num7;

                        case TypeCode.UInt16:
                            return (ushort) num7;

                        case TypeCode.Int32:
                            return (int) num7;

                        case TypeCode.UInt32:
                            return num7;

                        case TypeCode.Int64:
                            return (long) num7;

                        case TypeCode.UInt64:
                            return (ulong) num7;

                        case TypeCode.Single:
                            return (float) num7;

                        case TypeCode.Double:
                            return (double) num7;

                        case TypeCode.Decimal:
                            return (decimal) num7;

                        case TypeCode.DateTime:
                            return new DateTime((long) num7);

                        case TypeCode.String:
                            return num7.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.Int64:
                    num8 = iConvertible.ToInt64(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num8 != 0L);

                        case TypeCode.Char:
                            return (char) ((ushort) num8);

                        case TypeCode.SByte:
                            return (sbyte) num8;

                        case TypeCode.Byte:
                            return (byte) num8;

                        case TypeCode.Int16:
                            return (short) num8;

                        case TypeCode.UInt16:
                            return (ushort) num8;

                        case TypeCode.Int32:
                            return (int) num8;

                        case TypeCode.UInt32:
                            return (uint) num8;

                        case TypeCode.Int64:
                            return num8;

                        case TypeCode.UInt64:
                            return (ulong) num8;

                        case TypeCode.Single:
                            return (float) num8;

                        case TypeCode.Double:
                            return (double) num8;

                        case TypeCode.Decimal:
                            return (decimal) num8;

                        case TypeCode.DateTime:
                            return new DateTime(num8);

                        case TypeCode.String:
                            return num8.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;

                case TypeCode.UInt64:
                {
                    ulong num9 = iConvertible.ToUInt64(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (num9 != 0L);

                        case TypeCode.Char:
                            return (char) num9;

                        case TypeCode.SByte:
                            return (sbyte) num9;

                        case TypeCode.Byte:
                            return (byte) num9;

                        case TypeCode.Int16:
                            return (short) num9;

                        case TypeCode.UInt16:
                            return (ushort) num9;

                        case TypeCode.Int32:
                            return (int) num9;

                        case TypeCode.UInt32:
                            return (uint) num9;

                        case TypeCode.Int64:
                            return (long) num9;

                        case TypeCode.UInt64:
                            return num9;

                        case TypeCode.Single:
                            return (float) num9;

                        case TypeCode.Double:
                            return (double) num9;

                        case TypeCode.Decimal:
                            return (decimal) num9;

                        case TypeCode.DateTime:
                            return new DateTime((long) num9);

                        case TypeCode.String:
                            return num9.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.Single:
                    num10 = iConvertible.ToSingle(null);
                    switch (target)
                    {
                        case TypeCode.Single:
                            return num10;

                        case TypeCode.Double:
                            return (double) num10;

                        case TypeCode.Decimal:
                            return (decimal) num10;

                        case TypeCode.String:
                            return ToString((double) num10);

                        case TypeCode.Boolean:
                            if (num10 != num10)
                            {
                                return false;
                            }
                            return !(num10 == 0f);
                    }
                    goto Label_0CD3;

                case TypeCode.Double:
                    num11 = iConvertible.ToDouble(null);
                    switch (target)
                    {
                        case TypeCode.Single:
                            return (float) num11;

                        case TypeCode.Double:
                            return num11;

                        case TypeCode.Decimal:
                            return (decimal) num11;

                        case TypeCode.String:
                            return ToString(num11);

                        case TypeCode.Boolean:
                            return ToBoolean(num11);
                    }
                    goto Label_0DE7;

                case TypeCode.Decimal:
                {
                    decimal val = iConvertible.ToDecimal(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return (val != 0M);

                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            return Coerce2WithTruncationPermitted(Runtime.UncheckedDecimalToInt64(val), target);

                        case TypeCode.Single:
                            return decimal.ToSingle(val);

                        case TypeCode.Double:
                            return decimal.ToDouble(val);

                        case TypeCode.Decimal:
                            return val;

                        case TypeCode.DateTime:
                            return new DateTime(Runtime.UncheckedDecimalToInt64(val));

                        case TypeCode.String:
                            return val.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.DateTime:
                {
                    DateTime time = iConvertible.ToDateTime(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                        case TypeCode.Char:
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
                            return Coerce2WithTruncationPermitted(time.Ticks, target);

                        case TypeCode.DateTime:
                            return time;

                        case TypeCode.String:
                            return time.ToString(CultureInfo.InvariantCulture);
                    }
                    goto Label_1100;
                }
                case TypeCode.String:
                    str = iConvertible.ToString(null);
                    switch (target)
                    {
                        case TypeCode.Boolean:
                            return ToBoolean(str, false);

                        case TypeCode.Char:
                            if (str.Length != 1)
                            {
                                throw new JScriptException(JSError.TypeMismatch);
                            }
                            return str[0];

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Double:
                            goto Label_105C;

                        case TypeCode.Int64:
                            goto Label_108A;

                        case TypeCode.UInt64:
                            goto Label_10B8;

                        case TypeCode.Single:
                            try
                            {
                                return float.Parse(str, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                goto Label_105C;
                            }
                            goto Label_108A;

                        case TypeCode.Decimal:
                            goto Label_10D0;

                        case TypeCode.DateTime:
                            goto Label_10EB;

                        case TypeCode.String:
                            return str;
                    }
                    goto Label_1100;

                default:
                    goto Label_1100;
            }
            switch (target)
            {
                case TypeCode.DBNull:
                    return DBNull.Value;

                case TypeCode.Boolean:
                    return false;

                case TypeCode.Char:
                    return '\0';

                case TypeCode.SByte:
                    return (sbyte) 0;

                case TypeCode.Byte:
                    return (byte) 0;

                case TypeCode.Int16:
                    return (short) 0;

                case TypeCode.UInt16:
                    return (ushort) 0;

                case TypeCode.Int32:
                    return 0;

                case TypeCode.UInt32:
                    return 0;

                case TypeCode.Int64:
                    return 0L;

                case TypeCode.UInt64:
                    return (ulong) 0L;

                case TypeCode.Single:
                    return (float) 1.0 / (float) 0.0;

                case TypeCode.Double:
                    return (double) 1.0 / (double) 0.0;

                case TypeCode.Decimal:
                    return 0M;

                case TypeCode.DateTime:
                    return new DateTime(0L);

                case TypeCode.String:
                    return "undefined";

                default:
                    goto Label_1100;
            }
        Label_0CD3:
            num8 = Runtime.DoubleToInt64((double) num10);
            switch (target)
            {
                case TypeCode.Char:
                    return (char) ((ushort) num8);

                case TypeCode.SByte:
                    return (sbyte) num8;

                case TypeCode.Byte:
                    return (byte) num8;

                case TypeCode.Int16:
                    return (short) num8;

                case TypeCode.UInt16:
                    return (ushort) num8;

                case TypeCode.Int32:
                    return (int) num8;

                case TypeCode.UInt32:
                    return (uint) num8;

                case TypeCode.Int64:
                    return num8;

                case TypeCode.UInt64:
                    return (ulong) num8;

                case TypeCode.DateTime:
                    return new DateTime(num8);

                default:
                    goto Label_1100;
            }
        Label_0DE7:
            num8 = Runtime.DoubleToInt64(num11);
            switch (target)
            {
                case TypeCode.Char:
                    return (char) ((ushort) num8);

                case TypeCode.SByte:
                    return (sbyte) num8;

                case TypeCode.Byte:
                    return (byte) num8;

                case TypeCode.Int16:
                    return (short) num8;

                case TypeCode.UInt16:
                    return (ushort) num8;

                case TypeCode.Int32:
                    return (int) num8;

                case TypeCode.UInt32:
                    return (uint) num8;

                case TypeCode.Int64:
                    return num8;

                case TypeCode.UInt64:
                    return (ulong) num8;

                case TypeCode.DateTime:
                    return new DateTime(num8);

                default:
                    goto Label_1100;
            }
        Label_105C:
            return Coerce2WithTruncationPermitted(ToNumber(str), target);
        Label_108A:
            try
            {
                return long.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return (long) ulong.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                    goto Label_105C;
                }
            }
        Label_10B8:
            try
            {
                return ulong.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                goto Label_105C;
            }
        Label_10D0:
            try
            {
                return decimal.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                goto Label_105C;
            }
        Label_10EB:
            return DateTime.Parse(str, CultureInfo.InvariantCulture);
        Label_1100:
            throw new JScriptException(JSError.TypeMismatch);
        }

        internal static object CoerceT(object value, Type type)
        {
            return CoerceT(value, type, false);
        }

        public static object CoerceT(object value, Type t, bool explicitOK)
        {
            if (t != typeof(object))
            {
                if ((t == typeof(string)) && (value is string))
                {
                    return value;
                }
                if ((t.IsEnum && !(t is EnumBuilder)) && !(t is TypeBuilder))
                {
                    IConvertible iConvertible = GetIConvertible(value);
                    TypeCode code = GetTypeCode(value, iConvertible);
                    if (code == TypeCode.String)
                    {
                        return Enum.Parse(t, iConvertible.ToString(CultureInfo.InvariantCulture));
                    }
                    if (!explicitOK && (code != TypeCode.Empty))
                    {
                        Type type = value.GetType();
                        if (type.IsEnum)
                        {
                            if (type != t)
                            {
                                throw new JScriptException(JSError.TypeMismatch);
                            }
                            return value;
                        }
                    }
                    return Enum.ToObject(t, CoerceT(value, GetUnderlyingType(t), explicitOK));
                }
                TypeCode typeCode = Type.GetTypeCode(t);
                if (typeCode != TypeCode.Object)
                {
                    return Coerce2(value, typeCode, explicitOK);
                }
                if (value is ConcatString)
                {
                    value = value.ToString();
                }
                if (((value == null) || ((value == DBNull.Value) && (t != typeof(object)))) || ((value is Microsoft.JScript.Missing) || (value is System.Reflection.Missing)))
                {
                    if (!t.IsValueType)
                    {
                        return null;
                    }
                    if (!t.IsPublic && (t.Assembly == typeof(ActiveXObjectConstructor).Assembly))
                    {
                        throw new JScriptException(JSError.CantCreateObject);
                    }
                    return Activator.CreateInstance(t);
                }
                if (t.IsAssignableFrom(value.GetType()))
                {
                    return value;
                }
                if (typeof(Delegate).IsAssignableFrom(t))
                {
                    if (value is Closure)
                    {
                        return ((Closure) value).ConvertToDelegate(t);
                    }
                    if (value is FunctionWrapper)
                    {
                        return ((FunctionWrapper) value).ConvertToDelegate(t);
                    }
                    if (value is FunctionObject)
                    {
                        return value;
                    }
                }
                else
                {
                    if ((value is ArrayObject) && typeof(Array).IsAssignableFrom(t))
                    {
                        return ((ArrayObject) value).ToNativeArray(t.GetElementType());
                    }
                    if (((value is Array) && (t == typeof(ArrayObject))) && (((Array) value).Rank == 1))
                    {
                        if (Globals.contextEngine == null)
                        {
                            Globals.contextEngine = new VsaEngine(true);
                            Globals.contextEngine.InitVsaEngine("JS7://Microsoft.JScript.Vsa.VsaEngine", new DefaultVsaSite());
                        }
                        return Globals.contextEngine.GetOriginalArrayConstructor().ConstructWrapper((Array) value);
                    }
                    if ((value is ClassScope) && (t == typeof(Type)))
                    {
                        return ((ClassScope) value).GetTypeBuilderOrEnumBuilder();
                    }
                    if ((value is TypedArray) && (t == typeof(Type)))
                    {
                        return ((TypedArray) value).ToType();
                    }
                }
                Type ir = value.GetType();
                MethodInfo method = null;
                if (explicitOK)
                {
                    method = t.GetMethod("op_Explicit", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { ir }, null);
                    if ((method != null) && ((method.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
                    {
                        method = new JSMethodInfo(method);
                        return method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
                    }
                    method = GetToXXXXMethod(ir, t, explicitOK);
                    if ((method != null) && ((method.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
                    {
                        method = new JSMethodInfo(method);
                        if (method.IsStatic)
                        {
                            return method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
                        }
                        return method.Invoke(value, BindingFlags.SuppressChangeType, null, new object[0], null);
                    }
                }
                method = t.GetMethod("op_Implicit", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { ir }, null);
                if ((method != null) && ((method.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
                {
                    method = new JSMethodInfo(method);
                    return method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
                }
                method = GetToXXXXMethod(ir, t, false);
                if ((method != null) && ((method.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
                {
                    method = new JSMethodInfo(method);
                    if (method.IsStatic)
                    {
                        return method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
                    }
                    return method.Invoke(value, BindingFlags.SuppressChangeType, null, new object[0], null);
                }
                if (t.IsByRef)
                {
                    return CoerceT(value, t.GetElementType());
                }
                if (!value.GetType().IsCOMObject)
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
            }
            return value;
        }

        private static double DoubleParse(string str)
        {
            try
            {
                return double.Parse(str, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                int num = 0;
                int length = str.Length;
                while ((num < length) && IsWhiteSpace(str[num]))
                {
                    num++;
                }
                if ((num < length) && (str[num] == '-'))
                {
                    return double.NegativeInfinity;
                }
                return double.PositiveInfinity;
            }
        }

        internal static void Emit(AST ast, ILGenerator il, Type source_type, Type target_type)
        {
            Emit(ast, il, source_type, target_type, false);
        }

        internal static void Emit(AST ast, ILGenerator il, Type source_type, Type target_type, bool truncationPermitted)
        {
            if (source_type != target_type)
            {
                if (target_type == Typeob.Void)
                {
                    il.Emit(OpCodes.Pop);
                    return;
                }
                if (target_type.IsEnum)
                {
                    if ((source_type == Typeob.String) || (source_type == Typeob.Object))
                    {
                        il.Emit(OpCodes.Ldtoken, target_type);
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                        ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                        il.Emit(OpCodes.Call, CompilerGlobals.coerceTMethod);
                        EmitUnbox(il, target_type, Type.GetTypeCode(GetUnderlyingType(target_type)));
                        return;
                    }
                    Emit(ast, il, source_type, GetUnderlyingType(target_type));
                    return;
                }
                if (!source_type.IsEnum)
                {
                    goto Label_013D;
                }
                if (target_type.IsPrimitive)
                {
                    Emit(ast, il, GetUnderlyingType(source_type), target_type);
                    return;
                }
                if ((target_type == Typeob.Object) || (target_type == Typeob.Enum))
                {
                    il.Emit(OpCodes.Box, source_type);
                    return;
                }
                if (!(target_type == Typeob.String))
                {
                    goto Label_013D;
                }
                il.Emit(OpCodes.Box, source_type);
                ConstantWrapper.TranslateToILInt(il, 0);
                il.Emit(OpCodes.Call, CompilerGlobals.toStringMethod);
            }
            return;
        Label_013D:
            while (source_type is TypeBuilder)
            {
                source_type = source_type.BaseType;
                if (source_type == null)
                {
                    source_type = Typeob.Object;
                }
                if (source_type == target_type)
                {
                    return;
                }
            }
            if (source_type.IsArray && target_type.IsArray)
            {
                return;
            }
            TypeCode typeCode = Type.GetTypeCode(source_type);
            TypeCode target = (target_type is TypeBuilder) ? TypeCode.Object : Type.GetTypeCode(target_type);
            switch (typeCode)
            {
                case TypeCode.Empty:
                    return;

                case TypeCode.Object:
                    if (source_type == Typeob.Void)
                    {
                        il.Emit(OpCodes.Ldnull);
                        source_type = Typeob.Object;
                    }
                    switch (target)
                    {
                        case TypeCode.Object:
                            if (!target_type.IsArray && !(target_type == Typeob.Array))
                            {
                                if (target_type is TypeBuilder)
                                {
                                    il.Emit(OpCodes.Castclass, target_type);
                                    return;
                                }
                                if ((target_type == Typeob.Enum) && (source_type.BaseType == Typeob.Enum))
                                {
                                    il.Emit(OpCodes.Box, source_type);
                                    return;
                                }
                                if ((target_type == Typeob.Object) || target_type.IsAssignableFrom(source_type))
                                {
                                    if (source_type.IsValueType)
                                    {
                                        il.Emit(OpCodes.Box, source_type);
                                    }
                                    return;
                                }
                                if (Typeob.JSObject.IsAssignableFrom(target_type))
                                {
                                    if (source_type.IsValueType)
                                    {
                                        il.Emit(OpCodes.Box, source_type);
                                    }
                                    ast.EmitILToLoadEngine(il);
                                    il.Emit(OpCodes.Call, CompilerGlobals.toObject2Method);
                                    il.Emit(OpCodes.Castclass, target_type);
                                    return;
                                }
                                if (!EmittedCallToConversionMethod(ast, il, source_type, target_type))
                                {
                                    if (target_type.IsValueType || target_type.IsArray)
                                    {
                                        il.Emit(OpCodes.Ldtoken, target_type);
                                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                                        ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                                        il.Emit(OpCodes.Call, CompilerGlobals.coerceTMethod);
                                    }
                                    if (target_type.IsValueType)
                                    {
                                        EmitUnbox(il, target_type, target);
                                        return;
                                    }
                                    il.Emit(OpCodes.Castclass, target_type);
                                }
                                return;
                            }
                            if (!(source_type == Typeob.ArrayObject) && !(source_type == Typeob.Object))
                            {
                                goto Label_02A9;
                            }
                            if (!target_type.IsArray)
                            {
                                il.Emit(OpCodes.Ldtoken, Typeob.Object);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldtoken, target_type.GetElementType());
                            }
                            goto Label_0299;

                        case TypeCode.DBNull:
                        case (TypeCode.DateTime | TypeCode.Object):
                            return;

                        case TypeCode.Boolean:
                            if (source_type.IsValueType)
                            {
                                il.Emit(OpCodes.Box, source_type);
                            }
                            ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                            il.Emit(OpCodes.Call, CompilerGlobals.toBooleanMethod);
                            return;

                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                        case TypeCode.DateTime:
                            if (source_type.IsValueType)
                            {
                                il.Emit(OpCodes.Box, source_type);
                            }
                            if (truncationPermitted && (target == TypeCode.Int32))
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.toInt32Method);
                                return;
                            }
                            ConstantWrapper.TranslateToILInt(il, (int) target);
                            ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                            il.Emit(OpCodes.Call, CompilerGlobals.coerce2Method);
                            if (target_type.IsValueType)
                            {
                                EmitUnbox(il, target_type, target);
                            }
                            return;

                        case TypeCode.Single:
                            if (source_type.IsValueType)
                            {
                                il.Emit(OpCodes.Box, source_type);
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.toNumberMethod);
                            il.Emit(OpCodes.Conv_R4);
                            return;

                        case TypeCode.Double:
                            if (source_type.IsValueType)
                            {
                                il.Emit(OpCodes.Box, source_type);
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.toNumberMethod);
                            return;

                        case TypeCode.String:
                            if (source_type.IsValueType)
                            {
                                il.Emit(OpCodes.Box, source_type);
                            }
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Castclass, Typeob.String);
                                return;
                            }
                            ConstantWrapper.TranslateToILInt(il, 1);
                            il.Emit(OpCodes.Call, CompilerGlobals.toStringMethod);
                            return;
                    }
                    return;

                case TypeCode.DBNull:
                    if (source_type.IsValueType)
                    {
                        il.Emit(OpCodes.Box, source_type);
                    }
                    if ((target == TypeCode.Object) || ((target == TypeCode.String) && !truncationPermitted))
                    {
                        if (target_type == Typeob.Object)
                        {
                            return;
                        }
                        if (!target_type.IsValueType)
                        {
                            il.Emit(OpCodes.Pop);
                            il.Emit(OpCodes.Ldnull);
                            return;
                        }
                    }
                    if (target_type.IsValueType)
                    {
                        il.Emit(OpCodes.Ldtoken, target_type);
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                        ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                        il.Emit(OpCodes.Call, CompilerGlobals.coerceTMethod);
                        EmitUnbox(il, target_type, target);
                        return;
                    }
                    ConstantWrapper.TranslateToILInt(il, (int) target);
                    ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                    il.Emit(OpCodes.Call, CompilerGlobals.coerce2Method);
                    return;

                case TypeCode.Boolean:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            return;

                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Conv_U8);
                            return;

                        case TypeCode.Single:
                            il.Emit(OpCodes.Conv_R4);
                            return;

                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R8);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                        {
                            Label label = il.DefineLabel();
                            Label label2 = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse, label);
                            il.Emit(OpCodes.Ldstr, "true");
                            il.Emit(OpCodes.Br, label2);
                            il.MarkLabel(label);
                            il.Emit(OpCodes.Ldstr, "false");
                            il.MarkLabel(label2);
                            return;
                        }
                    }
                    goto Label_1C18;

                case TypeCode.Char:
                case TypeCode.UInt16:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Conv_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R_Un);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            if (typeCode == TypeCode.Char)
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.convertCharToStringMethod);
                                return;
                            }
                            EmitLdloca(il, Typeob.UInt32);
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.SByte:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R8);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.Int32);
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Byte:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1_Un);
                            return;

                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Conv_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R_Un);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.UInt32);
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Int16:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                        case TypeCode.Int32:
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R8);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.Int32);
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Int32:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U8);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R8);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.Int32);
                            il.Emit(OpCodes.Call, CompilerGlobals.int32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.UInt32:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I4_Un);
                            return;

                        case TypeCode.UInt32:
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Conv_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R_Un);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.UInt32);
                            il.Emit(OpCodes.Call, CompilerGlobals.uint32ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Int64:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I4);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U8);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R8);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.int64ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.Int64);
                            il.Emit(OpCodes.Call, CompilerGlobals.int64ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.UInt64:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Conv_I8);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I4);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            if (truncationPermitted)
                            {
                                il.Emit(OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Conv_Ovf_I8_Un);
                            return;

                        case TypeCode.UInt64:
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Conv_R_Un);
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.uint64ToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, Typeob.UInt64);
                            il.Emit(OpCodes.Call, CompilerGlobals.uint64ToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Single:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                        case TypeCode.Decimal:
                        case TypeCode.String:
                            il.Emit(OpCodes.Conv_R8);
                            Emit(ast, il, Typeob.Double, target_type);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_I4);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_I4);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_Ovf_U4);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_U8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return;

                        case TypeCode.DateTime:
                            if (truncationPermitted)
                            {
                                EmitSingleToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                                il.Emit(OpCodes.Conv_Ovf_I8);
                            }
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Double:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Call, CompilerGlobals.doubleToBooleanMethod);
                            return;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_U2);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_I1);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_U1);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_I2);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_I4);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_I4);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_U8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.checkIfDoubleIsIntegerMethod);
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return;

                        case TypeCode.Decimal:
                            il.Emit(OpCodes.Call, CompilerGlobals.doubleToDecimalMethod);
                            return;

                        case TypeCode.DateTime:
                            if (truncationPermitted)
                            {
                                EmitDoubleToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.checkIfSingleIsIntegerMethod);
                                il.Emit(OpCodes.Conv_Ovf_I8);
                            }
                            il.Emit(OpCodes.Newobj, CompilerGlobals.dateTimeConstructor);
                            return;

                        case TypeCode.String:
                            il.Emit(OpCodes.Call, CompilerGlobals.doubleToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.Decimal:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                            il.Emit(OpCodes.Ldsfld, CompilerGlobals.decimalZeroField);
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalCompare);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Ceq);
                            return;

                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            if (truncationPermitted)
                            {
                                EmitDecimalToIntegerTruncatedConversion(il, OpCodes.Conv_I4);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.decimalToInt32Method);
                            }
                            Emit(ast, il, Typeob.Int32, target_type, truncationPermitted);
                            return;

                        case TypeCode.UInt32:
                            if (truncationPermitted)
                            {
                                EmitDecimalToIntegerTruncatedConversion(il, OpCodes.Conv_U4);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalToUInt32Method);
                            return;

                        case TypeCode.Int64:
                            if (truncationPermitted)
                            {
                                EmitDecimalToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalToInt64Method);
                            return;

                        case TypeCode.UInt64:
                            if (truncationPermitted)
                            {
                                EmitDecimalToIntegerTruncatedConversion(il, OpCodes.Conv_U8);
                                return;
                            }
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalToUInt64Method);
                            return;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalToDoubleMethod);
                            Emit(ast, il, Typeob.Double, target_type, truncationPermitted);
                            return;

                        case TypeCode.Decimal:
                            return;

                        case TypeCode.DateTime:
                            if (truncationPermitted)
                            {
                                EmitDecimalToIntegerTruncatedConversion(il, OpCodes.Conv_I8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.decimalToInt64Method);
                            }
                            Emit(ast, il, Typeob.Int64, target_type);
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, source_type);
                            il.Emit(OpCodes.Call, CompilerGlobals.decimalToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.DateTime:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if ((target_type == Typeob.Object) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                il.Emit(OpCodes.Box, source_type);
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                        case TypeCode.Char:
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
                            EmitLdloca(il, source_type);
                            il.Emit(OpCodes.Call, CompilerGlobals.dateTimeToInt64Method);
                            Emit(ast, il, Typeob.Int64, target_type, truncationPermitted);
                            return;

                        case TypeCode.DateTime:
                            return;

                        case TypeCode.String:
                            EmitLdloca(il, source_type);
                            il.Emit(OpCodes.Call, CompilerGlobals.dateTimeToStringMethod);
                            return;
                    }
                    goto Label_1C18;

                case TypeCode.String:
                    switch (target)
                    {
                        case TypeCode.Object:
                            if (((target_type == Typeob.Object) || (target_type is TypeBuilder)) || !EmittedCallToConversionMethod(ast, il, source_type, target_type))
                            {
                                Emit(ast, il, Typeob.Object, target_type);
                            }
                            return;

                        case TypeCode.Boolean:
                        case TypeCode.Char:
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
                        case TypeCode.DateTime:
                            if (truncationPermitted && (target == TypeCode.Int32))
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.toInt32Method);
                                return;
                            }
                            ConstantWrapper.TranslateToILInt(il, (int) target);
                            ConstantWrapper.TranslateToILInt(il, truncationPermitted ? 1 : 0);
                            il.Emit(OpCodes.Call, CompilerGlobals.coerce2Method);
                            if (target_type.IsValueType)
                            {
                                EmitUnbox(il, target_type, target);
                            }
                            return;

                        case TypeCode.String:
                            return;
                    }
                    goto Label_1C18;

                default:
                    goto Label_1C18;
            }
        Label_0299:
            il.Emit(OpCodes.Call, CompilerGlobals.toNativeArrayMethod);
        Label_02A9:
            il.Emit(OpCodes.Castclass, target_type);
            return;
        Label_1C18:
            Emit(ast, il, source_type, Typeob.Object);
            il.Emit(OpCodes.Call, CompilerGlobals.throwTypeMismatch);
            LocalBuilder local = il.DeclareLocal(target_type);
            il.Emit(OpCodes.Ldloc, local);
        }

        internal static void EmitDecimalToIntegerTruncatedConversion(ILGenerator il, OpCode opConversion)
        {
            il.Emit(OpCodes.Call, CompilerGlobals.uncheckedDecimalToInt64Method);
            if (!opConversion.Equals(OpCodes.Conv_I8))
            {
                il.Emit(opConversion);
            }
        }

        internal static void EmitDoubleToIntegerTruncatedConversion(ILGenerator il, OpCode opConversion)
        {
            il.Emit(OpCodes.Call, CompilerGlobals.doubleToInt64);
            if (!opConversion.Equals(OpCodes.Conv_I8))
            {
                il.Emit(opConversion);
            }
        }

        internal static void EmitLdarg(ILGenerator il, short argNum)
        {
            switch (argNum)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    return;
            }
            if (argNum < 0x100)
            {
                il.Emit(OpCodes.Ldarg_S, (byte) argNum);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, argNum);
            }
        }

        internal static void EmitLdloca(ILGenerator il, Type source_type)
        {
            LocalBuilder local = il.DeclareLocal(source_type);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloca, local);
        }

        internal static void EmitSingleToIntegerTruncatedConversion(ILGenerator il, OpCode opConversion)
        {
            il.Emit(OpCodes.Conv_R8);
            EmitDoubleToIntegerTruncatedConversion(il, opConversion);
        }

        private static bool EmittedCallToConversionMethod(AST ast, ILGenerator il, Type source_type, Type target_type)
        {
            MethodInfo meth = target_type.GetMethod("op_Explicit", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { source_type }, null);
            if (meth != null)
            {
                il.Emit(OpCodes.Call, meth);
                Emit(ast, il, meth.ReturnType, target_type);
                return true;
            }
            meth = GetToXXXXMethod(source_type, target_type, true);
            if (meth != null)
            {
                il.Emit(OpCodes.Call, meth);
                return true;
            }
            meth = target_type.GetMethod("op_Implicit", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { source_type }, null);
            if (meth != null)
            {
                il.Emit(OpCodes.Call, meth);
                Emit(ast, il, meth.ReturnType, target_type);
                return true;
            }
            meth = GetToXXXXMethod(source_type, target_type, false);
            if (meth != null)
            {
                il.Emit(OpCodes.Call, meth);
                return true;
            }
            return false;
        }

        internal static void EmitUnbox(ILGenerator il, Type target_type, TypeCode target)
        {
            il.Emit(OpCodes.Unbox, target_type);
            switch (target)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                    il.Emit(OpCodes.Ldind_U1);
                    return;

                case TypeCode.Char:
                case TypeCode.UInt16:
                    il.Emit(OpCodes.Ldind_U2);
                    return;

                case TypeCode.SByte:
                    il.Emit(OpCodes.Ldind_I1);
                    return;

                case TypeCode.Int16:
                    il.Emit(OpCodes.Ldind_I2);
                    return;

                case TypeCode.Int32:
                    il.Emit(OpCodes.Ldind_I4);
                    return;

                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldind_U4);
                    return;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldind_I8);
                    return;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldind_R4);
                    return;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldind_R8);
                    return;
            }
            il.Emit(OpCodes.Ldobj, target_type);
        }

        private static IReflect GetArrayElementType(IReflect ir)
        {
            if (ir is TypedArray)
            {
                return ((TypedArray) ir).elementType;
            }
            if ((ir is Type) && ((Type) ir).IsArray)
            {
                return ((Type) ir).GetElementType();
            }
            if (!(ir is ArrayObject) && (ir != Typeob.ArrayObject))
            {
                return null;
            }
            return Typeob.Object;
        }

        internal static int GetArrayRank(IReflect ir)
        {
            if ((ir == Typeob.ArrayObject) || (ir is ArrayObject))
            {
                return 1;
            }
            if (ir is TypedArray)
            {
                return ((TypedArray) ir).rank;
            }
            if ((ir is Type) && ((Type) ir).IsArray)
            {
                return ((Type) ir).GetArrayRank();
            }
            return -1;
        }

        internal static IConvertible GetIConvertible(object ob)
        {
            return (ob as IConvertible);
        }

        private static MethodInfo GetToXXXXMethod(IReflect ir, Type desiredType, bool explicitOK)
        {
            if (!(ir is TypeBuilder) && !(ir is EnumBuilder))
            {
                MemberInfo[] member = ir.GetMember(explicitOK ? "op_Explicit" : "op_Implicit", BindingFlags.Public | BindingFlags.Static);
                if (member != null)
                {
                    foreach (MemberInfo info in member)
                    {
                        if ((info is MethodInfo) && (((MethodInfo) info).ReturnType == desiredType))
                        {
                            return (MethodInfo) info;
                        }
                    }
                }
            }
            return null;
        }

        internal static TypeCode GetTypeCode(object ob)
        {
            return GetTypeCode(ob, GetIConvertible(ob));
        }

        internal static TypeCode GetTypeCode(object ob, IConvertible ic)
        {
            if (ob == null)
            {
                return TypeCode.Empty;
            }
            if (ic == null)
            {
                return TypeCode.Object;
            }
            return ic.GetTypeCode();
        }

        internal static Type GetUnderlyingType(Type type)
        {
            if (type is TypeBuilder)
            {
                return type.UnderlyingSystemType;
            }
            return Enum.GetUnderlyingType(type);
        }

        internal static bool IsArray(IReflect ir)
        {
            return ((((ir == Typeob.Array) || (ir == Typeob.ArrayObject)) || ((ir is TypedArray) || (ir is ArrayObject))) || ((ir is Type) && ((Type) ir).IsArray));
        }

        private static bool IsArrayElementTypeKnown(IReflect ir)
        {
            return ((((ir == Typeob.ArrayObject) || (ir is TypedArray)) || (ir is ArrayObject)) || ((ir is Type) && ((Type) ir).IsArray));
        }

        internal static bool IsArrayRankKnown(IReflect ir)
        {
            return ((((ir == Typeob.ArrayObject) || (ir is TypedArray)) || (ir is ArrayObject)) || ((ir is Type) && ((Type) ir).IsArray));
        }

        internal static bool IsArrayType(IReflect ir)
        {
            return ((((ir is TypedArray) || (ir == Typeob.Array)) || (ir == Typeob.ArrayObject)) || ((ir is Type) && ((Type) ir).IsArray));
        }

        public static bool IsBadIndex(AST ast)
        {
            int num;
            if (!(ast is ConstantWrapper))
            {
                return false;
            }
            try
            {
                num = (int) CoerceT(((ConstantWrapper) ast).value, typeof(int));
            }
            catch
            {
                return true;
            }
            return (num < 0);
        }

        internal static bool IsJScriptArray(IReflect ir)
        {
            return ((ir is ArrayObject) || (ir == Typeob.ArrayObject));
        }

        internal static bool IsPrimitiveIntegerType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        internal static bool IsPrimitiveNumericType(IReflect ir)
        {
            Type type = ir as Type;
            if (type == null)
            {
                return false;
            }
            return IsPrimitiveNumericTypeCode(Type.GetTypeCode(type));
        }

        internal static bool IsPrimitiveNumericTypeCode(TypeCode tc)
        {
            switch (tc)
            {
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
                    return true;
            }
            return false;
        }

        internal static bool IsPrimitiveNumericTypeFitForDouble(IReflect ir)
        {
            Type type = ir as Type;
            if (type != null)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsPrimitiveSignedIntegerType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
            }
            return false;
        }

        internal static bool IsPrimitiveSignedNumericType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
            }
            return false;
        }

        internal static bool IsPrimitiveUnsignedIntegerType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        internal static bool IsPromotableTo(IReflect source_ir, IReflect target_ir)
        {
            Type scriptFunction;
            if ((((source_ir is TypedArray) || (target_ir is TypedArray)) || ((source_ir is ArrayObject) || (target_ir is ArrayObject))) || ((source_ir == Typeob.ArrayObject) || (target_ir == Typeob.ArrayObject)))
            {
                return IsPromotableToArray(source_ir, target_ir);
            }
            if (target_ir is ClassScope)
            {
                if (((ClassScope) target_ir).owner is EnumDeclaration)
                {
                    if (IsPrimitiveNumericType(source_ir))
                    {
                        return IsPromotableTo(source_ir, ((EnumDeclaration) ((ClassScope) target_ir).owner).baseType.ToType());
                    }
                    return ((source_ir == Typeob.String) || (source_ir == target_ir));
                }
                return ((source_ir is ClassScope) && ((ClassScope) source_ir).IsSameOrDerivedFrom((ClassScope) target_ir));
            }
            if (target_ir is Type)
            {
                if (target_ir == Typeob.Object)
                {
                    if (source_ir is Type)
                    {
                        return !((Type) source_ir).IsByRef;
                    }
                    return true;
                }
                scriptFunction = (Type) target_ir;
            }
            else if (target_ir is ScriptFunction)
            {
                scriptFunction = Typeob.ScriptFunction;
            }
            else
            {
                scriptFunction = Globals.TypeRefs.ToReferenceContext(target_ir.GetType());
            }
            if (source_ir is ClassScope)
            {
                return ((ClassScope) source_ir).IsPromotableTo(scriptFunction);
            }
            return IsPromotableTo((source_ir is Type) ? ((Type) source_ir) : Globals.TypeRefs.ToReferenceContext(source_ir.GetType()), scriptFunction);
        }

        private static bool IsPromotableTo(Type source_type, Type target_type)
        {
            TypeCode typeCode = Type.GetTypeCode(source_type);
            TypeCode code2 = Type.GetTypeCode(target_type);
            if (promotable[(int) typeCode, (int) code2])
            {
                return true;
            }
            if (((typeCode == TypeCode.Object) || (typeCode == TypeCode.String)) && (code2 == TypeCode.Object))
            {
                if (target_type.IsAssignableFrom(source_type))
                {
                    return true;
                }
                if ((target_type == Typeob.BooleanObject) && (source_type == Typeob.Boolean))
                {
                    return true;
                }
                if ((target_type == Typeob.StringObject) && (source_type == Typeob.String))
                {
                    return true;
                }
                if ((target_type == Typeob.NumberObject) && IsPromotableTo(source_type, Typeob.Double))
                {
                    return true;
                }
                if (((target_type == Typeob.Array) || (source_type == Typeob.Array)) || (target_type.IsArray || source_type.IsArray))
                {
                    return IsPromotableToArray(source_type, target_type);
                }
            }
            if ((source_type == Typeob.BooleanObject) && (target_type == Typeob.Boolean))
            {
                return true;
            }
            if ((source_type == Typeob.StringObject) && (target_type == Typeob.String))
            {
                return true;
            }
            if ((source_type == Typeob.DateObject) && (target_type == Typeob.DateTime))
            {
                return true;
            }
            if (source_type == Typeob.NumberObject)
            {
                return IsPrimitiveNumericType(target_type);
            }
            if (source_type.IsEnum)
            {
                return (!target_type.IsEnum && IsPromotableTo(GetUnderlyingType(source_type), target_type));
            }
            if (target_type.IsEnum)
            {
                return (!source_type.IsEnum && IsPromotableTo(source_type, GetUnderlyingType(target_type)));
            }
            MethodInfo info = target_type.GetMethod("op_Implicit", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { source_type }, null);
            if ((info != null) && ((info.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope))
            {
                return true;
            }
            info = GetToXXXXMethod(source_type, target_type, false);
            return ((info != null) && ((info.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope));
        }

        private static bool IsPromotableToArray(IReflect source_ir, IReflect target_ir)
        {
            if (!IsArray(source_ir))
            {
                return false;
            }
            if (target_ir == Typeob.Object)
            {
                return true;
            }
            if (!IsArray(target_ir))
            {
                if (target_ir is Type)
                {
                    Type type = (Type) target_ir;
                    if (type.IsInterface && type.IsAssignableFrom(Typeob.Array))
                    {
                        return ((source_ir is TypedArray) || ((source_ir is Type) && ((Type) source_ir).IsArray));
                    }
                }
                return false;
            }
            if (IsJScriptArray(source_ir) && !IsJScriptArray(target_ir))
            {
                return false;
            }
            if (target_ir == Typeob.Array)
            {
                return !IsJScriptArray(source_ir);
            }
            if (source_ir == Typeob.Array)
            {
                return false;
            }
            if ((GetArrayRank(source_ir) == 1) && IsJScriptArray(target_ir))
            {
                return true;
            }
            if (GetArrayRank(source_ir) != GetArrayRank(target_ir))
            {
                return false;
            }
            IReflect arrayElementType = GetArrayElementType(source_ir);
            IReflect reflect2 = GetArrayElementType(target_ir);
            if ((arrayElementType == null) || (reflect2 == null))
            {
                return false;
            }
            if ((!(arrayElementType is Type) || !((Type) arrayElementType).IsValueType) && (!(reflect2 is Type) || !((Type) reflect2).IsValueType))
            {
                return IsPromotableTo(arrayElementType, reflect2);
            }
            return (arrayElementType == reflect2);
        }

        private static bool IsWhiteSpace(char c)
        {
            switch (c)
            {
                case '\t':
                case '\n':
                case '\v':
                case '\f':
                case '\r':
                case ' ':
                case '\x00a0':
                    return true;
            }
            return ((c >= '\x0080') && char.IsWhiteSpace(c));
        }

        private static bool IsWhiteSpaceTrailer(char[] s, int i, int max)
        {
            while (i < max)
            {
                if (!IsWhiteSpace(s[i]))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        internal static object LiteralToNumber(string str)
        {
            return LiteralToNumber(str, null);
        }

        internal static object LiteralToNumber(string str, Context context)
        {
            uint rdx = 10;
            if ((str[0] == '0') && (str.Length > 1))
            {
                if ((str[1] == 'x') || (str[1] == 'X'))
                {
                    rdx = 0x10;
                }
                else
                {
                    rdx = 8;
                }
            }
            object obj2 = parseRadix(str.ToCharArray(), rdx, (rdx == 0x10) ? 2 : 0, 1, false);
            if (obj2 != null)
            {
                if (((rdx == 8) && (context != null)) && ((obj2 is int) && (((int) obj2) > 7)))
                {
                    context.HandleError(JSError.OctalLiteralsAreDeprecated);
                }
                return obj2;
            }
            context.HandleError(JSError.BadOctalLiteral);
            return parseRadix(str.ToCharArray(), 10, 0, 1, false);
        }

        internal static bool NeedsWrapper(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
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
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        private static object parseRadix(char[] s, uint rdx, int i, int sign, bool ignoreTrailers)
        {
            int length = s.Length;
            if (i >= length)
            {
                return null;
            }
            ulong num2 = ulong.MaxValue / ((ulong) rdx);
            int num3 = RadixDigit(s[i], rdx);
            if (num3 < 0)
            {
                return null;
            }
            ulong num4 = (ulong) num3;
            int startIndex = i;
            while (true)
            {
                if (++i == length)
                {
                    break;
                }
                num3 = RadixDigit(s[i], rdx);
                if (num3 < 0)
                {
                    if (!ignoreTrailers && !IsWhiteSpaceTrailer(s, i, length))
                    {
                        return null;
                    }
                    break;
                }
                if (num4 > num2)
                {
                    goto Label_00E6;
                }
                ulong num6 = num4 * rdx;
                ulong num7 = num6 + num3;
                if (num6 > num7)
                {
                    goto Label_00E6;
                }
                num4 = num7;
            }
            if (sign < 0)
            {
                if (num4 <= 0x80000000L)
                {
                    return (int) -num4;
                }
                if (num4 < 9223372036854775808L)
                {
                    return (long) -num4;
                }
                if (num4 == 9223372036854775808L)
                {
                    return -9223372036854775808L;
                }
                return -((double) num4);
            }
            if (num4 <= 0x7fffffffL)
            {
                return (int) num4;
            }
            if (num4 <= 0x7fffffffffffffffL)
            {
                return (long) num4;
            }
            return num4;
        Label_00E6:
            if (rdx == 10)
            {
                try
                {
                    double num8 = DoubleParse(new string(s, startIndex, length - startIndex));
                    if (num8 == num8)
                    {
                        return (sign * num8);
                    }
                    if (!ignoreTrailers)
                    {
                        return null;
                    }
                }
                catch
                {
                }
            }
            double num9 = (num4 * rdx) + num3;
            while (++i != length)
            {
                num3 = RadixDigit(s[i], rdx);
                if (num3 < 0)
                {
                    if (!ignoreTrailers && !IsWhiteSpaceTrailer(s, i, length))
                    {
                        return null;
                    }
                    return (sign * num9);
                }
                num9 = (num9 * rdx) + num3;
            }
            return (sign * num9);
        }

        private static int RadixDigit(char c, uint r)
        {
            int num;
            if ((c >= '0') && (c <= '9'))
            {
                num = c - '0';
            }
            else if ((c >= 'A') && (c <= 'Z'))
            {
                num = ('\n' + c) - 0x41;
            }
            else if ((c >= 'a') && (c <= 'z'))
            {
                num = ('\n' + c) - 0x61;
            }
            else
            {
                return -1;
            }
            if (num >= r)
            {
                return -1;
            }
            return num;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void ThrowTypeMismatch(object val)
        {
            throw new JScriptException(JSError.TypeMismatch, new Context(new DocumentContext("", null), val.ToString()));
        }

        public static bool ToBoolean(double d)
        {
            return ((d == d) && !(d == 0.0));
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static bool ToBoolean(object value)
        {
            if (value is bool)
            {
                return (bool) value;
            }
            return ToBoolean(value, GetIConvertible(value));
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static bool ToBoolean(object value, bool explicitConversion)
        {
            if (value is bool)
            {
                return (bool) value;
            }
            if (!explicitConversion && (value is BooleanObject))
            {
                return ((BooleanObject) value).value;
            }
            return ToBoolean(value, GetIConvertible(value));
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static bool ToBoolean(object value, IConvertible ic)
        {
            switch (GetTypeCode(value, ic))
            {
                case TypeCode.Empty:
                    return false;

                case TypeCode.Object:
                {
                    if ((value is Microsoft.JScript.Missing) || (value is System.Reflection.Missing))
                    {
                        return false;
                    }
                    Type type = value.GetType();
                    MethodInfo method = type.GetMethod("op_True", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type }, null);
                    if (((method != null) && ((method.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope)) && (method.ReturnType == typeof(bool)))
                    {
                        method = new JSMethodInfo(method);
                        return (bool) method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
                    }
                    return true;
                }
                case TypeCode.DBNull:
                    return false;

                case TypeCode.Boolean:
                    return ic.ToBoolean(null);

                case TypeCode.Char:
                    return (ic.ToChar(null) != '\0');

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return (ic.ToInt32(null) != 0);

                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return (ic.ToInt64(null) != 0L);

                case TypeCode.UInt64:
                    return (ic.ToUInt64(null) != 0L);

                case TypeCode.Single:
                case TypeCode.Double:
                {
                    double num = ic.ToDouble(null);
                    return ((num == num) && !(num == 0.0));
                }
                case TypeCode.Decimal:
                    return (ic.ToDecimal(null) != 0M);

                case TypeCode.DateTime:
                    return true;

                case TypeCode.String:
                    return (ic.ToString(null).Length != 0);
            }
            return false;
        }

        internal static char ToChar(object value)
        {
            return (char) ToUint32(value);
        }

        private static char ToDigit(int digit)
        {
            if (digit >= 10)
            {
                return (char) ((0x61 + digit) - 10);
            }
            return (char) (0x30 + digit);
        }

        public static object ToForInObject(object value, VsaEngine engine)
        {
            if (value is ScriptObject)
            {
                return value;
            }
            IConvertible iConvertible = GetIConvertible(value);
            switch (GetTypeCode(value, iConvertible))
            {
                case TypeCode.Object:
                    return value;

                case TypeCode.Boolean:
                    return engine.Globals.globalObject.originalBoolean.ConstructImplicitWrapper(iConvertible.ToBoolean(null));

                case TypeCode.Char:
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
                    return engine.Globals.globalObject.originalNumber.ConstructImplicitWrapper(value);

                case TypeCode.DateTime:
                    return value;

                case TypeCode.String:
                    return engine.Globals.globalObject.originalString.ConstructImplicitWrapper(iConvertible.ToString(null));
            }
            return engine.Globals.globalObject.originalObject.ConstructObject();
        }

        public static int ToInt32(object value)
        {
            if (value is double)
            {
                return (int) Runtime.DoubleToInt64((double) value);
            }
            if (value is int)
            {
                return (int) value;
            }
            return ToInt32(value, GetIConvertible(value));
        }

        internal static int ToInt32(object value, IConvertible ic)
        {
            switch (GetTypeCode(value, ic))
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    object obj2 = ToPrimitive(value, PreferredType.Number, ref ic);
                    if (obj2 == value)
                    {
                        return 0;
                    }
                    return ToInt32(obj2, ic);
                }
                case TypeCode.DBNull:
                    return 0;

                case TypeCode.Boolean:
                    if (ic.ToBoolean(null))
                    {
                        return 1;
                    }
                    return 0;

                case TypeCode.Char:
                    return ic.ToChar(null);

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return ic.ToInt32(null);

                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return (int) ic.ToInt64(null);

                case TypeCode.UInt64:
                    return (int) ic.ToUInt64(null);

                case TypeCode.Single:
                case TypeCode.Double:
                    return (int) Runtime.DoubleToInt64(ic.ToDouble(null));

                case TypeCode.Decimal:
                    return (int) Runtime.UncheckedDecimalToInt64(ic.ToDecimal(null));

                case TypeCode.String:
                    return (int) Runtime.DoubleToInt64(ToNumber(ic.ToString(null)));
            }
            return 0;
        }

        internal static double ToInteger(double number)
        {
            if (number != number)
            {
                return 0.0;
            }
            return (Math.Sign(number) * Math.Floor(Math.Abs(number)));
        }

        internal static double ToInteger(object value)
        {
            if (value is double)
            {
                return ToInteger((double) value);
            }
            if (value is int)
            {
                return (double) ((int) value);
            }
            return ToInteger(value, GetIConvertible(value));
        }

        internal static double ToInteger(object value, IConvertible ic)
        {
            switch (GetTypeCode(value, ic))
            {
                case TypeCode.Empty:
                    return 0.0;

                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    object obj2 = ToPrimitive(value, PreferredType.Number, ref ic);
                    if (obj2 == value)
                    {
                        return double.NaN;
                    }
                    return ToInteger(ToNumber(obj2, ic));
                }
                case TypeCode.DBNull:
                    return 0.0;

                case TypeCode.Boolean:
                    return (ic.ToBoolean(null) ? ((double) 1) : ((double) 0));

                case TypeCode.Char:
                    return (double) ic.ToChar(null);

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return ic.ToDouble(null);

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return ToInteger(ic.ToDouble(null));

                case TypeCode.String:
                    return ToInteger(ToNumber(ic.ToString(null)));
            }
            return 0.0;
        }

        internal static IReflect ToIReflect(Type t, VsaEngine engine)
        {
            GlobalObject globalObject = engine.Globals.globalObject;
            object ob = t;
            if (t == Typeob.ArrayObject)
            {
                ob = globalObject.originalArray.Construct();
            }
            else if (t == Typeob.BooleanObject)
            {
                ob = globalObject.originalBoolean.Construct();
            }
            else if (t == Typeob.DateObject)
            {
                ob = globalObject.originalDate.Construct(new object[0]);
            }
            else if (t == Typeob.EnumeratorObject)
            {
                ob = globalObject.originalEnumerator.Construct(new object[0]);
            }
            else if (t == Typeob.ErrorObject)
            {
                ob = globalObject.originalError.Construct(new object[0]);
            }
            else if (t == Typeob.EvalErrorObject)
            {
                ob = globalObject.originalEvalError.Construct(new object[0]);
            }
            else if (t == Typeob.JSObject)
            {
                ob = globalObject.originalObject.Construct(new object[0]);
            }
            else if (t == Typeob.NumberObject)
            {
                ob = globalObject.originalNumber.Construct();
            }
            else if (t == Typeob.RangeErrorObject)
            {
                ob = globalObject.originalRangeError.Construct(new object[0]);
            }
            else if (t == Typeob.ReferenceErrorObject)
            {
                ob = globalObject.originalReferenceError.Construct(new object[0]);
            }
            else if (t == Typeob.RegExpObject)
            {
                ob = globalObject.originalRegExp.Construct(new object[0]);
            }
            else if (t == Typeob.ScriptFunction)
            {
                ob = FunctionPrototype.ob;
            }
            else if (t == Typeob.StringObject)
            {
                ob = globalObject.originalString.Construct();
            }
            else if (t == Typeob.SyntaxErrorObject)
            {
                ob = globalObject.originalSyntaxError.Construct(new object[0]);
            }
            else if (t == Typeob.TypeErrorObject)
            {
                ob = globalObject.originalTypeError.Construct(new object[0]);
            }
            else if (t == Typeob.URIErrorObject)
            {
                ob = globalObject.originalURIError.Construct(new object[0]);
            }
            else if (t == Typeob.VBArrayObject)
            {
                ob = globalObject.originalVBArray.Construct();
            }
            else if (t == Typeob.ArgumentsObject)
            {
                ob = globalObject.originalObject.Construct(new object[0]);
            }
            return (IReflect) ob;
        }

        internal static string ToLocaleString(object value)
        {
            return ToString(value, PreferredType.LocaleString, true);
        }

        public static object ToNativeArray(object value, RuntimeTypeHandle handle)
        {
            if (value is ArrayObject)
            {
                Type typeFromHandle = Type.GetTypeFromHandle(handle);
                return ((ArrayObject) value).ToNativeArray(typeFromHandle);
            }
            return value;
        }

        public static double ToNumber(object value)
        {
            if (value is int)
            {
                return (double) ((int) value);
            }
            if (value is double)
            {
                return (double) value;
            }
            return ToNumber(value, GetIConvertible(value));
        }

        public static double ToNumber(string str)
        {
            return ToNumber(str, true, false, Microsoft.JScript.Missing.Value);
        }

        internal static double ToNumber(object value, IConvertible ic)
        {
            switch (GetTypeCode(value, ic))
            {
                case TypeCode.Empty:
                    return double.NaN;

                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    object obj2 = ToPrimitive(value, PreferredType.Number, ref ic);
                    if (obj2 == value)
                    {
                        return double.NaN;
                    }
                    return ToNumber(obj2, ic);
                }
                case TypeCode.DBNull:
                    return 0.0;

                case TypeCode.Boolean:
                    return (ic.ToBoolean(null) ? ((double) 1) : ((double) 0));

                case TypeCode.Char:
                    return (double) ic.ToChar(null);

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return (double) ic.ToInt32(null);

                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return (double) ic.ToInt64(null);

                case TypeCode.UInt64:
                    return (double) ic.ToUInt64(null);

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return ic.ToDouble(null);

                case TypeCode.String:
                    return ToNumber(ic.ToString(null));
            }
            return 0.0;
        }

        internal static double ToNumber(string str, bool hexOK, bool octalOK, object radix)
        {
            int num7;
            if (!octalOK)
            {
                try
                {
                    double num = DoubleParse(str);
                    if (num != 0.0)
                    {
                        return num;
                    }
                    int num2 = 0;
                    int length = str.Length;
                    while ((num2 < length) && IsWhiteSpace(str[num2]))
                    {
                        num2++;
                    }
                    if ((num2 < length) && (str[num2] == '-'))
                    {
                        return 0.0;
                    }
                    return 0.0;
                }
                catch
                {
                    char ch;
                    int num4 = str.Length;
                    int num5 = num4 - 1;
                    int num6 = 0;
                    while ((num6 < num4) && IsWhiteSpace(str[num6]))
                    {
                        num6++;
                    }
                    if (hexOK)
                    {
                        while ((num5 >= num6) && IsWhiteSpace(str[num5]))
                        {
                            num5--;
                        }
                        if (num6 > num5)
                        {
                            return 0.0;
                        }
                        if (num5 < (num4 - 1))
                        {
                            return ToNumber(str.Substring(num6, (num5 - num6) + 1), hexOK, octalOK, radix);
                        }
                        goto Label_01EC;
                    }
                    if (((num4 - num6) < 8) || (string.CompareOrdinal(str, num6, "Infinity", 0, 8) != 0))
                    {
                        if (((num4 - num6) >= 9) && (string.CompareOrdinal(str, num6, "-Infinity", 0, 8) == 0))
                        {
                            return double.NegativeInfinity;
                        }
                        if (((num4 - num6) < 9) || (string.CompareOrdinal(str, num6, "+Infinity", 0, 8) != 0))
                        {
                            goto Label_018E;
                        }
                    }
                    return double.PositiveInfinity;
                Label_0175:
                    ch = str[num5];
                    if (!JSScanner.IsDigit(ch))
                    {
                        goto Label_01AF;
                    }
                    num5--;
                Label_018E:
                    if (num5 >= num6)
                    {
                        goto Label_0175;
                    }
                Label_01AF:
                    while (num5 >= num6)
                    {
                        ch = str[num5];
                        if (JSScanner.IsDigit(ch))
                        {
                            break;
                        }
                        num5--;
                    }
                    if (num5 < (num4 - 1))
                    {
                        return ToNumber(str.Substring(num6, (num5 - num6) + 1), hexOK, octalOK, radix);
                    }
                    return double.NaN;
                }
            }
        Label_01EC:
            num7 = str.Length;
            int startIndex = 0;
            while ((startIndex < num7) && IsWhiteSpace(str[startIndex]))
            {
                startIndex++;
            }
            if (startIndex >= num7)
            {
                if (hexOK && octalOK)
                {
                    return double.NaN;
                }
                return 0.0;
            }
            int sign = 1;
            bool flag = false;
            if (str[startIndex] == '-')
            {
                sign = -1;
                startIndex++;
                flag = true;
            }
            else if (str[startIndex] == '+')
            {
                startIndex++;
                flag = true;
            }
            while ((startIndex < num7) && IsWhiteSpace(str[startIndex]))
            {
                startIndex++;
            }
            bool flag2 = (radix == null) || (radix is Microsoft.JScript.Missing);
            if ((((startIndex + 8) <= num7) && flag2) && (!octalOK && str.Substring(startIndex, 8).Equals("Infinity")))
            {
                if (sign <= 0)
                {
                    return double.NegativeInfinity;
                }
                return double.PositiveInfinity;
            }
            int num10 = 10;
            if (!flag2)
            {
                num10 = ToInt32(radix);
            }
            if (num10 == 0)
            {
                flag2 = true;
                num10 = 10;
            }
            else if ((num10 < 2) || (num10 > 0x24))
            {
                return double.NaN;
            }
            if ((startIndex < (num7 - 2)) && (str[startIndex] == '0'))
            {
                if ((str[startIndex + 1] == 'x') || (str[startIndex + 1] == 'X'))
                {
                    if (!hexOK)
                    {
                        return 0.0;
                    }
                    if (flag && !octalOK)
                    {
                        return double.NaN;
                    }
                    if (flag2)
                    {
                        num10 = 0x10;
                        startIndex += 2;
                    }
                    else if (num10 == 0x10)
                    {
                        startIndex += 2;
                    }
                }
                else if (octalOK && flag2)
                {
                    num10 = 8;
                }
            }
            if (startIndex < num7)
            {
                return ToNumber(parseRadix(str.ToCharArray(), (uint) num10, startIndex, sign, hexOK && octalOK));
            }
            return double.NaN;
        }

        public static object ToObject(object value, VsaEngine engine)
        {
            if (value is ScriptObject)
            {
                return value;
            }
            string arg = value as string;
            if (arg != null)
            {
                return engine.Globals.globalObject.originalString.ConstructImplicitWrapper(arg);
            }
            IConvertible iConvertible = GetIConvertible(value);
            switch (GetTypeCode(value, iConvertible))
            {
                case TypeCode.Object:
                    if (value is Array)
                    {
                        return engine.Globals.globalObject.originalArray.ConstructImplicitWrapper((Array) value);
                    }
                    return value;

                case TypeCode.Boolean:
                    return engine.Globals.globalObject.originalBoolean.ConstructImplicitWrapper(iConvertible.ToBoolean(null));

                case TypeCode.Char:
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
                    return engine.Globals.globalObject.originalNumber.ConstructImplicitWrapper(value);

                case TypeCode.DateTime:
                    return iConvertible.ToDateTime(null);

                case TypeCode.String:
                    return engine.Globals.globalObject.originalString.ConstructImplicitWrapper(iConvertible.ToString(null));
            }
            throw new JScriptException(JSError.NeedObject);
        }

        public static object ToObject2(object value, VsaEngine engine)
        {
            if (value is ScriptObject)
            {
                return value;
            }
            IConvertible iConvertible = GetIConvertible(value);
            switch (GetTypeCode(value, iConvertible))
            {
                case TypeCode.Object:
                    if (value is Array)
                    {
                        return engine.Globals.globalObject.originalArray.ConstructImplicitWrapper((Array) value);
                    }
                    return value;

                case TypeCode.Boolean:
                    return engine.Globals.globalObject.originalBoolean.ConstructImplicitWrapper(iConvertible.ToBoolean(null));

                case TypeCode.Char:
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
                    return engine.Globals.globalObject.originalNumber.ConstructImplicitWrapper(value);

                case TypeCode.DateTime:
                    return iConvertible.ToDateTime(null);

                case TypeCode.String:
                    return engine.Globals.globalObject.originalString.ConstructImplicitWrapper(iConvertible.ToString(null));
            }
            return null;
        }

        internal static object ToObject3(object value, VsaEngine engine)
        {
            if (value is ScriptObject)
            {
                return value;
            }
            IConvertible iConvertible = GetIConvertible(value);
            switch (GetTypeCode(value, iConvertible))
            {
                case TypeCode.Object:
                    if (value is Array)
                    {
                        return engine.Globals.globalObject.originalArray.ConstructWrapper((Array) value);
                    }
                    return value;

                case TypeCode.Boolean:
                    return engine.Globals.globalObject.originalBoolean.ConstructWrapper(iConvertible.ToBoolean(null));

                case TypeCode.Char:
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
                    return engine.Globals.globalObject.originalNumber.ConstructWrapper(value);

                case TypeCode.DateTime:
                    return iConvertible.ToDateTime(null);

                case TypeCode.String:
                    return engine.Globals.globalObject.originalString.ConstructWrapper(iConvertible.ToString(null));
            }
            return null;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object ToPrimitive(object value, PreferredType preferredType)
        {
            IConvertible iConvertible = GetIConvertible(value);
            TypeCode typeCode = GetTypeCode(value, iConvertible);
            return ToPrimitive(value, preferredType, iConvertible, typeCode);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static object ToPrimitive(object value, PreferredType preferredType, ref IConvertible ic)
        {
            TypeCode typeCode = GetTypeCode(value, ic);
            switch (typeCode)
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    object obj2 = ToPrimitive(value, preferredType, ic, typeCode);
                    if (obj2 != value)
                    {
                        value = obj2;
                        ic = GetIConvertible(value);
                    }
                    break;
                }
            }
            return value;
        }

        [DebuggerStepThrough, DebuggerHidden]
        private static object ToPrimitive(object value, PreferredType preferredType, IConvertible ic, TypeCode tcode)
        {
            IReflect reflect;
            TypeCode code = tcode;
            if (code != TypeCode.Object)
            {
                if (code == TypeCode.DateTime)
                {
                    return DateConstructor.ob.Construct(ic.ToDateTime(null)).GetDefaultValue(preferredType);
                }
                return value;
            }
            Array array = value as Array;
            if ((array != null) && (array.Rank == 1))
            {
                value = new ArrayWrapper(ArrayPrototype.ob, array, true);
            }
            if (value is ScriptObject)
            {
                object defaultValue = ((ScriptObject) value).GetDefaultValue(preferredType);
                if (GetTypeCode(defaultValue) != TypeCode.Object)
                {
                    return defaultValue;
                }
                if (((value != defaultValue) || (preferredType != PreferredType.String)) && (preferredType != PreferredType.LocaleString))
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
                if (!(value is JSObject))
                {
                    return value.ToString();
                }
                ScriptObject parent = ((JSObject) value).GetParent();
                if (parent is ClassScope)
                {
                    return ((ClassScope) parent).GetFullName();
                }
                return "[object Object]";
            }
            if ((value is Microsoft.JScript.Missing) || (value is System.Reflection.Missing))
            {
                return null;
            }
            if ((value is IReflect) && !(value is Type))
            {
                reflect = (IReflect) value;
            }
            else
            {
                reflect = value.GetType();
            }
            MethodInfo method = null;
            if ((preferredType == PreferredType.String) || (preferredType == PreferredType.LocaleString))
            {
                method = GetToXXXXMethod(reflect, typeof(string), true);
            }
            else
            {
                method = GetToXXXXMethod(reflect, typeof(double), true);
                if (method == null)
                {
                    method = GetToXXXXMethod(reflect, typeof(long), true);
                }
                if (method == null)
                {
                    method = GetToXXXXMethod(reflect, typeof(ulong), true);
                }
            }
            if (method != null)
            {
                method = new JSMethodInfo(method);
                return method.Invoke(null, BindingFlags.SuppressChangeType, null, new object[] { value }, null);
            }
            try
            {
                try
                {
                    MemberInfo info2 = LateBinding.SelectMember(JSBinder.GetDefaultMembers(Runtime.TypeRefs, reflect));
                    if (info2 != null)
                    {
                        switch (info2.MemberType)
                        {
                            case MemberTypes.Property:
                                return JSProperty.GetValue((PropertyInfo) info2, value, null);

                            case MemberTypes.NestedType:
                                return info2;

                            case MemberTypes.Event:
                                return null;

                            case MemberTypes.Field:
                                return ((FieldInfo) info2).GetValue(value);

                            case MemberTypes.Method:
                                return ((MethodInfo) info2).Invoke(value, new object[0]);
                        }
                    }
                    if (value == reflect)
                    {
                        Type type = value.GetType();
                        if (TypeReflector.GetTypeReflectorFor(type).Is__ComObject() && (!VsaEngine.executeForJSEE || !(value is IDebuggerObject)))
                        {
                            reflect = type;
                        }
                    }
                    if (VsaEngine.executeForJSEE)
                    {
                        IDebuggerObject obj4 = reflect as IDebuggerObject;
                        if (obj4 != null)
                        {
                            if (!obj4.IsScriptObject())
                            {
                                throw new JScriptException(JSError.NonSupportedInDebugger);
                            }
                            return reflect.InvokeMember("< JScript-" + preferredType.ToString() + " >", BindingFlags.SuppressChangeType | BindingFlags.ExactBinding | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.InvokeMethod, null, value, new object[0], null, null, new string[0]);
                        }
                    }
                    return reflect.InvokeMember(string.Empty, BindingFlags.SuppressChangeType | BindingFlags.ExactBinding | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.InvokeMethod, null, value, new object[0], null, null, new string[0]);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (IndexOutOfRangeException)
            {
            }
            catch (MissingMemberException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode != -2147352573)
                {
                    throw exception2;
                }
            }
            if (preferredType == PreferredType.Number)
            {
                return value;
            }
            if (value.GetType().IsCOMObject)
            {
                return "ActiveXObject";
            }
            if (value is char[])
            {
                return new string((char[]) value);
            }
            return value.ToString();
        }

        public static string ToString(bool b)
        {
            if (!b)
            {
                return "false";
            }
            return "true";
        }

        public static string ToString(double d)
        {
            long num = (long) d;
            if (num == d)
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
            if (d != d)
            {
                return "NaN";
            }
            if (double.IsPositiveInfinity(d))
            {
                return "Infinity";
            }
            if (double.IsNegativeInfinity(d))
            {
                return "-Infinity";
            }
            double num2 = (d < 0.0) ? -d : d;
            int num3 = 15;
            string str = num2.ToString("e14", CultureInfo.InvariantCulture);
            if (DoubleParse(str) != num2)
            {
                str = num2.ToString("e15", CultureInfo.InvariantCulture);
                num3 = 0x10;
                if (DoubleParse(str) != num2)
                {
                    str = num2.ToString("e16", CultureInfo.InvariantCulture);
                    num3 = 0x11;
                    if (DoubleParse(str) != num2)
                    {
                        str = num2.ToString("e17", CultureInfo.InvariantCulture);
                        num3 = 0x12;
                    }
                }
            }
            int num4 = int.Parse(str.Substring(num3 + 2, str.Length - (num3 + 2)), CultureInfo.InvariantCulture);
            while (str[num3] == '0')
            {
                num3--;
            }
            int num5 = num4 + 1;
            if ((num3 <= num5) && (num5 <= 0x15))
            {
                StringBuilder builder = new StringBuilder(num5 + 1);
                if (d < 0.0)
                {
                    builder.Append('-');
                }
                builder.Append(str[0]);
                if (num3 > 1)
                {
                    builder.Append(str, 2, num3 - 1);
                }
                if ((num4 - num3) >= 0)
                {
                    builder.Append('0', num5 - num3);
                }
                return builder.ToString();
            }
            if ((0 < num5) && (num5 <= 0x15))
            {
                StringBuilder builder2 = new StringBuilder(num3 + 2);
                if (d < 0.0)
                {
                    builder2.Append('-');
                }
                builder2.Append(str[0]);
                if (num5 > 1)
                {
                    builder2.Append(str, 2, num5 - 1);
                }
                builder2.Append('.');
                builder2.Append(str, num5 + 1, num3 - num5);
                return builder2.ToString();
            }
            if ((-6 < num5) && (num5 <= 0))
            {
                StringBuilder builder3 = new StringBuilder(2 - num5);
                if (d < 0.0)
                {
                    builder3.Append("-0.");
                }
                else
                {
                    builder3.Append("0.");
                }
                if (num5 < 0)
                {
                    builder3.Append('0', -num5);
                }
                builder3.Append(str[0]);
                builder3.Append(str, 2, num3 - 1);
                return builder3.ToString();
            }
            StringBuilder builder4 = new StringBuilder(0x1c);
            if (d < 0.0)
            {
                builder4.Append('-');
            }
            builder4.Append(str.Substring(0, (num3 == 1) ? 1 : (num3 + 1)));
            builder4.Append('e');
            if (num4 >= 0)
            {
                builder4.Append('+');
            }
            builder4.Append(num4);
            return builder4.ToString();
        }

        internal static string ToString(object value)
        {
            return ToString(value, PreferredType.String, true);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static string ToString(object value, bool explicitOK)
        {
            return ToString(value, PreferredType.String, explicitOK);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static string ToString(object value, IConvertible ic)
        {
            return ToString(value, PreferredType.String, ic, true);
        }

        internal static string ToString(object value, int radix)
        {
            if (((radix == 10) || (radix < 2)) || (radix > 0x24))
            {
                return ToString(value);
            }
            double d = ToNumber(value);
            if (d == 0.0)
            {
                return "0";
            }
            if (double.IsNaN(d))
            {
                return "NaN";
            }
            if (double.IsPositiveInfinity(d))
            {
                return "Infinity";
            }
            if (double.IsNegativeInfinity(d))
            {
                return "-Infinity";
            }
            StringBuilder builder = new StringBuilder();
            if (d < 0.0)
            {
                builder.Append('-');
                d = -d;
            }
            int num2 = rgcchSig[radix - 2];
            if ((d < 8.6736173798840355E-19) || (d >= 2.305843009213694E+18))
            {
                int num3 = ((int) Math.Log(d, (double) radix)) + 1;
                double num4 = Math.Pow((double) radix, (double) num3);
                if (double.IsPositiveInfinity(num4))
                {
                    num4 = Math.Pow((double) radix, (double) (--num3));
                }
                else if (num4 == 0.0)
                {
                    num4 = Math.Pow((double) radix, (double) (++num3));
                }
                d /= num4;
                while (d < 1.0)
                {
                    d *= radix;
                    num3--;
                }
                int digit = (int) d;
                builder.Append(ToDigit(digit));
                num2--;
                d -= digit;
                if (d != 0.0)
                {
                    builder.Append('.');
                    while ((d != 0.0) && (num2-- > 0))
                    {
                        d *= radix;
                        digit = (int) d;
                        if (digit >= radix)
                        {
                            digit = radix - 1;
                        }
                        builder.Append(ToDigit(digit));
                        d -= digit;
                    }
                }
                builder.Append((num3 >= 0) ? "(e+" : "(e");
                builder.Append(num3.ToString(CultureInfo.InvariantCulture));
                builder.Append(')');
            }
            else
            {
                int num6;
                int num7;
                if (d >= 1.0)
                {
                    double num9;
                    num7 = 1;
                    double num8 = 1.0;
                    while ((num9 = num8 * radix) <= d)
                    {
                        num7++;
                        num8 = num9;
                    }
                    for (int i = 0; i < num7; i++)
                    {
                        num6 = (int) (d / num8);
                        if (num6 >= radix)
                        {
                            num6 = radix - 1;
                        }
                        builder.Append(ToDigit(num6));
                        d -= num6 * num8;
                        num8 /= (double) radix;
                    }
                }
                else
                {
                    builder.Append('0');
                    num7 = 0;
                }
                if ((d != 0.0) && (num7 < num2))
                {
                    builder.Append('.');
                    while ((d != 0.0) && (num7 < num2))
                    {
                        d *= radix;
                        num6 = (int) d;
                        if (num6 >= radix)
                        {
                            num6 = radix - 1;
                        }
                        builder.Append(ToDigit(num6));
                        d -= num6;
                        if ((num6 != 0) || (num7 != 0))
                        {
                            num7++;
                        }
                    }
                }
            }
            return builder.ToString();
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static string ToString(object value, PreferredType pref, bool explicitOK)
        {
            string str = value as string;
            if (str != null)
            {
                return str;
            }
            StringObject obj2 = value as StringObject;
            if ((obj2 != null) && obj2.noExpando)
            {
                return obj2.value;
            }
            return ToString(value, pref, GetIConvertible(value), explicitOK);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static string ToString(object value, PreferredType pref, IConvertible ic, bool explicitOK)
        {
            Enum enum2 = value as Enum;
            if (enum2 != 0)
            {
                return enum2.ToString("G");
            }
            EnumWrapper wrapper = value as EnumWrapper;
            if (wrapper != null)
            {
                return wrapper.ToString();
            }
            TypeCode typeCode = GetTypeCode(value, ic);
            if (pref == PreferredType.LocaleString)
            {
                switch (typeCode)
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    {
                        double num = ic.ToDouble(null);
                        return num.ToString(((num <= -1E+15) || (num >= 1E+15)) ? "g" : "n", NumberFormatInfo.CurrentInfo);
                    }
                    case TypeCode.Int64:
                        return ic.ToInt64(null).ToString("n", NumberFormatInfo.CurrentInfo);

                    case TypeCode.UInt64:
                        return ic.ToUInt64(null).ToString("n", NumberFormatInfo.CurrentInfo);

                    case TypeCode.Decimal:
                        return ic.ToDecimal(null).ToString("n", NumberFormatInfo.CurrentInfo);
                }
            }
            switch (typeCode)
            {
                case TypeCode.Empty:
                    if (explicitOK)
                    {
                        return "undefined";
                    }
                    return null;

                case TypeCode.Object:
                    return ToString(ToPrimitive(value, pref, ref ic), ic);

                case TypeCode.DBNull:
                    if (explicitOK)
                    {
                        return "null";
                    }
                    return null;

                case TypeCode.Boolean:
                    if (ic.ToBoolean(null))
                    {
                        return "true";
                    }
                    return "false";

                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return ic.ToString(null);

                case TypeCode.Single:
                case TypeCode.Double:
                    return ToString(ic.ToDouble(null));

                case TypeCode.DateTime:
                    return ToString(DateConstructor.ob.Construct(ic.ToDateTime(null)));
            }
            return null;
        }

        internal static Type ToType(IReflect ir)
        {
            return ToType(Globals.TypeRefs, ir);
        }

        internal static Type ToType(TypeReferences typeRefs, IReflect ir)
        {
            if (ir is Type)
            {
                return (Type) ir;
            }
            if (ir is ClassScope)
            {
                return ((ClassScope) ir).GetTypeBuilderOrEnumBuilder();
            }
            if (ir is TypedArray)
            {
                return typeRefs.ToReferenceContext(((TypedArray) ir).ToType());
            }
            if (ir is ScriptFunction)
            {
                return typeRefs.ScriptFunction;
            }
            return typeRefs.ToReferenceContext(ir.GetType());
        }

        internal static Type ToType(string descriptor, Type elementType)
        {
            Module module = elementType.Module;
            if (module is ModuleBuilder)
            {
                return module.GetType(elementType.FullName + descriptor);
            }
            return module.Assembly.GetType(elementType.FullName + descriptor);
        }

        internal static string ToTypeName(IReflect ir)
        {
            if (ir is ClassScope)
            {
                return ((ClassScope) ir).GetName();
            }
            if (ir is JSObject)
            {
                return ((JSObject) ir).GetClassName();
            }
            if (ir is GlobalScope)
            {
                return "Global Object";
            }
            return ir.ToString();
        }

        internal static uint ToUint32(object value)
        {
            if (value is uint)
            {
                return (uint) value;
            }
            return ToUint32(value, GetIConvertible(value));
        }

        internal static uint ToUint32(object value, IConvertible ic)
        {
            switch (GetTypeCode(value, ic))
            {
                case TypeCode.Empty:
                    return 0;

                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    object obj2 = ToPrimitive(value, PreferredType.Number, ref ic);
                    if (obj2 == value)
                    {
                        return 0;
                    }
                    return ToUint32(obj2, ic);
                }
                case TypeCode.DBNull:
                    return 0;

                case TypeCode.Boolean:
                    if (ic.ToBoolean(null))
                    {
                        return 1;
                    }
                    return 0;

                case TypeCode.Char:
                    return ic.ToChar(null);

                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (uint) ic.ToInt64(null);

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return ic.ToUInt32(null);

                case TypeCode.UInt64:
                    return (uint) ic.ToUInt64(null);

                case TypeCode.Single:
                case TypeCode.Double:
                    return (uint) Runtime.DoubleToInt64(ic.ToDouble(null));

                case TypeCode.Decimal:
                    return (uint) Runtime.UncheckedDecimalToInt64(ic.ToDecimal(null));

                case TypeCode.String:
                    return (uint) Runtime.DoubleToInt64(ToNumber(ic.ToString(null)));
            }
            return 0;
        }
    }
}

