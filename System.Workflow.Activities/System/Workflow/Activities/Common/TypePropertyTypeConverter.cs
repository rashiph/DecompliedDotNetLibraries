namespace System.Workflow.Activities.Common
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;

    internal class TypePropertyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }
            return (object.Equals(sourceType, typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            return (object.Equals(destinationType, typeof(Type)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            string str = valueToConvert as string;
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if (context != null)
            {
                ITypeProvider service = context.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (service != null)
                {
                    return service.GetType(str, true);
                }
            }
            return base.ConvertFrom(context, culture, valueToConvert);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((value is Type) && object.Equals(destinationType, typeof(string)))
            {
                return ((Type) value).FullName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

