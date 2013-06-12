namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;

    [Obsolete("The recommended alternative is System.Convert and String.Format. http://go.microsoft.com/fwlink/?linkid=14202")]
    public sealed class ObjectConverter
    {
        internal static readonly char[] formatSeparator = new char[] { ',' };

        public static object ConvertValue(object value, Type toType, string formatString)
        {
            if ((value == null) || Convert.IsDBNull(value))
            {
                return value;
            }
            Type c = value.GetType();
            if (toType.IsAssignableFrom(c))
            {
                return value;
            }
            if (typeof(string).IsAssignableFrom(c))
            {
                if (typeof(int).IsAssignableFrom(toType))
                {
                    return Convert.ToInt32((string) value, CultureInfo.InvariantCulture);
                }
                if (typeof(bool).IsAssignableFrom(toType))
                {
                    return Convert.ToBoolean((string) value, CultureInfo.InvariantCulture);
                }
                if (typeof(DateTime).IsAssignableFrom(toType))
                {
                    return Convert.ToDateTime((string) value, CultureInfo.InvariantCulture);
                }
                if (typeof(decimal).IsAssignableFrom(toType))
                {
                    TypeConverter converter = new DecimalConverter();
                    return converter.ConvertFromInvariantString((string) value);
                }
                if (typeof(double).IsAssignableFrom(toType))
                {
                    return Convert.ToDouble((string) value, CultureInfo.InvariantCulture);
                }
                if (!typeof(short).IsAssignableFrom(toType))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Cannot_convert_from_to", new object[] { c.ToString(), toType.ToString() }));
                }
                return Convert.ToInt16((short) value, CultureInfo.InvariantCulture);
            }
            if (!typeof(string).IsAssignableFrom(toType))
            {
                throw new ArgumentException(System.Web.SR.GetString("Cannot_convert_from_to", new object[] { c.ToString(), toType.ToString() }));
            }
            if (typeof(int).IsAssignableFrom(c))
            {
                int num = (int) value;
                return num.ToString(formatString, CultureInfo.InvariantCulture);
            }
            if (typeof(bool).IsAssignableFrom(c))
            {
                string[] strArray = null;
                if (formatString != null)
                {
                    strArray = formatString.Split(formatSeparator);
                    if (strArray.Length != 2)
                    {
                        strArray = null;
                    }
                }
                if ((bool) value)
                {
                    if (strArray != null)
                    {
                        return strArray[0];
                    }
                    return "true";
                }
                if (strArray != null)
                {
                    return strArray[1];
                }
                return "false";
            }
            if (typeof(DateTime).IsAssignableFrom(c))
            {
                DateTime time = (DateTime) value;
                return time.ToString(formatString, CultureInfo.InvariantCulture);
            }
            if (typeof(decimal).IsAssignableFrom(c))
            {
                decimal num2 = (decimal) value;
                return num2.ToString(formatString, CultureInfo.InvariantCulture);
            }
            if (typeof(double).IsAssignableFrom(c))
            {
                double num3 = (double) value;
                return num3.ToString(formatString, CultureInfo.InvariantCulture);
            }
            if (typeof(float).IsAssignableFrom(c))
            {
                float num4 = (float) value;
                return num4.ToString(formatString, CultureInfo.InvariantCulture);
            }
            if (!typeof(short).IsAssignableFrom(c))
            {
                throw new ArgumentException(System.Web.SR.GetString("Cannot_convert_from_to", new object[] { c.ToString(), toType.ToString() }));
            }
            short num5 = (short) value;
            return num5.ToString(formatString, CultureInfo.InvariantCulture);
        }
    }
}

