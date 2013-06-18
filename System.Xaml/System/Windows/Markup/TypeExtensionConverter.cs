namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Security;
    using System.Xaml;

    internal class TypeExtensionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        [SecurityTreatAsSafe, SecurityCritical]
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            TypeExtension extension = value as TypeExtension;
            if (extension == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("MustBeOfType", new object[] { "value", "TypeExtension" }));
            }
            return new InstanceDescriptor(typeof(TypeExtension).GetConstructor(new Type[] { typeof(Type) }), new object[] { extension.Type });
        }
    }
}

