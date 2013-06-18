namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class TypePropertyValueProviderTypeConverter : TypePropertyTypeConverter
    {
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            IPropertyValueProvider provider = null;
            object[] instance = context.Instance as object[];
            if ((instance != null) && (instance.Length > 0))
            {
                provider = instance[0] as IPropertyValueProvider;
            }
            else
            {
                provider = context.Instance as IPropertyValueProvider;
            }
            ICollection values = new object[0];
            if ((provider != null) && (context != null))
            {
                values = provider.GetPropertyValues(context);
            }
            return new TypeConverter.StandardValuesCollection(values);
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

