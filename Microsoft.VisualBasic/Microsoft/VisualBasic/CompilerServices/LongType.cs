namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LongType
    {
        private LongType()
        {
        }

        private static long DecimalToLong(IConvertible ValueInterface)
        {
            return Convert.ToInt64(ValueInterface.ToDecimal(null));
        }

        public static long FromObject(object Value)
        {
            if (Value == null)
            {
                return 0L;
            }
            IConvertible valueInterface = Value as IConvertible;
            if (valueInterface != null)
            {
                switch (valueInterface.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return (long) -(valueInterface.ToBoolean(null) > false);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (long) ((byte) Value);
                        }
                        return (long) valueInterface.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (long) ((short) Value);
                        }
                        return (long) valueInterface.ToInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (long) ((int) Value);
                        }
                        return (long) valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (long) Value;
                        }
                        return valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (long) Math.Round((double) ((float) Value));
                        }
                        return (long) Math.Round((double) valueInterface.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (long) Math.Round((double) Value);
                        }
                        return (long) Math.Round(valueInterface.ToDouble(null));

                    case TypeCode.Decimal:
                        return DecimalToLong(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Long" }));
        }

        public static long FromString(string Value)
        {
            long num;
            if (Value == null)
            {
                return 0L;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return num2;
                }
                num = Convert.ToInt64(DecimalType.Parse(Value, null));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Long" }), exception);
            }
            return num;
        }
    }
}

