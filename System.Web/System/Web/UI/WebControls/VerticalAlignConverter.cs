namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class VerticalAlignConverter : EnumConverter
    {
        private static string[] stringValues = new string[] { "NotSet", "Top", "Middle", "Bottom" };

        public VerticalAlignConverter() : base(typeof(VerticalAlign))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertTo(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is string)
            {
                string str = ((string) value).Trim();
                if (str.Length == 0)
                {
                    return VerticalAlign.NotSet;
                }
                string str2 = str;
                if (str2 != null)
                {
                    if (!(str2 == "NotSet"))
                    {
                        if (str2 == "Top")
                        {
                            return VerticalAlign.Top;
                        }
                        if (str2 == "Middle")
                        {
                            return VerticalAlign.Middle;
                        }
                        if (str2 == "Bottom")
                        {
                            return VerticalAlign.Bottom;
                        }
                    }
                    else
                    {
                        return VerticalAlign.NotSet;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (((int) value) <= 3))
            {
                return stringValues[(int) value];
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

