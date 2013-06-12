namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class DateTimeOffsetConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string input = ((string) value).Trim();
                if (input.Length == 0)
                {
                    return DateTimeOffset.MinValue;
                }
                try
                {
                    DateTimeFormatInfo formatProvider = null;
                    if (culture != null)
                    {
                        formatProvider = (DateTimeFormatInfo) culture.GetFormat(typeof(DateTimeFormatInfo));
                    }
                    if (formatProvider != null)
                    {
                        return DateTimeOffset.Parse(input, formatProvider);
                    }
                    return DateTimeOffset.Parse(input, culture);
                }
                catch (FormatException exception)
                {
                    throw new FormatException(SR.GetString("ConvertInvalidPrimitive", new object[] { (string) value, "DateTimeOffset" }), exception);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is DateTimeOffset))
            {
                string str;
                DateTimeOffset offset = (DateTimeOffset) value;
                if (offset == DateTimeOffset.MinValue)
                {
                    return string.Empty;
                }
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                DateTimeFormatInfo format = null;
                format = (DateTimeFormatInfo) culture.GetFormat(typeof(DateTimeFormatInfo));
                if (culture == CultureInfo.InvariantCulture)
                {
                    if (offset.TimeOfDay.TotalSeconds == 0.0)
                    {
                        return offset.ToString("yyyy-MM-dd zzz", culture);
                    }
                    return offset.ToString(culture);
                }
                if (offset.TimeOfDay.TotalSeconds == 0.0)
                {
                    str = format.ShortDatePattern + " zzz";
                }
                else
                {
                    str = format.ShortDatePattern + " " + format.ShortTimePattern + " zzz";
                }
                return offset.ToString(str, CultureInfo.CurrentCulture);
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is DateTimeOffset))
            {
                DateTimeOffset offset2 = (DateTimeOffset) value;
                if (offset2.Ticks == 0L)
                {
                    ConstructorInfo member = typeof(DateTimeOffset).GetConstructor(new Type[] { typeof(long) });
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, new object[] { offset2.Ticks });
                    }
                }
                ConstructorInfo constructor = typeof(DateTimeOffset).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(TimeSpan) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { offset2.Year, offset2.Month, offset2.Day, offset2.Hour, offset2.Minute, offset2.Second, offset2.Millisecond, offset2.Offset });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

