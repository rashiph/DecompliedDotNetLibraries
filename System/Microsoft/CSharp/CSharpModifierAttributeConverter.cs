namespace Microsoft.CSharp
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal abstract class CSharpModifierAttributeConverter : TypeConverter
    {
        protected CSharpModifierAttributeConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = (string) value;
                string[] names = this.Names;
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i].Equals(str))
                    {
                        return this.Values[i];
                    }
                }
            }
            return this.DefaultValue;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            object[] values = this.Values;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(value))
                {
                    return this.Names[i];
                }
            }
            return SR.GetString("toStringUnknown");
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new TypeConverter.StandardValuesCollection(this.Values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        protected abstract object DefaultValue { get; }

        protected abstract string[] Names { get; }

        protected abstract object[] Values { get; }
    }
}

