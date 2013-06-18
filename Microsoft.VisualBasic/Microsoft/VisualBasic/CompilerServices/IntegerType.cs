namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class IntegerType
    {
        private IntegerType()
        {
        }

        private static int DecimalToInteger(IConvertible ValueInterface)
        {
            return Convert.ToInt32(ValueInterface.ToDecimal(null));
        }

        public static int FromObject(object Value)
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
                        return (int) -(valueInterface.ToBoolean(null) > false);

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
                            return (int) Value;
                        }
                        return valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (int) ((long) Value);
                        }
                        return (int) valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (int) Math.Round((double) ((float) Value));
                        }
                        return (int) Math.Round((double) valueInterface.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (int) Math.Round((double) Value);
                        }
                        return (int) Math.Round(valueInterface.ToDouble(null));

                    case TypeCode.Decimal:
                        return DecimalToInteger(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Integer" }));
        }

        public static int FromString(string Value)
        {
            int num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (int) num2;
                }
                num = (int) Math.Round(DoubleType.Parse(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Integer" }), exception);
            }
            return num;
        }
    }
}

