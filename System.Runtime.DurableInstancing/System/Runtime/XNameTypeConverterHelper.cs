namespace System.Runtime
{
    using System;
    using System.ComponentModel;
    using System.Xaml;
    using System.Xml.Linq;

    internal static class XNameTypeConverterHelper
    {
        public static bool CanConvertFrom(Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public static bool CanConvertTo(Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object ConvertFrom(ITypeDescriptorContext context, object value)
        {
            return ConvertFromHelper(context, value);
        }

        internal static object ConvertFromHelper(ITypeDescriptorContext context, object value)
        {
            string str2;
            string str3;
            if (value == null)
            {
                return null;
            }
            string expandedName = value as string;
            if (expandedName == null)
            {
                return null;
            }
            expandedName = expandedName.Trim();
            if (expandedName == string.Empty)
            {
                return null;
            }
            IXamlNamespaceResolver service = context.GetService(typeof(IXamlNamespaceResolver)) as IXamlNamespaceResolver;
            if (service == null)
            {
                return null;
            }
            if (expandedName[0] == '{')
            {
                return XName.Get(expandedName);
            }
            int index = expandedName.IndexOf(':');
            if (index >= 0)
            {
                str2 = expandedName.Substring(0, index);
                str3 = expandedName.Substring(index + 1);
            }
            else
            {
                str2 = string.Empty;
                str3 = expandedName;
            }
            string namespaceName = service.GetNamespace(str2);
            if (namespaceName == null)
            {
                throw Fx.Exception.AsError(new FormatException(SRCore.CouldNotResolveNamespacePrefix(str2)));
            }
            return XName.Get(str3, namespaceName);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object ConvertTo(ITypeDescriptorContext context, object value, Type destinationType)
        {
            return ConvertToHelper(context, value, destinationType);
        }

        internal static object ConvertToHelper(ITypeDescriptorContext context, object value, Type destinationType)
        {
            XName name = value as XName;
            if (((destinationType == typeof(string)) && (name != null)) && (context != null))
            {
                INamespacePrefixLookup service = (INamespacePrefixLookup) context.GetService(typeof(INamespacePrefixLookup));
                if (service != null)
                {
                    string prefix = service.LookupPrefix(name.Namespace.NamespaceName);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return name.LocalName;
                    }
                    return (prefix + ":" + name.LocalName);
                }
            }
            return null;
        }
    }
}

