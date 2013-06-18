namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    public class PaddingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            str = str.Trim();
            if (str.Length == 0)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            char ch = culture.TextInfo.ListSeparator[0];
            string[] strArray = str.Split(new char[] { ch });
            int[] numArray = new int[strArray.Length];
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = (int) converter.ConvertFromString(context, culture, strArray[i]);
            }
            if (numArray.Length != 4)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("TextParseFailedFormat", new object[] { "value", str, "left, top, right, bottom" }));
            }
            return new Padding(numArray[0], numArray[1], numArray[2], numArray[3]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (value is Padding)
            {
                if (destinationType == typeof(string))
                {
                    Padding padding = (Padding) value;
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    string[] strArray = new string[4];
                    int num = 0;
                    strArray[num++] = converter.ConvertToString(context, culture, padding.Left);
                    strArray[num++] = converter.ConvertToString(context, culture, padding.Top);
                    strArray[num++] = converter.ConvertToString(context, culture, padding.Right);
                    strArray[num++] = converter.ConvertToString(context, culture, padding.Bottom);
                    return string.Join(separator, strArray);
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    Padding padding2 = (Padding) value;
                    if (padding2.ShouldSerializeAll())
                    {
                        return new InstanceDescriptor(typeof(Padding).GetConstructor(new System.Type[] { typeof(int) }), new object[] { padding2.All });
                    }
                    return new InstanceDescriptor(typeof(Padding).GetConstructor(new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }), new object[] { padding2.Left, padding2.Top, padding2.Right, padding2.Bottom });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (propertyValues == null)
            {
                throw new ArgumentNullException("propertyValues");
            }
            Padding padding = (Padding) context.PropertyDescriptor.GetValue(context.Instance);
            int all = (int) propertyValues["All"];
            if (padding.All != all)
            {
                return new Padding(all);
            }
            return new Padding((int) propertyValues["Left"], (int) propertyValues["Top"], (int) propertyValues["Right"], (int) propertyValues["Bottom"]);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(Padding), attributes).Sort(new string[] { "All", "Left", "Top", "Right", "Bottom" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

