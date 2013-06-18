namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class InfiniteTimeSpanConverter : ConfigurationConverterBase
    {
        private static readonly TypeConverter s_TimeSpanConverter = TypeDescriptor.GetConverter(typeof(TimeSpan));

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (((string) data) == "Infinite")
            {
                return TimeSpan.MaxValue;
            }
            return s_TimeSpanConverter.ConvertFromInvariantString((string) data);
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(TimeSpan));
            if (((TimeSpan) value) == TimeSpan.MaxValue)
            {
                return "Infinite";
            }
            return s_TimeSpanConverter.ConvertToInvariantString(value);
        }
    }
}

