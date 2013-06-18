namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class TimeoutDurationConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            TimeSpan zero = TimeSpan.Zero;
            string str = value as string;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    zero = TimeSpan.Parse(str, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
                if (zero.Ticks < 0L)
                {
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_NegativeValue"), new object[] { value.ToString(), "TimeoutDuration" }));
                }
            }
            return zero;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is TimeSpan))
            {
                TimeSpan span = (TimeSpan) value;
                return span.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            values.Add(new TimeSpan(0, 0, 0));
            values.Add(new TimeSpan(0, 0, 15));
            values.Add(new TimeSpan(0, 1, 0));
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

