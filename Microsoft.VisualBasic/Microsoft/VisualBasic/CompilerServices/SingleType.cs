namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SingleType
    {
        private SingleType()
        {
        }

        private static float DecimalToSingle(IConvertible ValueInterface)
        {
            return Convert.ToSingle(ValueInterface.ToDecimal(null));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static float FromObject(object Value)
        {
            return FromObject(Value, null);
        }

        public static float FromObject(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return 0f;
            }
            IConvertible valueInterface = Value as IConvertible;
            if (valueInterface != null)
            {
                switch (valueInterface.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return (float) -(valueInterface.ToBoolean(null) > false);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (float) ((byte) Value);
                        }
                        return (float) valueInterface.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (float) ((short) Value);
                        }
                        return (float) valueInterface.ToInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (float) ((int) Value);
                        }
                        return (float) valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (float) ((long) Value);
                        }
                        return (float) valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (float) Value;
                        }
                        return valueInterface.ToSingle(null);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (float) ((double) Value);
                        }
                        return (float) valueInterface.ToDouble(null);

                    case TypeCode.Decimal:
                        return DecimalToSingle(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Single" }));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static float FromString(string Value)
        {
            return FromString(Value, null);
        }

        public static float FromString(string Value, NumberFormatInfo NumberFormat)
        {
            float num;
            if (Value == null)
            {
                return 0f;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (float) num2;
                }
                double d = DoubleType.Parse(Value, NumberFormat);
                if (((d < -3.4028234663852886E+38) || (d > 3.4028234663852886E+38)) && !double.IsInfinity(d))
                {
                    throw new OverflowException();
                }
                num = (float) d;
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Single" }), exception);
            }
            return num;
        }
    }
}

