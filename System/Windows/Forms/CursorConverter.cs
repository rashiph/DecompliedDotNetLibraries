namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public class CursorConverter : TypeConverter
    {
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (!(sourceType == typeof(string)) && !(sourceType == typeof(byte[])))
            {
                return base.CanConvertFrom(context, sourceType);
            }
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(byte[])))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string b = ((string) value).Trim();
                foreach (PropertyInfo info in this.GetProperties())
                {
                    if (string.Equals(info.Name, b, StringComparison.OrdinalIgnoreCase))
                    {
                        object[] index = null;
                        return info.GetValue(null, index);
                    }
                }
            }
            if (value is byte[])
            {
                return new Cursor(new MemoryStream((byte[]) value));
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value != null))
            {
                PropertyInfo[] properties = this.GetProperties();
                int index = -1;
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo info = properties[i];
                    object[] objArray = null;
                    Cursor objA = (Cursor) info.GetValue(null, objArray);
                    if (objA == ((Cursor) value))
                    {
                        if (object.ReferenceEquals(objA, value))
                        {
                            return info.Name;
                        }
                        index = i;
                    }
                }
                if (index == -1)
                {
                    throw new FormatException(System.Windows.Forms.SR.GetString("CursorCannotCovertToString"));
                }
                return properties[index].Name;
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is Cursor))
            {
                foreach (PropertyInfo info2 in this.GetProperties())
                {
                    if (info2.GetValue(null, null) == value)
                    {
                        return new InstanceDescriptor(info2, null);
                    }
                }
            }
            if (!(destinationType == typeof(byte[])))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value != null)
            {
                MemoryStream stream = new MemoryStream();
                ((Cursor) value).SavePicture(stream);
                stream.Close();
                return stream.ToArray();
            }
            return new byte[0];
        }

        private PropertyInfo[] GetProperties()
        {
            return typeof(Cursors).GetProperties(BindingFlags.Public | BindingFlags.Static);
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

