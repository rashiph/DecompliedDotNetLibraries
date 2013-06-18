namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DoubleType
    {
        private DoubleType()
        {
        }

        private static double DecimalToDouble(IConvertible ValueInterface)
        {
            return Convert.ToDouble(ValueInterface.ToDecimal(null));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double FromObject(object Value)
        {
            return FromObject(Value, null);
        }

        public static double FromObject(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return 0.0;
            }
            IConvertible valueInterface = Value as IConvertible;
            if (valueInterface != null)
            {
                switch (valueInterface.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return (double) -(valueInterface.ToBoolean(null) > false);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (double) ((byte) Value);
                        }
                        return (double) valueInterface.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (double) ((short) Value);
                        }
                        return (double) valueInterface.ToInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (double) ((int) Value);
                        }
                        return (double) valueInterface.ToInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (double) ((long) Value);
                        }
                        return (double) valueInterface.ToInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (double) ((float) Value);
                        }
                        return (double) valueInterface.ToSingle(null);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (double) Value;
                        }
                        return valueInterface.ToDouble(null);

                    case TypeCode.Decimal:
                        return DecimalToDouble(valueInterface);

                    case TypeCode.String:
                        return FromString(valueInterface.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Double" }));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double FromString(string Value)
        {
            return FromString(Value, null);
        }

        public static double FromString(string Value, NumberFormatInfo NumberFormat)
        {
            double num;
            if (Value == null)
            {
                return 0.0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (double) num2;
                }
                num = Parse(Value, NumberFormat);
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Double" }), exception);
            }
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double Parse(string Value)
        {
            return Parse(Value, null);
        }

        public static double Parse(string Value, NumberFormatInfo NumberFormat)
        {
            double num;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            if (NumberFormat == null)
            {
                NumberFormat = cultureInfo.NumberFormat;
            }
            NumberFormatInfo normalizedNumberFormat = DecimalType.GetNormalizedNumberFormat(NumberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            try
            {
                num = double.Parse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat);
            }
            catch when (?)
            {
                num = double.Parse(Value, NumberStyles.Any, (IFormatProvider) NumberFormat);
            }
            catch (Exception exception2)
            {
                throw exception2;
            }
            return num;
        }

        internal static bool TryParse(string Value, ref double Result)
        {
            bool flag;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            NumberFormatInfo numberFormat = cultureInfo.NumberFormat;
            NumberFormatInfo normalizedNumberFormat = DecimalType.GetNormalizedNumberFormat(numberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            if (numberFormat == normalizedNumberFormat)
            {
                return double.TryParse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat, out Result);
            }
            try
            {
                Result = double.Parse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat);
                flag = true;
            }
            catch (FormatException)
            {
                try
                {
                    flag = double.TryParse(Value, NumberStyles.Any, (IFormatProvider) numberFormat, out Result);
                }
                catch (ArgumentException)
                {
                    flag = false;
                }
            }
            catch (StackOverflowException exception3)
            {
                throw exception3;
            }
            catch (OutOfMemoryException exception4)
            {
                throw exception4;
            }
            catch (ThreadAbortException exception5)
            {
                throw exception5;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
    }
}

