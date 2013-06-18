namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal class ManagementQueryConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(ManagementQuery)) || base.CanConvertFrom(context, sourceType));
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
            if ((value is EventQuery) && (destinationType == typeof(InstanceDescriptor)))
            {
                EventQuery query = (EventQuery) value;
                ConstructorInfo constructor = typeof(EventQuery).GetConstructor(new Type[] { typeof(string) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { query.QueryString });
                }
            }
            if ((value is ObjectQuery) && (destinationType == typeof(InstanceDescriptor)))
            {
                ObjectQuery query2 = (ObjectQuery) value;
                ConstructorInfo member = typeof(ObjectQuery).GetConstructor(new Type[] { typeof(string) });
                if (member != null)
                {
                    return new InstanceDescriptor(member, new object[] { query2.QueryString });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

