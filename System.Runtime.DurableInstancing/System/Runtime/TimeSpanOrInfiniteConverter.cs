namespace System.Runtime
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class TimeSpanOrInfiniteConverter : TimeSpanConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object data)
        {
            if (string.Equals((string) data, "infinite", StringComparison.OrdinalIgnoreCase))
            {
                return TimeSpan.MaxValue;
            }
            return base.ConvertFrom(context, cultureInfo, data);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type type)
        {
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            if (!(value is TimeSpan))
            {
                throw Fx.Exception.Argument("value", SRCore.IncompatibleArgumentType(typeof(TimeSpan), value.GetType()));
            }
            if (((TimeSpan) value) == TimeSpan.MaxValue)
            {
                return "Infinite";
            }
            return base.ConvertTo(context, cultureInfo, value, type);
        }
    }
}

