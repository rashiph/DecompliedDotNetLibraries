namespace System.Diagnostics.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal class CounterCreationDataConverter : ExpandableObjectConverter
    {
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
            if ((destinationType == typeof(InstanceDescriptor)) && (value is CounterCreationData))
            {
                CounterCreationData data = (CounterCreationData) value;
                ConstructorInfo constructor = typeof(CounterCreationData).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(PerformanceCounterType) });
                if (constructor != null)
                {
                    return new InstanceDescriptor(constructor, new object[] { data.CounterName, data.CounterHelp, data.CounterType });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

