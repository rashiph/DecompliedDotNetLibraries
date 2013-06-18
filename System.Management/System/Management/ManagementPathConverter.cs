namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal class ManagementPathConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(ManagementPath)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((value is ManagementPath) && (destinationType == typeof(InstanceDescriptor)))
            {
                ManagementPath path = (ManagementPath) value;
                ConstructorInfo constructor = typeof(ManagementPath).GetConstructor(new Type[] { typeof(string) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { path.Path });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

