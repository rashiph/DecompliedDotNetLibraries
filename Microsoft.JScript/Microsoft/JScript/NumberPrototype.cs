namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Text;

    public class NumberPrototype : NumberObject
    {
        internal static NumberConstructor _constructor;
        internal static readonly NumberPrototype ob = new NumberPrototype(ObjectPrototype.ob);

        internal NumberPrototype(ObjectPrototype parent) : base(parent, 0.0)
        {
            base.noExpando = true;
        }

        private static double ThisobToDouble(object thisob)
        {
            thisob = valueOf(thisob);
            return ((IConvertible) thisob).ToDouble(null);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toExponential)]
        public static string toExponential(object thisob, object fractionDigits)
        {
            double num2;
            double num = ThisobToDouble(thisob);
            if ((fractionDigits == null) || (fractionDigits is Missing))
            {
                num2 = 16.0;
            }
            else
            {
                num2 = Microsoft.JScript.Convert.ToInteger(fractionDigits);
            }
            if ((num2 < 0.0) || (num2 > 20.0))
            {
                throw new JScriptException(JSError.FractionOutOfRange);
            }
            StringBuilder builder = new StringBuilder("#.");
            for (int i = 0; i < num2; i++)
            {
                builder.Append('0');
            }
            builder.Append("e+0");
            return num.ToString(builder.ToString(), CultureInfo.InvariantCulture);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toFixed)]
        public static string toFixed(object thisob, double fractionDigits)
        {
            double num = ThisobToDouble(thisob);
            if (double.IsNaN(fractionDigits))
            {
                fractionDigits = 0.0;
            }
            else if ((fractionDigits < 0.0) || (fractionDigits > 20.0))
            {
                throw new JScriptException(JSError.FractionOutOfRange);
            }
            int num2 = (int) fractionDigits;
            return num.ToString("f" + num2.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toLocaleString)]
        public static string toLocaleString(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(valueOf(thisob), PreferredType.LocaleString, true);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toPrecision)]
        public static string toPrecision(object thisob, object precision)
        {
            string str;
            double d = ThisobToDouble(thisob);
            if ((precision == null) || (precision is Missing))
            {
                return Microsoft.JScript.Convert.ToString(d);
            }
            double num2 = Microsoft.JScript.Convert.ToInteger(precision);
            if ((num2 < 1.0) || (num2 > 21.0))
            {
                throw new JScriptException(JSError.PrecisionOutOfRange);
            }
            int num3 = (int) num2;
            if (double.IsNaN(d))
            {
                return "NaN";
            }
            if (double.IsInfinity(d))
            {
                if (d <= 0.0)
                {
                    return "-Infinity";
                }
                return "Infinity";
            }
            if (d >= 0.0)
            {
                str = "";
            }
            else
            {
                str = "-";
                d = -d;
            }
            string str2 = d.ToString("e" + ((num3 - 1)).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            int num4 = int.Parse(str2.Substring(str2.Length - 4), CultureInfo.InvariantCulture);
            str2 = str2.Substring(0, 1) + str2.Substring(2, num3 - 1);
            if ((num4 >= num3) || (num4 < -6))
            {
                return (str + str2.Substring(0, 1) + ((num3 > 1) ? ("." + str2.Substring(1)) : "") + ((num4 >= 0) ? "e+" : "e") + num4.ToString(CultureInfo.InvariantCulture));
            }
            if (num4 == (num3 - 1))
            {
                return (str + str2);
            }
            if (num4 >= 0)
            {
                return (str + str2.Substring(0, num4 + 1) + "." + str2.Substring(num4 + 1));
            }
            return (str + "0." + str2.PadLeft((num3 - num4) - 1, '0'));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toString)]
        public static string toString(object thisob, object radix)
        {
            int num = 10;
            if (radix is IConvertible)
            {
                double num2 = ((IConvertible) radix).ToDouble(CultureInfo.InvariantCulture);
                int num3 = (int) num2;
                if (num2 == num3)
                {
                    num = num3;
                }
            }
            if ((num < 2) || (num > 0x24))
            {
                num = 10;
            }
            return Microsoft.JScript.Convert.ToString(valueOf(thisob), num);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_valueOf)]
        public static object valueOf(object thisob)
        {
            if (thisob is NumberObject)
            {
                return ((NumberObject) thisob).value;
            }
            switch (Microsoft.JScript.Convert.GetTypeCode(thisob))
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
                    return thisob;
            }
            throw new JScriptException(JSError.NumberExpected);
        }

        public static NumberConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

