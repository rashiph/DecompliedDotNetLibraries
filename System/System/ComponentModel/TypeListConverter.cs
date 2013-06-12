namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class TypeListConverter : TypeConverter
    {
        private Type[] types;
        private TypeConverter.StandardValuesCollection values;

        protected TypeListConverter(Type[] types)
        {
            this.types = types;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                foreach (Type type in this.types)
                {
                    if (value.Equals(type.FullName))
                    {
                        return type;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return SR.GetString("toStringNone");
                }
                return ((Type) value).FullName;
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is Type))
            {
                MethodInfo method = typeof(Type).GetMethod("GetType", new Type[] { typeof(string) });
                if (method != null)
                {
                    return new InstanceDescriptor(method, new object[] { ((Type) value).AssemblyQualifiedName });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                object[] objArray;
                if (this.types != null)
                {
                    objArray = new object[this.types.Length];
                    Array.Copy(this.types, objArray, this.types.Length);
                }
                else
                {
                    objArray = null;
                }
                this.values = new TypeConverter.StandardValuesCollection(objArray);
            }
            return this.values;
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

