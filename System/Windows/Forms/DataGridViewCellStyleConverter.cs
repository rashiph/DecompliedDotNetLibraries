namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    public class DataGridViewCellStyleConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is DataGridViewCellStyle))
            {
                return new InstanceDescriptor(value.GetType().GetConstructor(new System.Type[0]), new object[0], false);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

