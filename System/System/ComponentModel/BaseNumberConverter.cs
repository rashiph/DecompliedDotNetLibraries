namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class BaseNumberConverter : TypeConverter
    {
        protected BaseNumberConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
        {
            if (!base.CanConvertTo(context, t) && !t.IsPrimitive)
            {
                return false;
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = ((string) value).Trim();
                try
                {
                    if (this.AllowHex && (str[0] == '#'))
                    {
                        return this.FromString(str.Substring(1), 0x10);
                    }
                    if ((this.AllowHex && str.StartsWith("0x")) || ((str.StartsWith("0X") || str.StartsWith("&h")) || str.StartsWith("&H")))
                    {
                        return this.FromString(str.Substring(2), 0x10);
                    }
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    NumberFormatInfo format = (NumberFormatInfo) culture.GetFormat(typeof(NumberFormatInfo));
                    return this.FromString(str, format);
                }
                catch (Exception exception)
                {
                    throw this.FromStringError(str, exception);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (((destinationType == typeof(string)) && (value != null)) && this.TargetType.IsInstanceOfType(value))
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                NumberFormatInfo format = (NumberFormatInfo) culture.GetFormat(typeof(NumberFormatInfo));
                return this.ToString(value, format);
            }
            if (destinationType.IsPrimitive)
            {
                return Convert.ChangeType(value, destinationType, culture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        internal abstract object FromString(string value, CultureInfo culture);
        internal abstract object FromString(string value, NumberFormatInfo formatInfo);
        internal abstract object FromString(string value, int radix);
        internal virtual Exception FromStringError(string failedText, Exception innerException)
        {
            return new Exception(SR.GetString("ConvertInvalidPrimitive", new object[] { failedText, this.TargetType.Name }), innerException);
        }

        internal abstract string ToString(object value, NumberFormatInfo formatInfo);

        internal virtual bool AllowHex
        {
            get
            {
                return true;
            }
        }

        internal abstract Type TargetType { get; }
    }
}

