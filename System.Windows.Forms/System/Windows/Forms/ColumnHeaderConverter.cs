namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class ColumnHeaderConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            ConstructorInfo constructor;
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(InstanceDescriptor)) || !(value is ColumnHeader))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            ColumnHeader header = (ColumnHeader) value;
            System.Type reflectionType = TypeDescriptor.GetReflectionType(value);
            InstanceDescriptor descriptor = null;
            if (header.ImageIndex != -1)
            {
                constructor = reflectionType.GetConstructor(new System.Type[] { typeof(int) });
                if (constructor != null)
                {
                    descriptor = new InstanceDescriptor(constructor, new object[] { header.ImageIndex }, false);
                }
            }
            if ((descriptor == null) && !string.IsNullOrEmpty(header.ImageKey))
            {
                constructor = reflectionType.GetConstructor(new System.Type[] { typeof(string) });
                if (constructor != null)
                {
                    descriptor = new InstanceDescriptor(constructor, new object[] { header.ImageKey }, false);
                }
            }
            if (descriptor != null)
            {
                return descriptor;
            }
            constructor = reflectionType.GetConstructor(new System.Type[0]);
            if (constructor == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("NoDefaultConstructor", new object[] { reflectionType.FullName }));
            }
            return new InstanceDescriptor(constructor, new object[0], false);
        }
    }
}

