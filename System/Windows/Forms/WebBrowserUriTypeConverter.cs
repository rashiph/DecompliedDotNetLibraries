namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class WebBrowserUriTypeConverter : UriTypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Uri uri = base.ConvertFrom(context, culture, value) as Uri;
            if (((uri != null) && !string.IsNullOrEmpty(uri.OriginalString)) && !uri.IsAbsoluteUri)
            {
                try
                {
                    uri = new Uri("http://" + uri.OriginalString.Trim());
                }
                catch (UriFormatException)
                {
                }
            }
            return uri;
        }
    }
}

