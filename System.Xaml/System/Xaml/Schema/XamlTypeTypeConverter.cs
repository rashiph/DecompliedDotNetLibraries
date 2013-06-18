namespace System.Xaml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;

    public class XamlTypeTypeConverter : TypeConverter
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
            string typeName = value as string;
            if ((context != null) && (typeName != null))
            {
                XamlType type = ConvertStringToXamlType(context, typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static XamlType ConvertStringToXamlType(ITypeDescriptorContext context, string typeName)
        {
            IXamlNamespaceResolver service = GetService<IXamlNamespaceResolver>(context);
            if (service == null)
            {
                return null;
            }
            XamlTypeName name = XamlTypeName.Parse(typeName, service);
            IXamlSchemaContextProvider provider = GetService<IXamlSchemaContextProvider>(context);
            if (provider == null)
            {
                return null;
            }
            if (provider.SchemaContext == null)
            {
                return null;
            }
            return GetXamlTypeOrUnknown(provider.SchemaContext, name);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XamlType xamlType = value as XamlType;
            if (((context != null) && (xamlType != null)) && (destinationType == typeof(string)))
            {
                string str = ConvertXamlTypeToString(context, xamlType);
                if (str != null)
                {
                    return str;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        internal static string ConvertXamlTypeToString(ITypeDescriptorContext context, XamlType xamlType)
        {
            INamespacePrefixLookup service = GetService<INamespacePrefixLookup>(context);
            if (service == null)
            {
                return null;
            }
            XamlTypeName name = new XamlTypeName(xamlType);
            return name.ToString(service);
        }

        private static TService GetService<TService>(ITypeDescriptorContext context) where TService: class
        {
            return (context.GetService(typeof(TService)) as TService);
        }

        private static XamlType GetXamlTypeOrUnknown(XamlSchemaContext schemaContext, XamlTypeName typeName)
        {
            XamlType xamlType = schemaContext.GetXamlType(typeName);
            if (xamlType != null)
            {
                return xamlType;
            }
            XamlType[] typeArguments = null;
            if (typeName.HasTypeArgs)
            {
                typeArguments = new XamlType[typeName.TypeArguments.Count];
                for (int i = 0; i < typeName.TypeArguments.Count; i++)
                {
                    typeArguments[i] = GetXamlTypeOrUnknown(schemaContext, typeName.TypeArguments[i]);
                }
            }
            return new XamlType(typeName.Namespace, typeName.Name, typeArguments, schemaContext);
        }
    }
}

