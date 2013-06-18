namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security;

    internal class DateTimeOffsetConverter2 : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (!(destinationType == typeof(string)) && !(destinationType == typeof(InstanceDescriptor)))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string input = ((string) value).Trim();
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            return DateTimeOffset.Parse(input, culture, DateTimeStyles.None);
        }

        [SecuritySafeCritical]
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is DateTimeOffset))
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                DateTimeOffset offset2 = (DateTimeOffset) value;
                return offset2.ToString("O", culture);
            }
            if (!(destinationType == typeof(InstanceDescriptor)) || !(value is DateTimeOffset))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            DateTimeOffset offset = (DateTimeOffset) value;
            Type type = typeof(int);
            ConstructorInfo constructor = typeof(DateTimeOffset).GetConstructor(new Type[] { type, type, type, type, type, type, type, typeof(TimeSpan) });
            if (constructor != null)
            {
                return new InstanceDescriptor(constructor, new object[] { offset.Year, offset.Month, offset.Day, offset.Hour, offset.Minute, offset.Second, offset.Millisecond, offset.Offset }, true);
            }
            return null;
        }
    }
}

