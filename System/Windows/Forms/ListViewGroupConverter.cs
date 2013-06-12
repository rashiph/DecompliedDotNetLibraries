namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal class ListViewGroupConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((((sourceType == typeof(string)) && (context != null)) && (context.Instance is ListViewItem)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || ((((destinationType == typeof(string)) && (context != null)) && (context.Instance is ListViewItem)) || base.CanConvertTo(context, destinationType)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = ((string) value).Trim();
                if ((context != null) && (context.Instance != null))
                {
                    ListViewItem instance = context.Instance as ListViewItem;
                    if ((instance != null) && (instance.ListView != null))
                    {
                        foreach (ListViewGroup group in instance.ListView.Groups)
                        {
                            if (group.Header == str)
                            {
                                return group;
                            }
                        }
                    }
                }
            }
            if ((value != null) && !value.Equals(System.Windows.Forms.SR.GetString("toStringNone")))
            {
                return base.ConvertFrom(context, culture, value);
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is ListViewGroup))
            {
                ListViewGroup group = (ListViewGroup) value;
                ConstructorInfo constructor = typeof(ListViewGroup).GetConstructor(new System.Type[] { typeof(string), typeof(HorizontalAlignment) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { group.Header, group.HeaderAlignment }, false);
                }
            }
            if ((destinationType == typeof(string)) && (value == null))
            {
                return System.Windows.Forms.SR.GetString("toStringNone");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                ListViewItem instance = context.Instance as ListViewItem;
                if ((instance != null) && (instance.ListView != null))
                {
                    ArrayList values = new ArrayList();
                    foreach (ListViewGroup group in instance.ListView.Groups)
                    {
                        values.Add(group);
                    }
                    values.Add(null);
                    return new TypeConverter.StandardValuesCollection(values);
                }
            }
            return null;
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

