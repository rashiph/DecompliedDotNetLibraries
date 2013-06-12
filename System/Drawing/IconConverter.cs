namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;

    public class IconConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(byte[]))
            {
                return true;
            }
            if (sourceType == typeof(InstanceDescriptor))
            {
                return false;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (((destinationType == typeof(Image)) || (destinationType == typeof(Bitmap))) || ((destinationType == typeof(byte[])) || base.CanConvertTo(context, destinationType)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is byte[])
            {
                return new Icon(new MemoryStream((byte[]) value));
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(Image)) || (destinationType == typeof(Bitmap)))
            {
                Icon icon = value as Icon;
                if (icon != null)
                {
                    return icon.ToBitmap();
                }
            }
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    return value.ToString();
                }
                return System.Drawing.SR.GetString("toStringNone");
            }
            if (!(destinationType == typeof(byte[])))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value == null)
            {
                return new byte[0];
            }
            MemoryStream outputStream = null;
            try
            {
                outputStream = new MemoryStream();
                Icon icon2 = value as Icon;
                if (icon2 != null)
                {
                    icon2.Save(outputStream);
                }
            }
            finally
            {
                if (outputStream != null)
                {
                    outputStream.Close();
                }
            }
            if (outputStream != null)
            {
                return outputStream.ToArray();
            }
            return null;
        }
    }
}

