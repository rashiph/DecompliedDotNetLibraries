namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class CommaDelimitedStringCollectionConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            CommaDelimitedStringCollection strings = new CommaDelimitedStringCollection();
            strings.FromString((string) data);
            return strings;
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(CommaDelimitedStringCollection));
            CommaDelimitedStringCollection strings = value as CommaDelimitedStringCollection;
            if (strings != null)
            {
                return strings.ToString();
            }
            return null;
        }
    }
}

