namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;

    public class ActivityBindTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            ITypeDescriptorContext realContext = null;
            TypeConverter realTypeConverter = null;
            this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
            if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
            {
                return realTypeConverter.CanConvertFrom(realContext, sourceType);
            }
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (((destinationType == typeof(string)) && (context != null)) && ((context.PropertyDescriptor != null) && (context.PropertyDescriptor.GetValue(context.Instance) is ActivityBind)))
            {
                return true;
            }
            ITypeDescriptorContext realContext = null;
            TypeConverter realTypeConverter = null;
            this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
            if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
            {
                return realTypeConverter.CanConvertTo(realContext, destinationType);
            }
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            string str = valueToConvert as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, valueToConvert);
            }
            str = str.Trim();
            string[] strArray = this.Parse(str);
            object obj2 = (strArray.Length == 2) ? new ActivityBind(strArray[0], strArray[1]) : null;
            if ((obj2 == null) && ((context == null) || (context.PropertyDescriptor == null)))
            {
                return base.ConvertFrom(context, culture, valueToConvert);
            }
            if (obj2 != null)
            {
                return obj2;
            }
            ITypeDescriptorContext realContext = null;
            TypeConverter realTypeConverter = null;
            this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
            if (((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter))) && realTypeConverter.CanConvertFrom(realContext, typeof(string)))
            {
                return realTypeConverter.ConvertFrom(realContext, culture, str);
            }
            return valueToConvert;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            ActivityBind bind = value as ActivityBind;
            if (bind != null)
            {
                Activity component = PropertyDescriptorUtils.GetComponent(context) as Activity;
                component = (component != null) ? Helpers.ParseActivityForBind(component, bind.Name) : null;
                return string.Format(CultureInfo.InvariantCulture, "Activity={0}, Path={1}", new object[] { (component != null) ? component.QualifiedName : bind.Name, bind.Path });
            }
            ITypeDescriptorContext realContext = null;
            TypeConverter realTypeConverter = null;
            this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
            if (((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter))) && realTypeConverter.CanConvertTo(realContext, destinationType))
            {
                return (realTypeConverter.ConvertTo(realContext, culture, value, destinationType) as string);
            }
            return (base.ConvertTo(context, culture, value, destinationType) as string);
        }

        private void GetActualTypeConverterAndContext(ITypeDescriptorContext currentContext, out TypeConverter realTypeConverter, out ITypeDescriptorContext realContext)
        {
            realContext = currentContext;
            realTypeConverter = null;
            if ((currentContext != null) && (currentContext.PropertyDescriptor != null))
            {
                realTypeConverter = TypeDescriptor.GetConverter(currentContext.PropertyDescriptor.PropertyType);
                ActivityBindPropertyDescriptor propertyDescriptor = currentContext.PropertyDescriptor as ActivityBindPropertyDescriptor;
                if (((propertyDescriptor != null) && (propertyDescriptor.RealPropertyDescriptor != null)) && ((propertyDescriptor.RealPropertyDescriptor.Converter != null) && (propertyDescriptor.RealPropertyDescriptor.Converter.GetType() != typeof(ActivityBindTypeConverter))))
                {
                    realTypeConverter = propertyDescriptor.RealPropertyDescriptor.Converter;
                    realContext = new TypeDescriptorContext(currentContext, propertyDescriptor.RealPropertyDescriptor, currentContext.Instance);
                }
            }
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            ArrayList list = new ArrayList();
            if ((value is ActivityBind) && (context != null))
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, new Attribute[] { BrowsableAttribute.Yes });
                PropertyDescriptor realPropertyDescriptor = properties["Name"];
                if (realPropertyDescriptor != null)
                {
                    list.Add(new ActivityBindNamePropertyDescriptor(context, realPropertyDescriptor));
                }
                PropertyDescriptor descriptor2 = properties["Path"];
                if (descriptor2 != null)
                {
                    list.Add(new ActivityBindPathPropertyDescriptor(context, descriptor2));
                }
            }
            else if ((context != null) && (context.PropertyDescriptor != null))
            {
                ITypeDescriptorContext realContext = null;
                TypeConverter realTypeConverter = null;
                this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
                if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
                {
                    list.AddRange(realTypeConverter.GetProperties(realContext, value, attributes));
                }
            }
            return new PropertyDescriptorCollection((PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor)));
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            bool propertiesSupported = false;
            if ((context != null) && (context.PropertyDescriptor != null))
            {
                if (context.PropertyDescriptor.GetValue(context.Instance) is ActivityBind)
                {
                    return true;
                }
                ITypeDescriptorContext realContext = null;
                TypeConverter realTypeConverter = null;
                this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
                if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
                {
                    propertiesSupported = realTypeConverter.GetPropertiesSupported(realContext);
                }
            }
            return propertiesSupported;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList list = new ArrayList();
            if ((context == null) || (context.PropertyDescriptor == null))
            {
                return new TypeConverter.StandardValuesCollection(new ArrayList());
            }
            ITypeDescriptorContext realContext = null;
            TypeConverter realTypeConverter = null;
            this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
            if (((realTypeConverter != null) && realTypeConverter.GetStandardValuesSupported(realContext)) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
            {
                list.AddRange(realTypeConverter.GetStandardValues(realContext));
            }
            return new TypeConverter.StandardValuesCollection(list.ToArray());
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            bool standardValuesExclusive = false;
            if ((context != null) && (context.PropertyDescriptor != null))
            {
                object obj2 = (context.Instance != null) ? context.PropertyDescriptor.GetValue(context.Instance) : null;
                if (!(obj2 is ActivityBind))
                {
                    ITypeDescriptorContext realContext = null;
                    TypeConverter realTypeConverter = null;
                    this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
                    if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
                    {
                        standardValuesExclusive = realTypeConverter.GetStandardValuesExclusive(realContext);
                    }
                }
            }
            return standardValuesExclusive;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            bool standardValuesSupported = false;
            if ((context != null) && (context.PropertyDescriptor != null))
            {
                object obj2 = (context.Instance != null) ? context.PropertyDescriptor.GetValue(context.Instance) : null;
                if (!(obj2 is ActivityBind))
                {
                    ITypeDescriptorContext realContext = null;
                    TypeConverter realTypeConverter = null;
                    this.GetActualTypeConverterAndContext(context, out realTypeConverter, out realContext);
                    if ((realTypeConverter != null) && (realTypeConverter.GetType() != typeof(ActivityBindTypeConverter)))
                    {
                        standardValuesSupported = realTypeConverter.GetStandardValuesSupported(realContext);
                    }
                }
            }
            return standardValuesSupported;
        }

        private string[] Parse(string value)
        {
            string[] strArray = value.Split(new char[] { ',' }, 2);
            if (strArray.Length == 2)
            {
                string str = "Activity=";
                string str2 = "Path=";
                string str3 = strArray[0].Trim();
                string str4 = strArray[1].Trim();
                if (str3.StartsWith(str, StringComparison.OrdinalIgnoreCase) && str4.StartsWith(str2, StringComparison.OrdinalIgnoreCase))
                {
                    str3 = str3.Substring(str.Length);
                    str4 = str4.Substring(str2.Length);
                    return new string[] { str3, str4 };
                }
            }
            return new string[0];
        }
    }
}

