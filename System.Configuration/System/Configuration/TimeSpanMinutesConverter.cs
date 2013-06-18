namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TimeSpanMinutesConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return TimeSpan.FromMinutes((double) long.Parse((string) data, CultureInfo.InvariantCulture));
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(TimeSpan));
            TimeSpan span = (TimeSpan) value;
            long totalMinutes = (long) span.TotalMinutes;
            return totalMinutes.ToString(CultureInfo.InvariantCulture);
        }
    }
}

