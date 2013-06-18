namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    public abstract class ConfigurationConverterBase : TypeConverter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ConfigurationConverterBase()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        internal void ValidateType(object value, Type expected)
        {
            if ((value != null) && (value.GetType() != expected))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Converter_unsupported_value_type", new object[] { expected.Name }));
            }
        }
    }
}

