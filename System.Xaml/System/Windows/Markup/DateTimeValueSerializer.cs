namespace System.Windows.Markup
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xaml;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class DateTimeValueSerializer : ValueSerializer
    {
        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return (value is DateTime);
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            if (value == null)
            {
                throw base.GetConvertFromException(value);
            }
            if (value.Length == 0)
            {
                return DateTime.MinValue;
            }
            DateTimeFormatInfo format = (DateTimeFormatInfo) TypeConverterHelper.InvariantEnglishUS.GetFormat(typeof(DateTimeFormatInfo));
            DateTimeStyles styles = DateTimeStyles.RoundtripKind | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowLeadingWhite;
            if (format != null)
            {
                return DateTime.Parse(value, format, styles);
            }
            return DateTime.Parse(value, TypeConverterHelper.InvariantEnglishUS, styles);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            if ((value == null) || !(value is DateTime))
            {
                throw base.GetConvertToException(value, typeof(string));
            }
            DateTime time = (DateTime) value;
            StringBuilder builder = new StringBuilder("yyyy-MM-dd");
            if (time.TimeOfDay.TotalSeconds == 0.0)
            {
                if (time.Kind != DateTimeKind.Unspecified)
                {
                    builder.Append("'T'HH':'mm");
                }
            }
            else
            {
                long num = time.Ticks % 0x989680L;
                int second = time.Second;
                builder.Append("'T'HH':'mm");
                if ((second != 0) || (num != 0L))
                {
                    builder.Append("':'ss");
                    if (num != 0L)
                    {
                        builder.Append("'.'FFFFFFF");
                    }
                }
            }
            builder.Append("K");
            return time.ToString(builder.ToString(), TypeConverterHelper.InvariantEnglishUS);
        }
    }
}

