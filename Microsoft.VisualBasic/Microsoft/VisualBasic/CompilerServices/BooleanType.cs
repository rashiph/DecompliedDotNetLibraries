namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class BooleanType
    {
        private BooleanType()
        {
        }

        private static bool DecimalToBoolean(IConvertible ValueInterface)
        {
            return Convert.ToBoolean(ValueInterface.ToDecimal(null));
        }

        public static bool FromObject(object Value)
        {
            if (Value == null)
            {
                return false;
            }
            IConvertible valueInterface = Value as IConvertible;
            if (valueInterface != null)
            {
                switch (valueInterface.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (bool) Value;
                        }
                        return valueInterface.ToBoolean(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (((byte) Value) > 0);
                        }
                        return (valueInterface.ToByte(null) > 0);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (((short) Value) > 0);
                        }
                        return (valueInterface.ToInt16(null) > 0);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (((int) Value) > 0);
                        }
                        return (valueInterface.ToInt32(null) > 0);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (((long) Value) > 0L);
                        }
                        return (valueInterface.ToInt64(null) > 0L);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return !(((float) Value) == 0f);
                        }
                        return !(valueInterface.ToSingle(null) == 0f);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return !(((double) Value) == 0.0);
                        }
                        return !(valueInterface.ToDouble(null) == 0.0);

                    case TypeCode.Decimal:
                        return DecimalToBoolean(valueInterface);

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str == null)
                        {
                            return FromString(valueInterface.ToString(null));
                        }
                        return FromString(str);
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Boolean" }));
        }

        public static bool FromString(string Value)
        {
            bool flag;
            if (Value == null)
            {
                Value = "";
            }
            try
            {
                long num;
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                if (string.Compare(Value, bool.FalseString, true, cultureInfo) == 0)
                {
                    return false;
                }
                if (string.Compare(Value, bool.TrueString, true, cultureInfo) == 0)
                {
                    return true;
                }
                if (Utils.IsHexOrOctValue(Value, ref num))
                {
                    return (num > 0L);
                }
                flag = !(DoubleType.Parse(Value) == 0.0);
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Boolean" }), exception);
            }
            return flag;
        }
    }
}

