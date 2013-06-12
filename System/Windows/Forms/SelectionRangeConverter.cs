namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class SelectionRangeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (!(sourceType == typeof(string)) && !(sourceType == typeof(DateTime)))
            {
                return base.CanConvertFrom(context, sourceType);
            }
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(DateTime)))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = ((string) value).Trim();
                if (str.Length == 0)
                {
                    return new SelectionRange(DateTime.Now.Date, DateTime.Now.Date);
                }
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                char ch = culture.TextInfo.ListSeparator[0];
                string[] strArray = str.Split(new char[] { ch });
                if (strArray.Length != 2)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TextParseFailedFormat", new object[] { str, "Start" + ch + " End" }));
                }
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(DateTime));
                DateTime lower = (DateTime) converter.ConvertFromString(context, culture, strArray[0]);
                return new SelectionRange(lower, (DateTime) converter.ConvertFromString(context, culture, strArray[1]));
            }
            if (value is DateTime)
            {
                DateTime time3 = (DateTime) value;
                return new SelectionRange(time3, time3);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            SelectionRange range = value as SelectionRange;
            if (range != null)
            {
                if (destinationType == typeof(string))
                {
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string separator = culture.TextInfo.ListSeparator + " ";
                    PropertyDescriptorCollection properties = base.GetProperties(value);
                    string[] strArray = new string[properties.Count];
                    for (int i = 0; i < properties.Count; i++)
                    {
                        object obj2 = properties[i].GetValue(value);
                        strArray[i] = TypeDescriptor.GetConverter(obj2).ConvertToString(context, culture, obj2);
                    }
                    return string.Join(separator, strArray);
                }
                if (destinationType == typeof(DateTime))
                {
                    return range.Start;
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    ConstructorInfo constructor = typeof(SelectionRange).GetConstructor(new System.Type[] { typeof(DateTime), typeof(DateTime) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { range.Start, range.End });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            object obj2;
            try
            {
                obj2 = new SelectionRange((DateTime) propertyValues["Start"], (DateTime) propertyValues["End"]);
            }
            catch (InvalidCastException exception)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyValueInvalidEntry"), exception);
            }
            catch (NullReferenceException exception2)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyValueInvalidEntry"), exception2);
            }
            return obj2;
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(SelectionRange), attributes).Sort(new string[] { "Start", "End" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

