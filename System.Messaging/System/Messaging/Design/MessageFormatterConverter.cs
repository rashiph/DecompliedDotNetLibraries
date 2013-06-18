namespace System.Messaging.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Messaging;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;

    internal class MessageFormatterConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value != null) && (value is string))
            {
                if (((string) value) == typeof(ActiveXMessageFormatter).Name)
                {
                    return new ActiveXMessageFormatter();
                }
                if (((string) value) == typeof(BinaryMessageFormatter).Name)
                {
                    return new BinaryMessageFormatter();
                }
                if (((string) value) == typeof(XmlMessageFormatter).Name)
                {
                    return new XmlMessageFormatter();
                }
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType != null) && (destinationType == typeof(string)))
            {
                if (value == null)
                {
                    return Res.GetString("toStringNone");
                }
                return value.GetType().Name;
            }
            if (destinationType == typeof(InstanceDescriptor))
            {
                if (value is XmlMessageFormatter)
                {
                    XmlMessageFormatter formatter = (XmlMessageFormatter) value;
                    ConstructorInfo constructor = typeof(XmlMessageFormatter).GetConstructor(new Type[] { typeof(string[]) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { formatter.TargetTypeNames });
                    }
                }
                else if (value is ActiveXMessageFormatter)
                {
                    ConstructorInfo member = typeof(ActiveXMessageFormatter).GetConstructor(new Type[0]);
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, new object[0]);
                    }
                }
                else if (value is BinaryMessageFormatter)
                {
                    BinaryMessageFormatter formatter2 = (BinaryMessageFormatter) value;
                    ConstructorInfo info3 = typeof(BinaryMessageFormatter).GetConstructor(new Type[] { typeof(FormatterAssemblyStyle), typeof(FormatterTypeStyle) });
                    if (info3 != null)
                    {
                        return new InstanceDescriptor(info3, new object[] { formatter2.TopObjectFormat, formatter2.TypeFormat });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            object[] values = new object[4];
            values[0] = new ActiveXMessageFormatter();
            values[1] = new BinaryMessageFormatter();
            values[2] = new XmlMessageFormatter();
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

