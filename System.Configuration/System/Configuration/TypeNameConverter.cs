namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class TypeNameConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            Type typeWithReflectionPermission = System.Configuration.TypeUtil.GetTypeWithReflectionPermission((string) data, false);
            if (typeWithReflectionPermission == null)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Type_cannot_be_resolved", new object[] { (string) data }));
            }
            return typeWithReflectionPermission;
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            if (!(value is Type))
            {
                base.ValidateType(value, typeof(Type));
            }
            string assemblyQualifiedName = null;
            if (value != null)
            {
                assemblyQualifiedName = ((Type) value).AssemblyQualifiedName;
            }
            return assemblyQualifiedName;
        }
    }
}

