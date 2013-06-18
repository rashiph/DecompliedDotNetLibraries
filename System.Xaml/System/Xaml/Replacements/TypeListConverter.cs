namespace System.Xaml.Replacements
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;

    internal class TypeListConverter : TypeConverter
    {
        private static readonly TypeTypeConverter typeTypeConverter = new TypeTypeConverter();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string typeList = (string) value;
            if (context == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            string[] strArray = StringHelpers.SplitTypeList(typeList);
            Type[] typeArray = new Type[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                typeArray[i] = (Type) typeTypeConverter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, strArray[i]);
            }
            return typeArray;
        }
    }
}

