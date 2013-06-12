namespace System.Drawing
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Reflection;

    public class ImageFormatConverter : TypeConverter
    {
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (str != null)
            {
                string b = str.Trim();
                foreach (PropertyInfo info in this.GetProperties())
                {
                    if (string.Equals(info.Name, b, StringComparison.OrdinalIgnoreCase))
                    {
                        object[] index = null;
                        return info.GetValue(null, index);
                    }
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
            if (value is ImageFormat)
            {
                PropertyInfo member = null;
                foreach (PropertyInfo info2 in this.GetProperties())
                {
                    if (info2.GetValue(null, null).Equals(value))
                    {
                        member = info2;
                        break;
                    }
                }
                if (member != null)
                {
                    if (destinationType == typeof(string))
                    {
                        return member.Name;
                    }
                    if (destinationType == typeof(InstanceDescriptor))
                    {
                        return new InstanceDescriptor(member, null);
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private PropertyInfo[] GetProperties()
        {
            return typeof(ImageFormat).GetProperties(BindingFlags.Public | BindingFlags.Static);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                ArrayList list = new ArrayList();
                foreach (PropertyInfo info in this.GetProperties())
                {
                    object[] index = null;
                    list.Add(info.GetValue(null, index));
                }
                this.values = new TypeConverter.StandardValuesCollection(list.ToArray());
            }
            return this.values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

