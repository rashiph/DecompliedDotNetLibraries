namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xaml;

    public class NameReferenceConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if ((context == null) || !(context.GetService(typeof(IXamlNameProvider)) is IXamlNameProvider))
            {
                return false;
            }
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            IXamlNameResolver service = (IXamlNameResolver) context.GetService(typeof(IXamlNameResolver));
            if (service == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MissingNameResolver"));
            }
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MustHaveName"));
            }
            object fixupToken = service.Resolve(str);
            if (fixupToken == null)
            {
                string[] names = new string[] { str };
                fixupToken = service.GetFixupToken(names, true);
            }
            return fixupToken;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            IXamlNameProvider service = (IXamlNameProvider) context.GetService(typeof(IXamlNameProvider));
            if (service == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MissingNameProvider"));
            }
            return service.GetName(value);
        }
    }
}

