namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class ImageIndexConverter : Int32Converter
    {
        private string parentImageListProperty = "Parent";

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string strA = value as string;
            if ((strA != null) && (string.Compare(strA, System.Windows.Forms.SR.GetString("toStringNone"), true, culture) == 0))
            {
                return -1;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (((destinationType == typeof(string)) && (value is int)) && (((int) value) == -1))
            {
                return System.Windows.Forms.SR.GetString("toStringNone");
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
                        PropertyDescriptor descriptor3 = properties[this.ParentImageListProperty];
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
                        object[] objArray;
                        int count = list.Images.Count;
                        if (this.IncludeNoneAsStandardValue)
                        {
                            objArray = new object[count + 1];
                            objArray[count] = -1;
                        }
                        else
                        {
                            objArray = new object[count];
                        }
                        for (int i = 0; i < count; i++)
                        {
                            objArray[i] = i;
                        }
                        return new TypeConverter.StandardValuesCollection(objArray);
                    }
                }
            }
            if (this.IncludeNoneAsStandardValue)
            {
                return new TypeConverter.StandardValuesCollection(new object[] { -1 });
            }
            return new TypeConverter.StandardValuesCollection(new object[0]);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        protected virtual bool IncludeNoneAsStandardValue
        {
            get
            {
                return true;
            }
        }

        internal string ParentImageListProperty
        {
            get
            {
                return this.parentImageListProperty;
            }
            set
            {
                this.parentImageListProperty = value;
            }
        }
    }
}

