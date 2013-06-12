namespace System
{
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class UriTypeConverter : TypeConverter
    {
        private UriKind m_UriKind;

        public UriTypeConverter() : this(UriKind.RelativeOrAbsolute)
        {
        }

        internal UriTypeConverter(UriKind uriKind)
        {
            this.m_UriKind = uriKind;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }
            return ((sourceType == typeof(string)) || (typeof(Uri).IsAssignableFrom(sourceType) || base.CanConvertFrom(context, sourceType)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || ((destinationType == typeof(string)) || ((destinationType == typeof(Uri)) || base.CanConvertTo(context, destinationType))));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string uriString = value as string;
            if (uriString != null)
            {
                return new Uri(uriString, this.m_UriKind);
            }
            Uri uri = value as Uri;
            if (uri != null)
            {
                return new Uri(uri.OriginalString, (this.m_UriKind == UriKind.RelativeOrAbsolute) ? (uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative) : this.m_UriKind);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Uri uri = value as Uri;
            if ((uri != null) && (destinationType == typeof(InstanceDescriptor)))
            {
                return new InstanceDescriptor(typeof(Uri).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(UriKind) }, null), new object[] { uri.OriginalString, (this.m_UriKind == UriKind.RelativeOrAbsolute) ? (uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative) : this.m_UriKind });
            }
            if ((uri != null) && (destinationType == typeof(string)))
            {
                return uri.OriginalString;
            }
            if ((uri != null) && (destinationType == typeof(Uri)))
            {
                return new Uri(uri.OriginalString, (this.m_UriKind == UriKind.RelativeOrAbsolute) ? (uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative) : this.m_UriKind);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            string uriString = value as string;
            if (uriString != null)
            {
                Uri uri;
                return Uri.TryCreate(uriString, this.m_UriKind, out uri);
            }
            return (value is Uri);
        }
    }
}

