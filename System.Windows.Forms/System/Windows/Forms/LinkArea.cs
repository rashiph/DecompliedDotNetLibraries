namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(LinkArea.LinkAreaConverter))]
    public struct LinkArea
    {
        private int start;
        private int length;
        public LinkArea(int start, int length)
        {
            this.start = start;
            this.length = length;
        }

        public int Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEmpty
        {
            get
            {
                return ((this.length == this.start) && (this.start == 0));
            }
        }
        public override bool Equals(object o)
        {
            if (!(o is LinkArea))
            {
                return false;
            }
            LinkArea area = (LinkArea) o;
            return (this == area);
        }

        public override string ToString()
        {
            return ("{Start=" + this.Start.ToString(CultureInfo.CurrentCulture) + ", Length=" + this.Length.ToString(CultureInfo.CurrentCulture) + "}");
        }

        public static bool operator ==(LinkArea linkArea1, LinkArea linkArea2)
        {
            return ((linkArea1.start == linkArea2.start) && (linkArea1.length == linkArea2.length));
        }

        public static bool operator !=(LinkArea linkArea1, LinkArea linkArea2)
        {
            return !(linkArea1 == linkArea2);
        }

        public override int GetHashCode()
        {
            return ((this.start << 4) | this.length);
        }
        public class LinkAreaConverter : TypeConverter
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
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TextParseFailedFormat", new object[] { str, "start, length" }));
                }
                return new LinkArea(numArray[0], numArray[1]);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == null)
                {
                    throw new ArgumentNullException("destinationType");
                }
                if ((destinationType == typeof(string)) && (value is LinkArea))
                {
                    LinkArea area = (LinkArea) value;
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    string[] strArray = new string[2];
                    int num = 0;
                    strArray[num++] = converter.ConvertToString(context, culture, area.Start);
                    strArray[num++] = converter.ConvertToString(context, culture, area.Length);
                    return string.Join(separator, strArray);
                }
                if ((destinationType == typeof(InstanceDescriptor)) && (value is LinkArea))
                {
                    LinkArea area2 = (LinkArea) value;
                    ConstructorInfo constructor = typeof(LinkArea).GetConstructor(new System.Type[] { typeof(int), typeof(int) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { area2.Start, area2.Length });
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                return new LinkArea((int) propertyValues["Start"], (int) propertyValues["Length"]);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(LinkArea), attributes).Sort(new string[] { "Start", "Length" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}

