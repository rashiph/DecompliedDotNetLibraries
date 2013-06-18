namespace MS.Internal.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Xaml;

    internal sealed class TypeConverterValueSerializer : ValueSerializer
    {
        private TypeConverter converter;

        public TypeConverterValueSerializer(TypeConverter converter)
        {
            this.converter = converter;
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return this.converter.CanConvertTo(context, typeof(string));
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            return this.converter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, value);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return this.converter.ConvertToString(context, TypeConverterHelper.InvariantEnglishUS, value);
        }
    }
}

