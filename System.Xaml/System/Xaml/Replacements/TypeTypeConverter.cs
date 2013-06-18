namespace System.Xaml.Replacements
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class TypeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string qualifiedTypeName = value as string;
            if ((context != null) && (qualifiedTypeName != null))
            {
                IXamlTypeResolver service = GetService<IXamlTypeResolver>(context);
                if (service != null)
                {
                    return service.Resolve(qualifiedTypeName);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Type type = value as Type;
            if (((context != null) && (type != null)) && (destinationType == typeof(string)))
            {
                string str = ConvertTypeToString(context, type);
                if (str != null)
                {
                    return str;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static string ConvertTypeToString(ITypeDescriptorContext context, Type type)
        {
            IXamlSchemaContextProvider service = GetService<IXamlSchemaContextProvider>(context);
            if (service == null)
            {
                return null;
            }
            if (service.SchemaContext == null)
            {
                return null;
            }
            XamlType xamlType = service.SchemaContext.GetXamlType(type);
            if (xamlType == null)
            {
                return null;
            }
            return XamlTypeTypeConverter.ConvertXamlTypeToString(context, xamlType);
        }

        private static TService GetService<TService>(ITypeDescriptorContext context) where TService: class
        {
            return (context.GetService(typeof(TService)) as TService);
        }
    }
}

