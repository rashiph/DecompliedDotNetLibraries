namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class DateTimeConverter : TypeConverter
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
                string s = ((string) value).Trim();
                if (s.Length == 0)
                {
                    return DateTime.MinValue;
                }
                try
                {
                    DateTimeFormatInfo provider = null;
                    if (culture != null)
                    {
                        provider = (DateTimeFormatInfo) culture.GetFormat(typeof(DateTimeFormatInfo));
                    }
                    if (provider != null)
                    {
                        return DateTime.Parse(s, provider);
                    }
                    return DateTime.Parse(s, culture);
                }
                catch (FormatException exception)
                {
                    throw new FormatException(SR.GetString("ConvertInvalidPrimitive", new object[] { (string) value, "DateTime" }), exception);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is DateTime))
            {
                string shortDatePattern;
                DateTime time = (DateTime) value;
                if (time == DateTime.MinValue)
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
                    if (time.TimeOfDay.TotalSeconds == 0.0)
                    {
                        return time.ToString("yyyy-MM-dd", culture);
                    }
                    return time.ToString(culture);
                }
                if (time.TimeOfDay.TotalSeconds == 0.0)
                {
                    shortDatePattern = format.ShortDatePattern;
                }
                else
                {
                    shortDatePattern = format.ShortDatePattern + " " + format.ShortTimePattern;
                }
                return time.ToString(shortDatePattern, CultureInfo.CurrentCulture);
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is DateTime))
            {
                DateTime time2 = (DateTime) value;
                if (time2.Ticks == 0L)
                {
                    ConstructorInfo member = typeof(DateTime).GetConstructor(new Type[] { typeof(long) });
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, new object[] { time2.Ticks });
                    }
                }
                ConstructorInfo constructor = typeof(DateTime).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { time2.Year, time2.Month, time2.Day, time2.Hour, time2.Minute, time2.Second, time2.Millisecond });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

