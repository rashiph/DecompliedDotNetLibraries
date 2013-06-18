namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class ThemeTypeConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (context.PropertyDescriptor != null))
            {
                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes).Sort(new PropertyDescriptorSorter());
        }

        private sealed class PropertyDescriptorSorter : IComparer
        {
            int IComparer.Compare(object obj1, object obj2)
            {
                PropertyDescriptor descriptor = obj1 as PropertyDescriptor;
                PropertyDescriptor descriptor2 = obj2 as PropertyDescriptor;
                DispIdAttribute attribute = descriptor.Attributes[typeof(DispIdAttribute)] as DispIdAttribute;
                DispIdAttribute attribute2 = descriptor2.Attributes[typeof(DispIdAttribute)] as DispIdAttribute;
                if (attribute == null)
                {
                    return 1;
                }
                if (attribute2 == null)
                {
                    return -1;
                }
                return (attribute.Value - attribute2.Value);
            }
        }
    }
}

