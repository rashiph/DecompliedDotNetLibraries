namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TreeViewImageIndexConverter : ImageIndexConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string strA = value as string;
            if (strA != null)
            {
                if (string.Compare(strA, System.Windows.Forms.SR.GetString("toStringDefault"), true, culture) == 0)
                {
                    return -1;
                }
                if (string.Compare(strA, System.Windows.Forms.SR.GetString("toStringNone"), true, culture) == 0)
                {
                    return -2;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value is int))
            {
                switch (((int) value))
                {
                    case -1:
                        return System.Windows.Forms.SR.GetString("toStringDefault");

                    case -2:
                        return System.Windows.Forms.SR.GetString("toStringNone");
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                object instance = context.Instance;
                PropertyDescriptor imageListProperty = ImageListUtils.GetImageListProperty(context.PropertyDescriptor, ref instance);
                while ((instance != null) && (imageListProperty == null))
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                    foreach (PropertyDescriptor descriptor2 in properties)
                    {
                        if (typeof(ImageList).IsAssignableFrom(descriptor2.PropertyType))
                        {
                            imageListProperty = descriptor2;
                            break;
                        }
                    }
                    if (imageListProperty == null)
                    {
                        PropertyDescriptor descriptor3 = properties[base.ParentImageListProperty];
                        if (descriptor3 != null)
                        {
                            instance = descriptor3.GetValue(instance);
                            continue;
                        }
                        instance = null;
                    }
                }
                if (imageListProperty != null)
                {
                    ImageList list = (ImageList) imageListProperty.GetValue(instance);
                    if (list != null)
                    {
                        int num = list.Images.Count + 2;
                        object[] values = new object[num];
                        values[num - 2] = -1;
                        values[num - 1] = -2;
                        for (int i = 0; i < (num - 2); i++)
                        {
                            values[i] = i;
                        }
                        return new TypeConverter.StandardValuesCollection(values);
                    }
                }
            }
            return new TypeConverter.StandardValuesCollection(new object[] { -1, -2 });
        }

        protected override bool IncludeNoneAsStandardValue
        {
            get
            {
                return false;
            }
        }
    }
}

