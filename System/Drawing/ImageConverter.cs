namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ImageConverter : TypeConverter
    {
        private Type iconType = typeof(Icon);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == this.iconType)
            {
                return true;
            }
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
            return ((destinationType == typeof(byte[])) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Icon)
            {
                Icon icon = (Icon) value;
                return icon.ToBitmap();
            }
            byte[] rawData = value as byte[];
            if (rawData == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            Stream bitmapStream = null;
            bitmapStream = this.GetBitmapStream(rawData);
            if (bitmapStream == null)
            {
                bitmapStream = new MemoryStream(rawData);
            }
            return Image.FromStream(bitmapStream);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    Image image = (Image) value;
                    return image.ToString();
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
            bool flag = false;
            MemoryStream stream = null;
            Image original = null;
            try
            {
                stream = new MemoryStream();
                original = (Image) value;
                if (original.RawFormat.Equals(ImageFormat.Icon))
                {
                    flag = true;
                    original = new Bitmap(original, original.Width, original.Height);
                }
                original.Save(stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (flag && (original != null))
                {
                    original.Dispose();
                }
            }
            if (stream != null)
            {
                return stream.ToArray();
            }
            return null;
        }

        private unsafe Stream GetBitmapStream(byte[] rawData)
        {
            try
            {
                try
                {
                    fixed (byte* numRef = rawData)
                    {
                        IntPtr ptr = (IntPtr) numRef;
                        if (ptr == IntPtr.Zero)
                        {
                            return null;
                        }
                        if ((rawData.Length <= sizeof(SafeNativeMethods.OBJECTHEADER)) || (Marshal.ReadInt16(ptr) != 0x1c15))
                        {
                            return null;
                        }
                        SafeNativeMethods.OBJECTHEADER objectheader = (SafeNativeMethods.OBJECTHEADER) Marshal.PtrToStructure(ptr, typeof(SafeNativeMethods.OBJECTHEADER));
                        if (rawData.Length <= (objectheader.headersize + 0x12))
                        {
                            return null;
                        }
                        if (Encoding.ASCII.GetString(rawData, objectheader.headersize + 12, 6) != "PBrush")
                        {
                            return null;
                        }
                        byte[] bytes = Encoding.ASCII.GetBytes("BM");
                        for (int i = objectheader.headersize + 0x12; (i < (objectheader.headersize + 510)) && ((i + 1) < rawData.Length); i++)
                        {
                            if ((bytes[0] == numRef[i]) && (bytes[1] == numRef[i + 1]))
                            {
                                return new MemoryStream(rawData, i, rawData.Length - i);
                            }
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
            catch (OutOfMemoryException)
            {
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(Image), attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

