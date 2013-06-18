namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ShortType
    {
        private ShortType()
        {
        }

        private static short DecimalToShort(IConvertible ValueInterface)
        {
            return Convert.ToInt16(ValueInterface.ToDecimal(null));
        }

        public static short FromObject(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible valueInterface = Value as IConvertible;
            if (valueInterface != null)
            {
                switch (valueInterface.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return (short) -(valueInterface.ToBoolean(null) > false);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return valueInterface.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (short) Value;
                        }
                        return valueInterface.ToInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (short) ((int) Value);
                        }
                        return (short) valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (short) ((long) Value);
                        }
                        return (short) valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (short) Math.Round((double) ((float) Value));
                        }
                        return (short) Math.Round((double) valueInterface.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (short) Math.Round((double) Value);
                        }
                        return (short) Math.Round(valueInterface.ToDouble(null));

                    case TypeCode.Decimal:
                        return DecimalToShort(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Short" }));
        }

        public static short FromString(string Value)
        {
            short num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (short) num2;
                }
                num = (short) Math.Round(DoubleType.Parse(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Short" }), exception);
            }
            return num;
        }
    }
}

