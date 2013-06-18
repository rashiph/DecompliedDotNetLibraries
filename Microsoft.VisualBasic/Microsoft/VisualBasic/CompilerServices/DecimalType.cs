namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DecimalType
    {
        private DecimalType()
        {
        }

        public static decimal FromBoolean(bool Value)
        {
            if (Value)
            {
                return decimal.MinusOne;
            }
            return decimal.Zero;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static decimal FromObject(object Value)
        {
            return FromObject(Value, null);
        }

        public static decimal FromObject(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return decimal.Zero;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return FromBoolean(convertible.ToBoolean(null));

                    case TypeCode.Byte:
                        return new decimal(convertible.ToByte(null));

                    case TypeCode.Int16:
                        return new decimal(convertible.ToInt16(null));

                    case TypeCode.Int32:
                        return new decimal(convertible.ToInt32(null));

                    case TypeCode.Int64:
                        return new decimal(convertible.ToInt64(null));

                    case TypeCode.Single:
                        return new decimal(convertible.ToSingle(null));

                    case TypeCode.Double:
                        return new decimal(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        return convertible.ToDecimal(null);

                    case TypeCode.String:
                        return FromString(convertible.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Decimal" }));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static decimal FromString(string Value)
        {
            return FromString(Value, null);
        }

        public static decimal FromString(string Value, NumberFormatInfo NumberFormat)
        {
            decimal num;
            if (Value == null)
            {
                return decimal.Zero;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return new decimal(num2);
                }
                num = Parse(Value, NumberFormat);
            }
            catch (OverflowException)
            {
                throw ExceptionUtils.VbMakeException(6);
            }
            catch (FormatException)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Decimal" }));
            }
            return num;
        }

        internal static NumberFormatInfo GetNormalizedNumberFormat(NumberFormatInfo InNumberFormat)
        {
            NumberFormatInfo info2;
            NumberFormatInfo info3 = InNumberFormat;
            if (((((info3.CurrencyDecimalSeparator != null) && (info3.NumberDecimalSeparator != null)) && ((info3.CurrencyGroupSeparator != null) && (info3.NumberGroupSeparator != null))) && (((info3.CurrencyDecimalSeparator.Length == 1) && (info3.NumberDecimalSeparator.Length == 1)) && ((info3.CurrencyGroupSeparator.Length == 1) && (info3.NumberGroupSeparator.Length == 1)))) && (((info3.CurrencyDecimalSeparator[0] == info3.NumberDecimalSeparator[0]) && (info3.CurrencyGroupSeparator[0] == info3.NumberGroupSeparator[0])) && (info3.CurrencyDecimalDigits == info3.NumberDecimalDigits)))
            {
                return InNumberFormat;
            }
            info3 = null;
            NumberFormatInfo info4 = InNumberFormat;
            if ((((info4.CurrencyDecimalSeparator != null) && (info4.NumberDecimalSeparator != null)) && ((info4.CurrencyDecimalSeparator.Length == info4.NumberDecimalSeparator.Length) && (info4.CurrencyGroupSeparator != null))) && ((info4.NumberGroupSeparator != null) && (info4.CurrencyGroupSeparator.Length == info4.NumberGroupSeparator.Length)))
            {
                int num;
                int num2 = info4.CurrencyDecimalSeparator.Length - 1;
                for (num = 0; num <= num2; num++)
                {
                    if (info4.CurrencyDecimalSeparator[num] != info4.NumberDecimalSeparator[num])
                    {
                        goto Label_018E;
                    }
                }
                int num3 = info4.CurrencyGroupSeparator.Length - 1;
                for (num = 0; num <= num3; num++)
                {
                    if (info4.CurrencyGroupSeparator[num] != info4.NumberGroupSeparator[num])
                    {
                        goto Label_018E;
                    }
                }
                return InNumberFormat;
            }
            info4 = null;
        Label_018E:
            info2 = (NumberFormatInfo) InNumberFormat.Clone();
            NumberFormatInfo info5 = info2;
            info5.CurrencyDecimalSeparator = info5.NumberDecimalSeparator;
            info5.CurrencyGroupSeparator = info5.NumberGroupSeparator;
            info5.CurrencyDecimalDigits = info5.NumberDecimalDigits;
            info5 = null;
            return info2;
        }

        public static decimal Parse(string Value, NumberFormatInfo NumberFormat)
        {
            decimal num;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            if (NumberFormat == null)
            {
                NumberFormat = cultureInfo.NumberFormat;
            }
            NumberFormatInfo normalizedNumberFormat = GetNormalizedNumberFormat(NumberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            try
            {
                num = decimal.Parse(Value, NumberStyles.Any, normalizedNumberFormat);
            }
            catch when (?)
            {
                num = decimal.Parse(Value, NumberStyles.Any, NumberFormat);
            }
            catch (Exception exception2)
            {
                throw exception2;
            }
            return num;
        }
    }
}

