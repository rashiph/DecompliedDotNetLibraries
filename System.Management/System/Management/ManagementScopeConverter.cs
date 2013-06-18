namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal class ManagementScopeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(ManagementScope)) || base.CanConvertFrom(context, sourceType));
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
            if ((value is ManagementScope) && (destinationType == typeof(InstanceDescriptor)))
            {
                ManagementScope scope = (ManagementScope) value;
                ConstructorInfo constructor = typeof(ManagementScope).GetConstructor(new Type[] { typeof(string) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { scope.Path.Path });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

