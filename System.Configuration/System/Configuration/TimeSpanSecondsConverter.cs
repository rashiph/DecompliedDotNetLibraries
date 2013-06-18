namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TimeSpanSecondsConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            long num = 0L;
            try
            {
                num = long.Parse((string) data, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Converter_timespan_not_in_second"));
            }
            return TimeSpan.FromSeconds((double) num);
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(TimeSpan));
            TimeSpan span = (TimeSpan) value;
            long totalSeconds = (long) span.TotalSeconds;
            return totalSeconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}

