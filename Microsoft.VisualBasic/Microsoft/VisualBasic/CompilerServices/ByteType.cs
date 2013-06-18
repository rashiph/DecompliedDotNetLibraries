namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ByteType
    {
        private ByteType()
        {
        }

        private static byte DecimalToByte(IConvertible ValueInterface)
        {
            return Convert.ToByte(ValueInterface.ToDecimal(null));
        }

        public static byte FromObject(object Value)
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
                        return (byte) -(valueInterface.ToBoolean(null) > false);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return valueInterface.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (byte) ((short) Value);
                        }
                        return (byte) valueInterface.ToInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (byte) ((int) Value);
                        }
                        return (byte) valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (byte) ((long) Value);
                        }
                        return (byte) valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (byte) Math.Round((double) ((float) Value));
                        }
                        return (byte) Math.Round((double) valueInterface.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (byte) Math.Round((double) Value);
                        }
                        return (byte) Math.Round(valueInterface.ToDouble(null));

                    case TypeCode.Decimal:
                        return DecimalToByte(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Byte" }));
        }

        public static byte FromString(string Value)
        {
            byte num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (byte) num2;
                }
                num = (byte) Math.Round(DoubleType.Parse(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Byte" }), exception);
            }
            return num;
        }
    }
}

