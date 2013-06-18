namespace System.Drawing
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class RectangleConverter : TypeConverter
    {
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
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str2 = str.Trim();
            if (str2.Length == 0)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            char ch = culture.TextInfo.ListSeparator[0];
            string[] strArray = str2.Split(new char[] { ch });
            int[] numArray = new int[strArray.Length];
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = (int) converter.ConvertFromString(context, culture, strArray[i]);
            }
            if (numArray.Length != 4)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("TextParseFailedFormat", new object[] { "text", str2, "x, y, width, height" }));
            }
            return new Rectangle(numArray[0], numArray[1], numArray[2], numArray[3]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (value is Rectangle)
            {
                if (destinationType == typeof(string))
                {
                    Rectangle rectangle = (Rectangle) value;
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    string[] strArray = new string[4];
                    int num = 0;
                    strArray[num++] = converter.ConvertToString(context, culture, rectangle.X);
                    strArray[num++] = converter.ConvertToString(context, culture, rectangle.Y);
                    strArray[num++] = converter.ConvertToString(context, culture, rectangle.Width);
                    strArray[num++] = converter.ConvertToString(context, culture, rectangle.Height);
                    return string.Join(separator, strArray);
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    Rectangle rectangle2 = (Rectangle) value;
                    ConstructorInfo constructor = typeof(Rectangle).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { rectangle2.X, rectangle2.Y, rectangle2.Width, rectangle2.Height });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
            {
                throw new ArgumentNullException("propertyValues");
            }
            object obj2 = propertyValues["X"];
            object obj3 = propertyValues["Y"];
            object obj4 = propertyValues["Width"];
            object obj5 = propertyValues["Height"];
            if ((((obj2 == null) || (obj3 == null)) || ((obj4 == null) || (obj5 == null))) || ((!(obj2 is int) || !(obj3 is int)) || (!(obj4 is int) || !(obj5 is int))))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("PropertyValueInvalidEntry"));
            }
            return new Rectangle((int) obj2, (int) obj3, (int) obj4, (int) obj5);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(Rectangle), attributes).Sort(new string[] { "X", "Y", "Width", "Height" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

