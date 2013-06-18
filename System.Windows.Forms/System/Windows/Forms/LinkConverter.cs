namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class LinkConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(string)))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str = ((string) value).Trim();
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
            if (numArray.Length != 2)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("TextParseFailedFormat", new object[] { str, "Start, Length" }));
            }
            return new LinkLabel.Link(numArray[0], numArray[1]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (value is LinkLabel.Link)
            {
                if (destinationType == typeof(string))
                {
                    LinkLabel.Link link = (LinkLabel.Link) value;
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    string[] strArray = new string[2];
                    int num = 0;
                    strArray[num++] = converter.ConvertToString(context, culture, link.Start);
                    strArray[num++] = converter.ConvertToString(context, culture, link.Length);
                    return string.Join(separator, strArray);
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    MemberInfo constructor;
                    LinkLabel.Link link2 = (LinkLabel.Link) value;
                    if (link2.LinkData == null)
                    {
                        constructor = typeof(LinkLabel.Link).GetConstructor(new System.Type[] { typeof(int), typeof(int) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { link2.Start, link2.Length }, true);
                        }
                    }
                    else
                    {
                        constructor = typeof(LinkLabel.Link).GetConstructor(new System.Type[] { typeof(int), typeof(int), typeof(object) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { link2.Start, link2.Length, link2.LinkData }, true);
                        }
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

