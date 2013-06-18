namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class TimeSpanMinutesOrInfiniteConverter : TimeSpanMinutesConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (((string) data) == "Infinite")
            {
                return TimeSpan.MaxValue;
            }
            return base.ConvertFrom(ctx, ci, data);
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(TimeSpan));
            if (((TimeSpan) value) == TimeSpan.MaxValue)
            {
                return "Infinite";
            }
            return base.ConvertTo(ctx, ci, value, type);
        }
    }
}

