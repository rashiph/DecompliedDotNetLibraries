namespace System.Xaml.Replacements
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Markup;

    internal class DateTimeConverter2 : TypeConverter
    {
        private DateTimeValueSerializer _dateTimeValueSerializer = new DateTimeValueSerializer();
        private IValueSerializerContext _valueSerializerContext = new DateTimeValueSerializerContext();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return this._dateTimeValueSerializer.ConvertFromString(value as string, this._valueSerializerContext);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is DateTime))
            {
                return this._dateTimeValueSerializer.ConvertToString(value, this._valueSerializerContext);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

