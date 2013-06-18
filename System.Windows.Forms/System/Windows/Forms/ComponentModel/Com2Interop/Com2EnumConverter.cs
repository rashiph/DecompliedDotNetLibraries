namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class Com2EnumConverter : TypeConverter
    {
        internal readonly Com2Enum com2Enum;
        private TypeConverter.StandardValuesCollection values;

        public Com2EnumConverter(Com2Enum enumObj)
        {
            this.com2Enum = enumObj;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return (base.CanConvertTo(context, destType) || destType.IsEnum);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return this.com2Enum.FromString((string) value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value != null))
            {
                string str = this.com2Enum.ToString(value);
                if (str != null)
                {
                    return str;
                }
                return "";
            }
            if (destinationType.IsEnum)
            {
                return Enum.ToObject(destinationType, value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                object[] values = this.com2Enum.Values;
                if (values != null)
                {
                    this.values = new TypeConverter.StandardValuesCollection(values);
                }
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return this.com2Enum.IsStrictEnum;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            string str = this.com2Enum.ToString(value);
            return ((str != null) && (str.Length > 0));
        }

        public void RefreshValues()
        {
            this.values = null;
        }
    }
}

