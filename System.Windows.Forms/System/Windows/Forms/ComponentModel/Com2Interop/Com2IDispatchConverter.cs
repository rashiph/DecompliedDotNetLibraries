namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class Com2IDispatchConverter : Com2ExtendedTypeConverter
    {
        private bool allowExpand;
        protected static readonly string none = SR.GetString("toStringNone");
        private Com2PropertyDescriptor propDesc;

        public Com2IDispatchConverter(Com2PropertyDescriptor propDesc, bool allowExpand) : base(propDesc.PropertyType)
        {
            this.propDesc = propDesc;
            this.allowExpand = allowExpand;
        }

        public Com2IDispatchConverter(Com2PropertyDescriptor propDesc, bool allowExpand, TypeConverter baseConverter) : base(baseConverter)
        {
            this.propDesc = propDesc;
            this.allowExpand = allowExpand;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value == null)
            {
                return none;
            }
            string name = ComNativeDescriptor.Instance.GetName(value);
            if ((name == null) || (name.Length == 0))
            {
                name = ComNativeDescriptor.Instance.GetClassName(value);
            }
            if (name == null)
            {
                return "(Object)";
            }
            return name;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(value, attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return this.allowExpand;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}

