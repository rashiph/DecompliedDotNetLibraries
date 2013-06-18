namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class InfiniteIntConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (((string) data) == "Infinite")
            {
                return 0x7fffffff;
            }
            return Convert.ToInt32((string) data, 10);
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(int));
            if (((int) value) == 0x7fffffff)
            {
                return "Infinite";
            }
            int num = (int) value;
            return num.ToString(CultureInfo.InvariantCulture);
        }
    }
}

