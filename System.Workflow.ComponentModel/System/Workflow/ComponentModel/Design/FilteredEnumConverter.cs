namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class FilteredEnumConverter : PropertyValueProviderTypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return Enum.Parse(context.PropertyDescriptor.PropertyType, (string) value);
        }
    }
}

