namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DateType
    {
        private DateType()
        {
        }

        public static DateTime FromObject(object Value)
        {
            if (Value == null)
            {
                DateTime time;
                return time;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.DateTime:
                        return convertible.ToDateTime(null);

                    case TypeCode.String:
                        return FromString(convertible.ToString(null), Utils.GetCultureInfo());
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Date" }));
        }

        public static DateTime FromString(string Value)
        {
            return FromString(Value, Utils.GetCultureInfo());
        }

        public static DateTime FromString(string Value, CultureInfo culture)
        {
            DateTime time2;
            if (!TryParse(Value, ref time2))
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Date" }));
            }
            return time2;
        }

        internal static bool TryParse(string Value, ref DateTime Result)
        {
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            return DateTime.TryParse(Utils.ToHalfwidthNumbers(Value, cultureInfo), cultureInfo, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out Result);
        }
    }
}

