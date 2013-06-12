namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class DecimalConverter : BaseNumberConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(InstanceDescriptor)) || !(value is decimal))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            object[] arguments = new object[] { decimal.GetBits((decimal) value) };
            MemberInfo constructor = typeof(decimal).GetConstructor(new Type[] { typeof(int[]) });
            if (constructor != null)
            {
                return new InstanceDescriptor(constructor, arguments);
            }
            return null;
        }

        internal override object FromString(string value, CultureInfo culture)
        {
            return decimal.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return decimal.Parse(value, NumberStyles.Float, formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToDecimal(value, CultureInfo.CurrentCulture);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            decimal num = (decimal) value;
            return num.ToString("G", formatInfo);
        }

        internal override bool AllowHex
        {
            get
            {
                return false;
            }
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(decimal);
            }
        }
    }
}

